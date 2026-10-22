using Microsoft.EntityFrameworkCore;
using SportHub.Application.Interfaces;
using SportHub.Domain.Entities;

namespace SportHub.Infrastructure.Data;

public class EventRepository(SportHubDbContext db) : IEventRepository
{
    public Task<Event?> GetByIdAsync(int id) => db.Events.FindAsync(id).AsTask();
    public Task<List<Event>> GetAllAsync() => db.Events.ToListAsync();
    public async Task SaveAsync(Event ev) { db.Events.Add(ev); await db.SaveChangesAsync(); }
    public async Task<bool> UpdateAsync(int id, Event ev)
    {
        var existing = await db.Events.FindAsync(id);
        if (existing is null) return false;
        db.Entry(existing).CurrentValues.SetValues(ev);
        await db.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await db.Events.FindAsync(id);
        if (existing is null) return false;
        db.Events.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}

public class TeamRepository(SportHubDbContext db) : ITeamRepository
{
    public Task<Team?> GetByIdAsync(int id) => db.Teams.FindAsync(id).AsTask();
    public Task<List<Team>> GetAllAsync() => db.Teams.ToListAsync();
    public async Task SaveAsync(Team team) { db.Teams.Add(team); await db.SaveChangesAsync(); }
}

public class RegistrationRepository(SportHubDbContext db) : IRegistrationRepository
{
    public Task<Registration?> GetByIdAsync(int id) => db.Registrations.FindAsync(id).AsTask();
    public Task<List<Registration>> GetByEventIdAsync(int eventId) =>
        db.Registrations.Where(r => r.EventId == eventId).ToListAsync();
    public async Task SaveAsync(Registration reg)
    {
        if (reg.Id == 0)
        {
            db.Registrations.Add(reg);
        }
        else
        {
            var existing = await db.Registrations.FindAsync(reg.Id);
            if (existing is null)
                db.Registrations.Add(reg);
            else
                db.Entry(existing).CurrentValues.SetValues(reg);
        }
        await db.SaveChangesAsync();
    }
}

public class FixtureRepository(SportHubDbContext db) : IFixtureRepository
{
    public Task<List<Fixture>> GetByEventIdAsync(int eventId) =>
        db.Fixtures.Where(f => f.EventId == eventId).ToListAsync();
    public async Task SaveAsync(Fixture fixture) 
    { 
        // DEBUG LOG
        Console.WriteLine($"[DEBUG] FixtureRepository.SaveAsync - BEFORE Save: PitchId={fixture.PitchId}, HomeTeam={fixture.HomeTeamId}, AwayTeam={fixture.AwayTeamId}");
        
        db.Fixtures.Add(fixture); 
        await db.SaveChangesAsync(); 
        
        // DEBUG LOG
        Console.WriteLine($"[DEBUG] FixtureRepository.SaveAsync - AFTER Save: PitchId={fixture.PitchId}, Id={fixture.Id}");
    }
}

public class NotificationRepository(SportHubDbContext db) : INotificationRepository
{
    public async Task SaveAsync(Notification notification) { db.Notifications.Add(notification); await db.SaveChangesAsync(); }
}
