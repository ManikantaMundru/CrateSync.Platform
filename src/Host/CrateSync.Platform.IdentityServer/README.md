# CrateSync IdentityServer

The standalone .NET 10 host uses Duende IdentityServer and ASP.NET Core Identity. It owns user credentials, tenant memberships and persisted grants in a separate SQL Server database named `CrateSync.Identity`. The operational API and Vue client can be wired to this authority separately.

## Local setup

Run from the repository root in PowerShell. Set `ConnectionStrings__IdentityDatabase` to a SQL Server connection string and have the EF Core 10 CLI available.

```powershell
$identityHost = 'src/Host/CrateSync.Platform.IdentityServer'
$identityInfrastructure = 'src/Modules/Identity/CrateSync.Platform.Identity.Infrastructure'

dotnet ef migrations add InitialIdentity --project $identityInfrastructure --startup-project $identityHost --context CrateSyncIdentityDbContext --output-dir Persistence/Migrations
dotnet ef migrations add InitialIdentityServerOperational --project $identityHost --startup-project $identityHost --context PersistedGrantDbContext --output-dir Persistence/IdentityServer/Migrations

dotnet ef database update --project $identityInfrastructure --startup-project $identityHost --context CrateSyncIdentityDbContext
dotnet ef database update --project $identityHost --startup-project $identityHost --context PersistedGrantDbContext
```

Generate the initial migrations once and apply them to the Identity database before bootstrapping an account. To seed the first SuperAdmin, set `BootstrapSuperAdmin__Enabled` to `true` and provide `BootstrapSuperAdmin__UserName`, `BootstrapSuperAdmin__Email`, and `BootstrapSuperAdmin__Password` through local configuration or environment variables. Disable seeding after the account is created.

```powershell
dotnet run --project $identityHost
dotnet run --project tests/UnitTests/CrateSync.Platform.Identity.Tests
```

The development discovery endpoint is `https://localhost:7201/.well-known/openid-configuration`. The preconfigured `cratesync.web` client uses Authorization Code with PKCE and defaults to `http://localhost:5173/auth/callback` for its redirect URI.
