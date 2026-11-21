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
            title: 'Users',
          },
        },
        ...[
          ['sales', 'Sales'],
          ['shipments', 'Inbound Shipments'],
          ['inventory', 'Inventory'],
          ['customers', 'Customers'],
          ['suppliers', 'Suppliers'],
          ['finance', 'Finance'],
          ['warehouses', 'Warehouses'],
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

  return true
})
