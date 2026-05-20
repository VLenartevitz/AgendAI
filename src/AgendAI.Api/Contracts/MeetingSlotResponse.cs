namespace AgendAI.Api.Contracts;

public sealed record MeetingSlotResponse(
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    int DurationMinutes);

