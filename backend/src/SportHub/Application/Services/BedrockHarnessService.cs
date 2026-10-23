using Amazon.Runtime;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SportHub.Application.Services;

public class BedrockHarnessService
{
    private readonly IAmazonBedrockAgentRuntime _bedrockClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BedrockHarnessService> _logger;

    public BedrockHarnessService(
        IAmazonBedrockAgentRuntime bedrockClient,
        IConfiguration configuration,
        ILogger<BedrockHarnessService> logger)
    {
        _bedrockClient = bedrockClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> InvokeAsync(string userMessage, string sessionId)
    {
        try
        {
            // NOTE: AgentCore Harness has a different ID format than classic agents
            // The Harness ID is: amusema_sports_agent-Azjer58UxL (too long for agentId parameter)
            // We need to extract just the suffix part: Azjer58UxL
            
            var fullHarnessId = "amusema_sports_agent-Azjer58UxL";
            var agentId = fullHarnessId.Split('-').Last(); // Get "Azjer58UxL"
            var agentAliasId = "TSTALIASID"; // Default test alias
            
            _logger.LogInformation("Invoking Bedrock Agent: {AgentId} (from Harness {HarnessId}), Session: {SessionId}", 
                agentId, fullHarnessId, sessionId);

            var request = new InvokeAgentRequest
            {
                AgentId = agentId,
                AgentAliasId = agentAliasId,
                SessionId = sessionId,
                InputText = userMessage
            };

            var response = await _bedrockClient.InvokeAgentAsync(request);

            // Process streaming response - collect all chunks
            var fullResponse = new StringBuilder();
            
            await foreach (var eventItem in response.Completion)
            {
                // Try to get text from the event
                var eventType = eventItem.GetType().Name;
                _logger.LogDebug("Received event type: {EventType}", eventType);
                
                // Handle PayloadPart which contains the actual response text
                if (eventItem is PayloadPart payloadPart && payloadPart.Bytes != null)
                {
                    var text = Encoding.UTF8.GetString(payloadPart.Bytes.ToArray());
                    fullResponse.Append(text);
                }
            }

            var result = fullResponse.ToString();
            
            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogWarning("Empty response from Bedrock Agent");
                return "I'm here to help! Ask me about events, fixtures, or team registration.";
            }

            _logger.LogInformation("Bedrock response: {Length} chars", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking Bedrock Agent");
            return "I'm having trouble right now. Try: 'events', 'fixtures', or 'register'.";
        }
    }
}
