<script setup lang="ts">
import {
  computed,
  onMounted,
  reactive,
  ref,
} from 'vue'
import {
  CBadge,
  CButton,
  CCard,
  CCardBody,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
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

interface AdministratorProvisionResult {
  userId: string
  createdNewIdentity: boolean
  requiresActivation: boolean
  activationUrl: string | null
}

const auth = useAuthStore()
const tenants = ref<TenantSummary[]>([])
const loading = ref(true)
const saving = ref(false)
const error = ref<string | null>(null)
const success = ref<string | null>(null)
const search = ref('')
const createModalVisible = ref(false)
const adminModalVisible = ref(false)
const activationModalVisible = ref(false)
const selectedTenant = ref<TenantSummary | null>(null)
const editingTenantId = ref<string | null>(null)
const renameValue = ref('')
const activationUrl = ref('')

const createForm = reactive({
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

const filteredTenants = computed(() => {
  const term = search.value.trim().toLowerCase()

  if (!term)
    return tenants.value

  return tenants.value.filter((tenant) =>
    tenant.name.toLowerCase().includes(term) ||
    tenant.id.toLowerCase().includes(term) ||
    tenant.status.toLowerCase().includes(term),
  )
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
        : 'Unable to load organisations.'
  } finally {
    loading.value = false
  }
}

onMounted(loadTenants)

function openCreateModal() {
  createForm.tenantName = ''
  createForm.firstName = ''
  createForm.lastName = ''
  createForm.email = ''
  createForm.userName = ''
  createModalVisible.value = true
}

function closeCreateModal() {
  if (!saving.value)
    createModalVisible.value = false
}

function openAdminModal(
  tenant: TenantSummary,
) {
  selectedTenant.value = tenant
  adminForm.firstName = ''
  adminForm.lastName = ''
  adminForm.email = ''
  adminForm.userName = ''
  adminModalVisible.value = true
}

function closeAdminModal() {
  if (!saving.value) {
    adminModalVisible.value = false
    selectedTenant.value = null
  }
}

function showActivationLink(
  result: AdministratorProvisionResult,
) {
  if (
    result.requiresActivation &&
    result.activationUrl
  ) {
    activationUrl.value =
      result.activationUrl
    activationModalVisible.value = true
  }
}

function selectActivationLink(
  event: FocusEvent,
) {
  const target =
    event.target as HTMLTextAreaElement

  target.select()
}

async function copyActivationLink() {
  if (!activationUrl.value)
    return

  try {
    await navigator.clipboard.writeText(
      activationUrl.value,
    )
    success.value =
      'Activation link copied to clipboard.'
  } catch {
    error.value =
      'Unable to copy automatically. Select and copy the activation link manually.'
  }
}

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
            name: createForm.tenantName,
          }),
        },
      )

    const administrator =
      await apiFetch<AdministratorProvisionResult>(
        `/api/tenants/${tenantId}/administrators`,
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            email: createForm.email,
            userName:
              createForm.userName.trim() ||
              null,
            firstName: createForm.firstName,
            lastName: createForm.lastName,
          }),
        },
      )

    createModalVisible.value = false
    success.value =
      'Organisation and initial administrator created.'

    showActivationLink(administrator)
    await loadTenants()
  } catch (exception) {
    const message =
      exception instanceof Error
        ? exception.message
        : 'Unable to create organisation.'

    error.value =
      tenantId
        ? `Organisation was created, but the initial administrator could not be provisioned: ${message}`
        : message

    if (tenantId)
      await loadTenants()
  } finally {
    saving.value = false
  }
}

async function provisionAdministrator() {
  if (!selectedTenant.value)
    return

  error.value = null
  success.value = null
  saving.value = true

  try {
    const result =
      await apiFetch<AdministratorProvisionResult>(
        `/api/tenants/${selectedTenant.value.id}/administrators`,
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            email: adminForm.email,
            userName:
              adminForm.userName.trim() ||
              null,
            firstName: adminForm.firstName,
            lastName: adminForm.lastName,
          }),
        },
      )

    adminModalVisible.value = false
    selectedTenant.value = null
    success.value =
      'Administrator added successfully.'

    showActivationLink(result)
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to add TenantAdmin.'
  } finally {
    saving.value = false
  }
}

