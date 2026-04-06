namespace AgendAI.Web.Models;

public sealed record UserSessionRequest(
    string FullName,
    string Email,
    string Password,
    string? BusinessName,
    string? WhatsAppNumber);
