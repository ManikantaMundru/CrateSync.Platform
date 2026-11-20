using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Application.Tenants.Common;
using CrateSync.Platform.Tenancy.Domain.Tenants;

namespace CrateSync.Platform.Tenancy.Application.Tenants.ActivateTenant;

internal sealed class ActivateTenantCommandHandler(
    ITenantRepository repository,
    ITenancyUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<ActivateTenantCommand, Result>
{
    public async Task<Result> Handle(
        ActivateTenantCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await repository.GetByIdAsync(
            TenantId.From(request.TenantId),
            cancellationToken);

        if (tenant is null)
            return Result.Failure(TenantErrors.NotFound);

        tenant.Activate(timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
