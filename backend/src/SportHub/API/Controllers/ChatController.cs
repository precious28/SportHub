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
            
            _logger.LogInformation("Chat request received: {Message}, Session: {SessionId}", 
                request.Message, sessionId);

            // Simple keyword-based responses 
            // TODO: Replace with actual Bedrock Harness invocation
            var response = GetSimpleResponse(request.Message);

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

    private string GetSimpleResponse(string message)
    {
        var messageLower = message.ToLower();

        if (messageLower.Contains("hello") || messageLower.Contains("hi"))
        {
            return "👋 Hello Coach! I'm your SportHub AI assistant.\n\n" +
                   "I can help you with:\n" +
                   "• *Register* - Register your team for a tournament\n" +
                   "• *Fixtures* - View your team's match schedule\n" +
                   "• *Events* - Browse upcoming tournaments\n" +
                   "• *Rules* - Tournament rules and FAQs\n\n" +
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
    string? SessionId = null
);

public record ChatResponse(
    string SessionId,
    string Response,
    DateTime Timestamp
);
