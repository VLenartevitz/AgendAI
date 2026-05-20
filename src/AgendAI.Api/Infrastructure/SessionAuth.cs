using AgendAI.Api.Data;
using AgendAI.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgendAI.Api.Infrastructure;

public static class SessionAuth
{
    public const string SessionHeaderName = "X-AgendAI-Session";
    public const string SessionTokenClaim = "SessionToken";

    public static Guid? TryGetToken(HttpContext context)
    {
        if (context?.User?.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        var raw = context.User.FindFirst(SessionTokenClaim)?.Value;
        return Guid.TryParse(raw, out var token) ? token : null;
    }

    public static Guid? TryGetToken(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(SessionHeaderName, out var values))
        {
            return null;
        }

        var raw = values.ToString();
        return Guid.TryParse(raw, out var token) ? token : null;
    }

    public static Task<Session?> GetSessionAsync(
        AgendAIDbContext dbContext,
        HttpRequest request,
        CancellationToken cancellationToken)
        => GetSessionAsync(dbContext, request.HttpContext, cancellationToken);

    public static async Task<Session?> GetSessionAsync(
        AgendAIDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var token = TryGetToken(context) ?? TryGetToken(context.Request);
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

