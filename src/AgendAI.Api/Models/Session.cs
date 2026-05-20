namespace AgendAI.Api.Models;

public sealed class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid Token { get; set; } = Guid.NewGuid();
    public string PrincipalType { get; set; } = string.Empty; // "User" | "Enterprise"
    public Guid? UserId { get; set; }
    public Guid? EnterpriseId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddDays(30);
}

