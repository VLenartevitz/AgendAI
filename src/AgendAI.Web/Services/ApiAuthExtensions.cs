using System.Net.Http.Headers;

namespace AgendAI.Web.Services;

public static class ApiAuthExtensions
{
    public static void AttachSession(this HttpClient client, UserSessionState sessionState)
    {
        client.DefaultRequestHeaders.Remove("X-AgendAI-Session");

        if (!string.IsNullOrWhiteSpace(sessionState.SessionToken))
        {
            client.DefaultRequestHeaders.Add("X-AgendAI-Session", sessionState.SessionToken);
        }
    }
}

