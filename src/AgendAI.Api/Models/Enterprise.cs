namespace AgendAI.Api.Models;

public sealed class Enterprise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<Meeting> Meetings { get; set; } = [];
}
