namespace CrateSync.Platform.BuildingBlocks.Domain;

public sealed class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(
            brokenRule.Code,
            brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }

    public IBusinessRule BrokenRule { get; }
}
