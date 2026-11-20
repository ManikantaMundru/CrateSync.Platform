using CrateSync.Platform.BuildingBlocks.Domain;
namespace CrateSync.Platform.Sales.Domain.Orders.Events;
public sealed record SalesOrderAllocatedDomainEvent(SalesOrderId SalesOrderId, Guid TenantId, DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
