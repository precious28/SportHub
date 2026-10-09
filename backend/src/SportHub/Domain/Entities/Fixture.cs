namespace SportHub.Domain.Entities;

public class Fixture
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventId { get; set; } = string.Empty;
    public string HomeTeamId { get; set; } = string.Empty;
    public string AwayTeamId { get; set; } = string.Empty;
    public string PitchId { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = "Scheduled";
}
