namespace AgendAI.Web.Models;

public sealed record EnterpriseDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc);
