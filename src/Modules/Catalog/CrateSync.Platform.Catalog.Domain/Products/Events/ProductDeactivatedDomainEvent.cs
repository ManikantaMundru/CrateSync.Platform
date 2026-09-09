using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Catalog.Domain.Products.Events;

public sealed record ProductDeactivatedDomainEvent(ProductId ProductId, Guid TenantId, DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
