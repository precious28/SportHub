using Microsoft.EntityFrameworkCore;
using SportHub.Application.Interfaces;
using SportHub.Domain.Entities;

namespace SportHub.Infrastructure.Data;

public class EventRepository(SportHubDbContext db) : IEventRepository
{
    public Task<Event?> GetByIdAsync(string id) => db.Events.FindAsync(id).AsTask();
    public Task<List<Event>> GetAllAsync() => db.Events.ToListAsync();
    public async Task SaveAsync(Event ev) { db.Events.Add(ev); await db.SaveChangesAsync(); }
}

public class TeamRepository(SportHubDbContext db) : ITeamRepository
{
    public Task<Team?> GetByIdAsync(string id) => db.Teams.FindAsync(id).AsTask();
    public Task<List<Team>> GetAllAsync() => db.Teams.ToListAsync();
    public async Task SaveAsync(Team team) { db.Teams.Add(team); await db.SaveChangesAsync(); }
}

public class RegistrationRepository(SportHubDbContext db) : IRegistrationRepository
{
    public Task<Registration?> GetByIdAsync(string id) => db.Registrations.FindAsync(id).AsTask();
    public Task<List<Registration>> GetByEventIdAsync(string eventId) =>
        db.Registrations.Where(r => r.EventId == eventId).ToListAsync();
    public async Task SaveAsync(Registration reg) { db.Registrations.Add(reg); await db.SaveChangesAsync(); }
}

public class FixtureRepository(SportHubDbContext db) : IFixtureRepository
{
    public Task<List<Fixture>> GetByEventIdAsync(string eventId) =>
        db.Fixtures.Where(f => f.EventId == eventId).ToListAsync();
    public async Task SaveAsync(Fixture fixture) { db.Fixtures.Add(fixture); await db.SaveChangesAsync(); }
}

public class NotificationRepository(SportHubDbContext db) : INotificationRepository
{
    public async Task SaveAsync(Notification notification) { db.Notifications.Add(notification); await db.SaveChangesAsync(); }
}
