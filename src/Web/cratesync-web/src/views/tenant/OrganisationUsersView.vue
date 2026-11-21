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

interface WarehouseSummary {
  id: string
  name: string
  code: string
  city: string
  country: string
  isActive: boolean
}

interface WarehouseAccess {
  warehouseId: string
  role: 'WarehouseAdmin' | 'WarehouseUser'
}

interface OrganisationUser {
  userId: string
  userName: string
  email: string | null
  firstName: string
  lastName: string
  isActive: boolean
  requiresActivation: boolean
  roles: string[]
  warehouses: WarehouseAccess[]
}

interface ProvisionResult {
  userId: string
  createdNewIdentity: boolean
  requiresActivation: boolean
  activationUrl: string | null
}

const auth = useAuthStore()
const users = ref<OrganisationUser[]>([])
const warehouses = ref<WarehouseSummary[]>([])
const loading = ref(true)
const saving = ref(false)
const search = ref('')
const error = ref<string | null>(null)
const success = ref<string | null>(null)
const addModalError = ref<string | null>(null)
const editModalError = ref<string | null>(null)
const activationModalError = ref<string | null>(null)
const addModalVisible = ref(false)
const editModalVisible = ref(false)
const activationModalVisible = ref(false)
const selectedUser = ref<OrganisationUser | null>(null)
const activationUrl = ref('')

const addForm = reactive({
  firstName: '',
  lastName: '',
  email: '',
  userName: '',
  tenantAdmin: false,
  viewer: false,
  warehouseRoles: {} as Record<string, string>,
})

const editForm = reactive({
  tenantAdmin: false,
  viewer: false,
  warehouseRoles: {} as Record<string, string>,
})

const filteredUsers = computed(() => {
  const term = search.value.trim().toLowerCase()

  if (!term)
    return users.value

  return users.value.filter((user) => {
    const fullName =
      `${user.firstName} ${user.lastName}`
        .toLowerCase()

    return (
      fullName.includes(term) ||
      user.userName.toLowerCase().includes(term) ||
      user.email?.toLowerCase().includes(term) === true ||
      displayRoles(user).toLowerCase().includes(term)
    )
  })
})

const currentUserId = computed(
  () => auth.me?.userId ?? null,
)

