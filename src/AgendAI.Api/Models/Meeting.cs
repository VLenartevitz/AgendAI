namespace AgendAI.Api.Models;

public sealed class Meeting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EnterpriseId { get; set; }
    public Enterprise? Enterprise { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string? ClientEmail { get; set; }
    public string? ClientCpf { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Confirmada";
    public string SourceChannel { get; set; } = "Dashboard";
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
