namespace CrateSync.Platform.Tenancy.Application.Tenants.GetTenant;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
