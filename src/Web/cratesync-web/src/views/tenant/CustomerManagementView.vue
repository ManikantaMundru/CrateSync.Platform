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

interface CustomerSummary {
  id: string
  name: string
  commercialIdentifier: string | null
  isActive: boolean
}

interface CustomerAddress {
  line1: string
  line2: string | null
  city: string
  stateOrProvince: string | null
  postalCode: string
  country: string
}

interface CustomerContactDetails {
  contactName: string | null
  email: string | null
  phone: string | null
}

interface CustomerDetails extends CustomerSummary {
  address: CustomerAddress | null
  contactDetails: CustomerContactDetails | null
}

interface CreateCustomerResponse {
  id: string
}

const auth = useAuthStore()
const customers = ref<CustomerSummary[]>([])
const selectedCustomer = ref<CustomerDetails | null>(null)
const expandedCustomerId = ref<string | null>(null)
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

const canManageCustomers = computed(
  () =>
    auth.me?.currentTenant?.accessMode ===
      'SuperAdmin' ||
    auth.me?.currentTenant?.roles.includes(
      'TenantAdmin',
    ) === true,
)

const filteredCustomers = computed(() => {
  const term = search.value.trim().toLowerCase()

  return customers.value.filter((customer) => {
    const matchesSearch =
      !term ||
      customer.name.toLowerCase().includes(term) ||
      customer.commercialIdentifier
        ?.toLowerCase()
        .includes(term) === true

    const matchesStatus =
      !statusFilter.value ||
      (statusFilter.value === 'active'
        ? customer.isActive
        : !customer.isActive)

    return matchesSearch && matchesStatus
  })
})

watch(filteredCustomers, async (items) => {
  if (items.length === 0) {
    selectedCustomer.value = null
    expandedCustomerId.value = null
    return
  }

  if (isCompact.value) {
    if (
      expandedCustomerId.value &&
      !items.some(
        (customer) =>
          customer.id === expandedCustomerId.value,
      )
    ) {
      expandedCustomerId.value = null
      selectedCustomer.value = null
    }

    return
  }

  if (
    !selectedCustomer.value ||
    !items.some(
      (customer) =>
        customer.id === selectedCustomer.value?.id,
    )
  ) {
    await selectCustomer(items[0]!)
  }
})

async function loadCustomers() {
  loading.value = true
  error.value = null

  try {
    customers.value =
      await apiFetch<CustomerSummary[]>(
        '/api/Customer',
      )

    if (
      !isCompact.value &&
      filteredCustomers.value.length > 0
    ) {
      await selectCustomer(
        filteredCustomers.value[0]!,
      )
    }
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load customers.'
  } finally {
    loading.value = false
  }
}

async function selectCustomer(
  customer: CustomerSummary,
) {
  const requestId = ++selectionRequest
  detailsLoading.value = true
  detailsError.value = null

  try {
    const details =
      await apiFetch<CustomerDetails>(
        `/api/Customer/${customer.id}`,
      )

    if (requestId === selectionRequest) {
      selectedCustomer.value = details
    }
  } catch (exception) {
    if (requestId === selectionRequest) {
      detailsError.value =
        exception instanceof Error
          ? exception.message
          : 'Unable to load customer details.'
    }
  } finally {
    if (requestId === selectionRequest) {
      detailsLoading.value = false
    }
  }
}

async function toggleCustomer(
  customer: CustomerSummary,
) {
  if (!isCompact.value) {
    await selectCustomer(customer)
    return
  }

  if (
    expandedCustomerId.value === customer.id
  ) {
    expandedCustomerId.value = null
    selectedCustomer.value = null
    ++selectionRequest
    detailsLoading.value = false
    detailsError.value = null
    return
  }

  expandedCustomerId.value = customer.id
  await selectCustomer(customer)
}

