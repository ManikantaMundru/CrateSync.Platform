using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Tenancy.Application.Tenants.RenameTenant;

public sealed record RenameTenantCommand(
    Guid TenantId,
    string Name) : ICommand<Result>;
