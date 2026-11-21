<script setup lang="ts">
import {
  onMounted,
  reactive,
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
const saving = ref(false)
const error = ref<string | null>(null)
const success = ref<string | null>(null)
const editingTenantId = ref<string | null>(null)
const provisioningTenantId = ref<string | null>(null)
const renameValue = ref('')

const form = reactive({
  tenantName: '',
  firstName: '',
  lastName: '',
  email: '',
  userName: '',
})

const adminForm = reactive({
  firstName: '',
  lastName: '',
  email: '',
  userName: '',
})

async function loadTenants() {
  error.value = null

  try {
    tenants.value =
      await apiFetch<TenantSummary[]>(
        '/api/tenants',
      )
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load tenants.'
  } finally {
    loading.value = false
  }
}

onMounted(loadTenants)

async function createTenant() {
  error.value = null
  success.value = null
  saving.value = true

  let tenantId: string | null = null

  try {
    tenantId =
      await apiFetch<string>(
        '/api/tenants',
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            name: form.tenantName,
          }),
        },
      )

    const administrator =
      await apiFetch<{
        requiresActivation: boolean
      }>(
      `/api/tenants/${tenantId}/administrators`,
      {
        method: 'POST',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify({
          email: form.email,
          userName:
            form.userName.trim() || null,
          firstName: form.firstName,
          lastName: form.lastName,
        }),
      },
    )

    success.value =
      administrator.requiresActivation
        ? 'Tenant and initial TenantAdmin created. The administrator must complete account activation before first login.'
        : 'Tenant and initial TenantAdmin created successfully.'

    form.tenantName = ''
    form.firstName = ''
    form.lastName = ''
    form.email = ''
    form.userName = ''

    await loadTenants()
  } catch (exception) {
    const message =
      exception instanceof Error
        ? exception.message
        : 'Unable to create tenant.'

    error.value =
      tenantId
        ? `Tenant was created, but the initial administrator could not be provisioned: ${message}`
        : message

    if (tenantId)
      await loadTenants()
  } finally {
    saving.value = false
  }
}

function startAdministrator(
  tenant: TenantSummary,
) {
  provisioningTenantId.value = tenant.id
  adminForm.firstName = ''
  adminForm.lastName = ''
  adminForm.email = ''
  adminForm.userName = ''
}

function cancelAdministrator() {
  provisioningTenantId.value = null
}

async function provisionAdministrator(
  tenant: TenantSummary,
) {
  error.value = null
  success.value = null

  try {
    const result =
      await apiFetch<{
        requiresActivation: boolean
      }>(
        `/api/tenants/${tenant.id}/administrators`,
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            email: adminForm.email,
            userName:
              adminForm.userName.trim() || null,
            firstName: adminForm.firstName,
            lastName: adminForm.lastName,
          }),
        },
      )

    success.value =
      result.requiresActivation
        ? 'TenantAdmin added. Account activation is required before first login.'
        : 'TenantAdmin added successfully.'

    cancelAdministrator()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to add TenantAdmin.'
  }
}

function startRename(tenant: TenantSummary) {
  editingTenantId.value = tenant.id
  renameValue.value = tenant.name
}

function cancelRename() {
  editingTenantId.value = null
  renameValue.value = ''
}

async function renameTenant(
  tenant: TenantSummary,
) {
  error.value = null
  success.value = null

  try {
    await apiFetch(
      `/api/tenants/${tenant.id}`,
      {
        method: 'PUT',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify({
          name: renameValue.value,
        }),
      },
    )

    success.value =
      'Tenant renamed successfully.'
    cancelRename()
    await loadTenants()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to rename tenant.'
  }
}

