using ModelContextProtocol.Server;
using SportHub.Application.Services;
using System.ComponentModel;

namespace SportHub.MCP.Tools;

[McpServerToolType]
public class FixtureTools(FixtureService fixtureService)
{
    [McpServerTool, Description("Generate round-robin fixtures for an event")]
    public Task<object> GenerateFixtures(string eventId, string pitchId, DateTime startDate)
        => fixtureService.GenerateAsync(eventId, pitchId, startDate)
                         .ContinueWith(t => (object)t.Result);

    [McpServerTool, Description("Get all fixtures for an event")]
    public Task<object> GetFixtures(string eventId)
        => fixtureService.GetByEventAsync(eventId)
                         .ContinueWith(t => (object)t.Result);
}
