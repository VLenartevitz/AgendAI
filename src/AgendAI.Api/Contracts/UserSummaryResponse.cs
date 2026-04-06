namespace AgendAI.Api.Contracts;

public sealed record UserSummaryResponse(
    Guid Id,
    string FullName,
    string BusinessName,
    string Email,
    string? WhatsAppNumber,
    DateTime CreatedAtUtc,
    DateTime LastAccessAtUtc);
