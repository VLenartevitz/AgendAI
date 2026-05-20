namespace AgendAI.Web.Models;

public sealed record LoginApiResponse<TPrincipal>(
    TPrincipal Principal,
    string SessionToken);

