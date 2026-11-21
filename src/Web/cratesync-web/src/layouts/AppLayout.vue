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

const tenantId = computed(
  () => auth.me?.currentTenantId ?? null,
)

const tenantLinks = computed(() => {
  if (!tenantId.value)
    return []

  return [
    ['Dashboard', ''],
    ['Sales', '/sales'],
    ['Inbound Shipments', '/shipments'],
    ['Inventory', '/inventory'],
    ['Customers', '/customers'],
    ['Suppliers', '/suppliers'],
    ['Finance', '/finance'],
    ['Warehouses', '/warehouses'],
    ['Users', '/users'],
  ].map(([label, suffix]) => ({
    label,
    to: `/tenant/${tenantId.value}${suffix}`,
  }))
})

function closeSidebar() {
  sidebarOpen.value = false
}

function isActive(path: string) {
  return route.path === path
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
          class="nav-entry"
          :class="{ active: isActive('/') }"
          to="/"
          @click="closeSidebar"
        >
          Home
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
          Platform Administration
        </RouterLink>

        <template v-if="tenantId">
          <div class="nav-section-label">
            Current tenant
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
            {{ link.label }}
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

        <CButton
          color="light"
          class="w-100"
          @click="auth.logout()"
        >
          Sign out
        </CButton>
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
          <span v-if="tenantId">
            Tenant workspace
          </span>
          <span v-else>
            Platform workspace
          </span>
        </div>

        <div class="header-actions">
          <CBadge
            v-if="
              auth.me?.actingAsPlatformAdmin
            "
            color="warning"
          >
            Platform admin mode
          </CBadge>

          <CButton
            v-if="tenantId"
            color="secondary"
            variant="outline"
            size="sm"
            @click="auth.exitTenant()"
          >
            Exit tenant
          </CButton>
        </div>
      </header>

      <div
        v-if="
          auth.me?.actingAsPlatformAdmin
        "
        class="platform-admin-banner"
      >
        <strong>
          Platform administrator mode
        </strong>
        <span>
          You are operating inside a tenant
          using SuperAdmin access.
        </span>
      </div>

      <main class="content-area">
        <RouterView />
      </main>
    </div>
  </div>
</template>
