# CrateSync.Platform - Catalog Module

The Catalog module manages product master data and product varieties within CrateSync.Platform.

## Responsibilities

The module is responsible for:

- Creating and maintaining products
- Adding and maintaining product varieties
- Activating/deactivating products and varieties
- Enforcing product and variety business rules
- Providing product read models for API/query use
- Publishing catalog integration events for other modules when required

## Architecture

The Catalog module follows the CrateSync modular monolith structure:

```text
Catalog.Domain
    Aggregates, entities, strongly typed IDs, domain events

Catalog.Application
    Vertical slices with commands, queries, handlers, validators and abstractions

Catalog.Infrastructure
    EF Core write persistence, Dapper reads, repositories and SQL Server configuration

Catalog.Contracts
    Stable cross-module contracts and integration events
```

### Main Aggregate

`Product` is the aggregate root.

`ProductVariety` is an entity owned by the Product aggregate and is not persisted through a separate repository.

Important rules include:

- Product names are unique per tenant
- Variety names are unique within a product
- Inactive products/varieties cannot be used for new operational activity
- Historical references remain valid after deactivation

## Persistence

The Catalog module uses:

- SQL Server
- EF Core for the write side
- Dapper for the read side
- `catalog` database schema
- A dedicated EF migration history table:

```text
catalog.__EFMigrationsHistory
```

Representative tables:

```text
catalog.Products
catalog.ProductVarieties
```

## EF Core Commands

Run commands from the solution root.

### Add migration

```powershell
dotnet ef migrations add InitialCatalog --project .\src\Modules\Catalog\CrateSync.Platform.Catalog.Infrastructure\CrateSync.Platform.Catalog.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.Api\CrateSync.Platform.Api.csproj --context CatalogDbContext --output-dir Persistence\Migrations
```

### Apply migrations

```powershell
dotnet ef database update --project .\src\Modules\Catalog\CrateSync.Platform.Catalog.Infrastructure\CrateSync.Platform.Catalog.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.Api\CrateSync.Platform.Api.csproj --context CatalogDbContext
```

### List migrations

```powershell
dotnet ef migrations list --project .\src\Modules\Catalog\CrateSync.Platform.Catalog.Infrastructure\CrateSync.Platform.Catalog.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.Api\CrateSync.Platform.Api.csproj --context CatalogDbContext
```

### Remove the latest migration

```powershell
dotnet ef migrations remove --project .\src\Modules\Catalog\CrateSync.Platform.Catalog.Infrastructure\CrateSync.Platform.Catalog.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.Api\CrateSync.Platform.Api.csproj --context CatalogDbContext
```
