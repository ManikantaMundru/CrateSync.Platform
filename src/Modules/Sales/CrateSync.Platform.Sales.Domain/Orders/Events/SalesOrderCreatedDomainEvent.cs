using CrateSync.Platform.BuildingBlocks.Domain;
namespace CrateSync.Platform.Sales.Domain.Orders.Events;
public sealed record SalesOrderCreatedDomainEvent(SalesOrderId SalesOrderId, Guid TenantId, Guid CustomerId, string OrderNumber, DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
