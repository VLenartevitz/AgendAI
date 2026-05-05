namespace AgendAI.Web.Models;

public sealed record UserSummaryApiDto(
    Guid Id,
    string FullName,
    string Email,
    string? WhatsAppNumber,
    DateTime CreatedAtUtc,
    DateTime LastAccessAtUtc);

