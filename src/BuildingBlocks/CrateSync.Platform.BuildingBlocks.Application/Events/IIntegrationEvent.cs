using MediatR;

namespace CrateSync.Platform.BuildingBlocks.Application.Events;

public interface IIntegrationEvent: INotification
{
    Guid Id { get; }

    DateTimeOffset OccurredAtUtc { get; }
}
