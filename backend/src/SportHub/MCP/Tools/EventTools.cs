using ModelContextProtocol.Server;
using SportHub.Application.Services;
using System.ComponentModel;

namespace SportHub.MCP.Tools;

[McpServerToolType]
public class EventTools(EventService eventService)
{
    [McpServerTool, Description("Create a new sport event")]
    public Task<object> CreateEvent(
        string name, string description,
        DateTime startDate, DateTime endDate,
        string pitchId, int maxTeams, string ageGroupId)
        => eventService.CreateAsync(name, description, startDate, endDate, pitchId, maxTeams, ageGroupId)
                       .ContinueWith(t => (object)t.Result);

    [McpServerTool, Description("Get a sport event by ID")]
    public async Task<object> GetEvent(string id)
    {
        var ev = await eventService.GetAsync(id);
        return ev is null ? new { error = "Not found" } : (object)ev;
    }

    [McpServerTool, Description("Get availability (slot count) for an event")]
    public Task<object> GetEventAvailability(string eventId)
        => eventService.GetAvailabilityAsync(eventId);
}
