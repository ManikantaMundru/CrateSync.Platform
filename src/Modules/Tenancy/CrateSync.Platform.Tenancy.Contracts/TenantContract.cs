namespace CrateSync.Platform.Tenancy.Contracts;

public sealed record TenantContract(
    Guid TenantId,
    string Name,
    string Status);
