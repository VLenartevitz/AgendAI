namespace AgendAI.Api.Contracts;

public sealed record LoginResponse<TPrincipal>(
    TPrincipal Principal,
    string SessionToken);

