namespace AgendAI.Web.Models;

public sealed record UserSummaryDto(
    Guid Id,
    string FullName,
    string BusinessName,
    string Email,
    string? WhatsAppNumber,
    DateTime CreatedAtUtc,
    DateTime LastAccessAtUtc);
