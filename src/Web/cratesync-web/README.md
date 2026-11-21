# CrateSync Web

Responsive Vue 3 portal for CrateSync.

## Stack

- Vue 3
- TypeScript 6 (pinned for current `vue-tsc` compatibility)
- Vite
- Vue Router
- Pinia
- CoreUI Free
- oidc-client-ts
- OIDC Authorization Code + PKCE

## Local prerequisites

Use Node.js 22.12 or later.

Run IdentityServer first:

```powershell
dotnet run --project .\src\Host\CrateSync.Platform.IdentityServer\CrateSync.Platform.IdentityServer.csproj
```

Run the operational API:

```powershell
dotnet run --project .\src\Host\CrateSync.Platform.Api\CrateSync.Platform.Api.csproj --launch-profile https
```

Then run the web app:

```powershell
cd .\src\Web\cratesync-web
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

Development defaults:

```text
IdentityServer: https://localhost:7201
API:            https://localhost:7038
Vue:            http://localhost:5173
```

Copy `.env.example` to `.env.local` only when overriding these defaults.

## Authentication flow

The SPA uses the `cratesync.web` OIDC client with Authorization Code + PKCE.

```text
Vue
  -> IdentityServer login
  -> /auth/callback
  -> access token
  -> GET /api/me
```

Tenant switching uses the IdentityServer session pages. After IdentityServer changes its authentication cookie, it redirects to:

```text
/auth/session-changed
```

The SPA then performs a fresh OIDC authorization request so the new access token contains the selected tenant context.

Normal membership:

```text
Vue
  -> /Account/SelectTenant
  -> /auth/session-changed
  -> fresh OIDC token
  -> /tenant/{tenantId}
```

SuperAdmin:

```text
Platform Administration
  -> /Account/EnterTenant
  -> acting_as_platform_admin=true
  -> /auth/session-changed
  -> fresh OIDC token
  -> tenant workspace
```

Exit tenant:

```text
Vue
  -> /Account/ExitTenant
  -> platform-only Identity session
  -> /auth/session-changed
  -> fresh OIDC token
  -> /
```

## Responsive layout

The shell is designed mobile-first:

- desktop: persistent sidebar
- tablet: drawer sidebar and two-column cards
- mobile: drawer sidebar and single-column cards

Business modules are placeholders in this initial shell PR. Build them as vertical frontend feature slices rather than putting every API call into one global store.

## TypeScript version

The web project currently pins TypeScript to `6.0.2`.

`vue-tsc 3.3.x` does not yet support the TypeScript 7 package layout, so upgrading this app to TypeScript 7 causes:

```text
ERR_PACKAGE_PATH_NOT_EXPORTED: Package subpath './lib/tsc'
```

Keep TypeScript on 6.x until Vue language-tools officially supports the TypeScript 7 API.

## Platform tenant management

Platform administration is restricted to `SuperAdmin`.

The tenant management screen supports:

- creating a tenant organisation
- provisioning its first `TenantAdmin`
- renaming a tenant
- activating or deactivating a tenant
- explicitly entering an active tenant using SuperAdmin support access

TenantAdmin remains tenant-scoped and cannot create or manage other tenant organisations.

The application header contains Sign out and, when a tenant session exists, Exit tenant. The sidebar footer is identity-only so narrow layouts do not wrap the sign-out action.
