import {
  createRouter,
  createWebHistory,
} from 'vue-router'
import { pinia } from '@/stores'
import { useAuthStore } from '@/stores/auth'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/auth/callback',
      component: () =>
        import(
          '@/views/auth/AuthCallbackView.vue'
        ),
      meta: { public: true },
    },
    {
      path: '/auth/session-changed',
      component: () =>
        import(
          '@/views/auth/SessionChangedView.vue'
        ),
      meta: { public: true },
    },
    {
      path: '/auth/silent-callback',
      component: () =>
        import(
          '@/views/auth/SilentCallbackView.vue'
        ),
      meta: { public: true },
    },
    {
      path: '/',
      component: () =>
        import('@/layouts/AppLayout.vue'),
      children: [
        {
          path: '',
          name: 'home',
          component: () =>
            import('@/views/HomeView.vue'),
        },
        {
          path: 'admin',
          name: 'platform-admin',
          component: () =>
            import(
              '@/views/admin/PlatformAdminView.vue'
            ),
          meta: {
            requiresSuperAdmin: true,
          },
        },
        {
          path: 'tenant/:tenantId',
          name: 'tenant-dashboard',
          component: () =>
            import(
              '@/views/tenant/TenantDashboardView.vue'
            ),
          meta: {
            requiresTenant: true,
            title: 'Dashboard',
          },
        },
        {
          path: 'tenant/:tenantId/users',
          name: 'organisation-users',
          component: () =>
            import(
              '@/views/tenant/OrganisationUsersView.vue'
            ),
          meta: {
            requiresTenant: true,
            requiresTenantAdmin: true,
            title: 'Users',
          },
        },
        {
          path: 'tenant/:tenantId/products',
          name: 'products',
          component: () =>
            import(
              '@/views/tenant/ProductCatalogView.vue'
            ),
          meta: {
            requiresTenant: true,
            title: 'Products',
          },
        },
        {
          path: 'tenant/:tenantId/warehouses',
          name: 'warehouses',
          component: () =>
            import(
              '@/views/tenant/WarehouseManagementView.vue'
            ),
          meta: {
            requiresTenant: true,
            title: 'Warehouses',
          },
        },
        {
          path: 'tenant/:tenantId/suppliers',
          name: 'suppliers',
          component: () =>
            import(
              '@/views/tenant/SupplierManagementView.vue'
            ),
          meta: {
            requiresTenant: true,
            title: 'Suppliers',
          },
        },
        {
          path: 'tenant/:tenantId/customers',
          name: 'customers',
          component: () =>
            import(
              '@/views/tenant/CustomerManagementView.vue'
            ),
          meta: {
            requiresTenant: true,
            title: 'Customers',
          },
        },
        {
          path: 'tenant/:tenantId/shipments',
          name: 'inbound-shipments',
          component: () =>
            import(
              '@/views/tenant/InboundShipmentManagementView.vue'
            ),
          meta: {
            requiresTenant: true,
            title: 'Inbound Shipments',
          },
        },
        {
          path: 'tenant/:tenantId/inventory',
          name: 'inventory',
          component: () =>
            import(
              '@/views/tenant/InventoryManagementView.vue'
            ),
          meta: {
            requiresTenant: true,
            title: 'Inventory',
          },
        },
        ...[
          ['sales', 'Sales'],
          ['finance', 'Finance'],
        ].map(([path, title]) => ({
          path: `tenant/:tenantId/${path}`,
          component: () =>
            import(
              '@/views/tenant/ModulePlaceholderView.vue'
            ),
          meta: {
            requiresTenant: true,
            title,
          },
        })),
      ],
    },
  ],
})

router.beforeEach(async (to) => {
  if (to.meta.public)
    return true

  const auth = useAuthStore(pinia)

  await auth.initialize()

  if (!auth.isAuthenticated) {
    await auth.login(to.fullPath)
    return false
  }

  if (
    to.meta.requiresSuperAdmin &&
    !auth.isSuperAdmin
  ) {
    return { name: 'home' }
  }

  if (
    to.name === 'home' &&
    !auth.isSuperAdmin &&
    auth.me?.tenants.length === 1 &&
    auth.me.currentTenant
  ) {
    return auth.me.currentTenant.path
  }

  if (
    to.meta.requiresTenantAdmin &&
    auth.me?.currentTenant?.accessMode !==
      'SuperAdmin' &&
    auth.me?.currentTenant?.roles.includes('TenantOwner') !== true &&
    auth.me?.currentTenant?.roles.includes(
      'TenantAdmin',
    ) !== true
  ) {
    return auth.me?.currentTenant?.path ??
      { name: 'home' }
  }

  if (to.meta.requiresTenant) {
    const requestedTenantId =
      String(to.params.tenantId ?? '')

    if (
      !auth.me?.currentTenant ||
      auth.me.currentTenant.tenantId !==
        requestedTenantId
    ) {
      return { name: 'home' }
    }
  }

  const current = auth.me?.currentTenant
  if (to.meta.requiresTenant && current?.accessMode !== 'SuperAdmin') {
    const roles = current?.roles ?? []
    const financial = roles.includes('TenantOwner') || roles.includes('Finance')
    if (to.meta.title === 'Finance' && !financial)
      return current?.path ?? { name: 'home' }
    if (!financial && !roles.includes('TenantAdmin') && !roles.includes('Viewer') &&
      !['tenant-dashboard', 'inventory', 'inbound-shipments'].includes(String(to.name)) &&
      !(current?.warehouses.some(w => w.role === 'WarehouseAdmin') && (to.name === 'customers' || to.meta.title === 'Sales')))
      return current?.path ?? { name: 'home' }
  }
  return true
})
