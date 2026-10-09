using SportHub.Application.Interfaces;
using SportHub.Domain.Entities;

namespace SportHub.Application.Services;

public class RegistrationService(IRegistrationRepository registrations, INotificationRepository notifications)
{
    public Task<Registration?> GetAsync(string id) => registrations.GetByIdAsync(id);

    public async Task<Registration> CreateAsync(string eventId, string teamId)
    {
        var reg = new Registration { EventId = eventId, TeamId = teamId };
        await registrations.SaveAsync(reg);

        await notifications.SaveAsync(new Notification
        {
            RecipientId = teamId,
            Message = $"Registration submitted for event {eventId}",
            Type = "Push"
        });

        return reg;
    }
}
