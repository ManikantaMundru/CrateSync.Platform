namespace CrateSync.Platform.BuildingBlocks.Domain;

public interface IBusinessRule
{
    string Code { get; }

    string Message { get; }

    bool IsBroken();
}
