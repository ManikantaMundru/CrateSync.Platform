using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Catalog.Domain.Products.Events;

public sealed record ProductCreatedDomainEvent(ProductId ProductId, Guid TenantId, string Name, DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
