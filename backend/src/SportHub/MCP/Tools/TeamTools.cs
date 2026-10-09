using ModelContextProtocol.Server;
using SportHub.Application.Services;
using System.ComponentModel;

namespace SportHub.MCP.Tools;

[McpServerToolType]
public class TeamTools(TeamService teamService)
{
    [McpServerTool, Description("Create a new team")]
    public Task<object> CreateTeam(string name, string coachId, string ageGroupId)
        => teamService.CreateAsync(name, coachId, ageGroupId)
                      .ContinueWith(t => (object)t.Result);

    [McpServerTool, Description("Get all teams")]
    public Task<object> ListTeams()
        => teamService.ListAsync().ContinueWith(t => (object)t.Result);

    [McpServerTool, Description("Get a team by ID")]
    public async Task<object> GetTeam(string id)
    {
        var team = await teamService.GetAsync(id);
        return team is null ? new { error = "Not found" } : (object)team;
    }
}
