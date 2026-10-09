using SportHub.Application.Interfaces;
using SportHub.Domain.Entities;

namespace SportHub.Application.Services;

public class EventService(IEventRepository events)
{
    public Task<Event?> GetAsync(string id) => events.GetByIdAsync(id);
    public Task<List<Event>> ListAsync() => events.GetAllAsync();

    public async Task<Event> CreateAsync(string name, string description, DateTime start, DateTime end, string pitchId, int maxTeams, string ageGroupId)
    {
        var ev = new Event
        {
            Name = name,
            Description = description,
            StartDate = start,
            EndDate = end,
            PitchId = pitchId,
            MaxTeams = maxTeams,
            AgeGroupId = ageGroupId
        };
        await events.SaveAsync(ev);
        return ev;
    }

    public async Task<object> GetAvailabilityAsync(string eventId)
    {
        var ev = await events.GetByIdAsync(eventId);
        if (ev is null) return new { available = false, reason = "Event not found" };
        return new { available = true, maxTeams = ev.MaxTeams, eventId };
    }
}
