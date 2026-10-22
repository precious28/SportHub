using Microsoft.AspNetCore.Mvc;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using System.Text;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IAmazonBedrockAgentRuntime _bedrockRuntime;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IAmazonBedrockAgentRuntime bedrockRuntime,
        IConfiguration configuration,
        ILogger<ChatController> logger)
    {
        _bedrockRuntime = bedrockRuntime;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Chat with the Bedrock Agent - Used by web frontend
    /// </summary>
    [HttpPost("invoke")]
    public async Task<IActionResult> InvokeAgent([FromBody] ChatRequest request)
    {
        try
        {
            var agentId = _configuration["AWS:BedrockAgentId"];
            var agentAliasId = _configuration["AWS:BedrockAgentAliasId"];

            if (string.IsNullOrEmpty(agentId) || string.IsNullOrEmpty(agentAliasId))
            {
                return BadRequest(new { error = "Bedrock Agent not configured" });
            }

            _logger.LogInformation("Invoking Bedrock Agent for session: {SessionId}", request.SessionId);

            var invokeRequest = new InvokeAgentRequest
            {
                AgentId = agentId,
                AgentAliasId = agentAliasId,
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                InputText = request.Message
            };

            var response = await _bedrockRuntime.InvokeAgentAsync(invokeRequest);

            // Read the response stream
            var responseText = new StringBuilder();
            var completion = response.Completion;

            await foreach (var item in completion)
            {
                if (item is PayloadPart payloadPart)
                {
                    var text = Encoding.UTF8.GetString(payloadPart.Bytes.ToArray());
                    responseText.Append(text);
                }
            }

            var result = new ChatResponse
            {
                SessionId = invokeRequest.SessionId,
                Response = responseText.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Bedrock response: {Response}", result.Response);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking Bedrock Agent");
            return StatusCode(500, new { error = "Failed to process chat request", details = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        var agentId = _configuration["AWS:BedrockAgentId"];
        var agentAliasId = _configuration["AWS:BedrockAgentAliasId"];
        
        return Ok(new 
        { 
            status = "healthy",
            bedrockConfigured = !string.IsNullOrEmpty(agentId) && !string.IsNullOrEmpty(agentAliasId),
            timestamp = DateTime.UtcNow
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
