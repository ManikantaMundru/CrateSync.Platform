# CrateSync IdentityServer

The standalone .NET 10 host uses Duende IdentityServer and ASP.NET Core Identity. It uses the API’s `CrateSync` SQL Server database. The `identity` schema holds users and tenant memberships, and `idsrv` holds persisted grants and other IdentityServer operational data. The operational API and Vue client can be wired to this authority separately.

## Local setup

Run all commands below from the repository root:

```text
C:\Users\manik\source\repos\CrateSync.Platform
```

The Identity projects are located at:

```text
src/Modules/Identity/CrateSync.Platform.Identity.Domain
src/Modules/Identity/CrateSync.Platform.Identity.Infrastructure
src/Host/CrateSync.Platform.IdentityServer
```

The IdentityServer project is the startup project for EF commands. Visual Studio's selected startup project does not affect `dotnet ef`, so the commands below always specify `--startup-project` explicitly.

Use the EF Core 10 CLI:

```powershell
dotnet ef --version
```

If required:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.13
```

The Development connection string matches the API’s local SQL Server default. If you override it, set `ConnectionStrings__CrateSync` to the same value for both hosts.

### Verify the project paths

Before running migrations, verify that both projects exist:

```powershell
Test-Path .\src\Modules\Identity\CrateSync.Platform.Identity.Infrastructure\CrateSync.Platform.Identity.Infrastructure.csproj
Test-Path .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj
```

Both commands should return `True`.

You can also list the Identity projects with:

```powershell
Get-ChildItem .\src -Recurse -Filter *.csproj | Where-Object { $_.FullName -like "*Identity*" } | Select-Object FullName
```

### Build IdentityServer first

Before running EF migrations, make sure the startup project builds:

```powershell
dotnet build .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj
```

Do not continue to migrations until this succeeds.

## Identity database migrations

CrateSync Identity uses two EF Core contexts:

| Context | Project | Purpose |
| --- | --- | --- |
| `CrateSyncIdentityDbContext` | Identity Infrastructure | ASP.NET Core Identity users, roles, memberships, and tenant roles |
| `PersistedGrantDbContext` | IdentityServer host | Duende IdentityServer operational data such as persisted grants |

### 1. Verify the Identity DbContext

```powershell
dotnet ef dbcontext list --project .\src\Modules\Identity\CrateSync.Platform.Identity.Infrastructure\CrateSync.Platform.Identity.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj -- --environment Development
```

You should see `CrateSyncIdentityDbContext`.

### 2. Create the initial Identity migration

Run this once when creating the initial Identity schema:

```powershell
dotnet ef migrations add InitialIdentity --project .\src\Modules\Identity\CrateSync.Platform.Identity.Infrastructure\CrateSync.Platform.Identity.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --context CrateSyncIdentityDbContext --output-dir Persistence\Migrations -- --environment Development
```

### 3. Apply the Identity migration

```powershell
dotnet ef database update --project .\src\Modules\Identity\CrateSync.Platform.Identity.Infrastructure\CrateSync.Platform.Identity.Infrastructure.csproj --startup-project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --context CrateSyncIdentityDbContext -- --environment Development
```

This creates the ASP.NET Core Identity and CrateSync membership tables in the `identity` schema.

### 4. Verify the IdentityServer operational DbContext

```powershell
dotnet ef dbcontext list --project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --startup-project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj -- --environment Development
```

You should see `PersistedGrantDbContext`.

### 5. Create the initial IdentityServer operational migration

Run this once when creating the initial `idsrv` operational schema:

```powershell
dotnet ef migrations add InitialIdentityServerOperational --project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --startup-project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --context PersistedGrantDbContext --output-dir Persistence\IdentityServer\Migrations -- --environment Development
```

### 6. Apply the IdentityServer operational migration

```powershell
dotnet ef database update --project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --startup-project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj --context PersistedGrantDbContext -- --environment Development
```

After both updates, the database should contain the `identity` schema for users and memberships and the `idsrv` schema for IdentityServer operational data.

> Create the initial migrations only once. Once they are committed, other developers normally run the two `database update` commands rather than creating `InitialIdentity` and `InitialIdentityServerOperational` again.

## Bootstrap the first SuperAdmin

Generate and apply both initial migrations before bootstrapping an account.

Set `BootstrapSuperAdmin__Enabled` to `true` and provide `BootstrapSuperAdmin__UserName`, `BootstrapSuperAdmin__Email`, and `BootstrapSuperAdmin__Password` through local configuration or environment variables. Never commit the password.

Example:

```powershell
$env:BootstrapSuperAdmin__Enabled = 'true'
$env:BootstrapSuperAdmin__UserName = 'cratesyncadmin'
$env:BootstrapSuperAdmin__Email = 'admin@cratesync.local'
$env:BootstrapSuperAdmin__Password = '<strong-local-password>'
```

Start IdentityServer:

```powershell
dotnet run --project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj
```

After the SuperAdmin has been created successfully, disable bootstrap:

```powershell
$env:BootstrapSuperAdmin__Enabled = 'false'
```

Run the Identity unit tests with:

```powershell
dotnet run --project .\tests\UnitTests\CrateSync.Platform.Identity.Tests\CrateSync.Platform.Identity.Tests.csproj
```

When clients request the `cratesync.api` scope, the resulting access token is issued for the `cratesync.api` audience.

The development discovery endpoint is `https://localhost:7201/.well-known/openid-configuration`. The preconfigured `cratesync.web` client uses Authorization Code with PKCE and defaults to `http://localhost:5173/auth/callback` for its redirect URI.
