using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Catalog.Domain.Products.Events;

public sealed record ProductVarietyAddedDomainEvent(ProductId ProductId, ProductVarietyId ProductVarietyId, string Name, DateTimeOffset OccurredAtUtc) : DomainEvent(OccurredAtUtc);
