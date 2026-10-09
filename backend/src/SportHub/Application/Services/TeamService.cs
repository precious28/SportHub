using SportHub.Application.Interfaces;
using SportHub.Domain.Entities;

namespace SportHub.Application.Services;

public class TeamService(ITeamRepository teams)
{
    public Task<Team?> GetAsync(string id) => teams.GetByIdAsync(id);
    public Task<List<Team>> ListAsync() => teams.GetAllAsync();

    public async Task<Team> CreateAsync(string name, string coachId, string ageGroupId)
    {
        var team = new Team { Name = name, CoachId = coachId, AgeGroupId = ageGroupId };
        await teams.SaveAsync(team);
        return team;
    }
}
