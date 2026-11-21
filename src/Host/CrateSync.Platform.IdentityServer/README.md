# CrateSync IdentityServer

The standalone .NET 10 host uses Duende IdentityServer and ASP.NET Core Identity. It uses the API’s `CrateSync` SQL Server database. The `identity` schema holds users and tenant memberships, and `idsrv` holds persisted grants and other IdentityServer operational data. The operational API and Vue client can be wired to this authority separately.

## Local setup

Run from the repository root in PowerShell with the EF Core 10 CLI installed. The Development connection string matches the API’s local SQL Server default. If you override it, set `ConnectionStrings__CrateSync` to the same value for both hosts.

```powershell
$identityHost = 'src/Host/CrateSync.Platform.IdentityServer'
$identityInfrastructure = 'src/Modules/Identity/CrateSync.Platform.Identity.Infrastructure'

dotnet ef migrations add InitialIdentity --project $identityInfrastructure --startup-project $identityHost --context CrateSyncIdentityDbContext --output-dir Persistence/Migrations -- --environment Development
dotnet ef migrations add InitialIdentityServerOperational --project $identityHost --startup-project $identityHost --context PersistedGrantDbContext --output-dir Persistence/IdentityServer/Migrations -- --environment Development

dotnet ef database update --project $identityInfrastructure --startup-project $identityHost --context CrateSyncIdentityDbContext -- --environment Development
dotnet ef database update --project $identityHost --startup-project $identityHost --context PersistedGrantDbContext -- --environment Development
```

Generate the initial migrations once and apply them to the shared `CrateSync` database before bootstrapping an account. To seed the first SuperAdmin, set `BootstrapSuperAdmin__Enabled` to `true` and provide `BootstrapSuperAdmin__UserName`, `BootstrapSuperAdmin__Email`, and `BootstrapSuperAdmin__Password` through local configuration or environment variables. Disable seeding after the account is created.

```powershell
dotnet run --project $identityHost
dotnet run --project tests/UnitTests/CrateSync.Platform.Identity.Tests
```

When clients request the `cratesync.api` scope, the resulting access token is issued for the `cratesync.api` audience.

The development discovery endpoint is `https://localhost:7201/.well-known/openid-configuration`. The preconfigured `cratesync.web` client uses Authorization Code with PKCE and defaults to `http://localhost:5173/auth/callback` for its redirect URI.
