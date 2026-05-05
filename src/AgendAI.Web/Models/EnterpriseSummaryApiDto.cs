namespace AgendAI.Web.Models;

public sealed record EnterpriseSummaryApiDto(
    Guid Id,
    string Name,
    string Slug,
    string Email,
    string? WhatsAppNumber,
    DateTime CreatedAtUtc,
    DateTime LastAccessAtUtc);

