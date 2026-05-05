namespace AgendAI.Web.Models;

public sealed record SessionPrincipalDto(
    Guid Id,
    string AccountType,
    string DisplayName,
    string Email);

