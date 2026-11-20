using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Tenancy.Application.Tenants.ActivateTenant;

public sealed record ActivateTenantCommand(Guid TenantId) : ICommand<Result>;
