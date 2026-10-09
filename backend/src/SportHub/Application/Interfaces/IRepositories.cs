using SportHub.Domain.Entities;

namespace SportHub.Application.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(string id);
    Task<List<Event>> GetAllAsync();
    Task SaveAsync(Event ev);
}

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(string id);
    Task<List<Team>> GetAllAsync();
    Task SaveAsync(Team team);
}

public interface IRegistrationRepository
{
    Task<Registration?> GetByIdAsync(string id);
    Task<List<Registration>> GetByEventIdAsync(string eventId);
    Task SaveAsync(Registration registration);
}

public interface IFixtureRepository
{
    Task<List<Fixture>> GetByEventIdAsync(string eventId);
    Task SaveAsync(Fixture fixture);
}

public interface INotificationRepository
{
    Task SaveAsync(Notification notification);
}
