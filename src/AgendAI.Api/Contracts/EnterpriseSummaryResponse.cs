namespace AgendAI.Api.Contracts;

public sealed record EnterpriseSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    string Email,
    string? WhatsAppNumber,
    DateTime CreatedAtUtc,
    DateTime LastAccessAtUtc);

