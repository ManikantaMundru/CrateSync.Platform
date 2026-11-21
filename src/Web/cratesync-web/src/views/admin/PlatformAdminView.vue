<script setup lang="ts">
import {
  onMounted,
  ref,
} from 'vue'
import {
  CBadge,
  CButton,
  CCard,
  CCardBody,
  CSpinner,
} from '@coreui/vue'
import { apiFetch } from '@/services/api'
import { useAuthStore } from '@/stores/auth'

interface TenantSummary {
  id: string
  name: string
  status: string
  updatedAtUtc: string
}

const auth = useAuthStore()
const tenants = ref<TenantSummary[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    tenants.value =
      await apiFetch<TenantSummary[]>(
        '/api/tenants?isActive=true',
      )
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load tenants.'
  } finally {
    loading.value = false
  }
})

function enterTenant(
  tenant: TenantSummary,
) {
  auth.selectTenant(
    tenant.id,
    `/tenant/${tenant.id}`,
    true,
  )
}
</script>

<template>
  <section class="page-stack">
    <div class="page-heading">
      <div>
        <p class="eyebrow">
          Platform
        </p>
        <h1>
          Platform Administration
        </h1>
        <p>
          Review active tenants and enter a
          tenant when using SuperAdmin access.
        </p>
      </div>
    </div>

    <div
      v-if="loading"
      class="loading-state"
    >
      <CSpinner />
      Loading tenants…
    </div>

    <CCard
      v-else-if="error"
      class="empty-state"
    >
      <CCardBody>
        <h3>
          Unable to load tenants
        </h3>
        <p>{{ error }}</p>
      </CCardBody>
    </CCard>

    <div
      v-else
      class="workspace-grid"
    >
      <CCard
        v-for="tenant in tenants"
        :key="tenant.id"
        class="workspace-card"
      >
        <CCardBody>
          <div class="card-topline">
            <div>
              <p class="eyebrow">
                Tenant
              </p>
              <h3>
                {{ tenant.name }}
              </h3>
            </div>

            <CBadge color="success">
              {{ tenant.status }}
            </CBadge>
          </div>

          <p class="muted-id">
            {{ tenant.id }}
          </p>

          <CButton
            v-if="auth.isSuperAdmin"
            color="warning"
            @click="enterTenant(tenant)"
          >
            Enter tenant
          </CButton>
        </CCardBody>
      </CCard>
    </div>
  </section>
</template>
