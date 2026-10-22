using Microsoft.AspNetCore.Mvc;
using SportHub.Application.Services;
using SportHub.Application.DTOs;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly TwilioWhatsAppService _whatsappService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppWebhookController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    // In-memory session storage (for production, use Redis or database)
    private static readonly ConcurrentDictionary<string, UserSession> _sessions = new();

    public WhatsAppWebhookController(
        TwilioWhatsAppService whatsappService,
        IConfiguration configuration,
        ILogger<WhatsAppWebhookController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _whatsappService = whatsappService;
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("webhook")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ReceiveMessage([FromForm] TwilioWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Received WhatsApp message from {From}: {Body}", 
                request.From, request.Body);

            // Extract phone number and message
            var userMessage = request.Body;

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", "application/xml");
            }

            // Call Bedrock agent with the user's message
            var agentResponse = await InvokeBedrockAgentAsync(userMessage, request.From);

            // Return TwiML response directly (this way we don't need to send a separate message)
            var twimlResponse = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response>
    <Message>{System.Security.SecurityElement.Escape(agentResponse)}</Message>
</Response>";

            _logger.LogInformation("Sending response to {From}", request.From);

            return Content(twimlResponse, "application/xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp webhook");
            
            var errorResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response>
    <Message>Sorry, I encountered an error. Please try again later.</Message>
