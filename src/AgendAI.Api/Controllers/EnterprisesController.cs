using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Infrastructure;
using AgendAI.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/enterprises")]
public sealed class EnterprisesController(
    AgendAIDbContext dbContext,
    IPasswordHasher<Enterprise> passwordHasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<EnterpriseSummaryResponse>> Register(
        [FromBody] RegisterEnterpriseRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var name = request.Name.Trim();
        var slug = NormalizeSlug(name);

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(slug) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem("Nome, email e senha são obrigatórios.");
        }

        var emailExists = await dbContext.Enterprises
            .AnyAsync(e => e.Email == email, cancellationToken);

        if (emailExists)
        {
            return Conflict("Já existe uma empresa com esse email.");
        }

        var slugExists = await dbContext.Enterprises
            .AnyAsync(e => e.Slug == slug, cancellationToken);

        if (slugExists)
        {
            return Conflict("Já existe uma empresa com esse nome.");
        }

        var enterprise = new Enterprise
        {
            Name = name,
            Slug = slug,
            Email = email,
            WhatsAppNumber = request.WhatsAppNumber?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            LastAccessAtUtc = DateTime.UtcNow
        };

        enterprise.PasswordHash = passwordHasher.HashPassword(enterprise, request.Password);

        dbContext.Enterprises.Add(enterprise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new EnterpriseSummaryResponse(
            enterprise.Id,
            enterprise.Name,
            enterprise.Slug,
            enterprise.Email ?? string.Empty,
            enterprise.WhatsAppNumber,
            enterprise.CreatedAtUtc,
            enterprise.LastAccessAtUtc));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse<EnterpriseSummaryResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem("Email e senha são obrigatórios.");
        }

        var enterprise = await dbContext.Enterprises
            .SingleOrDefaultAsync(e => e.Email == email, cancellationToken);

        if (enterprise is null)
        {
            return Unauthorized("Credenciais inválidas.");
        }

        var result = passwordHasher.VerifyHashedPassword(enterprise, enterprise.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Credenciais inválidas.");
        }

        enterprise.LastAccessAtUtc = DateTime.UtcNow;

        var session = new Session
        {
            PrincipalType = "Enterprise",
            EnterpriseId = enterprise.Id,
            UserId = null,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };

        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        var principal = new EnterpriseSummaryResponse(
            enterprise.Id,
            enterprise.Name,
            enterprise.Slug,
            enterprise.Email ?? string.Empty,
            enterprise.WhatsAppNumber,
            enterprise.CreatedAtUtc,
            enterprise.LastAccessAtUtc);

        return Ok(new LoginResponse<EnterpriseSummaryResponse>(principal, session.Token.ToString()));
    }

    private static string NormalizeSlug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var chars = normalized
            .Select(currentChar => char.IsLetterOrDigit(currentChar) ? currentChar : '-')
            .ToArray();

        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
