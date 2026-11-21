<script setup lang="ts">
import { computed, ref } from 'vue'
import {
  CBadge,
  CButton,
} from '@coreui/vue'
import {
  RouterLink,
  RouterView,
  useRoute,
} from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const route = useRoute()
const sidebarOpen = ref(false)

const initials = computed(() => {
  const name =
    auth.me?.displayName?.trim() ?? ''

  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('') || 'CS'
})

const currentTenant = computed(
  () => auth.me?.currentTenant ?? null,
)

const tenantId = computed(
  () => currentTenant.value?.tenantId ?? null,
)

const organisationCount = computed(
  () => auth.me?.tenants.length ?? 0,
)

const isSuperAdminSession = computed(
  () =>
    currentTenant.value?.accessMode ===
    'SuperAdmin',
)

const canSwitchOrganisation = computed(
  () =>
    !isSuperAdminSession.value &&
    organisationCount.value > 1,
)

const canManageTenant = computed(
  () =>
    isSuperAdminSession.value ||
    currentTenant.value?.roles.includes(
      'TenantAdmin',
    ) === true,
)

const showHome = computed(
  () =>
    !currentTenant.value ||
    auth.isSuperAdmin ||
    organisationCount.value > 1,
)

const tenantLinks = computed(() => {
  if (!tenantId.value)
    return []

  const links = [
    {
      label: 'Dashboard',
      suffix: '',
      icon: 'cil-speedometer',
    },
    {
      label: 'Sales',
      suffix: '/sales',
      icon: 'cil-cart',
    },
    {
      label: 'Inbound Shipments',
      suffix: '/shipments',
      icon: 'cil-truck',
    },
    {
      label: 'Inventory',
      suffix: '/inventory',
      icon: 'cil-storage',
    },
    {
      label: 'Customers',
      suffix: '/customers',
      icon: 'cil-people',
    },
    {
      label: 'Suppliers',
      suffix: '/suppliers',
      icon: 'cil-user',
    },
    {
      label: 'Finance',
      suffix: '/finance',
      icon: 'cil-money',
    },
    {
      label: 'Warehouses',
      suffix: '/warehouses',
      icon: 'cil-industry',
    },
  ]

  if (canManageTenant.value) {
    links.push({
      label: 'Users',
      suffix: '/users',
      icon: 'cil-people',
    })
  }

  return links.map((link) => ({
    ...link,
    to:
      `/tenant/${tenantId.value}${link.suffix}`,
  }))
})

function closeSidebar() {
  sidebarOpen.value = false
}

function isActive(path: string) {
  return route.path === path
}

function leaveOrganisation() {
  auth.exitTenant('/')
}
</script>

<template>
  <div class="app-shell">
    <div
      v-if="sidebarOpen"
      class="sidebar-backdrop"
      @click="closeSidebar"
    />

    <aside
      class="app-sidebar"
      :class="{ 'is-open': sidebarOpen }"
    >
      <div class="brand-block">
        <div class="brand-mark">C</div>
        <div>
          <div class="brand-name">CrateSync</div>
          <div class="brand-subtitle">
            Operations Platform
          </div>
        </div>
      </div>

      <nav class="sidebar-nav">
        <RouterLink
          v-if="showHome"
          class="nav-entry"
          :class="{ active: isActive('/') }"
          to="/"
          @click="closeSidebar"
        >
          <i
            class="nav-icon cil-home"
            aria-hidden="true"
          />
          <span>Home</span>
        </RouterLink>

        <RouterLink
          v-if="
            auth.me?.platform
              .canAccessAdminPortal
          "
          class="nav-entry"
          :class="{
            active:
              route.path.startsWith('/admin'),
          }"
          to="/admin"
          @click="closeSidebar"
        >
          <i
            class="nav-icon cil-settings"
            aria-hidden="true"
          />
          <span>Platform Administration</span>
        </RouterLink>

        <template v-if="tenantId">
          <div class="nav-section-label">
            {{ currentTenant?.name }}
          </div>

          <RouterLink
            v-for="link in tenantLinks"
            :key="link.to"
            class="nav-entry"
            :class="{
              active:
                route.path === link.to,
            }"
            :to="link.to"
            @click="closeSidebar"
          >
            <i
              class="nav-icon"
              :class="link.icon"
              aria-hidden="true"
            />
            <span>{{ link.label }}</span>
          </RouterLink>
        </template>
      </nav>

      <div class="sidebar-footer">
        <div class="user-summary">
          <div class="avatar-circle">
            {{ initials }}
          </div>
          <div class="user-copy">
            <strong>
              {{ auth.me?.displayName }}
            </strong>
            <small>
              {{
                auth.me?.email ??
                'Signed in'
              }}
            </small>
          </div>
        </div>
      </div>
    </aside>

    <div class="app-main">
      <header class="app-header">
        <button
          class="mobile-menu-button"
          type="button"
          aria-label="Open navigation"
          @click="sidebarOpen = true"
        >
          ☰
        </button>

        <div class="header-title">
          <strong>CrateSync</strong>
          <span v-if="currentTenant">
            {{ currentTenant.name }}
          </span>
          <span v-else>
            Platform workspace
          </span>
        </div>

        <div class="header-actions">
          <CBadge
            v-if="isSuperAdminSession"
            color="warning"
          >
            Platform admin mode
          </CBadge>

          <CButton
            v-if="isSuperAdminSession"
            color="secondary"
            variant="outline"
            size="sm"
            @click="leaveOrganisation"
          >
            <i
              class="button-icon cil-arrow-left"
              aria-hidden="true"
            />
            Return to platform
          </CButton>

          <CButton
            v-else-if="canSwitchOrganisation"
            color="secondary"
            variant="outline"
            size="sm"
            @click="leaveOrganisation"
          >
            <i
              class="button-icon cil-swap-horizontal"
              aria-hidden="true"
            />
            Switch organisation
          </CButton>

          <CButton
            color="secondary"
            variant="outline"
            size="sm"
            @click="auth.logout()"
          >
            <i
              class="button-icon cil-account-logout"
              aria-hidden="true"
            />
            Sign out
          </CButton>
        </div>
      </header>

      <div
        v-if="isSuperAdminSession"
        class="platform-admin-banner"
      >
        <strong>
          Platform administrator mode
        </strong>
        <span>
          You are operating inside
          <strong>{{ currentTenant?.name }}</strong>
          using SuperAdmin access.
        </span>
      </div>

      <main class="content-area">
        <RouterView />
      </main>
    </div>
  </div>
</template>
