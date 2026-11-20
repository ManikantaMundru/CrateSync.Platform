using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Tenancy.Application.Tenants.DeactivateTenant;

public sealed record DeactivateTenantCommand(Guid TenantId) : ICommand<Result>;
