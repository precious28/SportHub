using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace SportHub.Application.Services;

public class BedrockHarnessService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BedrockHarnessService> _logger;

    public BedrockHarnessService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<BedrockHarnessService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> InvokeAsync(string userMessage, string sessionId)
    {
        try
        {
            // Call Bedrock Harness service (Lambda or Render)
            var pythonServiceUrl = _configuration["Bedrock:PythonServiceUrl"] ?? "http://localhost:5001";
            
            var httpClient = _httpClientFactory.CreateClient();
            
            var requestBody = new
            {
                message = userMessage,
                sessionId = sessionId
            };
            
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );
            
            _logger.LogInformation("Calling Bedrock Harness service - Session: {SessionId}, Message: {Message}", 
                sessionId, userMessage.Substring(0, Math.Min(50, userMessage.Length)));
            
            // Lambda URLs already include the full path, Render needs /invoke added
            var endpoint = pythonServiceUrl.Contains("amazonaws.com") 
                ? pythonServiceUrl  // Lambda - use as is
                : $"{pythonServiceUrl}/invoke";  // Render - add /invoke
            
            var response = await httpClient.PostAsync(endpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bedrock service returned {StatusCode}", response.StatusCode);
                return "I'm here to help! What would you like to know?";
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            
            // Use case-insensitive deserialization to match Python's lowercase property names
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            var result = JsonSerializer.Deserialize<BedrockResponse>(responseJson, options);
            
            _logger.LogInformation("Received response from Bedrock Harness: {ResponseLength} chars", 
                result?.Response?.Length ?? 0);
            
            return result?.Response ?? "I'm here to help! What would you like to know?";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Bedrock Harness service");
            return "I'm here to help! Try asking about events, fixtures, or registration.";
        }
    }
    
    private class BedrockResponse
    {
        public string Response { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
}
