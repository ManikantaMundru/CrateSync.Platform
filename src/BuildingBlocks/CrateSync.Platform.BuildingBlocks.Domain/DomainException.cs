namespace CrateSync.Platform.BuildingBlocks.Domain;

public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
