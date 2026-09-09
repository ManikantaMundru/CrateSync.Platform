using MediatR;

namespace CrateSync.Platform.BuildingBlocks.Application.Events;

public interface IIntegrationEventHandler
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : INotificationHandler<TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent;
}
