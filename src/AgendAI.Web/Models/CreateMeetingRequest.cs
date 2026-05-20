namespace AgendAI.Web.Models;

public sealed record CreateMeetingRequest(
    string Title,
    string ClientName,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    int DurationMinutes,
    string? ClientPhone,
    string? ClientEmail,
    string? ClientCpf,
    string? Notes,
    string? Status,
    string? SourceChannel,
    Guid? CreatedByUserId);
