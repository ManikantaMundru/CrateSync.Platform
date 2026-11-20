using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Application.Tenants.Common;
using CrateSync.Platform.Tenancy.Domain.Tenants;

namespace CrateSync.Platform.Tenancy.Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandHandler(
    ITenantRepository repository,
    ITenancyUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateTenantCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(
                request.Name,
                cancellationToken: cancellationToken))
        {
            return Result<Guid>.Failure(TenantErrors.NameExists);
        }

        var tenant = Tenant.Create(
            request.Name,
            timeProvider.GetUtcNow());

        repository.Add(tenant);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tenant.Id.Value);
    }
}
