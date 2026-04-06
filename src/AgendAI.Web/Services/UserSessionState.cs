using AgendAI.Web.Models;

namespace AgendAI.Web.Services;

public sealed class UserSessionState
{
    public UserSummaryDto? CurrentUser { get; private set; }

    public void SetUser(UserSummaryDto user)
    {
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }
}
