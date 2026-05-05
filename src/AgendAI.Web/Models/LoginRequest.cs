namespace AgendAI.Web.Models;

public sealed record LoginRequest(
    string Email,
    string Password);

