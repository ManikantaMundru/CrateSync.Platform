# CrateSync Platform backend

CrateSync is a .NET 10 modular monolith for managing product catalogues, trading partners, warehouses, inbound shipments, stock, sales orders, and invoices. The operational API runs as a single ASP.NET Core host. Its modules write through EF Core, use Dapper for SQL read models, and exchange integration events in-process through MediatR.

- [Run the API locally](#run-the-api-locally)
- [Solution structure](#solution-structure)
- [Database structure](#database-structure)
- [EF Core commands](#ef-core-commands)
- [Authentication and local tenant context](#authentication-and-local-tenant-context)
- [Endpoint reference](#endpoint-reference)
- [Try the endpoints](#try-the-endpoints)
- [Tests](#tests)

## Run the API locally

Use the .NET 10 SDK, an accessible SQL Server instance, and PowerShell from the repository root. For Visual Studio, open `CrateSync.Platform.slnx`, set `CrateSync.Platform.Api` as the startup project, and use its `https` launch profile.

```powershell
dotnet --version
dotnet restore CrateSync.Platform.slnx
dotnet build CrateSync.Platform.slnx --no-restore
dotnet dev-certs https --trust
```

The Development connection string is `ConnectionStrings:CrateSync` in [appsettings.Development.json](src/Host/CrateSync.Platform.Api/appsettings.Development.json). It defaults to a local SQL Server instance, the `CrateSync` database, and Windows integrated authentication. Override it in your shell if you use a named instance, a container, or SQL authentication.

```powershell
$env:ConnectionStrings__CrateSync = 'Server=localhost;Database=CrateSync;Trusted_Connection=True;TrustServerCertificate=True;'
```

Set the value to match your local instance. Supply SQL credentials through your environment or secret store and keep them out of the repository. The account running the initial migrations needs permission to create the database or alter its schemas.

Apply all eight operational contexts using the [EF Core commands](#ef-core-commands). The API does not apply migrations on startup. Then start the host in the same shell.

```powershell
dotnet run --project src/Host/CrateSync.Platform.Api --launch-profile https
```

| URL | Use |
| --- | --- |
| [https://localhost:7038](https://localhost:7038) | API base URL for the HTTPS profile |
| [https://localhost:7038/health](https://localhost:7038/health) | Process health check. It does not test SQL Server connectivity. |
| [https://localhost:7038/swagger/index.html](https://localhost:7038/swagger/index.html) | Interactive Swagger UI |
| [https://localhost:7038/swagger/v1/swagger.json](https://localhost:7038/swagger/v1/swagger.json) | OpenAPI document for Postman or other clients |
| [http://localhost:5255](http://localhost:5255) | HTTP profile address. The HTTPS profile also binds it and redirects HTTP requests to HTTPS. |

## Solution structure

| Location | Responsibility |
| --- | --- |
| `src/Host/CrateSync.Platform.Api` | Operational host, controllers, request and response DTOs, validation pipeline, Swagger, tenant middleware, and Problem Details |
| `src/Host/CrateSync.Platform.IdentityServer` | Standalone Duende IdentityServer host for identity and token issuance |
| `src/Modules/<Module>` | Domain aggregates and business rules, Application feature slices and handlers, Infrastructure persistence and migrations, and Contracts where defined |
| `src/Modules/Identity` | Identity Domain and Infrastructure projects for users, tenant memberships, and roles |
| `src/BuildingBlocks` | Shared application and domain abstractions, SQL and EF infrastructure, and shared integration contracts |
| `tests/UnitTests`, `tests/IntegrationTests`, `tests/ArchitectureTests` | Test projects grouped by purpose |

Operational modules are Tenancy, Catalog, TradingPartners, Warehousing, InboundLogistics, Inventory, Sales, and Finance. Shipment receipt updates inventory. Sales confirmation attempts stock allocation, dispatch consumes reserved stock, and fulfilment triggers creation of a customer invoice. Those workflows run through module handlers and integration events in the API process.

## Database structure

The operational API uses one SQL Server database, `CrateSync` by default. Each module owns a schema and its own `<schema>.__EFMigrationsHistory` table. The module migration snapshots under `Persistence/Migrations` contain the column, key, and index definitions.

| Schema | EF Core context | Tables |
| --- | --- | --- |
| `tenancy` | `TenancyDbContext` | `Tenants` |
| `catalog` | `CatalogDbContext` | `Products`, `ProductVarieties` |
| `partners` | `TradingPartnersDbContext` | `Customers`, `Suppliers` |
| `warehousing` | `WarehousingDbContext` | `Warehouses` |
| `logistics` | `InboundLogisticsDbContext` | `InboundShipments`, `InboundShipmentLines` |
| `inventory` | `InventoryDbContext` | `InventoryBalances`, `StockMovements`, `StockReservations` |
| `sales` | `SalesDbContext` | `SalesOrders`, `SalesOrderLines` |
| `finance` | `FinanceDbContext` | `CustomerInvoices`, `CustomerInvoiceLines`, `CustomerPayments`, `SupplierInvoices`, `SupplierInvoiceLines`, `SupplierPayments` |

Operational records are scoped by tenant. Cross-module references such as a shipment's supplier ID or an order's warehouse ID are stored as identifiers. The checked-in foreign keys link records within their own module schemas.

The standalone IdentityServer uses the same `CrateSync` database and `ConnectionStrings__CrateSync` setting as the API. The `identity` schema holds ASP.NET Core Identity users, roles, and tenant memberships. The `idsrv` schema holds IdentityServer operational data such as persisted grants and signing keys. The [IdentityServer setup](src/Host/CrateSync.Platform.IdentityServer/README.md) covers generating its two initial migrations, setting up the database, and bootstrapping the first SuperAdmin.

## EF Core commands

Install the EF CLI version used by the backend. If it is already installed, check it with `dotnet ef --version` and use `dotnet tool update --global dotnet-ef --version 10.0.13` if needed.

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
dotnet ef --version
```

Run from the root, with any connection string override set in the same shell. This applies the checked-in migrations for all eight operational contexts and stops on the first failure.

```powershell
$api = 'src/Host/CrateSync.Platform.Api'
$modules = @(
    'Tenancy', 'Catalog', 'TradingPartners', 'Warehousing',
    'InboundLogistics', 'Inventory', 'Sales', 'Finance'
)

foreach ($module in $modules) {
    $project = "src/Modules/$module/CrateSync.Platform.$module.Infrastructure"
    dotnet ef database update --project $project --startup-project $api --context "${module}DbContext" -- --environment Development
    if ($LASTEXITCODE -ne 0) { throw "Migration failed for $module" }
}
```

For a single module, point `--project` at its Infrastructure project and keep the API as `--startup-project`. The following example uses Catalog. Use `migrations remove` only to discard a local migration that has not been shared or applied.

```powershell
$api = 'src/Host/CrateSync.Platform.Api'
$catalog = 'src/Modules/Catalog/CrateSync.Platform.Catalog.Infrastructure'

dotnet ef dbcontext list --project $api --startup-project $api -- --environment Development
dotnet ef migrations list --project $catalog --startup-project $api --context CatalogDbContext -- --environment Development
dotnet ef migrations add DescribeCatalogChange --project $catalog --startup-project $api --context CatalogDbContext --output-dir Persistence/Migrations -- --environment Development
dotnet ef migrations has-pending-model-changes --project $catalog --startup-project $api --context CatalogDbContext -- --environment Development
dotnet ef database update --project $catalog --startup-project $api --context CatalogDbContext -- --environment Development
dotnet ef migrations script --idempotent --output catalog-migrations.sql --project $catalog --startup-project $api --context CatalogDbContext -- --environment Development
dotnet ef migrations remove --project $catalog --startup-project $api --context CatalogDbContext -- --environment Development
```

## Authentication and tenant context

The operational API now validates bearer tokens issued by the standalone IdentityServer.

Development defaults:

- IdentityServer authority: `https://localhost:7201`
- API audience: `cratesync.api`
- Vue origin: `http://localhost:5173`

The API keeps tenant selection claim-based. A valid `tenant_id` claim is required for tenant-scoped operational endpoints. The API does not accept an `X-Tenant-Id` header or an arbitrary tenant ID in a request body as a replacement for the authenticated tenant context.

`GET /api/me` is authenticated but does not require a selected tenant. It returns the current user's display information, platform role, tenant memberships, tenant roles, and warehouse assignments. This is the bootstrap endpoint for the CrateSync Vue portal.

Platform administration remains separate from tenant operations:

- `PlatformRole.Admin` and `PlatformRole.SuperAdmin` can access platform administration endpoints.
- `/api/tenants` is currently protected by the `PlatformAdmin` policy.
- A platform-only SuperAdmin token does not automatically bypass tenant isolation on operational endpoints. The later **Enter Tenant** flow will establish an explicit tenant context.

Initial tenant access is intentionally simple:

- `TenantAdmin` is tenant-wide and can eventually manage all operational areas plus tenant users.
- `Viewer` is tenant-wide read-only access reserved for later endpoint policy rollout.
- `WarehouseAdmin` and `WarehouseUser` are stored as warehouse-scoped assignments rather than tenant-wide roles.

After pulling the warehouse-access change, apply the checked-in Identity migration:

```powershell
dotnet ef database update --project .\src\Modules\Identity\CrateSync.Platform.Identity.Infrastructure\CrateSync.Platform.Identity.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --context CrateSyncIdentityDbContext -- --environment Development
```

Authentication is now enforced globally for controllers. The host root, health endpoint, and Swagger middleware remain available for local diagnostics.

## Endpoint reference

Paths use the casing shown in Swagger. Values in braces are GUIDs. Send JSON with `Content-Type: application/json` for requests with a body. For payload fields, expand the operation in Swagger or see the [request DTOs](src/Host/CrateSync.Platform.Api/Requests). Successful reads return `200`. Creates return `201` unless indicated. Updates and lifecycle actions typically return `204`. Common failure codes are `400` for validation, `404` for missing records, and `409` for conflicts or invalid business state.

### Host and master data

| Area | Base path | Operations relative to the base path | List filters |
| --- | --- | --- | --- |
| Host | `/` | `GET /` and `GET /health` | |
| Tenancy | `/api/tenants` | `GET` list, `POST` create, `GET /{id}`, `PUT /{id}` rename, `PATCH /{id}/activate`, `PATCH /{id}/deactivate` | `isActive` |
| Catalog | `/api/catalog/products` | `GET` list, `POST` create product, `GET /{productId}`, `POST /{productId}/varieties`, `PATCH /{productId}/deactivate` | `search`, `isActive` |
| Customers | `/api/Customer` | `GET` list, `POST` create, `GET /{customerId}`, `PUT /{customerId}`, `PATCH /{customerId}/deactivate` | `search`, `isActive` |
| Suppliers | `/api/Supplier` | `GET` list, `POST` create, `GET /{supplierId}`, `PUT /{supplierId}`, `PATCH /{supplierId}/deactivate` | `search`, `isActive` |
| Warehouses | `/api/Warehouse` | `GET` list, `POST` create, `GET /{warehouseId}`, `PUT /{warehouseId}`, `PATCH /{warehouseId}/activate`, `PATCH /{warehouseId}/deactivate` | `search`, `isActive` |

### Inbound, inventory, and sales

| Area | Base path | Operations relative to the base path | Query parameters |
| --- | --- | --- | --- |
| Inbound shipments | `/api/InboundShipment` | `GET` list, `POST` create, `GET /{shipmentId}`, `POST /{shipmentId}/lines`, `PATCH /{shipmentId}/ship`, `PATCH /{shipmentId}/arrive`, `POST /{shipmentId}/receive` (returns `204`), `PATCH /{shipmentId}/cancel` | List accepts `searchTerm`, `status`, `supplierId`, `warehouseId`. |
| Inventory | `/api/inventory` | `GET` list balances, `GET /stock`, `GET /movements`, `POST /adjustments` (returns `204`) | `warehouseId` and `productVarietyId` are optional for the list, and both are needed for stock or movement lookup. |
| Sales orders | `/api/sales-orders` | `GET` list, `POST` create, `GET /{id}`, `POST /{id}/lines`, `PUT /{id}/lines/{lineId}`, `DELETE /{id}/lines/{lineId}`, `PATCH /{id}/confirm`, `PATCH /{id}/dispatch`, `PATCH /{id}/fulfil`, `PATCH /{id}/cancel` | List accepts `status`, `customerId`. |

### Finance

| Area | Base path | Operations relative to the base path | List filters |
| --- | --- | --- | --- |
| Customer invoices | `/api/finance/customer-invoices` | `GET` list, `GET /{id}`, `POST /{id}/payments`, `PATCH /{id}/void` | `status`, `customerId` |
| Supplier invoices | `/api/finance/supplier-invoices` | `GET` list, `POST` create invoice, `GET /{id}`, `POST /{id}/payments`, `PATCH /{id}/void` | `status`, `supplierId` |

Customer invoices are created by the sales fulfilment handler. There is no public customer invoice creation endpoint. The list endpoints accept enum names in the query string, for example `status=Received` for shipments, `status=Allocated` for sales orders, and `status=Pending` for invoices.

## Try the endpoints

After the API starts and the operational migrations are applied, open Swagger UI, expand an operation, select **Try it out**, enter parameters or a JSON body, and select **Execute**. For Postman, import the Swagger JSON URL and use `https://localhost:7038` as the base URL. No bearer token is required for these local examples.

### Health and tenant administration

```powershell
$base = 'https://localhost:7038'
Invoke-RestMethod "$base/health"

$tenantId = Invoke-RestMethod -Method Post -Uri "$base/api/tenants" -ContentType 'application/json' -Body (@{
    name = "Local demo $([guid]::NewGuid().ToString('N').Substring(0, 8))"
} | ConvertTo-Json)

Invoke-RestMethod "$base/api/tenants/$tenantId"
Invoke-RestMethod "$base/api/tenants?isActive=true"
```

Tenant creation returns the new ID as a JSON GUID string. To list balances for a warehouse and variety, call `GET /api/inventory?warehouseId=<warehouse-guid>&productVarietyId=<variety-guid>`. With the current Development tenant fallback, an empty result only describes rows scoped to `Guid.Empty`.

### Operational workflows with a tenant context

Use these sequences in an environment that supplies a non-empty tenant context. The existing [API integration tests](tests/IntegrationTests/CrateSync.Platform.Api.IntegrationTests/Journeys) show the HTTP flow while replacing the tenant context in the test host.

1. **Master data.** Create a product and variety, supplier, warehouse, and customer. Reuse the returned entity IDs in shipment and sales requests. Product, variety, supplier, warehouse, and customer creation return an object with an `id` property. Sales-order creation returns the ID as a JSON GUID string.
2. **Receive stock.** Create an inbound shipment with the supplier and warehouse IDs. Add a shipment line with the product and variety IDs. Call `ship` and then `arrive`. Post `{"shipmentLineId":"<line-guid>","quantity":40}` to the shipment's `receive` route. Check the balance at `/api/inventory/stock` and the audit trail at `/api/inventory/movements` using the warehouse and variety IDs.
3. **Sell and invoice.** Create an order for the customer at the same warehouse, add a line for the stocked variety, and confirm the order. Read the order and check that the status is `Allocated` before dispatching. Confirmation can return `204` while the order remains `Confirmed` if there is insufficient stock. Dispatch, then fulfil, and query `/api/finance/customer-invoices?customerId=<customer-guid>` for the generated invoice.

## Tests

The unit projects are executable xUnit v3 tests and can run without SQL Server. Build the solution first, then run each unit project from PowerShell.

```powershell
foreach ($project in (Get-ChildItem tests/UnitTests -Directory)) {
    dotnet run --project $project.FullName --no-build
    if ($LASTEXITCODE -ne 0) { throw "Unit tests failed in $($project.Name)" }
}
```

The API integration tests run against SQL Server and migrate and clear tables in their test database. Use a disposable database on a local test instance. The current fixture supplies its generated connection string under `ConnectionStrings:Database`, while the API reads `ConnectionStrings:CrateSync`. Until those keys are aligned, pass the API key explicitly in a separate test shell so the suite cannot target your regular development database.

```powershell
$testDb = "CrateSyncTests_$([guid]::NewGuid().ToString('N'))"
$env:ConnectionStrings__CrateSync = "Server=localhost;Database=$testDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet run --project tests/IntegrationTests/CrateSync.Platform.Api.IntegrationTests
$testExitCode = $LASTEXITCODE
Remove-Item Env:ConnectionStrings__CrateSync
if ($testExitCode -ne 0) { throw "API integration tests failed" }
```
