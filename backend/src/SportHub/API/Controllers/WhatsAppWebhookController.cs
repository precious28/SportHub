using Microsoft.AspNetCore.Mvc;
using SportHub.Application.Services;
using SportHub.Application.DTOs;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly TwilioWhatsAppService _whatsappService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppWebhookController> _logger;
    private readonly BedrockHarnessService _bedrockService;

    public WhatsAppWebhookController(
        TwilioWhatsAppService whatsappService,
        IConfiguration configuration,
        ILogger<WhatsAppWebhookController> logger,
        BedrockHarnessService bedrockService)
    {
        _whatsappService = whatsappService;
        _configuration = configuration;
        _logger = logger;
        _bedrockService = bedrockService;
    }

    [HttpPost("webhook")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ReceiveMessage([FromForm] TwilioWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Received WhatsApp message from {From}: {Body}", 
                request.From, request.Body);

            var userMessage = request.Body;

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", "application/xml");
            }

            // Call Bedrock Harness for intelligent response
            var agentResponse = await InvokeBedrockHarnessAsync(userMessage, request.From);

            // Return TwiML response
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

    private async Task<string> InvokeBedrockHarnessAsync(string userMessage, string userId)
    {
        try
        {
            // Clean userId to use as session ID
            var sessionId = userId
                .Replace("whatsapp:", "")
                .Replace("+", "")
                .Replace(" ", "")
                .Replace("-", "");

            _logger.LogInformation("Calling Bedrock Harness - User: {UserId}, Session: {SessionId}, Message: {Message}", 
                userId, sessionId, userMessage);

            // Call Bedrock AgentCore Harness
            var response = await _bedrockService.InvokeAsync(userMessage, sessionId);

            _logger.LogInformation("Bedrock response received for session {SessionId}", sessionId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Bedrock Harness");
            return "I'm having trouble right now. Please try again in a moment.";
        }
    }

    [HttpGet("webhook")]
    public IActionResult VerifyWebhook()
    {
        return Ok(new { status = "Webhook endpoint is active", timestamp = DateTime.UtcNow });
    }
}
