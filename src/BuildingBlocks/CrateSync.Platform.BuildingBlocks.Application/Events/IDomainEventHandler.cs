using CrateSync.Platform.BuildingBlocks.Domain;
using MediatR;

namespace CrateSync.Platform.BuildingBlocks.Application.Events;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent, INotification;
