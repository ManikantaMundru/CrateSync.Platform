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
import TradingPartnerTable from '@/components/trading-partners/TradingPartnerTable.vue'
import TradingPartnerCard from '@/components/trading-partners/TradingPartnerCard.vue'
import TradingPartnerPager from '@/components/trading-partners/TradingPartnerPager.vue'
import type {
  PagedResponse,
  TradingPartnerSummary,
} from '@/types/tradingPartners'

type SupplierSummary = TradingPartnerSummary

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
const page = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const totalPages = ref(0)
let searchTimer: ReturnType<typeof setTimeout> | null = null
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

const filteredSuppliers = computed(
  () => suppliers.value,
)

watch(
  [statusFilter, pageSize],
  () => {
    page.value = 1
    void loadSuppliers()
  },
)

watch(page, () => {
  void loadSuppliers()
})

watch(search, () => {
  if (searchTimer)
    clearTimeout(searchTimer)

  searchTimer = setTimeout(() => {
    page.value = 1
    void loadSuppliers()
  }, 250)
})

async function loadSuppliers() {
  loading.value = true
  error.value = null

  try {
    const params = new URLSearchParams({
      page: String(page.value),
      pageSize: String(pageSize.value),
    })

    const term = search.value.trim()
    if (term)
      params.set('search', term)

    if (statusFilter.value)
      params.set(
        'isActive',
        statusFilter.value === 'active'
          ? 'true'
          : 'false',
      )

    const result =
      await apiFetch<PagedResponse<SupplierSummary>>(
        `/api/Supplier?${params.toString()}`,
      )

    suppliers.value = result.items
    totalCount.value = result.totalCount
    totalPages.value = result.totalPages

    if (
      result.totalPages > 0 &&
      page.value > result.totalPages
    ) {
      page.value = result.totalPages
      return
    }

    if (
      !isCompact.value &&
      suppliers.value.length > 0
    ) {
      await selectSupplier(
        suppliers.value[0]!,
      )
    } else if (suppliers.value.length === 0) {
      selectedSupplier.value = null
      expandedSupplierId.value = null
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

  if (suppliers.value.length > 0) {
    void selectSupplier(
      suppliers.value[0]!,
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

  if (searchTimer)
    clearTimeout(searchTimer)
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

async function inspectSupplier(
  supplier: SupplierSummary,
) {
  await selectSupplier(supplier)

  if (selectedSupplier.value?.id === supplier.id)
    openEdit()
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
          placeholder="Search by supplier, ID, country, or contact..."
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
          {{ totalCount }} suppliers
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
        suppliers.length === 0
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
          class="supplier-card-list trading-partner-card-list"
        >
          <TradingPartnerCard
            v-for="supplier in suppliers"
            :key="supplier.id"
            :partner="supplier"
            :expanded="
              expandedSupplierId ===
              supplier.id
            "
            @toggle="toggleSupplier(supplier)"
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

          </TradingPartnerCard>
        </div>

        <div
          v-else
          class="user-table-shell trading-partner-table-shell"
        >
          <TradingPartnerTable
            :partners="suppliers"
            entity-label="Supplier"
            @select="inspectSupplier"
          />

          <TradingPartnerPager
            :page="page"
            :page-size="pageSize"
            :total-count="totalCount"
            :total-pages="totalPages"
            @update:page="page = $event"
            @update:page-size="
              pageSize = $event
            "
          />
        </div>
      </section>
      <TradingPartnerPager
        v-if="isCompact"
        :page="page"
        :page-size="pageSize"
        :total-count="totalCount"
        :total-pages="totalPages"
        @update:page="page = $event"
        @update:page-size="
          pageSize = $event
        "
      />


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