function syncLayout(matches: boolean) {
  isCompact.value = matches

  if (matches) {
    expandedCustomerId.value = null
    selectedCustomer.value = null
    ++selectionRequest
    detailsLoading.value = false
    return
  }

  expandedCustomerId.value = null

  if (filteredCustomers.value.length > 0) {
    void selectCustomer(
      filteredCustomers.value[0]!,
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

  await loadCustomers()
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
  if (!selectedCustomer.value)
    return

  const customer = selectedCustomer.value

  Object.assign(form, {
    name: customer.name,
    commercialIdentifier:
      customer.commercialIdentifier ?? '',
    addressLine1:
      customer.address?.line1 ?? '',
    addressLine2:
      customer.address?.line2 ?? '',
    city:
      customer.address?.city ?? '',
    stateOrProvince:
      customer.address?.stateOrProvince ?? '',
    postalCode:
      customer.address?.postalCode ?? '',
    country:
      customer.address?.country ?? '',
    contactName:
      customer.contactDetails?.contactName ?? '',
    email:
      customer.contactDetails?.email ?? '',
    phone:
      customer.contactDetails?.phone ?? '',
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

function customerPayload() {
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

async function saveCustomer() {
  saving.value = true
  formError.value = null
  success.value = null

  try {
    if (formMode.value === 'create') {
      const response =
        await apiFetch<CreateCustomerResponse>(
          '/api/Customer',
          {
            method: 'POST',
            headers: {
              'Content-Type':
                'application/json',
            },
            body: JSON.stringify(
              customerPayload(),
            ),
          },
        )

      formVisible.value = false
      success.value =
        'Customer created successfully.'
      await loadCustomers()

      if (!isCompact.value) {
        const created =
          customers.value.find(
            (customer) =>
              customer.id === response.id,
          )

        if (created)
          await selectCustomer(created)
      }
    } else if (selectedCustomer.value) {
      await apiFetch(
        `/api/Customer/${selectedCustomer.value.id}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify(
            customerPayload(),
          ),
        },
      )

      formVisible.value = false
      success.value =
        'Customer updated successfully.'

      const id = selectedCustomer.value.id
      await loadCustomers()

      const updated =
        customers.value.find(
          (customer) => customer.id === id,
        )

      if (updated) {
        if (isCompact.value)
          expandedCustomerId.value = id

        await selectCustomer(updated)
      }
    }
  } catch (exception) {
    formError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to save customer.'
  } finally {
    saving.value = false
  }
}

async function setCustomerActive(
  active: boolean,
) {
  if (!selectedCustomer.value)
    return

  if (
    !active &&
    !window.confirm(
      `Deactivate ${selectedCustomer.value.name}? Historical references will be preserved.`,
    )
  ) {
    return
  }

  saving.value = true
  detailsError.value = null
  success.value = null

  const id = selectedCustomer.value.id

  try {
    await apiFetch(
      `/api/Customer/${id}/${active ? 'activate' : 'deactivate'}`,
      {
        method: 'PATCH',
      },
    )

    success.value = active
      ? 'Customer reactivated.'
      : 'Customer deactivated.'

    await loadCustomers()

    const refreshed =
      customers.value.find(
        (customer) => customer.id === id,
      )

    if (refreshed) {
      if (isCompact.value)
        expandedCustomerId.value = id

      await selectCustomer(refreshed)
    }
  } catch (exception) {
    detailsError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update customer status.'
  } finally {
    saving.value = false
  }
}

function formatAddress(
  address: CustomerAddress | null,
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
  <section class="page-stack users-page customer-page">
    <div class="page-heading customer-page-heading">
      <div>
        <p class="eyebrow">
          Trading partners
        </p>
        <h1>Customers</h1>
        <p>
          Manage buyers and customer accounts used across sales and fulfilment.
        </p>
      </div>

      <CButton
        v-if="canManageCustomers"
        color="primary"
        class="customer-add-button"
        @click="openCreate"
      >
        <i
          class="button-icon cil-plus"
          aria-hidden="true"
        />
        Add customer
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

    <div class="tenant-toolbar customer-toolbar">
      <div class="search-field customer-search">
        <i
          class="cil-search"
          aria-hidden="true"
        />
        <input
          v-model="search"
          class="form-control tenant-search"
          type="search"
          placeholder="Search customers..."
          aria-label="Search customers"
        >
      </div>

      <div class="customer-filter-row">
        <div
          class="catalog-status-segmented"
          role="group"
          aria-label="Filter customer status"
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

        <span class="customer-result-count">
          {{ filteredCustomers.length }}
          of
          {{ customers.length }}
          customers
        </span>
      </div>
    </div>

    <div
      v-if="loading"
      class="loading-state"
    >
      <CSpinner />
      Loading customers…
    </div>

    <CCard
      v-else-if="
        filteredCustomers.length === 0
      "
      class="empty-state"
    >
      <CCardBody>
        <h3>
          {{
            search || statusFilter
              ? 'No matching customers'
              : 'No customers yet'
          }}
        </h3>
        <p>
          {{
            search || statusFilter
              ? 'Try another customer name, identifier or status.'
              : canManageCustomers
                ? 'Add the first customer for this organisation.'
                : 'No customers are available.'
          }}
        </p>
      </CCardBody>
    </CCard>

    <div
      v-else
      class="customer-master-detail"
    >
      <section class="customer-list-panel">
        <div
          v-if="isCompact"
          class="customer-card-list"
        >
          <article
            v-for="customer in filteredCustomers"
            :key="customer.id"
            class="customer-card"
            :class="{
              expanded:
                expandedCustomerId ===
                customer.id,
            }"
          >
            <button
              type="button"
              class="customer-card-trigger"
              :aria-expanded="
                expandedCustomerId ===
                customer.id
              "
              @click="toggleCustomer(customer)"
            >
              <span class="customer-icon">
                <i
                  class="cil-people"
                  aria-hidden="true"
                />
              </span>

              <span class="customer-card-copy">
                <strong>
                  {{ customer.name }}
                </strong>
                <small>
                  {{
                    customer.commercialIdentifier ||
                    'No commercial ID'
                  }}
                </small>
              </span>

              <span
                v-if="customer.isActive"
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
                    expandedCustomerId ===
                    customer.id,
                }"
                aria-hidden="true"
              >
                ›
              </span>
            </button>

            <div
              v-if="
                expandedCustomerId ===
                customer.id
              "
              class="customer-card-detail"
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
                Loading customer…
              </div>

              <template v-else-if="selectedCustomer">
                <div class="customer-detail-grid">
                  <div>
                    <span>Country</span>
                    <strong>
                      {{
                        selectedCustomer.address
                          ?.country ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                  <div>
                    <span>Contact</span>
                    <strong>
                      {{
                        selectedCustomer.contactDetails
                          ?.contactName ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                  <div class="customer-detail-wide">
                    <span>Address</span>
                    <strong>
                      {{
                        formatAddress(
                          selectedCustomer.address,
                        )
                      }}
                    </strong>
                  </div>
                  <div>
                    <span>Email</span>
                    <strong>
                      {{
                        selectedCustomer.contactDetails
                          ?.email ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                  <div>
                    <span>Phone</span>
                    <strong>
                      {{
                        selectedCustomer.contactDetails
                          ?.phone ||
                        'Not set'
                      }}
                    </strong>
                  </div>
                </div>

                <div
                  v-if="canManageCustomers"
                  class="customer-detail-actions"
                >
                  <CButton
                    color="secondary"
                    variant="outline"
                    size="sm"
                    :disabled="saving"
                    @click="openEdit"
                  >
                    Edit customer
                  </CButton>

                  <CButton
                    v-if="selectedCustomer.isActive"
                    color="danger"
                    variant="outline"
                    size="sm"
                    :disabled="saving"
                    @click="
                      setCustomerActive(false)
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
                      setCustomerActive(true)
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
            <table class="user-table customer-table">
              <thead>
                <tr>
                  <th>Customer</th>
                  <th>Commercial ID</th>
                  <th>Status</th>
                </tr>
              </thead>

              <tbody>
                <tr
                  v-for="customer in filteredCustomers"
                  :key="customer.id"
                  class="product-select-row"
                  :class="{
                    selected:
                      selectedCustomer?.id ===
                      customer.id,
                  }"
                  tabindex="0"
                  @click="
                    selectCustomer(customer)
                  "
                  @keydown.enter="
                    selectCustomer(customer)
                  "
                >
                  <td>
                    <div class="customer-identity">
                      <span class="customer-icon">
                        <i
                          class="cil-people"
                          aria-hidden="true"
                        />
                      </span>
                      <strong>
                        {{ customer.name }}
                      </strong>
                    </div>
                  </td>

                  <td>
                    {{
                      customer.commercialIdentifier ||
                      '—'
                    }}
                  </td>

                  <td>
                    <span
                      v-if="customer.isActive"
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
        class="catalog-detail-panel customer-detail-panel"
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
          Loading customer…
        </div>

        <template v-else-if="selectedCustomer">
          <div class="catalog-detail-header">
            <div>
              <p class="eyebrow">
                Customer
              </p>
              <h2>
                {{ selectedCustomer.name }}
              </h2>
              <small>
                {{
                  selectedCustomer.commercialIdentifier ||
                  'No commercial identifier'
                }}
              </small>
            </div>

            <span
              v-if="selectedCustomer.isActive"
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

          <div class="customer-detail-grid">
            <div class="customer-detail-wide">
              <span>Address</span>
              <strong>
                {{
                  formatAddress(
                    selectedCustomer.address,
                  )
                }}
              </strong>
            </div>

            <div>
              <span>Country</span>
              <strong>
                {{
                  selectedCustomer.address
                    ?.country ||
                  'Not set'
                }}
              </strong>
            </div>

            <div>
              <span>Contact</span>
              <strong>
                {{
                  selectedCustomer.contactDetails
                    ?.contactName ||
                  'Not set'
                }}
              </strong>
            </div>

            <div>
              <span>Email</span>
              <strong>
                {{
                  selectedCustomer.contactDetails
                    ?.email ||
                  'Not set'
                }}
              </strong>
            </div>

            <div>
              <span>Phone</span>
              <strong>
                {{
                  selectedCustomer.contactDetails
                    ?.phone ||
                  'Not set'
                }}
              </strong>
            </div>
          </div>

          <div
            v-if="canManageCustomers"
            class="customer-detail-actions"
          >
            <CButton
              color="secondary"
              variant="outline"
              :disabled="saving"
              @click="openEdit"
            >
              Edit customer
            </CButton>

            <CButton
              v-if="selectedCustomer.isActive"
              color="danger"
              variant="outline"
              :disabled="saving"
              @click="setCustomerActive(false)"
            >
              Deactivate
            </CButton>

            <CButton
              v-else
              color="success"
              variant="outline"
              :disabled="saving"
              @click="setCustomerActive(true)"
            >
              Reactivate
            </CButton>
          </div>
        </template>

        <div
          v-else
          class="catalog-empty-inline"
        >
          Select a customer to view details.
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
              ? 'Add customer'
              : 'Edit customer'
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
          id="customer-form"
          class="customer-form"
          @submit.prevent="saveCustomer"
        >
          <section class="customer-form-section">
            <h3>Customer</h3>

            <div class="customer-form-grid">
              <label class="form-field customer-form-wide">
                <span>Name</span>
                <input
                  v-model.trim="form.name"
                  class="form-control"
                  maxlength="200"
                  required
                  placeholder="Fresh Market India"
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
                  placeholder="CUS-001"
                >
              </label>
            </div>
          </section>

          <section class="customer-form-section">
            <h3>Address</h3>

            <div class="customer-form-grid">
              <label class="form-field customer-form-wide">
                <span>Address line 1</span>
                <input
                  v-model.trim="form.addressLine1"
                  class="form-control"
                  maxlength="200"
                  placeholder="Street address"
                >
              </label>

              <label class="form-field customer-form-wide">
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

          <section class="customer-form-section">
            <h3>Contact</h3>

            <div class="customer-form-grid">
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
                  placeholder="customer@example.com"
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
          form="customer-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Saving…'
              : formMode === 'create'
                ? 'Create customer'
                : 'Save changes'
          }}
        </CButton>
      </CModalFooter>
    </CModal>
  </section>
</template>
