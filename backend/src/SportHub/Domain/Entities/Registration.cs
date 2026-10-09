namespace SportHub.Domain.Entities;

public class Registration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Rejected
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
