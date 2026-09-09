using MediatR;

namespace CrateSync.Platform.BuildingBlocks.Domain;

public interface IDomainEvent: INotification
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }
}
