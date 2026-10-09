namespace SportHub.Domain.Entities;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RecipientId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Email, Push, SMS
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
