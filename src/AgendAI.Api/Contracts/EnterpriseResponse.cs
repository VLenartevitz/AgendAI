namespace AgendAI.Api.Contracts;

public sealed record EnterpriseResponse(
    Guid Id,
    string Name,
    string? Email,
    string? WhatsAppNumber,
    DateTime CreatedAtUtc);
