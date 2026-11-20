using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Tenancy.Application.Tenants.CreateTenant;

public sealed record CreateTenantCommand(string Name)
    : ICommand<Result<Guid>>;
