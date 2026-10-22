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
            return "👋 Hello! I'm your SportHub AI assistant. I can help you with tournaments, registrations, and fixtures. What would you like to know?";
        }
        else if (messageLower.Contains("event") || messageLower.Contains("tournament"))
        {
            return "I can help you with tournaments! You can:\n• View upcoming events\n• Create new tournaments\n• Register teams\n\nWhat would you like to do?";
        }
        else if (messageLower.Contains("register") || messageLower.Contains("sign up"))
        {
            return "Great! To register your team, I'll need:\n• Team name\n• School/organization\n• Age group\n• Coach details\n\nWhich tournament are you interested in?";
        }
        else if (messageLower.Contains("fixture"))
        {
            return "I can help you generate or view fixtures. Would you like to:\n• Generate fixtures for an event\n• View existing fixtures\n• Check match schedules?";
        }
        else
        {
            return "I'm here to help with SportHub tournaments! You can ask me about:\n• Upcoming events\n• Team registration\n• Fixtures and schedules\n• Tournament rules\n\nWhat would you like to know?";
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
