using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/enterprises")]
public sealed class EnterprisesController(AgendAIDbContext dbContext) : ControllerBase
{
    [HttpGet("{enterpriseSlug}")]
    public async Task<ActionResult<EnterpriseResponse>> GetBySlug(
        string enterpriseSlug,
        CancellationToken cancellationToken)
    {
        var slug = NormalizeSlug(enterpriseSlug);
        if (string.IsNullOrWhiteSpace(slug))
        {
            return ValidationProblem("Enterprise invalida.");
        }

        var enterprise = await dbContext.Enterprises
            .AsNoTracking()
            .SingleOrDefaultAsync(currentEnterprise => currentEnterprise.Slug == slug, cancellationToken);

        return enterprise is null ? NotFound() : Ok(MapEnterprise(enterprise));
    }

    [HttpPost("{enterpriseSlug}/session")]
    public async Task<ActionResult<EnterpriseResponse>> EnsureSessionEnterprise(
        string enterpriseSlug,
        CancellationToken cancellationToken)
    {
        var slug = NormalizeSlug(enterpriseSlug);
        if (string.IsNullOrWhiteSpace(slug))
        {
            return ValidationProblem("Enterprise invalida.");
        }

        var enterprise = await dbContext.Enterprises
            .SingleOrDefaultAsync(currentEnterprise => currentEnterprise.Slug == slug, cancellationToken);

        if (enterprise is not null)
        {
            return Ok(MapEnterprise(enterprise));
        }

        enterprise = new Enterprise
        {
            Name = BuildNameFromSlug(slug),
            Slug = slug,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Enterprises.Add(enterprise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetBySlug), new { enterpriseSlug = enterprise.Slug }, MapEnterprise(enterprise));
    }

    private static EnterpriseResponse MapEnterprise(Enterprise enterprise)
        => new(
            enterprise.Id,
            enterprise.Name,
            enterprise.Slug,
            enterprise.CreatedAtUtc);

    private static string NormalizeSlug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var chars = normalized
            .Select(currentChar => char.IsLetterOrDigit(currentChar) ? currentChar : '-')
            .ToArray();

        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string BuildNameFromSlug(string slug)
        => string.Join(' ', slug.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
}
