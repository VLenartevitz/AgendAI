namespace AgendAI.Api.Contracts;

public sealed record LoginRequest(
    string Email,
    string Password);

