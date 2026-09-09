namespace CrateSync.Platform.BuildingBlocks.Domain;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent(DateTimeOffset occurredOnUtc)
    {
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredOnUtc { get; }
}
