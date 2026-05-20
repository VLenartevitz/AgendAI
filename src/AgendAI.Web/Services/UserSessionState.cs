using AgendAI.Web.Models;

namespace AgendAI.Web.Services;

public sealed class UserSessionState
{
    public SessionPrincipalDto? CurrentPrincipal { get; private set; }
    public string? SessionToken { get; private set; }

    public void SetPrincipal(SessionPrincipalDto principal, string sessionToken)
    {
        CurrentPrincipal = principal;
        SessionToken = sessionToken;
    }

    public void Clear()
    {
        CurrentPrincipal = null;
        SessionToken = null;
    }
}
