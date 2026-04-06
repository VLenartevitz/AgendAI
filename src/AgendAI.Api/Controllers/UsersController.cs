using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    AgendAIDbContext dbContext,
    IPasswordHasher<AppUser> passwordHasher) : ControllerBase
{
    [HttpPost("session")]
    public async Task<ActionResult<UserSummaryResponse>> CreateSession(
        [FromBody] UserSessionRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem("Email e senha sao obrigatorios.");
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(currentUser => currentUser.Email == email, cancellationToken);

        if (user is null)
        {
            user = new AppUser
            {
                FullName = ResolveFullName(request.FullName, email),
                BusinessName = ResolveBusinessName(request.BusinessName, request.FullName, email),
                Email = email,
                WhatsAppNumber = request.WhatsAppNumber?.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                LastAccessAtUtc = DateTime.UtcNow
            };

            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetById), new { userId = user.Id }, MapUser(user));
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Email ou senha invalidos.");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.BusinessName))
        {
            user.BusinessName = request.BusinessName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.WhatsAppNumber))
        {
            user.WhatsAppNumber = request.WhatsAppNumber.Trim();
        }

        user.LastAccessAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapUser(user));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserSummaryResponse>> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

        return user is null ? NotFound() : Ok(MapUser(user));
    }

    private static UserSummaryResponse MapUser(AppUser user)
        => new(
            user.Id,
            user.FullName,
            user.BusinessName,
            user.Email,
            user.WhatsAppNumber,
            user.CreatedAtUtc,
            user.LastAccessAtUtc);

    private static string ResolveFullName(string? fullName, string email)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName.Trim();
        }

        var localPart = email.Split('@', StringSplitOptions.RemoveEmptyEntries)[0];
        return localPart.Replace('.', ' ').Replace('_', ' ').Trim();
    }

    private static string ResolveBusinessName(string? businessName, string? fullName, string email)
    {
        if (!string.IsNullOrWhiteSpace(businessName))
        {
            return businessName.Trim();
        }

        var ownerName = ResolveFullName(fullName, email);
        return $"Agenda de {ownerName}";
    }
}
