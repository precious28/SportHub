namespace SportHub.Domain.Entities;

public class AgeGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty; // e.g. "U10", "U13"
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
}
