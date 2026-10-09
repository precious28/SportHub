using SportHub.Application.Interfaces;
using SportHub.Domain.Entities;

namespace SportHub.Application.Services;

public class FixtureService(IFixtureRepository fixtures, IRegistrationRepository registrations)
{
    public Task<List<Fixture>> GetByEventAsync(string eventId) => fixtures.GetByEventIdAsync(eventId);

    public async Task<List<Fixture>> GenerateAsync(string eventId, string pitchId, DateTime startDate)
    {
        var regs = await registrations.GetByEventIdAsync(eventId);
        var teamIds = regs.Where(r => r.Status == "Confirmed").Select(r => r.TeamId).ToList();

        var generated = new List<Fixture>();
        var slot = startDate;

        for (int i = 0; i < teamIds.Count; i++)
        for (int j = i + 1; j < teamIds.Count; j++)
        {
            var fixture = new Fixture
            {
                EventId = eventId,
                HomeTeamId = teamIds[i],
                AwayTeamId = teamIds[j],
                PitchId = pitchId,
                ScheduledAt = slot
            };
            await fixtures.SaveAsync(fixture);
            generated.Add(fixture);
            slot = slot.AddHours(1.5);
        }

        return generated;
    }
}
