<script setup lang="ts">
import {
  computed,
  onBeforeUnmount,
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
import { apiFetch } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { countries } from '@/data/countries'

interface SupplierSummary {
  id: string
  name: string
  commercialIdentifier: string | null
  isActive: boolean
}

interface SupplierAddress {
  line1: string
  line2: string | null
  city: string
  stateOrProvince: string | null
  postalCode: string
  country: string
}

interface SupplierContactDetails {
  contactName: string | null
  email: string | null
  phone: string | null
}

interface SupplierDetails extends SupplierSummary {
  address: SupplierAddress | null
  contactDetails: SupplierContactDetails | null
}

interface CreateSupplierResponse {
  id: string
}

const auth = useAuthStore()
const suppliers = ref<SupplierSummary[]>([])
const selectedSupplier = ref<SupplierDetails | null>(null)
const expandedSupplierId = ref<string | null>(null)
const isCompact = ref(false)
const loading = ref(true)
const detailsLoading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const detailsError = ref<string | null>(null)
const success = ref<string | null>(null)
const search = ref('')
const statusFilter = ref('')
const formVisible = ref(false)
const formMode = ref<'create' | 'edit'>('create')
const formError = ref<string | null>(null)
let compactMedia: MediaQueryList | null = null
let selectionRequest = 0

const form = reactive({
  name: '',
  commercialIdentifier: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  stateOrProvince: '',
  postalCode: '',
  country: '',
  contactName: '',
  email: '',
  phone: '',
})

const canManageSuppliers = computed(
  () =>
    auth.me?.currentTenant?.accessMode ===
      'SuperAdmin' ||
    auth.me?.currentTenant?.roles.includes(
      'TenantAdmin',
    ) === true,
)

const filteredSuppliers = computed(() => {
  const term = search.value.trim().toLowerCase()

  return suppliers.value.filter((supplier) => {
    const matchesSearch =
      !term ||
      supplier.name.toLowerCase().includes(term) ||
      supplier.commercialIdentifier
        ?.toLowerCase()
        .includes(term) === true

    const matchesStatus =
      !statusFilter.value ||
      (statusFilter.value === 'active'
        ? supplier.isActive
        : !supplier.isActive)

    return matchesSearch && matchesStatus
  })
})

watch(filteredSuppliers, async (items) => {
  if (items.length === 0) {
    selectedSupplier.value = null
    expandedSupplierId.value = null
    return
  }

  if (isCompact.value) {
    if (
      expandedSupplierId.value &&
      !items.some(
        (supplier) =>
          supplier.id === expandedSupplierId.value,
      )
    ) {
      expandedSupplierId.value = null
      selectedSupplier.value = null
    }

    return
  }

  if (
    !selectedSupplier.value ||
    !items.some(
      (supplier) =>
        supplier.id === selectedSupplier.value?.id,
    )
  ) {
    await selectSupplier(items[0]!)
  }
})

async function loadSuppliers() {
  loading.value = true
  error.value = null

  try {
    suppliers.value =
      await apiFetch<SupplierSummary[]>(
        '/api/Supplier',
      )

    if (
      !isCompact.value &&
      filteredSuppliers.value.length > 0
    ) {
      await selectSupplier(
        filteredSuppliers.value[0]!,
      )
    }
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load suppliers.'
  } finally {
    loading.value = false
  }
}

async function selectSupplier(
  supplier: SupplierSummary,
) {
  const requestId = ++selectionRequest
  detailsLoading.value = true
  detailsError.value = null

  try {
    const details =
      await apiFetch<SupplierDetails>(
        `/api/Supplier/${supplier.id}`,
      )

    if (requestId === selectionRequest) {
      selectedSupplier.value = details
    }
  } catch (exception) {
    if (requestId === selectionRequest) {
      detailsError.value =
        exception instanceof Error
          ? exception.message
          : 'Unable to load supplier details.'
    }
  } finally {
    if (requestId === selectionRequest) {
      detailsLoading.value = false
    }
  }
}

async function toggleSupplier(
  supplier: SupplierSummary,
) {
  if (!isCompact.value) {
    await selectSupplier(supplier)
    return
  }

  if (
    expandedSupplierId.value === supplier.id
  ) {
    expandedSupplierId.value = null
    selectedSupplier.value = null
    ++selectionRequest
    detailsLoading.value = false
    detailsError.value = null
    return
  }

  expandedSupplierId.value = supplier.id
  await selectSupplier(supplier)
}

function syncLayout(matches: boolean) {
  isCompact.value = matches

  if (matches) {
    expandedSupplierId.value = null
    selectedSupplier.value = null
    ++selectionRequest
    detailsLoading.value = false
    return
  }

  expandedSupplierId.value = null

  if (filteredSuppliers.value.length > 0) {
    void selectSupplier(
      filteredSuppliers.value[0]!,
    )
  }
}

function handleMediaChange(
  event: MediaQueryListEvent,
) {
  syncLayout(event.matches)
}

onMounted(async () => {
  compactMedia = window.matchMedia(
    '(max-width: 991.98px)',
  )
  isCompact.value = compactMedia.matches
  compactMedia.addEventListener(
    'change',
    handleMediaChange,
  )

  await loadSuppliers()
})

onBeforeUnmount(() => {
  compactMedia?.removeEventListener(
    'change',
    handleMediaChange,
  )
})

function resetForm() {
  Object.assign(form, {
    name: '',
    commercialIdentifier: '',
    addressLine1: '',
    addressLine2: '',
    city: '',
    stateOrProvince: '',
    postalCode: '',
    country: '',
    contactName: '',
    email: '',
    phone: '',
  })
}

function openCreate() {
  resetForm()
  formMode.value = 'create'
  formError.value = null
  formVisible.value = true
}

function openEdit() {
  if (!selectedSupplier.value)
    return

  const supplier = selectedSupplier.value

  Object.assign(form, {
    name: supplier.name,
    commercialIdentifier:
      supplier.commercialIdentifier ?? '',
    addressLine1:
      supplier.address?.line1 ?? '',
    addressLine2:
      supplier.address?.line2 ?? '',
    city:
      supplier.address?.city ?? '',
    stateOrProvince:
      supplier.address?.stateOrProvince ?? '',
    postalCode:
      supplier.address?.postalCode ?? '',
    country:
      supplier.address?.country ?? '',
    contactName:
      supplier.contactDetails?.contactName ?? '',
    email:
      supplier.contactDetails?.email ?? '',
    phone:
      supplier.contactDetails?.phone ?? '',
  })

  formMode.value = 'edit'
  formError.value = null
  formVisible.value = true
}

function closeForm() {
  if (!saving.value) {
    formVisible.value = false
    formError.value = null
  }
}

function supplierPayload() {
  const valueOrNull = (value: string) =>
    value.trim() || null

  return {
    name: form.name.trim(),
    commercialIdentifier:
      valueOrNull(form.commercialIdentifier),
    addressLine1:
      valueOrNull(form.addressLine1),
    addressLine2:
      valueOrNull(form.addressLine2),
    city:
      valueOrNull(form.city),
    stateOrProvince:
      valueOrNull(form.stateOrProvince),
    postalCode:
      valueOrNull(form.postalCode),
    country:
      valueOrNull(form.country),
    contactName:
      valueOrNull(form.contactName),
    email:
      valueOrNull(form.email),
    phone:
      valueOrNull(form.phone),
  }
}

async function saveSupplier() {
  saving.value = true
  formError.value = null
  success.value = null

  try {
    if (formMode.value === 'create') {
      const response =
        await apiFetch<CreateSupplierResponse>(
          '/api/Supplier',
          {
            method: 'POST',
            headers: {
              'Content-Type':
                'application/json',
            },
            body: JSON.stringify(
              supplierPayload(),
            ),
          },
        )

      formVisible.value = false
      success.value =
        'Supplier created successfully.'
      await loadSuppliers()

      if (!isCompact.value) {
        const created =
          suppliers.value.find(
            (supplier) =>
              supplier.id === response.id,
          )

        if (created)
          await selectSupplier(created)
      }
    } else if (selectedSupplier.value) {
      await apiFetch(
        `/api/Supplier/${selectedSupplier.value.id}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify(
            supplierPayload(),
          ),
        },
      )

      formVisible.value = false
      success.value =
        'Supplier updated successfully.'

      const id = selectedSupplier.value.id
      await loadSuppliers()

      const updated =
        suppliers.value.find(
          (supplier) => supplier.id === id,
        )

      if (updated) {
        if (isCompact.value)
          expandedSupplierId.value = id

        await selectSupplier(updated)
      }
    }
  } catch (exception) {
    formError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to save supplier.'
  } finally {
    saving.value = false
  }
}

async function setSupplierActive(
  active: boolean,
) {
  if (!selectedSupplier.value)
    return

  if (
    !active &&
    !window.confirm(
      `Deactivate ${selectedSupplier.value.name}? Historical references will be preserved.`,
    )
  ) {
    return
  }

  saving.value = true
  detailsError.value = null
  success.value = null

  const id = selectedSupplier.value.id

  try {
    await apiFetch(
      `/api/Supplier/${id}/${active ? 'activate' : 'deactivate'}`,
      {
        method: 'PATCH',
      },
    )

    success.value = active
      ? 'Supplier reactivated.'
      : 'Supplier deactivated.'

    await loadSuppliers()

    const refreshed =
      suppliers.value.find(
        (supplier) => supplier.id === id,
      )

    if (refreshed) {
      if (isCompact.value)
        expandedSupplierId.value = id

      await selectSupplier(refreshed)
    }
  } catch (exception) {
    detailsError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update supplier status.'
  } finally {
    saving.value = false
  }
}

function formatAddress(
  address: SupplierAddress | null,
) {
  if (!address)
    return 'No address configured'

  return [
    address.line1,
    address.line2,
    address.city,
    address.stateOrProvince,
    address.postalCode,
    address.country,
  ]
    .filter(Boolean)
    .join(', ')
}
</script>

<template>
  <section class="page-stack users-page supplier-page">
    <div class="page-heading supplier-page-heading">
      <div>
        <p class="eyebrow">
          Trading partners
        </p>
        <h1>Suppliers</h1>
        <p>
          Manage growers, exporters and other
          suppliers used across inbound logistics.
        </p>
      </div>

      <CButton
        v-if="canManageSuppliers"
        color="primary"
        class="supplier-add-button"
        @click="openCreate"
      >
        <i
          class="button-icon cil-plus"
          aria-hidden="true"
        />
        Add supplier
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

    <div class="tenant-toolbar supplier-toolbar">
      <div class="search-field supplier-search">
        <i
          class="cil-search"
          aria-hidden="true"
        />
        <input
          v-model="search"
          class="form-control tenant-search"
          type="search"
          placeholder="Search suppliers..."
          aria-label="Search suppliers"
        >
      </div>

      <div class="supplier-filter-row">
        <div
          class="catalog-status-segmented"
          role="group"
          aria-label="Filter supplier status"
        >
          <button
            type="button"
            :class="{ active: statusFilter === '' }"
            @click="statusFilter = ''"
          >
            All
          </button>
          <button
            type="button"
            :class="{ active: statusFilter === 'active' }"
            @click="statusFilter = 'active'"
          >
            Active
          </button>
          <button
            type="button"
            :class="{ active: statusFilter === 'inactive' }"
            @click="statusFilter = 'inactive'"
          >
            Inactive
          </button>
        </div>

        <span class="supplier-result-count">
          {{ filteredSuppliers.length }}
          of
          {{ suppliers.length }}
          suppliers
        </span>
      </div>
    </div>

    <div
      v-if="loading"
      class="loading-state"
    >
      <CSpinner />
      Loading suppliers…
    </div>

    <CCard
      v-else-if="
        filteredSuppliers.length === 0
      "
      class="empty-state"
    >
      <CCardBody>
        <h3>
          {{
            search || statusFilter
              ? 'No matching suppliers'
              : 'No suppliers yet'
          }}
        </h3>
        <p>
          {{
            search || statusFilter
              ? 'Try another supplier name, identifier or status.'
              : canManageSuppliers
                ? 'Add the first supplier for this organisation.'
                : 'No suppliers are available.'
          }}
        </p>
      </CCardBody>
    </CCard>

    <div
      v-else
      class="supplier-master-detail"
    >
      <section class="supplier-list-panel">
        <div
          v-if="isCompact"
          class="supplier-card-list"
        >
          <article
            v-for="supplier in filteredSuppliers"
            :key="supplier.id"
            class="supplier-card"
            :class="{
              expanded:
                expandedSupplierId ===
                supplier.id,
            }"
          >
            <button
              type="button"
              class="supplier-card-trigger"
              :aria-expanded="
                expandedSupplierId ===
                supplier.id
              "
              @click="toggleSupplier(supplier)"
            >
              <span class="supplier-icon">
                <i
                  class="cil-truck"
                  aria-hidden="true"
                />
              </span>

              <span class="supplier-card-copy">
                <strong>
                  {{ supplier.name }}
                </strong>
                <small>
                  {{
                    supplier.commercialIdentifier ||
                    'No commercial ID'
                  }}
                </small>
              </span>

              <span
                v-if="supplier.isActive"
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

              <span
                class="product-accordion-chevron"
                :class="{
                  expanded:
                    expandedSupplierId ===
                    supplier.id,
                }"
                aria-hidden="true"
              >
                ›
              </span>
            </button>

            <div
              v-if="
                expandedSupplierId ===
                supplier.id
              "
              class="supplier-card-detail"
            >
              <div
                v-if="detailsError"
                class="status-message status-error"
              >
                {{ detailsError }}
              </div>

              <div
                v-if="detailsLoading"
                class="loading-state"
              >
                <CSpinner />
                Loading supplier…
              </div>

              <template v-else-if="selectedSupplier">
                <div class="supplier-detail-grid">
                  <div>
                    <span>Country</span>
                    <strong>
                      {{
                        selectedSupplier.address
                          ?.country ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                  <div>
                    <span>Contact</span>
                    <strong>
                      {{
                        selectedSupplier.contactDetails
                          ?.contactName ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                  <div class="supplier-detail-wide">
                    <span>Address</span>
                    <strong>
                      {{
                        formatAddress(
                          selectedSupplier.address,
                        )
                      }}
                    </strong>
                  </div>
                  <div>
                    <span>Email</span>
                    <strong>
                      {{
                        selectedSupplier.contactDetails
                          ?.email ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                  <div>
                    <span>Phone</span>
                    <strong>
                      {{
                        selectedSupplier.contactDetails
                          ?.phone ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                </div>

                <div
                  v-if="canManageSuppliers"
                  class="supplier-detail-actions"
                >
                  <CButton
                    color="secondary"
                    variant="outline"
                    size="sm"
                    :disabled="saving"
                    @click="openEdit"
                  >
                    Edit supplier
                  </CButton>

                  <CButton
                    v-if="selectedSupplier.isActive"
                    color="danger"
                    variant="outline"
                    size="sm"
                    :disabled="saving"
                    @click="
                      setSupplierActive(false)
                    "
                  >
                    Deactivate
                  </CButton>

                  <CButton
                    v-else
                    color="success"
                    variant="outline"
                    size="sm"
                    :disabled="saving"
                    @click="
                      setSupplierActive(true)
                    "
                  >
                    Reactivate
                  </CButton>
                </div>
              </template>
            </div>
          </article>
        </div>

        <div
          v-else
          class="user-table-shell"
        >
          <div class="table-responsive">
            <table class="user-table supplier-table">
              <thead>
                <tr>
                  <th>Supplier</th>
                  <th>Commercial ID</th>
                  <th>Status</th>
                </tr>
              </thead>

              <tbody>
                <tr
                  v-for="supplier in filteredSuppliers"
                  :key="supplier.id"
                  class="product-select-row"
                  :class="{
                    selected:
                      selectedSupplier?.id ===
                      supplier.id,
                  }"
                  tabindex="0"
                  @click="
                    selectSupplier(supplier)
                  "
                  @keydown.enter="
                    selectSupplier(supplier)
                  "
                >
                  <td>
                    <div class="supplier-identity">
                      <span class="supplier-icon">
                        <i
                          class="cil-truck"
                          aria-hidden="true"
                        />
                      </span>
                      <strong>
                        {{ supplier.name }}
                      </strong>
                    </div>
                  </td>

                  <td>
                    {{
                      supplier.commercialIdentifier ||
                      '—'
                    }}
                  </td>

                  <td>
                    <span
                      v-if="supplier.isActive"
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
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <aside
        v-if="!isCompact"
        class="catalog-detail-panel supplier-detail-panel"
        aria-live="polite"
      >
        <div
          v-if="detailsError"
          class="status-message status-error"
        >
          {{ detailsError }}
        </div>

        <div
          v-if="detailsLoading"
          class="loading-state catalog-detail-loading"
        >
          <CSpinner />
          Loading supplier…
        </div>

        <template v-else-if="selectedSupplier">
          <div class="catalog-detail-header">
            <div>
              <p class="eyebrow">
                Supplier
              </p>
              <h2>
                {{ selectedSupplier.name }}
              </h2>
              <small>
                {{
                  selectedSupplier.commercialIdentifier ||
                  'No commercial identifier'
                }}
              </small>
            </div>

            <span
              v-if="selectedSupplier.isActive"
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
          </div>

          <div class="supplier-detail-grid">
            <div class="supplier-detail-wide">
              <span>Address</span>
              <strong>
                {{
                  formatAddress(
                    selectedSupplier.address,
                  )
                }}
              </strong>
            </div>

            <div>
              <span>Country</span>
              <strong>
                {{
                  selectedSupplier.address
                    ?.country ||
                  'Not set'
                }}
              </strong>
            </div>

            <div>
              <span>Contact</span>
              <strong>
                {{
                  selectedSupplier.contactDetails
                    ?.contactName ||
                  'Not set'
                }}
              </strong>
            </div>

            <div>
              <span>Email</span>
              <strong>
                {{
                  selectedSupplier.contactDetails
                    ?.email ||
                  'Not set'
                }}
              </strong>
            </div>

            <div>
              <span>Phone</span>
              <strong>
                {{
                  selectedSupplier.contactDetails
                    ?.phone ||
                  'Not set'
                }}
              </strong>
            </div>
          </div>

          <div
            v-if="canManageSuppliers"
            class="supplier-detail-actions"
          >
            <CButton
              color="secondary"
              variant="outline"
              :disabled="saving"
              @click="openEdit"
            >
              Edit supplier
            </CButton>

            <CButton
              v-if="selectedSupplier.isActive"
              color="danger"
              variant="outline"
              :disabled="saving"
              @click="setSupplierActive(false)"
            >
              Deactivate
            </CButton>

            <CButton
              v-else
              color="success"
              variant="outline"
              :disabled="saving"
              @click="setSupplierActive(true)"
            >
              Reactivate
            </CButton>
          </div>
        </template>

        <div
          v-else
          class="catalog-empty-inline"
        >
          Select a supplier to view details.
        </div>
      </aside>
    </div>

    <CModal
      :visible="formVisible"
      size="lg"
      alignment="center"
      @close="closeForm"
    >
      <CModalHeader>
        <CModalTitle>
          {{
            formMode === 'create'
              ? 'Add supplier'
              : 'Edit supplier'
          }}
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <div
          v-if="formError"
          class="status-message status-error modal-status-message"
        >
          {{ formError }}
        </div>

        <form
          id="supplier-form"
          class="supplier-form"
          @submit.prevent="saveSupplier"
        >
          <section class="supplier-form-section">
            <h3>Supplier</h3>

            <div class="supplier-form-grid">
              <label class="form-field supplier-form-wide">
                <span>Name</span>
                <input
                  v-model.trim="form.name"
                  class="form-control"
                  maxlength="200"
                  required
                  placeholder="Cape Fresh Exports"
                >
              </label>

              <label class="form-field">
                <span>Commercial identifier</span>
                <input
                  v-model.trim="
                    form.commercialIdentifier
                  "
                  class="form-control"
                  maxlength="100"
                  placeholder="SUP-001"
                >
              </label>
            </div>
          </section>

          <section class="supplier-form-section">
            <h3>Address</h3>

            <div class="supplier-form-grid">
              <label class="form-field supplier-form-wide">
                <span>Address line 1</span>
                <input
                  v-model.trim="form.addressLine1"
                  class="form-control"
                  maxlength="200"
                  placeholder="Street address"
                >
              </label>

              <label class="form-field supplier-form-wide">
                <span>Address line 2</span>
                <input
                  v-model.trim="form.addressLine2"
                  class="form-control"
                  maxlength="200"
                  placeholder="Building, farm or unit"
                >
              </label>

              <label class="form-field">
                <span>City</span>
                <input
                  v-model.trim="form.city"
                  class="form-control"
                  maxlength="100"
                  placeholder="Cape Town"
                >
              </label>

              <label class="form-field">
                <span>State / province</span>
                <input
                  v-model.trim="
                    form.stateOrProvince
                  "
                  class="form-control"
                  maxlength="100"
                  placeholder="Western Cape"
                >
              </label>

              <label class="form-field">
                <span>Postal code</span>
                <input
                  v-model.trim="form.postalCode"
                  class="form-control"
                  maxlength="30"
                  placeholder="7441"
                >
              </label>

              <label class="form-field">
                <span>Country</span>
                <select
                  v-model="form.country"
                  class="form-select"
                >
                  <option value="">
                    Select country
                  </option>
                  <option
                    v-for="country in countries"
                    :key="country.code"
                    :value="country.name"
                  >
                    {{ country.name }}
                  </option>
                </select>
              </label>
            </div>
          </section>

          <section class="supplier-form-section">
            <h3>Contact</h3>

            <div class="supplier-form-grid">
              <label class="form-field">
                <span>Contact name</span>
                <input
                  v-model.trim="form.contactName"
                  class="form-control"
                  maxlength="150"
                  placeholder="Contact person"
                >
              </label>

              <label class="form-field">
                <span>Email</span>
                <input
                  v-model.trim="form.email"
                  class="form-control"
                  type="email"
                  maxlength="254"
                  placeholder="supplier@example.com"
                >
              </label>

              <label class="form-field">
                <span>Phone</span>
                <input
                  v-model.trim="form.phone"
                  class="form-control"
                  maxlength="50"
                  placeholder="+27..."
                >
              </label>
            </div>
          </section>
        </form>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="closeForm"
        >
          Cancel
        </CButton>

        <CButton
          color="primary"
          type="submit"
          form="supplier-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Saving…'
              : formMode === 'create'
                ? 'Create supplier'
                : 'Save changes'
          }}
        </CButton>
      </CModalFooter>
    </CModal>
  </section>
</template>
