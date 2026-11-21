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

function openOrganisation(
  organisation: TenantAccess,
) {
  const target =
    `/tenant/${organisation.tenantId}`

  if (
    auth.me?.currentTenant?.tenantId ===
    organisation.tenantId
  ) {
    router.push(target)
    return
  }

  auth.selectTenant(
    organisation.tenantId,
    target,
    false,
  )
}

function displayRole(role: string) {
  switch (role) {
    case 'TenantAdmin':
      return 'Administrator'
    case 'WarehouseAdmin':
      return 'Warehouse administrator'
    case 'WarehouseUser':
      return 'Warehouse user'
    default:
      return role
  }
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
            Manage organisations and
            platform-level administration.
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
            Manage organisations and enter one
            when administrative access is
            required.
          </p>

          <CButton
            color="primary"
            @click="
              router.push('/admin')
            "
          >
            <i
              class="button-icon cil-settings"
              aria-hidden="true"
            />
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
            Organisations available to your
            account.
          </p>
        </div>
      </div>

      <div
        v-if="auth.me?.tenants.length"
        class="workspace-grid"
      >
        <CCard
          v-for="organisation in auth.me.tenants"
          :key="organisation.tenantId"
          class="workspace-card"
        >
          <CCardBody>
            <div class="card-topline">
              <div>
                <p class="eyebrow">
                  Organisation
                </p>
                <h3>
                  {{ organisation.name }}
                </h3>
              </div>

              <CBadge
                :color="
                  organisation.status === 'Active'
                    ? 'success'
                    : 'secondary'
                "
              >
                {{ organisation.status }}
              </CBadge>
            </div>

            <div class="badge-row">
              <CBadge
                v-for="role in organisation.roles"
                :key="role"
                color="info"
              >
                {{ displayRole(role) }}
              </CBadge>

              <CBadge
                v-for="warehouse in organisation.warehouses"
                :key="
                  warehouse.warehouseId
                "
                color="secondary"
              >
                {{ displayRole(warehouse.role) }}
              </CBadge>
            </div>

            <CButton
              color="primary"
              @click="
                openOrganisation(organisation)
              "
            >
              <i
                class="button-icon cil-arrow-right"
                aria-hidden="true"
              />
              Open organisation
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
            No organisations available
          </h3>
          <p>
            Your account is not currently
            assigned to an organisation.
          </p>
        </CCardBody>
      </CCard>
    </section>
  </section>
</template>
