<script setup lang="ts">
import {
  computed,
  onMounted,
  reactive,
  ref,
  watch,
} from 'vue'
import {
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
import {
  createColumnHelper,
  createPaginatedRowModel,
  rowPaginationFeature,
  tableFeatures,
  useTable,
} from '@tanstack/vue-table'
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

interface WarehouseDetails extends WarehouseSummary {
  addressLine1: string
  addressLine2: string | null
  stateOrProvince: string | null
  postalCode: string
  createdAtUtc: string
  updatedAtUtc: string
  deactivatedAtUtc: string | null
}

interface WarehouseForm {
  name: string
  code: string
  addressLine1: string
  addressLine2: string
  city: string
  stateOrProvince: string
  postalCode: string
  country: string
}

const auth = useAuthStore()
const warehouses = ref<WarehouseSummary[]>([])
const loading = ref(true)
const saving = ref(false)
const search = ref('')
const statusFilter = ref('')
const error = ref<string | null>(null)
const success = ref<string | null>(null)
const createModalError = ref<string | null>(null)
const editModalError = ref<string | null>(null)
const createModalVisible = ref(false)
const editModalVisible = ref(false)
const selectedWarehouse = ref<WarehouseDetails | null>(null)

const createForm = reactive<WarehouseForm>({
  name: '',
  code: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  stateOrProvince: '',
  postalCode: '',
  country: '',
})

const editForm = reactive<WarehouseForm>({
  name: '',
  code: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  stateOrProvince: '',
  postalCode: '',
  country: '',
})

const canManageWarehouses = computed(
  () =>
    auth.me?.currentTenant?.accessMode ===
      'SuperAdmin' ||
    auth.me?.currentTenant?.roles.includes('TenantOwner') === true ||
    auth.me?.currentTenant?.roles.includes(
      'TenantAdmin',
    ) === true,
)

const filteredWarehouses = computed(() => {
  const term = search.value.trim().toLowerCase()

  return warehouses.value.filter((warehouse) => {
    const matchesSearch =
      !term ||
      warehouse.name.toLowerCase().includes(term) ||
      warehouse.code.toLowerCase().includes(term) ||
      warehouse.city.toLowerCase().includes(term) ||
      warehouse.country.toLowerCase().includes(term)

    const matchesStatus =
      !statusFilter.value ||
      (statusFilter.value === 'active'
        ? warehouse.isActive
        : !warehouse.isActive)

    return matchesSearch && matchesStatus
  })
})

const warehouseTableFeatures = tableFeatures({
  rowPaginationFeature,
  paginatedRowModel: createPaginatedRowModel(),
})

const warehouseColumnHelper =
  createColumnHelper<
    typeof warehouseTableFeatures,
    WarehouseSummary
  >()

const warehouseColumns =
  warehouseColumnHelper.columns([
    warehouseColumnHelper.accessor('name', {
      header: 'Warehouse',
    }),
    warehouseColumnHelper.accessor('code', {
      header: 'Code',
    }),
    warehouseColumnHelper.accessor('city', {
      header: 'Location',
    }),
    warehouseColumnHelper.accessor(
      'isActive',
      {
        header: 'Status',
      },
    ),
    warehouseColumnHelper.display({
      id: 'actions',
      header: 'Actions',
    }),
  ])

const warehouseTable = useTable({
  key: 'organisation-warehouses',
  features: warehouseTableFeatures,
  data: filteredWarehouses,
  columns: warehouseColumns,
  initialState: {
    pagination: {
      pageIndex: 0,
      pageSize: 10,
    },
  },
})

const pagination = computed(
  () => warehouseTable.atoms.pagination.get(),
)

const pageCount = computed(
  () => warehouseTable.getPageCount(),
)

const firstVisibleWarehouse = computed(() => {
  if (filteredWarehouses.value.length === 0)
    return 0

  return (
    pagination.value.pageIndex *
      pagination.value.pageSize +
    1
  )
})

const lastVisibleWarehouse = computed(() =>
  Math.min(
    filteredWarehouses.value.length,
    (pagination.value.pageIndex + 1) *
      pagination.value.pageSize,
  ),
)

function changePageSize(event: Event) {
  const value = Number(
    (event.target as HTMLSelectElement).value,
  )

  warehouseTable.setPageSize(value)
}

watch([search, statusFilter], () => {
  warehouseTable.setPageIndex(0)
})

async function load() {
  loading.value = true
  error.value = null

  try {
    warehouses.value =
      await apiFetch<WarehouseSummary[]>(
        '/api/Warehouse',
      )
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load warehouses.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

function resetForm(form: WarehouseForm) {
  form.name = ''
  form.code = ''
  form.addressLine1 = ''
  form.addressLine2 = ''
  form.city = ''
  form.stateOrProvince = ''
  form.postalCode = ''
  form.country = ''
}

function copyWarehouseToForm(
  warehouse: WarehouseDetails,
  form: WarehouseForm,
) {
  form.name = warehouse.name
  form.code = warehouse.code
  form.addressLine1 =
    warehouse.addressLine1
  form.addressLine2 =
    warehouse.addressLine2 ?? ''
  form.city = warehouse.city
  form.stateOrProvince =
    warehouse.stateOrProvince ?? ''
  form.postalCode = warehouse.postalCode
  form.country = warehouse.country
}

function openCreateModal() {
  createModalError.value = null
  resetForm(createForm)
  createModalVisible.value = true
}

function closeCreateModal() {
  if (!saving.value) {
    createModalError.value = null
    createModalVisible.value = false
  }
}

async function openEditModal(
  warehouse: WarehouseSummary,
) {
  editModalError.value = null
  error.value = null

  try {
    const details =
      await apiFetch<WarehouseDetails>(
        `/api/Warehouse/${warehouse.id}`,
      )

    selectedWarehouse.value = details
    copyWarehouseToForm(
      details,
      editForm,
    )
    editModalVisible.value = true
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load warehouse details.'
  }
}

function closeEditModal() {
  if (!saving.value) {
    editModalError.value = null
    selectedWarehouse.value = null
    editModalVisible.value = false
  }
}

function requestBody(form: WarehouseForm) {
  return {
    name: form.name.trim(),
    code: form.code.trim(),
    addressLine1:
      form.addressLine1.trim(),
    addressLine2:
      form.addressLine2.trim() || null,
    city: form.city.trim(),
    stateOrProvince:
      form.stateOrProvince.trim() || null,
    postalCode:
      form.postalCode.trim(),
    country: form.country.trim(),
  }
}

async function createWarehouse() {
  saving.value = true
  createModalError.value = null
  success.value = null

  try {
    await apiFetch(
      '/api/Warehouse',
      {
        method: 'POST',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify(
          requestBody(createForm),
        ),
      },
    )

    createModalVisible.value = false
    success.value =
      'Warehouse created successfully.'
    await load()
  } catch (exception) {
    createModalError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to create warehouse.'
  } finally {
    saving.value = false
  }
}

async function updateWarehouse() {
  if (!selectedWarehouse.value)
    return

  saving.value = true
  editModalError.value = null
  success.value = null

  try {
    await apiFetch(
      `/api/Warehouse/${selectedWarehouse.value.id}`,
      {
        method: 'PUT',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify(
          requestBody(editForm),
        ),
      },
    )

    editModalVisible.value = false
    selectedWarehouse.value = null
    success.value =
      'Warehouse updated successfully.'
    await load()
  } catch (exception) {
    editModalError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update warehouse.'
  } finally {
    saving.value = false
  }
}

async function setWarehouseActive(
  warehouse: WarehouseSummary,
  active: boolean,
) {
  if (
    !active &&
    !window.confirm(
      `Deactivate ${warehouse.name}? It will no longer be available for active operations or new user assignments.`,
    )
  ) {
    return
  }

  error.value = null
  success.value = null

  try {
    await apiFetch(
      `/api/Warehouse/${warehouse.id}/${active ? 'activate' : 'deactivate'}`,
      {
        method: 'PATCH',
      },
    )

    success.value =
      active
        ? 'Warehouse activated.'
        : 'Warehouse deactivated.'

    await load()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update warehouse status.'
  }
}
</script>

<template>
  <section class="page-stack users-page">
    <div class="page-heading">
      <div>
        <p class="eyebrow">
          Organisation administration
        </p>
        <h1>Warehouses</h1>
        <p>
          Manage storage locations used by
          inventory, inbound logistics and
          warehouse-scoped users.
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

    <div class="tenant-toolbar users-toolbar">
      <div>
        <h2>Organisation warehouses</h2>
        <p>
          {{ filteredWarehouses.length }}
          of
          {{ warehouses.length }}
          warehouses
        </p>
      </div>

      <div class="users-toolbar-actions">
        <div class="search-field">
          <i
            class="cil-search"
            aria-hidden="true"
          />
          <input
            v-model="search"
            class="form-control tenant-search"
            type="search"
            placeholder="Search warehouses..."
            aria-label="Search warehouses"
          >
        </div>

        <select
          v-model="statusFilter"
          class="form-select user-role-filter"
          aria-label="Filter warehouse status"
        >
          <option value="">
            All statuses
          </option>
          <option value="active">
            Active
          </option>
          <option value="inactive">
            Inactive
          </option>
        </select>

        <CButton
          v-if="canManageWarehouses"
          color="primary"
          class="users-add-button"
          @click="openCreateModal"
        >
          <i
            class="button-icon cil-plus"
            aria-hidden="true"
          />
          Add warehouse
        </CButton>
      </div>
    </div>

    <div
      v-if="loading"
      class="loading-state"
    >
      <CSpinner />
      Loading warehouses…
    </div>

    <CCard
      v-else-if="
        filteredWarehouses.length === 0
      "
      class="empty-state"
    >
      <CCardBody>
        <h3>
          {{
            search || statusFilter
              ? 'No matching warehouses'
              : 'No warehouses yet'
          }}
        </h3>
        <p>
          {{
            search || statusFilter
              ? 'Try another warehouse, code, location or status.'
              : canManageWarehouses
                ? 'Add the first warehouse for this organisation.'
                : 'No warehouses are available to your account.'
          }}
        </p>
      </CCardBody>
    </CCard>

    <div
      v-else
      class="user-table-shell"
    >
      <div class="table-responsive">
        <table class="user-table warehouse-table">
          <thead>
            <tr>
              <th>Warehouse</th>
              <th>Code</th>
              <th>Location</th>
              <th>Status</th>
              <th
                v-if="canManageWarehouses"
                class="user-actions-column"
              >
                Actions
              </th>
            </tr>
          </thead>

          <tbody>
            <tr
              v-for="row in warehouseTable.getRowModel().rows"
              :key="row.id"
            >
              <td data-label="Warehouse">
                <div class="warehouse-identity">
                  <div class="warehouse-icon">
                    <i
                      class="cil-industry"
                      aria-hidden="true"
                    />
                  </div>

                  <strong>
                    {{ row.original.name }}
                  </strong>
                </div>
              </td>

              <td data-label="Code">
                <span class="warehouse-code">
                  {{ row.original.code }}
                </span>
              </td>

              <td data-label="Location">
                <div class="warehouse-location">
                  <strong>
                    {{ row.original.city }}
                  </strong>
                  <small>
                    {{ row.original.country }}
                  </small>
                </div>
              </td>

              <td data-label="Status">
                <span
                  v-if="row.original.isActive"
                  class="user-status-pill active"
                >
                  <span class="status-dot" />
                  Active
                </span>

                <span
                  v-else
                  class="user-status-pill inactive"
                >
                  <span class="status-dot" />
                  Inactive
                </span>
              </td>

              <td
                v-if="canManageWarehouses"
                data-label="Actions"
                class="user-actions-cell"
              >
                <div class="user-row-actions">
                  <CButton
                    color="secondary"
                    variant="outline"
                    size="sm"
                    @click="
                      openEditModal(row.original)
                    "
                  >
                    <i
                      class="button-icon cil-pencil"
                      aria-hidden="true"
                    />
                    Edit
                  </CButton>

                  <CButton
                    v-if="row.original.isActive"
                    color="danger"
                    variant="outline"
                    size="sm"
                    @click="
                      setWarehouseActive(
                        row.original,
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
                      setWarehouseActive(
                        row.original,
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
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="user-grid-pagination">
        <div class="pagination-summary">
          Showing
          <strong>
            {{ firstVisibleWarehouse }}
          </strong>
          –
          <strong>
            {{ lastVisibleWarehouse }}
          </strong>
          of
          <strong>
            {{ filteredWarehouses.length }}
          </strong>
        </div>

        <div class="pagination-controls">
          <label class="page-size-control">
            <span>Rows</span>
            <select
              :value="pagination.pageSize"
              class="form-select form-select-sm"
              aria-label="Rows per page"
              @change="changePageSize"
            >
              <option :value="10">10</option>
              <option :value="25">25</option>
              <option :value="50">50</option>
            </select>
          </label>

          <span class="page-number">
            Page
            {{ pagination.pageIndex + 1 }}
            of
            {{ Math.max(pageCount, 1) }}
          </span>

          <CButton
            color="secondary"
            variant="outline"
            size="sm"
            :disabled="
              !warehouseTable.getCanPreviousPage()
            "
            @click="
              warehouseTable.previousPage()
            "
          >
            Previous
          </CButton>

          <CButton
            color="secondary"
            variant="outline"
            size="sm"
            :disabled="
              !warehouseTable.getCanNextPage()
            "
            @click="
              warehouseTable.nextPage()
            "
          >
            Next
          </CButton>
        </div>
      </div>
    </div>

    <CModal
      :visible="createModalVisible"
      alignment="center"
      size="lg"
      @close="closeCreateModal"
    >
      <CModalHeader>
        <CModalTitle>
          Add warehouse
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <div
          v-if="createModalError"
          class="status-message status-error modal-status-message"
        >
          {{ createModalError }}
        </div>

        <form
          id="create-warehouse-form"
          class="admin-form warehouse-form"
          @submit.prevent="createWarehouse"
        >
          <label class="form-field">
            <span>Name</span>
            <input
              v-model.trim="createForm.name"
              class="form-control"
              required
              maxlength="200"
              placeholder="Cape Town Distribution Centre"
            >
          </label>

          <label class="form-field">
            <span>Code</span>
            <input
              v-model.trim="createForm.code"
              class="form-control"
              required
              maxlength="50"
              placeholder="CPT-01"
            >
          </label>

          <label class="form-field form-field-wide">
            <span>Address line 1</span>
            <input
              v-model.trim="createForm.addressLine1"
              class="form-control"
              required
              maxlength="200"
            >
          </label>

          <label class="form-field form-field-wide">
            <span>Address line 2 (optional)</span>
            <input
              v-model.trim="createForm.addressLine2"
              class="form-control"
              maxlength="200"
            >
          </label>

          <label class="form-field">
            <span>City</span>
            <input
              v-model.trim="createForm.city"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>State / province (optional)</span>
            <input
              v-model.trim="createForm.stateOrProvince"
              class="form-control"
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Postal code</span>
            <input
              v-model.trim="createForm.postalCode"
              class="form-control"
              required
              maxlength="30"
            >
          </label>

          <label class="form-field">
            <span>Country</span>
            <input
              v-model.trim="createForm.country"
              class="form-control"
              required
              maxlength="100"
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
          form="create-warehouse-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Creating…'
              : 'Create warehouse'
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
          Edit warehouse
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <div
          v-if="editModalError"
          class="status-message status-error modal-status-message"
        >
          {{ editModalError }}
        </div>

        <form
          id="edit-warehouse-form"
          class="admin-form warehouse-form"
          @submit.prevent="updateWarehouse"
        >
          <label class="form-field">
            <span>Name</span>
            <input
              v-model.trim="editForm.name"
              class="form-control"
              required
              maxlength="200"
            >
          </label>

          <label class="form-field">
            <span>Code</span>
            <input
              v-model.trim="editForm.code"
              class="form-control"
              required
              maxlength="50"
            >
          </label>

          <label class="form-field form-field-wide">
            <span>Address line 1</span>
            <input
              v-model.trim="editForm.addressLine1"
              class="form-control"
              required
              maxlength="200"
            >
          </label>

          <label class="form-field form-field-wide">
            <span>Address line 2 (optional)</span>
            <input
              v-model.trim="editForm.addressLine2"
              class="form-control"
              maxlength="200"
            >
          </label>

          <label class="form-field">
            <span>City</span>
            <input
              v-model.trim="editForm.city"
              class="form-control"
              required
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>State / province (optional)</span>
            <input
              v-model.trim="editForm.stateOrProvince"
              class="form-control"
              maxlength="100"
            >
          </label>

          <label class="form-field">
            <span>Postal code</span>
            <input
              v-model.trim="editForm.postalCode"
              class="form-control"
              required
              maxlength="30"
            >
          </label>

          <label class="form-field">
            <span>Country</span>
            <input
              v-model.trim="editForm.country"
              class="form-control"
              required
              maxlength="100"
            >
          </label>
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
          form="edit-warehouse-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Saving…'
              : 'Save warehouse'
          }}
        </CButton>
      </CModalFooter>
    </CModal>
  </section>
</template>
