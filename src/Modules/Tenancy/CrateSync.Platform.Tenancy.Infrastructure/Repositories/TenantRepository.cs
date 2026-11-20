using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Domain.Tenants;
using CrateSync.Platform.Tenancy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Tenancy.Infrastructure.Repositories;

internal sealed class TenantRepository(
    TenancyDbContext dbContext)
    : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(
        TenantId id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        TenantId? excludingTenantId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();

        return dbContext.Tenants.AnyAsync(
            x => x.Name == normalized &&
                 (!excludingTenantId.HasValue ||
                  x.Id != excludingTenantId.Value),
            cancellationToken);
    }

    public void Add(Tenant tenant) =>
        dbContext.Tenants.Add(tenant);
}
