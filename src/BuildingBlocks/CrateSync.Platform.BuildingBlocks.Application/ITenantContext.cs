namespace CrateSync.Platform.BuildingBlocks.Application;

public interface ITenantContext
{
    Guid TenantId { get; }

    bool HasTenant { get; }
}
