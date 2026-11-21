<script setup lang="ts">
import {
  CBadge,
  CButton,
  CCard,
  CCardBody,
} from '@coreui/vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import type { TenantAccess } from '@/types/me'

const router = useRouter()
const auth = useAuthStore()

function openTenant(
  tenant: TenantAccess,
) {
  const target =
    `/tenant/${tenant.tenantId}`

  if (
    auth.me?.currentTenantId ===
    tenant.tenantId
  ) {
    router.push(target)
    return
  }

  auth.selectTenant(
    tenant.tenantId,
    target,
    false,
  )
}
</script>

<template>
  <section class="page-stack">
    <div class="page-heading">
      <div>
        <p class="eyebrow">
          Workspace
        </p>
        <h1>
          Welcome,
          {{ auth.me?.displayName }}
        </h1>
        <p>
          Choose where you want to work.
        </p>
      </div>
    </div>

    <section
      v-if="
        auth.me?.platform
          .canAccessAdminPortal
      "
      class="section-block"
    >
      <div class="section-heading">
        <div>
          <h2>Platform access</h2>
          <p>
            Manage tenants and platform-level
            administration.
          </p>
        </div>
      </div>

      <CCard class="workspace-card">
        <CCardBody>
          <div class="card-topline">
            <div>
              <p class="eyebrow">
                Platform
              </p>
              <h3>
                Platform Administration
              </h3>
            </div>

            <CBadge color="dark">
              {{
                auth.me.platform.role
              }}
            </CBadge>
          </div>

          <p>
            Review tenants and enter a tenant
            when operating as SuperAdmin.
          </p>

          <CButton
            color="primary"
            @click="
              router.push('/admin')
            "
          >
            Open administration
          </CButton>
        </CCardBody>
      </CCard>
    </section>

    <section class="section-block">
      <div class="section-heading">
        <div>
          <h2>Your organisations</h2>
          <p>
            Tenant memberships available to
            your account.
          </p>
        </div>
      </div>

      <div
        v-if="auth.me?.tenants.length"
        class="workspace-grid"
      >
        <CCard
          v-for="tenant in auth.me.tenants"
          :key="tenant.tenantId"
          class="workspace-card"
        >
          <CCardBody>
            <div class="card-topline">
              <div>
                <p class="eyebrow">
                  Organisation
                </p>
                <h3>
                  {{ tenant.name }}
                </h3>
              </div>

              <CBadge
                :color="
                  tenant.status === 'Active'
                    ? 'success'
                    : 'secondary'
                "
              >
                {{ tenant.status }}
              </CBadge>
            </div>

            <div class="badge-row">
              <CBadge
                v-for="role in tenant.roles"
                :key="role"
                color="info"
              >
                {{ role }}
              </CBadge>

              <CBadge
                v-for="warehouse in tenant.warehouses"
                :key="
                  warehouse.warehouseId
                "
                color="secondary"
              >
                {{ warehouse.role }}
              </CBadge>
            </div>

            <CButton
              color="primary"
              @click="openTenant(tenant)"
            >
              Open CrateSync
            </CButton>
          </CCardBody>
        </CCard>
      </div>

      <CCard
        v-else
        class="empty-state"
      >
        <CCardBody>
          <h3>
            No organisation memberships
          </h3>
          <p>
            Your account currently has no
            tenant memberships.
          </p>
        </CCardBody>
      </CCard>
    </section>
  </section>
</template>