async function load() {
  error.value = null
  loading.value = true

  try {
    const [userResult, warehouseResult] =
      await Promise.all([
        apiFetch<OrganisationUser[]>(
          '/api/tenant/users',
        ),
        apiFetch<WarehouseSummary[]>(
          '/api/Warehouse?isActive=true',
        ),
      ])

    users.value = userResult
    warehouses.value = warehouseResult
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load users.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

function displayRole(role: string) {
  switch (role) {
    case 'TenantAdmin':
      return 'Administrator'
    case 'Viewer':
      return 'Viewer'
    case 'WarehouseAdmin':
      return 'Warehouse administrator'
    case 'WarehouseUser':
      return 'Warehouse user'
    default:
      return role
  }
}

function displayRoles(user: OrganisationUser) {
  const roles = user.roles.map(displayRole)

  if (user.warehouses.length > 0)
    roles.push('Warehouse access')

  return roles.length > 0
    ? roles.join(', ')
    : 'No access'
}

function warehouseName(id: string) {
  const warehouse =
    warehouses.value.find((item) => item.id === id)

  return warehouse
    ? `${warehouse.name} (${warehouse.code})`
    : id
}

function resetWarehouseRoles(
  target: Record<string, string>,
) {
  for (const key of Object.keys(target))
    delete target[key]

  for (const warehouse of warehouses.value)
    target[warehouse.id] = ''
}

function openAddModal() {
  addModalError.value = null
  addForm.firstName = ''
  addForm.lastName = ''
  addForm.email = ''
  addForm.userName = ''
  addForm.tenantAdmin = false
  addForm.viewer = false
  resetWarehouseRoles(addForm.warehouseRoles)
  addModalVisible.value = true
}

function closeAddModal() {
  if (!saving.value) {
    addModalError.value = null
    addModalVisible.value = false
  }
}

function openEditModal(user: OrganisationUser) {
  editModalError.value = null
  selectedUser.value = user
  editForm.tenantAdmin =
    user.roles.includes('TenantAdmin')
  editForm.viewer =
    user.roles.includes('Viewer')
  resetWarehouseRoles(editForm.warehouseRoles)

  for (const access of user.warehouses) {
    editForm.warehouseRoles[access.warehouseId] =
      access.role
  }

  editModalVisible.value = true
}

function closeEditModal() {
  if (!saving.value) {
    editModalError.value = null
    editModalVisible.value = false
    selectedUser.value = null
  }
}

function buildRoles(
  tenantAdmin: boolean,
  viewer: boolean,
) {
  const roles: string[] = []

  if (tenantAdmin)
    roles.push('TenantAdmin')

  if (viewer)
    roles.push('Viewer')

  return roles
}

function buildWarehouses(
  values: Record<string, string>,
): WarehouseAccess[] {
  return Object.entries(values)
    .filter(([, role]) => role)
    .map(([warehouseId, role]) => ({
      warehouseId,
      role:
        role as WarehouseAccess['role'],
    }))
}

function hasAccess(
  tenantAdmin: boolean,
  viewer: boolean,
  values: Record<string, string>,
) {
  return (
    tenantAdmin ||
    viewer ||
    Object.values(values).some(Boolean)
  )
}

function showActivation(
  result: ProvisionResult,
) {
  if (
    result.requiresActivation &&
    result.activationUrl
  ) {
    activationModalError.value = null
    activationUrl.value =
      result.activationUrl
    activationModalVisible.value = true
  }
}

async function createUser() {
  if (
    !hasAccess(
      addForm.tenantAdmin,
      addForm.viewer,
      addForm.warehouseRoles,
    )
  ) {
    addModalError.value =
      'Assign at least one organisation role or warehouse.'
    return
  }

  saving.value = true
  addModalError.value = null
  error.value = null
  success.value = null

  try {
    const result =
      await apiFetch<ProvisionResult>(
        '/api/tenant/users',
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            email: addForm.email,
            userName:
              addForm.userName.trim() || null,
            firstName: addForm.firstName,
            lastName: addForm.lastName,
            roles: buildRoles(
              addForm.tenantAdmin,
              addForm.viewer,
            ),
            warehouses: buildWarehouses(
              addForm.warehouseRoles,
            ),
          }),
        },
      )

    addModalVisible.value = false
    success.value = 'User added successfully.'
    showActivation(result)
    await load()
  } catch (exception) {
    addModalError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to add user.'
  } finally {
    saving.value = false
  }
}

async function updateAccess() {
  if (!selectedUser.value)
    return

  if (
    !hasAccess(
      editForm.tenantAdmin,
      editForm.viewer,
      editForm.warehouseRoles,
    )
  ) {
    editModalError.value =
      'Assign at least one organisation role or warehouse.'
    return
  }

  saving.value = true
  editModalError.value = null
  error.value = null
  success.value = null

  try {
    await apiFetch(
      `/api/tenant/users/${selectedUser.value.userId}/access`,
      {
        method: 'PUT',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify({
          roles: buildRoles(
            editForm.tenantAdmin,
            editForm.viewer,
          ),
          warehouses: buildWarehouses(
            editForm.warehouseRoles,
          ),
        }),
      },
    )

    editModalVisible.value = false
    selectedUser.value = null
    success.value =
      'User access updated successfully.'
    await load()
  } catch (exception) {
    editModalError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update user access.'
  } finally {
    saving.value = false
  }
}

async function setUserActive(
  user: OrganisationUser,
  active: boolean,
) {
  if (
    !active &&
    !window.confirm(
      `Deactivate ${user.firstName} ${user.lastName}? They will no longer be able to access this organisation.`,
    )
  ) {
    return
  }

  error.value = null
  success.value = null

  try {
    await apiFetch(
      `/api/tenant/users/${user.userId}/${active ? 'activate' : 'deactivate'}`,
      {
        method: 'PATCH',
      },
    )

    success.value =
      active
        ? 'User activated.'
        : 'User deactivated.'

    await load()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update user status.'
  }
}

async function generateActivationLink(
  user: OrganisationUser,
) {
  error.value = null

  try {
    const result =
      await apiFetch<{
        activationUrl: string
      }>(
        `/api/tenant/users/${user.userId}/activation-link`,
        {
          method: 'POST',
        },
      )

    activationUrl.value =
      result.activationUrl
    activationModalVisible.value = true
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to generate activation link.'
  }
}

function closeActivationModal() {
  activationModalError.value = null
  activationModalVisible.value = false
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
    activationModalError.value =
      'Unable to copy automatically. Select and copy the link manually.'
  }
}
</script>

