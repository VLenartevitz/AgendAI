namespace AgendAI.Api.Models;

public sealed class Enterprise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string? WhatsAppNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessAtUtc { get; set; } = DateTime.UtcNow;

    public List<Meeting> Meetings { get; set; } = [];
}
