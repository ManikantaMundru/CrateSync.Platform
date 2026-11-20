namespace CrateSync.Platform.Tenancy.Application.Tenants.SearchTenants;

public sealed record TenantSummaryResponse(
    Guid Id,
    string Name,
    string Status,
    DateTimeOffset UpdatedAtUtc);
