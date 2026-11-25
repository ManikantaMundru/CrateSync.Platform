<script setup lang="ts">
import { computed } from 'vue'
import {
  CBadge,
  CCard,
  CCardBody,
} from '@coreui/vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()

const currentTenant = computed(
  () => auth.me?.currentTenant ?? null,
)

const tenantWide = computed(() => currentTenant.value?.accessMode === 'SuperAdmin' || currentTenant.value?.roles.some(r => ['TenantAdmin', 'TenantOwner', 'Finance', 'Viewer'].includes(r)) === true)
const financial = computed(() => currentTenant.value?.accessMode === 'SuperAdmin' || currentTenant.value?.roles.some(r => ['TenantOwner', 'Finance'].includes(r)) === true)

function displayRole(role: string) {
  return role === 'TenantAdmin'
    ? 'Administrator'
    : role
}
</script>

<template>
  <section class="page-stack">
    <div class="page-heading">
      <div>
        <p class="eyebrow">
          Organisation workspace
        </p>
        <h1>
          {{
            currentTenant?.name ??
            'Organisation workspace'
          }}
        </h1>
        <p>
          Your operational dashboard will
          live here.
        </p>
      </div>

      <div class="badge-row">
        <CBadge
          v-if="
            currentTenant?.accessMode ===
            'SuperAdmin'
          "
          color="warning"
        >
          SuperAdmin
        </CBadge>

        <CBadge
          v-for="role in currentTenant?.roles ?? []"
          :key="role"
          color="info"
        >
          {{ displayRole(role) }}
        </CBadge>
      </div>
    </div>

    <div class="metric-grid">
      <CCard v-if="tenantWide || currentTenant?.warehouses.some(w => w.role === 'WarehouseAdmin')" class="metric-card">
        <CCardBody>
          <span>Sales</span>
          <strong>Ready</strong>
          <small>
            Orders and dispatch workflows
          </small>
        </CCardBody>
      </CCard>

      <CCard class="metric-card">
        <CCardBody>
          <span>Inventory</span>
          <strong>Ready</strong>
          <small>
            Stock and warehouse balances
          </small>
        </CCardBody>
      </CCard>

      <CCard class="metric-card">
        <CCardBody>
          <span>Inbound</span>
          <strong>Ready</strong>
          <small>
            Shipment and receiving workflows
          </small>
        </CCardBody>
      </CCard>

      <CCard v-if="financial" class="metric-card">
        <CCardBody>
          <span>Finance</span>
          <strong>Ready</strong>
          <small>
            Receivables and payables
          </small>
        </CCardBody>
      </CCard>
    </div>
  </section>
</template>
