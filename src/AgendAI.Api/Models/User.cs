namespace AgendAI.Api.Models;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
  
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessAtUtc { get; set; } = DateTime.UtcNow;
}
