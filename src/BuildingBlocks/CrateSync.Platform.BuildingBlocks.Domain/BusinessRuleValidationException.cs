namespace CrateSync.Platform.BuildingBlocks.Domain;

public sealed class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string message)
        : base(message)
    {
    }
}
