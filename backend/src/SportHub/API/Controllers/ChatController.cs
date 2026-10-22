using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SportHub.Application.DTOs;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatController(
        IConfiguration configuration,
        ILogger<ChatController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// General chat endpoint - No event context
    /// </summary>
    [HttpPost("invoke")]
    public async Task<IActionResult> InvokeAgent([FromBody] GeneralChatRequest request)
    {
        try
        {
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            
            _logger.LogInformation("General chat request: {Message}, Session: {SessionId}", 
                request.Message, sessionId);

            // General chat responses (like WhatsApp without phone number context)
            var response = await GetGeneralChatResponseAsync(request.Message);

            var result = new ChatResponse(
                sessionId,
                response,
                DateTime.UtcNow
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing general chat request");
            return StatusCode(500, new { error = "Failed to process chat request", details = ex.Message });
        }
    }

    /// <summary>
    /// Event-specific chat endpoint - Coach is viewing a specific event on dashboard
    /// Bot knows the event context and can help with registration, fixtures for that event, etc.
    /// </summary>
    [HttpPost("invoke-event")]
    public async Task<IActionResult> InvokeAgentWithEvent([FromBody] EventChatRequest request)
    {
        try
        {
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            
            _logger.LogInformation("Event chat request: {Message}, Session: {SessionId}, EventId: {EventId}", 
                request.Message, sessionId, request.EventId);

            // Get event info
            string? eventName = await GetEventContextAsync(request.EventId);

            // Event-specific chat (coach is looking at Event 4, help them with that event)
            var response = await GetEventChatResponseAsync(request.Message, request.EventId, eventName);

            var result = new ChatResponse(
                sessionId,
                response,
                DateTime.UtcNow
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event chat request");
            return StatusCode(500, new { error = "Failed to process chat request", details = ex.Message });
        }
    }

    private async Task<string?> GetEventContextAsync(int eventId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            var response = await httpClient.GetAsync($"{apiUrl}/Events/{eventId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var eventInfo = JsonSerializer.Deserialize<JsonElement>(json);
                var name = eventInfo.GetProperty("name").GetString();
                return name;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get event context for {EventId}", eventId);
        }
        return null;
    }

    private async Task<List<(int id, string name, DateTime startDate)>> GetEventsAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            var response = await httpClient.GetAsync($"{apiUrl}/Events");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<JsonElement>>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                }) ?? new List<JsonElement>();
                
                return events
                    .Select(e => (
                        id: e.GetProperty("id").GetInt32(),
                        name: e.GetProperty("name").GetString() ?? "",
                        startDate: e.GetProperty("startDate").GetDateTime()
                    ))
                    .Where(e => e.startDate >= DateTime.Today)
                    .OrderBy(e => e.startDate)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch events");
        }
        return new List<(int, string, DateTime)>();
    }

    private async Task<string> GetGeneralChatResponseAsync(string message)
    {
        var messageLower = message.ToLower();
        
        // Handle fixture queries - need to ask for team/coach name (like WhatsApp without phone)
        if (messageLower.Contains("fixture") || messageLower.Contains("match") || messageLower.Contains("schedule"))
        {
            // Try to extract team/coach name from message
            var teamName = ExtractTeamOrCoachName(message, messageLower);
            
            if (!string.IsNullOrEmpty(teamName))
            {
                return await GetFixturesResponseAsync(null, teamName);
            }
            
            return "📋 *Let me find your fixtures!*\n\n" +
                   "Please provide either:\n" +
                   "• Your **team name**, or\n" +
                   "• Your **coach name**\n\n" +
                   "Example: \"Show fixtures for Pretoria Ladies FC\" or \"Coach John Smith\"";
        }
        
        return GetSimpleResponse(message, null);
    }
    
    private async Task<string> GetEventChatResponseAsync(string message, int eventId, string? eventName)
    {
        var messageLower = message.ToLower();
        
        // Handle fixture queries for this specific event
        if (messageLower.Contains("fixture") || messageLower.Contains("match") || messageLower.Contains("schedule"))
        {
            // For event-specific chat, show all fixtures for this event
            return await GetEventFixturesAsync(eventId, eventName);
        }
        
        // Handle registration for this event
        if (messageLower.Contains("register") || messageLower.Contains("sign up") || messageLower.Contains("join"))
        {
            return $"🏆 *Register for {eventName}*\n\n" +
                   "To register your team, I'll need:\n" +
                   "• Team name\n" +
                   "• School/organization name\n" +
                   "• Age group\n" +
                   "• Coach name and contact\n\n" +
                   "Let's start - what's your team name?";
        }
        
        // Event-specific info
        if (messageLower.Contains("info") || messageLower.Contains("detail") || messageLower.Contains("about"))
        {
            return await GetEventDetailsAsync(eventId);
        }
        
        return GetSimpleResponse(message, eventName);
    }
    
    private string ExtractTeamOrCoachName(string message, string messageLower)
    {
        // Try to extract team name after "for", "of", or coach name after "coach"
        if (messageLower.Contains(" for "))
        {
            var parts = message.Split(" for ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                return parts[1].Trim();
            }
        }
        
        if (messageLower.Contains("coach "))
        {
            var parts = message.Split("coach ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 1)
            {
                return parts[1].Trim();
            }
        }
        
        return string.Empty;
    }
    
    private async Task<string> GetEventDetailsAsync(int eventId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            var response = await httpClient.GetAsync($"{apiUrl}/Events/{eventId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var evt = JsonSerializer.Deserialize<JsonElement>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                var name = evt.GetProperty("name").GetString();
                var sport = evt.GetProperty("sport").GetString();
                var startDate = evt.GetProperty("startDate").GetDateTime();
                var location = evt.TryGetProperty("location", out var loc) ? loc.GetString() : "TBD";
                var ageGroups = evt.TryGetProperty("ageGroups", out var ag) ? ag.GetString() : "";
                
                return $"🏆 *{name}*\n\n" +
                       $"Sport: {sport}\n" +
                       $"📅 Date: {startDate:MMMM dd, yyyy}\n" +
                       $"📍 Location: {location}\n" +
                       $"👥 Age Groups: {ageGroups}\n\n" +
                       "What would you like to know more about?";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event details");
        }
        
        return "I can help you with this event! Ask me about fixtures, registration, or rules.";
    }
    
    private async Task<string> GetEventFixturesAsync(int eventId, string? eventName)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            // Get fixtures for this event
            var fixturesResponse = await httpClient.GetAsync($"{apiUrl}/Fixtures/{eventId}");
            if (!fixturesResponse.IsSuccessStatusCode)
            {
                return "📋 *No fixtures generated yet*\n\n" +
                       "Fixtures will be available once the tournament reaches full capacity and fixtures are generated.";
            }

            var fixturesJson = await fixturesResponse.Content.ReadAsStringAsync();
            var fixtures = JsonSerializer.Deserialize<List<FixtureInfo>>(fixturesJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new List<FixtureInfo>();

            if (!fixtures.Any())
            {
                return "📋 *No fixtures found for this event*\n\n" +
                       "Fixtures will be generated once all teams are registered.";
            }
            
            // Format fixtures (same as WhatsApp)
            var fixtureList = string.Join("\n\n", fixtures
                .GroupBy(f => f.Id) // Remove duplicates
                .Select(g => g.First())
                .OrderBy(f => f.KickOffTime)
                .Select(f => 
                {
                    return $"{f.AgeGroup} {f.Division}\n" +
                           $"⚽ {f.HomeTeam} vs {f.AwayTeam}\n" +
                           $"⏰ Kick-off: {f.KickOffTime}\n" +
                           $"📍 Pitch {(f.PitchId == 0 ? "TBD" : f.PitchId.ToString())}";
                }));
            
            return $"📋 *{eventName ?? "Event"} Fixtures*\n\n{fixtureList}\n\n" +
                   "Good luck! 🏆";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event fixtures");
            return "❌ Unable to fetch fixtures. Please try again later.";
        }
    }

    private async Task<string> GetResponseAsync(string message, string? eventName = null, int? eventId = null, string? teamName = null)
    {
        var messageLower = message.ToLower();
        
        // Handle fixture queries
        if (messageLower.Contains("fixture") || messageLower.Contains("match") || messageLower.Contains("schedule"))
        {
            return await GetFixturesResponseAsync(eventId, teamName);
        }
        
        return GetSimpleResponse(message, eventName);
    }
    
    private async Task<string> GetFixturesResponseAsync(int? eventId = null, string? teamName = null)
    {
        // If no team name provided, ask for it
        if (string.IsNullOrEmpty(teamName))
        {
            return "📋 *Let me find your fixtures!*\n\n" +
                   "Please provide either:\n" +
                   "• Your **team name**, or\n" +
                   "• Your **coach name**\n\n" +
                   "Example: \"Show fixtures for Pretoria Ladies FC\" or \"Coach John Smith\"";
        }
        
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["API:BaseUrl"] ?? "https://sporthub-rng4.onrender.com";
            
            // Get all teams
            var teamsResponse = await httpClient.GetAsync($"{apiUrl}/Teams");
            if (!teamsResponse.IsSuccessStatusCode)
            {
                return "❌ Unable to fetch team information. Please try again later.";
            }

            var teamsJson = await teamsResponse.Content.ReadAsStringAsync();
            var allTeams = JsonSerializer.Deserialize<List<JsonElement>>(teamsJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new List<JsonElement>();

            // Search by team name OR coach name
            var matchingTeams = allTeams.Where(t => 
            {
                var teamNameMatch = false;
                var coachNameMatch = false;
                
                if (t.TryGetProperty("name", out var nameProp))
                {
                    var name = nameProp.GetString() ?? "";
                    teamNameMatch = name.Contains(teamName, StringComparison.OrdinalIgnoreCase);
                }
                
                if (t.TryGetProperty("coachName", out var coachProp))
                {
                    var coach = coachProp.GetString() ?? "";
                    coachNameMatch = coach.Contains(teamName, StringComparison.OrdinalIgnoreCase);
                }
                
                return teamNameMatch || coachNameMatch;
            }).ToList();

            if (!matchingTeams.Any())
            {
                return $"📋 *No team found matching '{teamName}'*\n\n" +
                       "Please check the spelling and try again.\n\n" +
                       "Tips:\n" +
                       "• Use partial names (e.g., \"Pretoria\" instead of full name)\n" +
                       "• Try your coach's name instead\n" +
                       "• Make sure your team is registered";
            }

            // Get team IDs and names for fixture filtering
            var teamIds = matchingTeams
                .Where(t => t.TryGetProperty("id", out _))
                .Select(t => t.GetProperty("id").GetInt32())
                .ToList();
                
            var teamNames = matchingTeams
                .Where(t => t.TryGetProperty("name", out _))
                .Select(t => t.GetProperty("name").GetString() ?? "")
                .ToList();

            // Get fixtures
            var allFixtures = new List<FixtureInfo>();
            
            if (eventId.HasValue)
            {
                // Get fixtures for specific event
                var fixturesResponse = await httpClient.GetAsync($"{apiUrl}/Fixtures/{eventId.Value}");
                if (fixturesResponse.IsSuccessStatusCode)
                {
                    var fixturesJson = await fixturesResponse.Content.ReadAsStringAsync();
                    var fixtures = JsonSerializer.Deserialize<List<FixtureInfo>>(fixturesJson, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    }) ?? new List<FixtureInfo>();
                    
                    allFixtures.AddRange(fixtures);
                }
            }
            else
            {
                // Get fixtures from all events
                var eventsResponse = await httpClient.GetAsync($"{apiUrl}/Events");
                if (eventsResponse.IsSuccessStatusCode)
                {
                    var eventsJson = await eventsResponse.Content.ReadAsStringAsync();
                    var events = JsonSerializer.Deserialize<List<JsonElement>>(eventsJson, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    }) ?? new List<JsonElement>();

                    foreach (var evt in events)
                    {
                        if (evt.TryGetProperty("id", out var idProp))
                        {
                            var evtId = idProp.GetInt32();
                            var fixturesResponse = await httpClient.GetAsync($"{apiUrl}/Fixtures/{evtId}");
                            
                            if (fixturesResponse.IsSuccessStatusCode)
                            {
                                var fixturesJson = await fixturesResponse.Content.ReadAsStringAsync();
                                var fixtures = JsonSerializer.Deserialize<List<FixtureInfo>>(fixturesJson, new JsonSerializerOptions 
                                { 
                                    PropertyNameCaseInsensitive = true 
                                }) ?? new List<FixtureInfo>();
                                
                                allFixtures.AddRange(fixtures);
                            }
                        }
                    }
                }
            }

            // Filter fixtures where any of the matching teams are playing
            var teamFixtures = allFixtures.Where(f => 
                teamNames.Any(tn => 
                    f.HomeTeam.Equals(tn, StringComparison.OrdinalIgnoreCase) || 
                    f.AwayTeam.Equals(tn, StringComparison.OrdinalIgnoreCase)
                )).ToList();

            if (!teamFixtures.Any())
            {
                var teamList = string.Join(", ", teamNames);
                return $"📋 *No fixtures found for: {teamList}*\n\n" +
                       "This could mean:\n" +
                       "• Fixtures haven't been generated yet\n" +
                       "• No matches scheduled\n" +
                       "• Team not registered for any events\n\n" +
                       "Contact the organizers if you think this is an error.";
            }

            // Fetch event names for better display
            var eventIds = teamFixtures.Select(f => f.EventId).Distinct().ToList();
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
            
            // Format fixtures (same as WhatsApp)
            var fixtureList = string.Join("\n\n", teamFixtures
                .GroupBy(f => f.Id) // Remove duplicates by fixture ID
                .Select(g => g.First())
                .OrderBy(f => f.KickOffTime)
                .Select(f => 
                {
                    var eventNameStr = eventNames.ContainsKey(f.EventId) 
                        ? eventNames[f.EventId] 
                        : $"Event {f.EventId}";
                    
                    return $"🏆 *{eventNameStr}*\n" +
                           $"{f.AgeGroup} {f.Division}\n" +
                           $"⚽ {f.HomeTeam} vs {f.AwayTeam}\n" +
                           $"⏰ Kick-off: {f.KickOffTime}\n" +
                           $"📍 Pitch {(f.PitchId == 0 ? "TBD" : f.PitchId.ToString())}";
                }));
            
            return $"📋 *Your Upcoming Fixtures*\n\n{fixtureList}\n\n" +
                   "Good luck with your matches! 🏆";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching fixtures");
            return "❌ Unable to fetch fixtures. Please try again later.";
        }
    }

    private string GetSimpleResponse(string message, string? eventName = null)
    {
        var messageLower = message.ToLower();

        if (messageLower.Contains("hello") || messageLower.Contains("hi"))
        {
            var greeting = "👋 Hello Coach! I'm your SportHub AI assistant.";
            if (!string.IsNullOrEmpty(eventName))
            {
                greeting = $"👋 Welcome to *{eventName}*! I'm your tournament assistant.";
            }
            
            return greeting + "\n\n" +
                   "I can help you with:\n" +
                   "• *Register* - Register your team\n" +
                   "• *Event info* - Tournament details\n" +
                   "• *Fixtures* - View match schedules\n" +
                   "• *Rules* - Tournament rules & FAQs\n" +
                   "• *Help* - Show this menu\n\n" +
                   "What would you like to do?";
        }
        else if (messageLower.Contains("event") || messageLower.Contains("tournament"))
        {
            return "📅 I can show you upcoming tournaments!\n\n" +
                   "You can:\n" +
                   "• View available tournaments\n" +
                   "• See tournament details\n" +
                   "• Register your team\n\n" +
                   "Would you like to see the list of upcoming events?";
        }
        else if (messageLower.Contains("register") || messageLower.Contains("sign up"))
        {
            return "🏆 Great! Let's register your team.\n\n" +
                   "I'll need:\n" +
                   "• Team name\n" +
                   "• School/organization name\n" +
                   "• Age group\n" +
                   "• Coach name and contact\n\n" +
                   "First, which tournament would you like to register for?";
        }
        else if (messageLower.Contains("fixture") || messageLower.Contains("match") || messageLower.Contains("schedule"))
        {
            return "📋 I can show you fixtures!\n\n" +
                   "You can ask:\n" +
                   "• Show me fixtures for [event name]\n" +
                   "• Show fixtures for [team name]\n" +
                   "• Show all fixtures\n\n" +
                   "What would you like to see?";
        }
        else if (messageLower.Contains("rule") || messageLower.Contains("regulation") || messageLower.Contains("faq"))
        {
            return "📖 *Tournament Rules & FAQs*\n\n" +
                   "• Match duration varies by age group\n" +
                   "• Registration fee: R500 per team\n" +
                   "• Minimum 7 days before event\n" +
                   "• Required: Team jerseys, shin guards\n" +
                   "• Fair play mandatory\n\n" +
                   "Need specific information about rules or have questions?";
        }
        else
        {
            return "I'm here to help with SportHub tournaments! 🏆\n\n" +
                   "Ask me about:\n" +
                   "• *Events* - Upcoming tournaments\n" +
                   "• *Register* - Sign up your team\n" +
                   "• *Fixtures* - View match schedules\n" +
                   "• *Rules* - Tournament regulations\n\n" +
                   "What can I help you with?";
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy",
            timestamp = DateTime.UtcNow,
            note = "Chat API active. Using pattern-based responses (Bedrock Harness integration pending)."
        });
    }
}

// General chat (no event context)
public record GeneralChatRequest(
    string Message,
    string? SessionId = null
);

// Event-specific chat (coach is viewing a specific event on dashboard)
public record EventChatRequest(
    string Message,
    int EventId,
    string? SessionId = null
);

public record ChatResponse(
    string SessionId,
    string Response,
    DateTime Timestamp
);
