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


## Manual account activation

CrateSync currently does not send onboarding email.

When SuperAdmin creates a new TenantAdmin whose account requires activation:

1. the API generates a one-time IdentityServer activation link
2. the web portal displays the link in an activation modal
3. SuperAdmin copies the link and sends it manually
4. the user opens the link and sets a password
5. IdentityServer confirms the account
6. the user continues to the CrateSync web app and signs in normally

The tenant management screen is browse-first: **New tenant** and **Add TenantAdmin** open modal forms, while the main page contains a searchable tenant grid with tenant actions.


## Theme selection

The Vue portal supports:

- System
- Light
- Dark

The selected preference is stored in browser local storage under `cratesync:theme`.

When `System` is selected, the portal follows the operating-system colour scheme and reacts to system theme changes while the app is open.

CoreUI theme tokens are switched through `data-coreui-theme`, and CrateSync-specific shell/card/form styles include matching dark-mode overrides.


## Organisation user management

The organisation **Users** route is available to organisation Administrators and to SuperAdmin while explicitly operating inside an organisation.

The screen supports:

- searching organisation users
- adding a new or existing CrateSync identity
- assigning organisation roles (`TenantAdmin`, displayed as **Administrator**, and `Viewer`)
- assigning active warehouses as `WarehouseUser` or `WarehouseAdmin`
- editing roles and warehouse assignments
- activating/deactivating organisation membership
- showing when account activation is still required
- generating and copying a fresh manual activation link

The Vue route is guarded for administrator access, while the API `TenantAdmin` policy remains the authoritative security boundary.

A user cannot deactivate their own membership or remove their own TenantAdmin role; these rules continue to be enforced server-side.


## Warehouse management

The organisation **Warehouses** route now uses the same compact TanStack grid pattern as Users.

All tenant users can view warehouses available to their current access scope.

Organisation Administrators and SuperAdmin operating inside the organisation can additionally:

- create warehouses
- edit warehouse details
- activate warehouses
- deactivate warehouses

Warehouse fields:

- name
- code
- address line 1
- optional address line 2
- city
- optional state/province
- postal code
- country

The view supports:

- search by warehouse name, code, city or country
- Active / Inactive status filtering
- 10 / 25 / 50 row pagination
- Light and Dark themes
- responsive rows on smaller screens

The API remains the authoritative permission boundary through the existing `TenantAdmin` policy on mutation endpoints.


## Product catalog

The organisation **Products** workspace provides the Catalog UI over the existing Catalog module.

All users in the current organisation can:

- search products
- filter Active / Inactive products
- view product details
- view configured varieties

Organisation Administrators and SuperAdmin operating inside the organisation can additionally:

- create products
- add varieties to active products
- deactivate active products

The grid uses TanStack pagination with 10 / 25 / 50 row options.

Current backend capabilities intentionally reflected in the UI:

- products can be deactivated
- there is currently no product reactivation command
- varieties can be added and read
- there are currently no variety edit/deactivate endpoints

The UI does not expose actions that the backend does not yet support.


## Silent access-token renewal

The Vue OIDC client now renews the 15-minute access token silently through:

```text
/auth/silent-callback
```

The IdentityServer SPA client allows this redirect URI, `apiFetch` requests a valid/renewed user before sending API calls, and the Pinia auth store listens for renewed OIDC users so route guards remain synchronized.

## Multi-add product varieties

The Product detail modal now supports multi-add variety pills:

- type a variety and press Enter or comma to queue it
- duplicate names are ignored case-insensitively
- draft pills can be removed with the close button before saving
- Save varieties persists all queued items through the existing add-variety API
- successfully persisted varieties render as compact saved pills

Persisted varieties are display-only because the backend currently has no variety delete/deactivate command.


### Responsive product catalog

The Products workspace uses different interaction patterns by viewport:

- Desktop (`>= 992px`): master-detail layout. The first visible product is selected automatically and details render in the right panel.
- Tablet/mobile (`< 992px`): accordion layout. No product is opened automatically; the user taps a product row to expand it inline.
- Only one product can be expanded at a time on compact layouts.
- Tapping the open product again collapses it.
- Search, status filters and pagination close an expanded product if it is no longer in the visible result set.
- Switching from compact to desktop selects the first visible product; switching from desktop to compact starts collapsed.
