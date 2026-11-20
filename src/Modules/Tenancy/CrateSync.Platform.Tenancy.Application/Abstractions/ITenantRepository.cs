using CrateSync.Platform.Tenancy.Domain.Tenants;

namespace CrateSync.Platform.Tenancy.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(
        TenantId id,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        TenantId? excludingTenantId = null,
        CancellationToken cancellationToken = default);

    void Add(Tenant tenant);
}
