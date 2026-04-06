namespace AgendAI.Api.Contracts;

public sealed record UserSessionRequest(
    string FullName,
    string Email,
    string Password,
    string? BusinessName,
    string? WhatsAppNumber);
