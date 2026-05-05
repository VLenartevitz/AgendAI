namespace AgendAI.Api.Contracts;

public sealed record CreateMeetingRequest(
    string Title,
    string ClientName,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    int DurationMinutes,
    string? ClientPhone,
    string? Notes,
    string? Status,
    string? SourceChannel,
    Guid? CreatedByUserId);
