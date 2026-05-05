namespace AgendAI.Web.Models;

public sealed record EnterpriseDto(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAtUtc);
