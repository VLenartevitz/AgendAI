namespace AgendAI.Api.Contracts;

public sealed record EnterpriseResponse(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAtUtc);
