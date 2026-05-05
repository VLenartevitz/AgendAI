namespace AgendAI.Api.Contracts;

public sealed record RegisterEnterpriseRequest(
    string Name,
    string Email,
    string Password,
    string? WhatsAppNumber);

