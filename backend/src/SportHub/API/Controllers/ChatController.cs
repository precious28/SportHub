using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    /// Chat with AI assistant - Used by web frontend
    /// TODO: Integrate with Bedrock Agent Harness when credentials are configured
    /// </summary>
    [HttpPost("invoke")]
    public async Task<IActionResult> InvokeAgent([FromBody] ChatRequest request)
    {
        try
        {
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            
            _logger.LogInformation("Chat request received: {Message}, Session: {SessionId}, EventId: {EventId}", 
                request.Message, sessionId, request.EventId);

            // Get event info if eventId provided
            string? eventContext = null;
            if (request.EventId.HasValue)
            {
                eventContext = await GetEventContextAsync(request.EventId.Value);
            }

            // Simple keyword-based responses 
            var response = GetSimpleResponse(request.Message, eventContext);

            var result = new ChatResponse(
                sessionId,
                response,
                DateTime.UtcNow
            );

            await Task.CompletedTask; // Remove this when adding async Bedrock call

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
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
            return "📋 I can show you your team's fixtures!\n\n" +
                   "To view your match schedule, please provide:\n" +
                   "• Your team name, or\n" +
                   "• The tournament name\n\n" +
                   "What's your team name?";
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

public record ChatRequest(
    string Message,
    string? SessionId = null,
    int? EventId = null
);

public record ChatResponse(
    string SessionId,
    string Response,
    DateTime Timestamp
);
