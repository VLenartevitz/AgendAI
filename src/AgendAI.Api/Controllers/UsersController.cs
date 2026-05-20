using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Infrastructure;
using AgendAI.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    AgendAIDbContext dbContext,
    IPasswordHasher<User> passwordHasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserSummaryResponse>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem("Nome, email e senha são obrigatórios.");
        }

        var exists = await dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (exists)
        {
            return Conflict("Já existe um usuário com esse email.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            WhatsAppNumber = request.WhatsAppNumber?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            LastAccessAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new UserSummaryResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.WhatsAppNumber,
            user.CreatedAtUtc,
            user.LastAccessAtUtc));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse<UserSummaryResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem("Email e senha são obrigatórios.");
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            return Unauthorized("Credenciais inválidas.");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Credenciais inválidas.");
        }

        user.LastAccessAtUtc = DateTime.UtcNow;

        var session = new Session
        {
            PrincipalType = "User",
            UserId = user.Id,
            EnterpriseId = null,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };

        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        var principal = new UserSummaryResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.WhatsAppNumber,
            user.CreatedAtUtc,
            user.LastAccessAtUtc);

        return Ok(new LoginResponse<UserSummaryResponse>(principal, session.Token.ToString()));
    }
}
