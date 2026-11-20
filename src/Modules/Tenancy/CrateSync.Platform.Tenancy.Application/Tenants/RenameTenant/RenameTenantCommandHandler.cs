using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Application.Tenants.Common;
using CrateSync.Platform.Tenancy.Domain.Tenants;

namespace CrateSync.Platform.Tenancy.Application.Tenants.RenameTenant;

internal sealed class RenameTenantCommandHandler(
    ITenantRepository repository,
    ITenancyUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RenameTenantCommand, Result>
{
    public async Task<Result> Handle(
        RenameTenantCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.From(request.TenantId);

        var tenant = await repository.GetByIdAsync(
            tenantId,
            cancellationToken);

        if (tenant is null)
            return Result.Failure(TenantErrors.NotFound);

        if (await repository.NameExistsAsync(
                request.Name,
                tenantId,
                cancellationToken))
        {
            return Result.Failure(TenantErrors.NameExists);
        }

        tenant.Rename(
            request.Name,
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