<template>
  <section class="page-stack">
    <div class="page-heading">
      <div>
        <p class="eyebrow">
          Organisation administration
        </p>
        <h1>Users</h1>
        <p>
          Manage organisation roles,
          warehouse access and account status.
        </p>
      </div>

      <CButton
        color="primary"
        @click="openAddModal"
      >
        <i
          class="button-icon cil-user-plus"
          aria-hidden="true"
        />
        Add user
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

    <div class="tenant-toolbar">
      <div>
        <h2>Organisation users</h2>
        <p>
          {{ filteredUsers.length }}
          of
          {{ users.length }}
          users
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
          placeholder="Search users..."
          aria-label="Search users"
        >
      </div>
    </div>

    <div
      v-if="loading"
      class="loading-state"
    >
      <CSpinner />
      Loading users…
    </div>

    <CCard
      v-else-if="filteredUsers.length === 0"
      class="empty-state"
    >
      <CCardBody>
        <h3>
          {{
            search
              ? 'No matching users'
              : 'No users yet'
          }}
        </h3>
        <p>
          {{
            search
              ? 'Try another name, email or role.'
              : 'Add the first organisation user.'
          }}
        </p>
      </CCardBody>
    </CCard>

    <div
      v-else
      class="user-grid"
    >
      <CCard
        v-for="user in filteredUsers"
        :key="user.userId"
        class="workspace-card user-card"
      >
        <CCardBody>
          <div class="card-topline">
            <div class="user-card-heading">
              <p class="eyebrow">User</p>
              <h3>
                {{ user.firstName }}
                {{ user.lastName }}
              </h3>
              <small>
                {{ user.email ?? user.userName }}
              </small>
            </div>

            <CBadge
              :color="
                user.isActive
                  ? 'success'
                  : 'secondary'
              "
            >
              {{
                user.isActive
                  ? 'Active'
                  : 'Inactive'
              }}
            </CBadge>
          </div>

          <div class="badge-row user-badges">
            <CBadge
              v-for="role in user.roles"
              :key="role"
              color="info"
            >
              {{ displayRole(role) }}
            </CBadge>

            <CBadge
              v-if="user.requiresActivation"
              color="warning"
            >
              Activation required
            </CBadge>
          </div>

          <div
            v-if="user.warehouses.length"
            class="warehouse-access-list"
          >
            <strong>Warehouse access</strong>

            <span
              v-for="access in user.warehouses"
              :key="access.warehouseId"
            >
              {{ warehouseName(access.warehouseId) }}
              ·
              {{ displayRole(access.role) }}
            </span>
          </div>

          <div class="tenant-actions">
            <CButton
              color="primary"
              variant="outline"
              size="sm"
              @click="openEditModal(user)"
            >
              <i
                class="button-icon cil-pencil"
                aria-hidden="true"
              />
              Edit access
            </CButton>

            <CButton
              v-if="user.requiresActivation"
              color="warning"
              variant="outline"
              size="sm"
              @click="generateActivationLink(user)"
            >
              <i
                class="button-icon cil-link"
                aria-hidden="true"
              />
              Activation link
            </CButton>

            <CButton
              v-if="
                user.isActive &&
                user.userId !== currentUserId
              "
              color="danger"
              variant="outline"
              size="sm"
              @click="setUserActive(user, false)"
            >
              <i
                class="button-icon cil-ban"
                aria-hidden="true"
              />
              Deactivate
            </CButton>

            <CButton
              v-else-if="!user.isActive"
              color="success"
              variant="outline"
              size="sm"
              @click="setUserActive(user, true)"
            >
              <i
                class="button-icon cil-check-circle"
                aria-hidden="true"
              />
              Activate
            </CButton>
          </div>
        </CCardBody>
      </CCard>
    </div>

    <CModal
      :visible="addModalVisible"
      alignment="center"
      size="lg"
      @close="closeAddModal"
    >
      <CModalHeader>
        <CModalTitle>Add user</CModalTitle>
      </CModalHeader>

      <CModalBody>
        <div
          v-if="addModalError"
          class="status-message status-error modal-status-message"
        >
          {{ addModalError }}
        </div>

        <form
          id="add-user-form"
          class="admin-form"
          @submit.prevent="createUser"
        >
          <label class="form-field">
            <span>First name</span>
            <input
              v-model.trim="addForm.firstName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Last name</span>
            <input
              v-model.trim="addForm.lastName"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Email</span>
            <input
              v-model.trim="addForm.email"
              class="form-control"
              type="email"
              required
            >
          </label>

          <label class="form-field">
            <span>Username (optional)</span>
            <input
              v-model.trim="addForm.userName"
              class="form-control"
              placeholder="Defaults to email"
            >
          </label>

          <div class="access-editor form-field-wide">
            <strong>Organisation roles</strong>

            <label class="check-row">
              <input
                v-model="addForm.tenantAdmin"
                type="checkbox"
              >
              Administrator
            </label>

            <label class="check-row">
              <input
                v-model="addForm.viewer"
                type="checkbox"
              >
              Viewer
            </label>
          </div>

          <div class="access-editor form-field-wide">
            <strong>Warehouse access</strong>

            <p v-if="warehouses.length === 0">
              No active warehouses are available.
            </p>

            <div
              v-for="warehouse in warehouses"
              :key="warehouse.id"
              class="warehouse-role-row"
            >
              <div>
                <strong>{{ warehouse.name }}</strong>
                <small>
                  {{ warehouse.code }}
                  ·
                  {{ warehouse.city }}
                </small>
              </div>

              <select
                v-model="
                  addForm.warehouseRoles[warehouse.id]
                "
                class="form-select"
              >
                <option value="">
                  No access
                </option>
                <option value="WarehouseUser">
                  Warehouse user
                </option>
                <option value="WarehouseAdmin">
                  Warehouse administrator
                </option>
              </select>
            </div>
          </div>
        </form>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="closeAddModal"
        >
          Cancel
        </CButton>

        <CButton
          color="primary"
          type="submit"
          form="add-user-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Adding…'
              : 'Add user'
          }}
        </CButton>
      </CModalFooter>
    </CModal>

    <CModal
      :visible="editModalVisible"
      alignment="center"
      size="lg"
      @close="closeEditModal"
    >
      <CModalHeader>
        <CModalTitle>
          Edit user access
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <div
          v-if="editModalError"
          class="status-message status-error modal-status-message"
        >
          {{ editModalError }}
        </div>

        <p
          v-if="selectedUser"
          class="modal-help"
        >
          {{ selectedUser.firstName }}
          {{ selectedUser.lastName }}
        </p>

        <form
          id="edit-user-form"
          class="admin-form admin-form-single"
          @submit.prevent="updateAccess"
        >
          <div class="access-editor">
            <strong>Organisation roles</strong>

            <label class="check-row">
              <input
                v-model="editForm.tenantAdmin"
                type="checkbox"
                :disabled="
                  selectedUser?.userId ===
                    currentUserId &&
                  editForm.tenantAdmin
                "
              >
              Administrator
            </label>

            <label class="check-row">
              <input
                v-model="editForm.viewer"
                type="checkbox"
              >
              Viewer
            </label>
          </div>

          <div class="access-editor">
            <strong>Warehouse access</strong>

            <p v-if="warehouses.length === 0">
              No active warehouses are available.
            </p>

            <div
              v-for="warehouse in warehouses"
              :key="warehouse.id"
              class="warehouse-role-row"
            >
              <div>
                <strong>{{ warehouse.name }}</strong>
                <small>
                  {{ warehouse.code }}
                  ·
                  {{ warehouse.city }}
                </small>
              </div>

              <select
                v-model="
                  editForm.warehouseRoles[warehouse.id]
                "
                class="form-select"
              >
                <option value="">
                  No access
                </option>
                <option value="WarehouseUser">
                  Warehouse user
                </option>
                <option value="WarehouseAdmin">
                  Warehouse administrator
                </option>
              </select>
            </div>
          </div>
        </form>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="closeEditModal"
        >
          Cancel
        </CButton>

        <CButton
          color="primary"
          type="submit"
          form="edit-user-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Saving…'
              : 'Save access'
          }}
        </CButton>
      </CModalFooter>
    </CModal>

    <CModal
      :visible="activationModalVisible"
      alignment="center"
      @close="closeActivationModal"
    >
      <CModalHeader>
        <CModalTitle>
          Activation link
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <div
          v-if="activationModalError"
          class="status-message status-error modal-status-message"
        >
          {{ activationModalError }}
        </div>

        <p class="modal-help">
          Send this link manually to the user.
          They can use it to set their password
          and activate the account.
        </p>

        <textarea
          class="form-control activation-link"
          :value="activationUrl"
          readonly
          rows="4"
        />
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          @click="closeActivationModal"
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