</Response>";

            return Content(errorResponse, "application/xml");
        }
    }

    private async Task<string> InvokeBedrockAgentAsync(string userMessage, string userId)
    {
        try
        {
            _logger.LogInformation("Processing message: {Message} from {User}", userMessage, userId);
            
            // Get or create user session
            var session = _sessions.GetOrAdd(userId, _ => new UserSession());
            
            // Handle cancellation
            if (userMessage.Equals("cancel", StringComparison.OrdinalIgnoreCase) || 
                userMessage.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                _sessions.TryRemove(userId, out _);
                return "Registration cancelled. Type 'register' to start again, or 'help' for options.";
            }
            
            // If user is in registration flow
            if (session.IsRegistering)
            {
                return await HandleRegistrationFlowAsync(userMessage, session, userId);
            }
            
            // Handle initial commands
            var messageLower = userMessage.ToLower();
            
            // Check for explicit command keywords first
            if (messageLower == "register" || messageLower == "sign up" || messageLower == "join")
            {
                var events = await GetAvailableEventsAsync();
                if (events.Any())
                {
                    session.IsRegistering = true;
                    session.Step = RegistrationStep.ChoosingEvent;
                    session.AvailableEvents = events;
                    
                    var eventList = string.Join("\n", events.Select((e, i) => $"{i + 1}. {e.Name} ({e.StartDate:MMM dd})"));
                    return $"🏆 Great! Let's register your team.\n\n" +
                           $"Available tournaments:\n{eventList}\n\n" +
                           $"Reply with the NUMBER (1, 2, etc.) of the event you want.\n\n" +
                           $"Type 'cancel' anytime to stop.";
                }
                return "I'm ready to help you register! However, there are no active events right now. Please check back later.";
            }
            else if (messageLower == "events" || messageLower == "tournaments")
            {
                var events = await GetAvailableEventsAsync();
                if (events.Any())
                {
                    var eventList = string.Join("\n", events.Select(e => $"• {e.Name} - {e.Sport} on {e.StartDate:MMM dd, yyyy}"));
                    return $"📅 Upcoming tournaments:\n\n{eventList}\n\nType 'register' to sign up your team!";
                }
                return "There are no active tournaments at the moment. Check back soon!";
            }
            else if (messageLower == "help" || messageLower == "hi" || messageLower == "hello")
            {
                return "👋 Welcome to SportHub! I'm your AI tournament assistant.\n\n" +
                       "I can help you:\n" +
                       "• *register* - Register your team\n" +
                       "• *events* - View tournaments\n" +
                       "• *fixtures* - Check your fixtures\n" +
                       "• *rules* - Tournament rules & FAQs\n" +
                       "• *help* - Show this menu\n\n" +
                       "Ask me anything about tournaments!";
            }
            else if (messageLower.Contains("fixture"))
            {
                // Query fixtures
                _logger.LogInformation("Fetching fixtures for user: {UserId}", userId);
                var fixtures = await GetUserFixturesAsync(userId);
                
                if (fixtures.Any())
                {
                    // Fetch event names for better display
                    var httpClient = _httpClientFactory.CreateClient();
                    var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
                    var eventIds = fixtures.Select(f => f.EventId).Distinct().ToList();
                    var eventNames = new Dictionary<int, string>();
                    
                    foreach (var evtId in eventIds)
                    {
                        var eventResponse = await httpClient.GetAsync($"{apiUrl}/Events/{evtId}");
                        if (eventResponse.IsSuccessStatusCode)
                        {
                            var eventJson = await eventResponse.Content.ReadAsStringAsync();
                            var eventData = JsonSerializer.Deserialize<JsonElement>(eventJson, new JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            });
                            
                            if (eventData.TryGetProperty("name", out var nameProp))
                            {
                                eventNames[evtId] = nameProp.GetString() ?? $"Event {evtId}";
                            }
                        }
                    }
                    
                    var fixtureList = string.Join("\n\n", fixtures
                        .GroupBy(f => f.Id) // Remove duplicates by fixture ID
                        .Select(g => g.First())
                        .Select(f => 
                        {
                            var eventName = eventNames.ContainsKey(f.EventId) 
                                ? eventNames[f.EventId] 
                                : $"Event {f.EventId}";
                            
                            return $"🏆 *{eventName}*\n" +
                                   $"{f.AgeGroup} {f.Division}\n" +
                                   $"⚽ {f.HomeTeam} vs {f.AwayTeam}\n" +
                                   $"⏰ Kick-off: {f.KickOffTime}\n" +
                                   $"📍 Pitch {(f.PitchId == 0 ? "TBD" : f.PitchId.ToString())}";
                        }));
                    
                    return $"📋 *Your Upcoming Fixtures*\n\n{fixtureList}\n\n" +
                           "Type 'events' to see other tournaments!";
                }
                
                _logger.LogWarning("No fixtures found for user: {UserId}", userId);
                return "📋 *No fixtures found*\n\n" +
                       "This could mean:\n" +
                       "• Your team hasn't been registered yet\n" +
                       "• Fixtures haven't been generated for your event\n" +
                       "• Your phone number doesn't match our records\n\n" +
                       "Type *register* to sign up your team!";
            }
            else
            {
                // Answer common questions with pattern matching
                var answer = GetAnswerForQuestion(userMessage);
                if (!string.IsNullOrEmpty(answer))
                {
                    return answer;
                }
                
                return "I can help you with:\n\n" +
                       "• *register* - Start registration\n" +
                       "• *events* - View tournaments\n" +
                       "• *fixtures* - Check fixtures\n" +
                       "• Ask questions like: 'What are the rules?' or 'How much is the fee?'\n\n" +
                       "What would you like to know?";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return "I'm having trouble right now. Please try again.";
        }
    }

    private async Task<string> HandleRegistrationFlowAsync(string userMessage, UserSession session, string userId)
    {
        switch (session.Step)
        {
            case RegistrationStep.ChoosingEvent:
                if (int.TryParse(userMessage, out int eventIndex) && 
                    eventIndex > 0 && eventIndex <= session.AvailableEvents.Count)
                {
                    session.SelectedEvent = session.AvailableEvents[eventIndex - 1];
                    session.Step = RegistrationStep.EnteringTeamName;
                    return $"✅ Great! You selected: *{session.SelectedEvent.Name}*\n\n" +
                           "What's your team name?";
                }
                return "Please enter a valid number from the list above.";

            case RegistrationStep.EnteringTeamName:
                session.TeamName = userMessage;
                session.Step = RegistrationStep.EnteringSchoolName;
                return $"✅ Team: *{userMessage}*\n\n" +
                       "What's your school/organization name?";

            case RegistrationStep.EnteringSchoolName:
                session.SchoolName = userMessage;
                session.Step = RegistrationStep.EnteringAgeGroup;
                
                // Show available age groups for this event
                var ageGroups = session.SelectedEvent?.AgeGroups ?? "";
                if (!string.IsNullOrEmpty(ageGroups))
                {
                    return $"✅ School: *{userMessage}*\n\n" +
                           $"Available age groups for this event:\n*{ageGroups}*\n\n" +
                           "Which age group is your team?";
                }
                return $"✅ School: *{userMessage}*\n\n" +
                       "What age group?\n" +
                       "(e.g., U11, U13, U15, U17, U19, Senior)";

            case RegistrationStep.EnteringAgeGroup:
                // Validate age group against event's allowed groups
                var allowedGroups = session.SelectedEvent?.AgeGroups ?? "";
                if (!string.IsNullOrEmpty(allowedGroups))
                {
                    var groupsList = allowedGroups.Split(',').Select(g => g.Trim().ToUpper()).ToList();
                    if (!groupsList.Contains(userMessage.Trim().ToUpper()))
                    {
                        return $"❌ '{userMessage}' is not allowed for this event.\n\n" +
                               $"Please choose from: *{allowedGroups}*";
                    }
                }
                
                session.AgeGroup = userMessage;
                session.Step = RegistrationStep.EnteringCoachName;
                return $"✅ Age Group: *{userMessage}*\n\n" +
                       "What's the coach's name?";

            case RegistrationStep.EnteringCoachName:
                session.CoachName = userMessage;
                session.Step = RegistrationStep.EnteringCoachEmail;
                return $"✅ Coach: *{userMessage}*\n\n" +
                       "What's the coach's email address?";

            case RegistrationStep.EnteringCoachEmail:
                if (!userMessage.Contains("@"))
                {
                    return "Please enter a valid email address (must contain @)";
                }
                session.CoachEmail = userMessage;
                session.CoachPhone = userId.Replace("whatsapp:", ""); // Use WhatsApp number
                session.Step = RegistrationStep.Confirming;
                
                return $"📋 *Registration Summary*\n\n" +
                       $"Event: {session.SelectedEvent?.Name}\n" +
                       $"Team: {session.TeamName}\n" +
                       $"School: {session.SchoolName}\n" +
                       $"Age Group: {session.AgeGroup}\n" +
                       $"Coach: {session.CoachName}\n" +
                       $"Email: {session.CoachEmail}\n" +
                       $"Phone: {session.CoachPhone} (from WhatsApp)\n\n" +
                       $"Reply *YES* to confirm or *CANCEL* to start over.";

            case RegistrationStep.Confirming:
                if (userMessage.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    // Register the team
                    var result = await RegisterTeamAsync(session);
                    _sessions.TryRemove(userId, out _); // Clear session
                    return result;
                }
                else if (userMessage.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                {
                    _sessions.TryRemove(userId, out _);
                    return "Registration cancelled. Type 'register' to start again!";
                }
                return "Please reply *YES* to confirm or *CANCEL* to abort.";

            default:
                return "Something went wrong. Type 'register' to start over.";
        }
    }

    private async Task<string> RegisterTeamAsync(UserSession session)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            // First, create the team
            var teamDto = new
            {
                name = session.TeamName,
                schoolName = session.SchoolName,
                coachName = session.CoachName,
                coachPhone = session.CoachPhone,
                coachEmail = session.CoachEmail,
                ageGroup = session.AgeGroup
            };

            var teamJson = JsonSerializer.Serialize(teamDto);
            var teamContent = new StringContent(teamJson, Encoding.UTF8, "application/json");
            
            var teamResponse = await httpClient.PostAsync($"{apiUrl}/Teams", teamContent);
            
            if (!teamResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create team: {StatusCode}", teamResponse.StatusCode);
                return "❌ Sorry, there was an error creating your team. Please try again later or contact support.";
            }

            var teamResponseJson = await teamResponse.Content.ReadAsStringAsync();
            var teamData = JsonSerializer.Deserialize<WhatsAppTeamResponse>(teamResponseJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            // Then, register for the event
            var registrationDto = new
            {
                eventId = session.SelectedEvent?.Id,
                teamId = teamData?.Id,
                ageGroup = session.AgeGroup,
                division = "" // Optional
            };

            var regJson = JsonSerializer.Serialize(registrationDto);
            var regContent = new StringContent(regJson, Encoding.UTF8, "application/json");
            
            var regResponse = await httpClient.PostAsync($"{apiUrl}/Registrations", regContent);
            
            if (regResponse.IsSuccessStatusCode)
            {
                return $"🎉 *Registration Successful!*\n\n" +
                       $"Team *{session.TeamName}* is now registered for *{session.SelectedEvent?.Name}*!\n\n" +
                       $"✅ Confirmation email sent to {session.CoachEmail}\n" +
                       $"✅ You'll receive fixture updates via WhatsApp\n\n" +
                       $"Good luck! 🏆\n\n" +
                       $"Type 'events' to see other tournaments.";
            }
            else
            {
                _logger.LogError("Failed to register team: {StatusCode}", regResponse.StatusCode);
                return "❌ Team created but registration failed. Please contact support with your team name.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return "❌ Registration failed. Please try again later.";
        }
    }

    private async Task<List<EventInfo>> GetAvailableEventsAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            var response = await httpClient.GetAsync($"{apiUrl}/Events");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<EventInfo>>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                // Only return future events
                return events?.Where(e => e.StartDate >= DateTime.Today).ToList() ?? new List<EventInfo>();
            }
            
            return new List<EventInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching events");
            return new List<EventInfo>();
        }
    }

    private string GetAnswerForQuestion(string question)
    {
        var questionLower = question.ToLower();
        
        // Rules and regulations
        if (questionLower.Contains("rule") || questionLower.Contains("regulation"))
        {
            return "📋 *Tournament Rules*\n\n" +
                   "• Teams: 11-18 players per squad\n" +
                   "• Registration: At least 7 days before event\n" +
                   "• Fee: R500 per team\n" +
                   "• Match duration varies by age group\n" +
                   "• Required equipment: Team jerseys, shin guards, appropriate footwear\n" +
                   "• Fair play and respect mandatory\n\n" +
                   "For full rules, email tournaments@sporthub.com";
        }
        
        // Fees and costs
        if (questionLower.Contains("fee") || questionLower.Contains("cost") || questionLower.Contains("price") || questionLower.Contains("pay"))
        {
            return "💰 *Registration Fees*\n\n" +
                   "Standard: R500 per team\n" +
                   "Late registration (< 7 days): R700\n\n" +
                   "Payment methods:\n" +
                   "• Bank transfer (details on registration)\n" +
                   "• Cash on tournament day\n" +
                   "• Card payment at venue";
        }
        
        // Match duration
        if (questionLower.Contains("long") || questionLower.Contains("duration") || questionLower.Contains("time") || questionLower.Contains("minutes"))
        {
            return "⏱️ *Match Duration by Age*\n\n" +
                   "• U11, U13: 2 × 25 min halves\n" +
                   "• U15, U17: 2 × 30 min halves\n" +
                   "• U19, Senior: 2 × 40 min halves\n\n" +
                   "Half-time: 5-10 minutes\n" +
                   "Minimum 60 minutes rest between matches for same team";
        }
        
        // Equipment
        if (questionLower.Contains("equipment") || questionLower.Contains("gear") || questionLower.Contains("kit") || questionLower.Contains("bring"))
        {
            return "⚽ *Required Equipment*\n\n" +
                   "Players must have:\n" +
                   "• Team jersey/kit (matching colors)\n" +
                   "• Shin guards (mandatory)\n" +
                   "• Appropriate footwear (boots or trainers)\n" +
                   "• Water bottle\n\n" +
                   "Balls and bibs provided by organizers";
        }
        
        // Weather and cancellation
        if (questionLower.Contains("rain") || questionLower.Contains("weather") || questionLower.Contains("cancel"))
        {
            return "🌦️ *Weather & Cancellations*\n\n" +
                   "• Matches proceed in light rain\n" +
                   "• Suspended if lightning or heavy rain\n" +
                   "• Updates sent via WhatsApp & email\n" +
                   "• Rescheduled fixtures announced 48hrs before\n\n" +
                   "Check messages on tournament day!";
        }
        
        // Prizes and trophies
        if (questionLower.Contains("prize") || questionLower.Contains("trophy") || questionLower.Contains("award") || questionLower.Contains("win"))
        {
            return "🏆 *Prizes & Awards*\n\n" +
                   "Each tournament offers:\n" +
                   "• Winner: Trophy + medals for all players\n" +
                   "• Runner-up: Medals\n" +
                   "• Fair Play Award: Certificate\n" +
                   "• Top Scorer: Individual trophy\n\n" +
                   "All participants receive certificates!";
        }
        
        // Registration process
        if (questionLower.Contains("how to register") || questionLower.Contains("sign up") || questionLower.Contains("join"))
        {
            return "📝 *How to Register*\n\n" +
                   "1. Type *register* to start\n" +
                   "2. Choose your tournament\n" +
                   "3. Provide team details\n" +
                   "4. Confirm your information\n" +
                   "5. Receive confirmation via email\n\n" +
                   "Takes less than 2 minutes! 🚀\n\n" +
                   "Ready? Type *register* now!";
        }
        
        return string.Empty;
    }

    private async Task<List<FixtureInfo>> GetUserFixturesAsync(string userId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            // Clean phone number - remove whatsapp:, +, spaces, and hyphens
            var phoneNumber = userId
                .Replace("whatsapp:", "")
                .Replace("+", "")
                .Replace(" ", "")
                .Replace("-", "")
                .Trim();
            
            _logger.LogInformation("Looking for teams with phone: {Phone} (cleaned from {UserId})", 
                phoneNumber, userId);
            
            // Get all teams
            var teamsResponse = await httpClient.GetAsync($"{apiUrl}/Teams");
            
            if (!teamsResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch teams: {StatusCode}", teamsResponse.StatusCode);
                return new List<FixtureInfo>();
            }

            var teamsJson = await teamsResponse.Content.ReadAsStringAsync();
            var allTeams = JsonSerializer.Deserialize<List<TeamInfo>>(teamsJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new List<TeamInfo>();

            _logger.LogInformation("Found {Count} total teams", allTeams.Count);

            // Find teams with this phone number (flexible matching)
            var userTeams = allTeams.Where(t => 
            {
                if (string.IsNullOrEmpty(t.CoachPhone)) return false;
                
                var cleanTeamPhone = t.CoachPhone
                    .Replace("+", "")
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Trim();
                
                return cleanTeamPhone.EndsWith(phoneNumber) || phoneNumber.EndsWith(cleanTeamPhone);
            }).ToList();
            
            _logger.LogInformation("Found {Count} teams for this phone number", userTeams.Count);
            
            if (!userTeams.Any())
            {
                return new List<FixtureInfo>();
            }

            // Get all fixtures from all events
            var eventsResponse = await httpClient.GetAsync($"{apiUrl}/Events");
            if (!eventsResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch events");
                return new List<FixtureInfo>();
            }

            var eventsJson = await eventsResponse.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<JsonElement>>(eventsJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new List<JsonElement>();

            var allFixtures = new List<FixtureInfo>();

            // Get fixtures for each event
            foreach (var evt in events)
            {
                if (evt.TryGetProperty("id", out var idProp))
                {
                    var eventId = idProp.GetInt32();
                    var fixturesResponse = await httpClient.GetAsync($"{apiUrl}/Fixtures/{eventId}");
                    
                    if (fixturesResponse.IsSuccessStatusCode)
                    {
                        var fixturesJson = await fixturesResponse.Content.ReadAsStringAsync();
                        var fixtures = JsonSerializer.Deserialize<List<FixtureInfo>>(fixturesJson, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        }) ?? new List<FixtureInfo>();

                        _logger.LogInformation("Event {EventId} has {Count} fixtures", eventId, fixtures.Count);

                        // Filter fixtures where user's teams are playing
                        foreach (var team in userTeams)
                        {
                            var teamFixtures = fixtures.Where(f => 
                                f.HomeTeam == team.Name || f.AwayTeam == team.Name).ToList();
                            
                            if (teamFixtures.Any())
                            {
                                _logger.LogInformation("Team '{TeamName}' has {Count} fixtures in event {EventId}", 
                                    team.Name, teamFixtures.Count, eventId);
                            }
                            
                            allFixtures.AddRange(teamFixtures);
                        }
                    }
                }
            }

            // Return all fixtures (remove date filter since API doesn't include matchDate)
            _logger.LogInformation("Total fixtures collected: {Count}", allFixtures.Count);
            
            var sortedFixtures = allFixtures
                .OrderBy(f => f.KickOffTime)
                .ToList();
            
            _logger.LogInformation("Returning {Count} fixtures", sortedFixtures.Count);
            
            return sortedFixtures;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user fixtures");
            return new List<FixtureInfo>();
        }
    }

    [HttpGet("webhook")]
    public IActionResult VerifyWebhook()
    {
        // Twilio doesn't require webhook verification like some platforms,
        // but this endpoint can be used for testing
        return Ok(new { status = "Webhook endpoint is active", timestamp = DateTime.UtcNow });
    }
}
