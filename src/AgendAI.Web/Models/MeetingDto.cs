namespace AgendAI.Web.Models;

public sealed record MeetingDto(
    Guid Id,
    Guid EnterpriseId,
    Guid? CreatedByUserId,
    string Title,
    string ClientName,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    int DurationMinutes,
    string Status,
    string SourceChannel,
    string? ClientPhone,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