function startRename(
  tenant: TenantSummary,
) {
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
      'Organisation renamed successfully.'
    cancelRename()
    await loadTenants()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to rename organisation.'
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
      `Deactivate ${tenant.name}? Organisation operations will be blocked until it is activated again.`,
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
        ? 'Organisation activated.'
        : 'Organisation deactivated.'

    await loadTenants()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update organisation status.'
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
          Organisation management
        </h1>
        <p>
          Find and manage CrateSync tenant
          organisations.
        </p>
      </div>

      <CButton
        color="primary"
        @click="openCreateModal"
      >
        <i
          class="button-icon cil-plus"
          aria-hidden="true"
        />
        New organisation
      </CButton>
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

    <section class="section-block">
      <div class="tenant-toolbar">
        <div>
          <h2>Organisations</h2>
          <p>
            {{
              filteredTenants.length
            }}
            of
            {{ tenants.length }}
            tenants
          </p>
        </div>

        <div class="search-field">
          <i
            class="cil-search"
            aria-hidden="true"
          />
          <input
          v-model="search"
          class="form-control tenant-search"
          type="search"
          placeholder="Search organisations..."
          aria-label="Search organisations"
          >
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
        v-else-if="filteredTenants.length === 0"
        class="empty-state"
      >
        <CCardBody>
          <h3>
            {{
              search
                ? 'No matching organisations'
                : 'No organisations yet'
            }}
          </h3>
          <p>
            {{
              search
                ? 'Try another organisation name, ID or status.'
                : 'Create the first organisation to get started.'
            }}
          </p>
        </CCardBody>
      </CCard>

      <div
        v-else
        class="tenant-grid"
      >
        <CCard
          v-for="tenant in filteredTenants"
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

              <template v-else>
                <CButton
                  v-if="tenant.status === 'Active'"
                  color="warning"
                  size="sm"
                  @click="enterTenant(tenant)"
                >
                  <i
                    class="button-icon cil-arrow-right"
                    aria-hidden="true"
                  />
                  Enter
                </CButton>

                <CButton
                  color="primary"
                  variant="outline"
                  size="sm"
                  @click="openAdminModal(tenant)"
                >
                  Add TenantAdmin
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  size="sm"
                  @click="startRename(tenant)"
                >
                  <i
                    class="button-icon cil-pencil"
                    aria-hidden="true"
                  />
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
                  <i
                    class="button-icon cil-ban"
                    aria-hidden="true"
                  />
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
                  <i
                    class="button-icon cil-check-circle"
                    aria-hidden="true"
                  />
                  Activate
                </CButton>
              </template>
            </div>
          </CCardBody>
        </CCard>
      </div>
    </section>

    <CModal
      :visible="createModalVisible"
      alignment="center"
      size="lg"
      @close="closeCreateModal"
    >
      <CModalHeader>
        <CModalTitle>
          New tenant
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <p class="modal-help">
          Create the organisation and its
          first TenantAdmin.
        </p>

        <form
          id="create-tenant-form"
          class="admin-form"
          @submit.prevent="createTenant"
        >
          <label class="form-field form-field-wide">
            <span>Business name</span>
            <input
              v-model.trim="createForm.tenantName"
              class="form-control"
              required
              maxlength="200"
              placeholder="FruitVibe India"
            >
          </label>

          <label class="form-field">
            <span>Admin first name</span>
            <input
              v-model.trim="createForm.firstName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Admin last name</span>
            <input
              v-model.trim="createForm.lastName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Admin email</span>
            <input
              v-model.trim="createForm.email"
              class="form-control"
              type="email"
              required
              maxlength="256"
            >
          </label>

          <label class="form-field">
            <span>Username (optional)</span>
            <input
              v-model.trim="createForm.userName"
              class="form-control"
              maxlength="256"
              placeholder="Defaults to email"
            >
          </label>
        </form>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="closeCreateModal"
        >
          Cancel
        </CButton>

        <CButton
          color="primary"
          type="submit"
          form="create-tenant-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Creating…'
              : 'Create organisation'
          }}
        </CButton>
      </CModalFooter>
    </CModal>

    <CModal
      :visible="adminModalVisible"
      alignment="center"
      @close="closeAdminModal"
    >
      <CModalHeader>
        <CModalTitle>
          Add TenantAdmin
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <p class="modal-help">
          {{
            selectedTenant
              ? `Add an administrator to ${selectedTenant.name}.`
              : 'Add a tenant administrator.'
          }}
        </p>

        <form
          id="add-admin-form"
          class="admin-form admin-form-single"
          @submit.prevent="provisionAdministrator"
        >
          <label class="form-field">
            <span>First name</span>
            <input
              v-model.trim="adminForm.firstName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Last name</span>
            <input
              v-model.trim="adminForm.lastName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Email</span>
            <input
              v-model.trim="adminForm.email"
              class="form-control"
              type="email"
              required
              maxlength="256"
            >
          </label>

          <label class="form-field">
            <span>Username (optional)</span>
            <input
              v-model.trim="adminForm.userName"
              class="form-control"
              maxlength="256"
              placeholder="Defaults to email"
            >
          </label>
        </form>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="closeAdminModal"
        >
          Cancel
        </CButton>

        <CButton
          color="primary"
          type="submit"
          form="add-admin-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Adding…'
              : 'Add administrator'
          }}
        </CButton>
      </CModalFooter>
    </CModal>

    <CModal
      :visible="activationModalVisible"
      alignment="center"
      @close="
        activationModalVisible = false
      "
    >
      <CModalHeader>
        <CModalTitle>
          Activation link
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <p class="modal-help">
          Send this link manually to the new
          administrator. They will use it to set
          their password and activate the
          account.
        </p>

        <label class="form-field">
          <span>Activation URL</span>
          <textarea
            class="form-control activation-link"
            :value="activationUrl"
            readonly
            rows="4"
            @focus="selectActivationLink"
          />
        </label>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          @click="
            activationModalVisible = false
          "
        >
          Close
        </CButton>

        <CButton
          color="primary"
          @click="copyActivationLink"
        >
          <i
            class="button-icon cil-copy"
            aria-hidden="true"
          />
          Copy activation link
        </CButton>
      </CModalFooter>
    </CModal>
  </section>
</template>