async function setTenantActive(
  tenant: TenantSummary,
  active: boolean,
) {
  error.value = null
  success.value = null

  if (
    !active &&
    !window.confirm(
      `Deactivate ${tenant.name}? Tenant operations will be blocked until it is activated again.`,
    )
  ) {
    return
  }

  try {
    await apiFetch(
      `/api/tenants/${tenant.id}/${active ? 'activate' : 'deactivate'}`,
      {
        method: 'PATCH',
      },
    )

    success.value =
      active
        ? 'Tenant activated.'
        : 'Tenant deactivated.'

    await loadTenants()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update tenant status.'
  }
}

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
          Tenant management
        </h1>
        <p>
          Create and manage CrateSync tenant
          organisations.
        </p>
      </div>
    </div>

    <div
      v-if="error"
      class="status-message status-error"
    >
      {{ error }}
    </div>

    <div
      v-if="success"
      class="status-message status-success"
    >
      {{ success }}
    </div>

    <CCard class="management-card">
      <CCardBody>
        <div class="section-heading">
          <h2>Onboard a new tenant</h2>
          <p>
            Create the organisation and its
            first TenantAdmin.
          </p>
        </div>

        <form
          class="admin-form"
          @submit.prevent="createTenant"
        >
          <label class="form-field form-field-wide">
            <span>Business name</span>
            <input
              v-model.trim="form.tenantName"
              class="form-control"
              required
              maxlength="200"
              placeholder="FruitVibe India"
            >
          </label>

          <label class="form-field">
            <span>Admin first name</span>
            <input
              v-model.trim="form.firstName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Admin last name</span>
            <input
              v-model.trim="form.lastName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Admin email</span>
            <input
              v-model.trim="form.email"
              class="form-control"
              type="email"
              required
              maxlength="256"
            >
          </label>

          <label class="form-field">
            <span>Username (optional)</span>
            <input
              v-model.trim="form.userName"
              class="form-control"
              maxlength="256"
              placeholder="Defaults to email"
            >
          </label>

          <div class="form-actions form-field-wide">
            <CButton
              color="primary"
              type="submit"
              :disabled="saving"
            >
              {{
                saving
                  ? 'Creating…'
                  : 'Create tenant'
              }}
            </CButton>
          </div>
        </form>
      </CCardBody>
    </CCard>

    <section class="section-block">
      <div class="section-heading">
        <h2>Tenants</h2>
        <p>
          Enter, rename, activate or deactivate
          tenant organisations.
        </p>
      </div>

      <div
        v-if="loading"
        class="loading-state"
      >
        <CSpinner />
        Loading tenants…
      </div>

      <CCard
        v-else-if="tenants.length === 0"
        class="empty-state"
      >
        <CCardBody>
          <h3>No tenants yet</h3>
          <p>
            Create the first tenant above.
          </p>
        </CCardBody>
      </CCard>

      <div
        v-else
        class="workspace-grid"
      >
        <CCard
          v-for="tenant in tenants"
          :key="tenant.id"
          class="workspace-card tenant-card"
        >
          <CCardBody>
            <div class="card-topline">
              <div class="tenant-card-title">
                <p class="eyebrow">
                  Tenant
                </p>

                <template
                  v-if="
                    editingTenantId === tenant.id
                  "
                >
                  <input
                    v-model.trim="renameValue"
                    class="form-control"
                    maxlength="200"
                    @keyup.enter="
                      renameTenant(tenant)
                    "
                    @keyup.esc="cancelRename"
                  >
                </template>

                <h3 v-else>
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

            <p class="muted-id">
              {{ tenant.id }}
            </p>

            <div class="tenant-actions">
              <template
                v-if="
                  editingTenantId === tenant.id
                "
              >
                <CButton
                  color="primary"
                  size="sm"
                  @click="renameTenant(tenant)"
                >
                  Save
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  size="sm"
                  @click="cancelRename"
                >
                  Cancel
                </CButton>
              </template>

              <template
                v-else-if="
                  provisioningTenantId !==
                  tenant.id
                "
              >
                <CButton
                  v-if="tenant.status === 'Active'"
                  color="warning"
                  size="sm"
                  @click="enterTenant(tenant)"
                >
                  Enter tenant
                </CButton>

                <CButton
                  color="primary"
                  variant="outline"
                  size="sm"
                  @click="
                    startAdministrator(tenant)
                  "
                >
                  Add TenantAdmin
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  size="sm"
                  @click="startRename(tenant)"
                >
                  Rename
                </CButton>

                <CButton
                  v-if="tenant.status === 'Active'"
                  color="danger"
                  variant="outline"
                  size="sm"
                  @click="
                    setTenantActive(
                      tenant,
                      false,
                    )
                  "
                >
                  Deactivate
                </CButton>

                <CButton
                  v-else
                  color="success"
                  variant="outline"
                  size="sm"
                  @click="
                    setTenantActive(
                      tenant,
                      true,
                    )
                  "
                >
                  Activate
                </CButton>
              </template>
            </div>

            <form
              v-if="
                provisioningTenantId ===
                tenant.id
              "
              class="inline-admin-form"
              @submit.prevent="
                provisionAdministrator(tenant)
              "
            >
              <strong>Add TenantAdmin</strong>

              <div class="inline-admin-grid">
                <input
                  v-model.trim="adminForm.firstName"
                  class="form-control"
                  required
                  maxlength="100"
                  placeholder="First name"
                  aria-label="First name"
                >

                <input
                  v-model.trim="adminForm.lastName"
                  class="form-control"
                  required
                  maxlength="100"
                  placeholder="Last name"
                  aria-label="Last name"
                >

                <input
                  v-model.trim="adminForm.email"
                  class="form-control"
                  type="email"
                  required
                  maxlength="256"
                  placeholder="Email"
                  aria-label="Email"
                >

                <input
                  v-model.trim="adminForm.userName"
                  class="form-control"
                  maxlength="256"
                  placeholder="Username (optional)"
                  aria-label="Username"
                >
              </div>

              <div class="tenant-actions">
                <CButton
                  color="primary"
                  size="sm"
                  type="submit"
                >
                  Add administrator
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  size="sm"
                  type="button"
                  @click="cancelAdministrator"
                >
                  Cancel
                </CButton>
              </div>
            </form>
          </CCardBody>
        </CCard>
      </div>
    </section>
  </section>
</template>
