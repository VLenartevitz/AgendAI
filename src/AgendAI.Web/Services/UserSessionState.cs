using AgendAI.Web.Models;

namespace AgendAI.Web.Services;

public sealed class UserSessionState
{
    public SessionPrincipalDto? CurrentPrincipal { get; private set; }

    public void SetPrincipal(SessionPrincipalDto principal) => CurrentPrincipal = principal;

    public void Clear()
    {
        CurrentPrincipal = null;
    }
}
