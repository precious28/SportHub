using ModelContextProtocol.Server;
using SportHub.Application.Services;
using System.ComponentModel;

namespace SportHub.MCP.Tools;

[McpServerToolType]
public class RegistrationTools(RegistrationService registrationService)
{
    [McpServerTool, Description("Register a team for an event")]
    public Task<object> CreateRegistration(string eventId, string teamId)
        => registrationService.CreateAsync(eventId, teamId)
                              .ContinueWith(t => (object)t.Result);

    [McpServerTool, Description("Get a registration by ID")]
    public async Task<object> GetRegistration(string id)
    {
        var reg = await registrationService.GetAsync(id);
        return reg is null ? new { error = "Not found" } : (object)reg;
    }
}
