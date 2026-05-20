using AgendAI.Api.Data;
using AgendAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Infrastructure;

public static class SessionAuth
{
    public const string SessionHeaderName = "X-AgendAI-Session";

    public static Guid? TryGetToken(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(SessionHeaderName, out var values))
        {
            return null;
        }

        var raw = values.ToString();
        return Guid.TryParse(raw, out var token) ? token : null;
    }

    public static async Task<Session?> GetSessionAsync(
        AgendAIDbContext dbContext,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var token = TryGetToken(request);
        if (token is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        return await dbContext.Sessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                session => session.Token == token.Value && session.ExpiresAtUtc > now,
                cancellationToken);
    }
}

