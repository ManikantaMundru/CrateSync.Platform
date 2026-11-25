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
import { apiFetch } from '@/services/api'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const isOwner = computed(() => auth.me?.currentTenant?.accessMode === 'SuperAdmin' || auth.me?.currentTenant?.roles.includes('TenantOwner') === true)
const canManageCosts = computed(() => isOwner.value || auth.me?.currentTenant?.roles.includes('Finance') === true)
const canReceive = (warehouseId: string) => canOperate.value || auth.me?.currentTenant?.warehouses.some(w => w.warehouseId === warehouseId && w.role === 'WarehouseAdmin') === true
const canOperate = computed(() => isOwner.value || (auth.me?.currentTenant?.roles.includes('TenantAdmin') === true && auth.me?.currentTenant?.roles.includes('Finance') !== true))
import { countries } from '@/data/countries'
import TradingPartnerPager from '@/components/trading-partners/TradingPartnerPager.vue'
import InboundShipmentForm from '@/components/inbound-shipments/InboundShipmentForm.vue'
import WarehouseDeliveryForm from '@/components/inbound-shipments/WarehouseDeliveryForm.vue'

interface ShipmentSummary {
  id: string
  supplierId: string
  trackingNumber: string
  originCountry: string
  status: string
  expectedArrivalAtUtc: string
  totalExpectedCrates: number
  totalReceivedCrates: number
  containerNumber: string | null
  billOfLadingNumber: string | null
  billOfEntryNumber: string | null
  customsStatus: string
  plantQuarantineStatus: string
  foodSafetyStatus: string
}

interface ShipmentLine {
  id: string
  productId: string
  productVarietyId: string
  batchCode: string
  expectedQuantity: number
  receivedQuantity: number
  packSizeKg: number | null
  fruitsPerPack?: number | null
  unitCost?: number | null
  currency?: string | null
}

interface ClearanceEvent {
  id: string
  authority: string
  status: string
  referenceNumber: string | null
  remarks: string | null
  occurredAtUtc: string
}

interface ShipmentReceipt {
  id: string
  shipmentLineId: string
  warehouseId: string
  deliveryId: string | null
  productVarietyId: string
  quantity: number
  receivedAtUtc: string
}

interface ShipmentDeliveryAllocation {
  id: string
  deliveryId: string
  shipmentLineId: string
  quantity: number
}

interface ShipmentDelivery {
  id: string
  warehouseId: string
  status: string
  transportCompany: string
  driverName: string
  driverPhone: string
  vehicleNumber: string | null
  originLocation: string
  destinationLocation: string
  transportReference: string | null
  remarks: string | null
  createdAtUtc: string
  dispatchedAtUtc: string | null
  receivedAtUtc: string | null
  allocations: ShipmentDeliveryAllocation[]
}

interface ShipmentDocument {
  id: string
  type: string
  documentName: string
  referenceNumber: string | null
  notes: string | null
  createdAtUtc: string
}

interface ShipmentDetails extends ShipmentSummary {
  shippedAtUtc: string | null
  arrivedAtUtc: string | null
  receivedAtUtc: string | null
  cancelledAtUtc: string | null
  createdAtUtc: string
  updatedAtUtc: string
  portOfLoading: string | null
  portOfDischarge: string | null
  vesselName: string | null
  voyageNumber: string | null
  customsBrokerName: string | null
  billOfEntryDate: string | null
  outOfChargeAtUtc: string | null
  lines: ShipmentLine[]
  clearanceEvents: ClearanceEvent[] | null
  documents: ShipmentDocument[] | null
  receipts: ShipmentReceipt[] | null
  deliveries: ShipmentDelivery[] | null
  isClearedForWarehouseDispatch: boolean
}

interface SupplierSummary {
  id: string
  name: string
  isActive: boolean
}

interface PagedResponse<T> {
  items: T[]
}

interface WarehouseSummary {
  id: string
  name: string
  code: string
  isActive: boolean
}

interface ProductSummary {
  id: string
  name: string
  isActive: boolean
}

const shipments = ref<ShipmentSummary[]>([])
const suppliers = ref<SupplierSummary[]>([])
const warehouses = ref<WarehouseSummary[]>([])
const products = ref<ProductSummary[]>([])
const selectedShipment = ref<ShipmentDetails | null>(null)
const costLineId = ref<string | null>(null)
const costAmount = ref<number | string>('')
const costCurrency = ref('USD')
const costError = ref<string | null>(null)

function openPurchaseCost(line: ShipmentLine) {
  if (!canManageCosts.value) return
  costLineId.value = line.id
  costAmount.value = line.unitCost ?? ''
  costCurrency.value = line.currency ?? 'USD'
  costError.value = null
}

async function savePurchaseCost() {
  if (!canManageCosts.value || !selectedShipment.value || !costLineId.value) return
  const amount = Number(costAmount.value)
  const currency = costCurrency.value.trim().toUpperCase()
  if (costAmount.value === '' || !Number.isFinite(amount) || amount < 0 || !/^[A-Z]{3}$/.test(currency)) {
    costError.value = 'Enter a non-negative purchase cost and three-letter currency.'
    return
  }
  const shipmentId = selectedShipment.value.id
  saving.value = true
  costError.value = null
  try {
    await apiFetch(`/api/InboundShipment/${shipmentId}/lines/${costLineId.value}/purchase-cost`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ unitCost: amount, currency }),
    })
    costLineId.value = null
    await loadShipment(shipmentId)
    success.value = 'Purchase cost saved.'
  } catch (exception) {
    costError.value = exception instanceof Error ? exception.message : 'Unable to save purchase cost.'
  } finally {
    saving.value = false
  }
}

const loading = ref(true)
const detailsLoading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const success = ref<string | null>(null)
const modalError = ref<string | null>(null)
const search = ref('')
const statusFilter = ref('')
const viewMode = ref<'list' | 'create' | 'delivery'>('list')
const clearanceVisible = ref(false)
const documentVisible = ref(false)
const page = ref(1)
const pageSize = ref(10)
let searchTimer: ReturnType<typeof setTimeout> | null = null

const clearanceForm = reactive({
  authority: 'Customs',
  status: 'NotStarted',
  referenceNumber: '',
  remarks: '',
})

const documentForm = reactive({
  type: 'BillOfLading',
  documentName: '',
  referenceNumber: '',
  notes: '',
})

const clearanceStatuses: Record<
  string,
  string[]
> = {
  Customs: [
    'NotStarted',
    'BillOfEntryFiled',
    'Assessment',
    'Examination',
    'QueryRaised',
    'Held',
    'Cleared',
    'OutOfCharge',
    'Rejected',
  ],
  PlantQuarantine: [
    'NotRequired',
    'Pending',
    'DocumentsSubmitted',
    'InspectionScheduled',
    'UnderInspection',
    'Sampling',
    'TreatmentRequired',
    'Cleared',
    'Rejected',
  ],
  FoodSafety: [
    'NotRequired',
    'Pending',
    'Referred',
    'DocumentScrutiny',
    'InspectionScheduled',
    'VisualInspection',
    'Sampling',
    'LabTesting',
    'NocIssued',
    'NonConformance',
    'Rejected',
  ],
  Port: ['Pending', 'Cleared', 'Held'],
  Other: ['Pending', 'Cleared'],
}

const documentTypes = [
  'BillOfLading',
  'CommercialInvoice',
  'PackingList',
  'BillOfEntry',
  'CertificateOfOrigin',
  'PhytosanitaryCertificate',
  'ImportPermit',
  'FoodSafetyNoc',
  'PlantQuarantineRelease',
  'CustomsOutOfCharge',
  'TreatmentCertificate',
  'Other',
]

const filteredShipments = computed(() =>
  statusFilter.value
    ? shipments.value.filter(
        (shipment) =>
          shipment.status === statusFilter.value,
      )
    : shipments.value,
)

const totalCount = computed(
  () => filteredShipments.value.length,
)

const totalPages = computed(() =>
  Math.ceil(
    totalCount.value / pageSize.value,
  ),
)

const pagedShipments = computed(() => {
  const start =
    (page.value - 1) * pageSize.value

  return filteredShipments.value.slice(
    start,
    start + pageSize.value,
  )
})

const supplierById = computed(
  () =>
    new Map(
      suppliers.value.map((item) => [
        item.id,
        item.name,
      ]),
    ),
)

const warehouseById = computed(
  () =>
    new Map(
      warehouses.value.map((item) => [
        item.id,
        item.name,
      ]),
    ),
)

watch([statusFilter, pageSize], () => {
  page.value = 1
})

watch(
  () => clearanceForm.authority,
  (authority) => {
    clearanceForm.status =
      clearanceStatuses[authority]?.[0] ??
      'Pending'
  },
)

watch(search, () => {
  if (searchTimer)
    clearTimeout(searchTimer)

  searchTimer = setTimeout(() => {
    page.value = 1
    void loadShipments()
  }, 250)
})

function countryName(code: string) {
  return (
    countries.find(
      (country) => country.code === code,
    )?.name ?? code
  )
}

function supplierName(id: string) {
  return supplierById.value.get(id) ?? 'Supplier'
}

function warehouseName(id: string) {
  return (
    warehouseById.value.get(id) ??
    'Warehouse'
  )
}

function formatDate(value: string | null) {
  if (!value)
    return '—'

  return new Intl.DateTimeFormat(
    undefined,
    {
      dateStyle: 'medium',
    },
  ).format(new Date(value))
}

function friendlyStatus(value: string) {
  return value.replace(
    /([a-z])([A-Z])/g,
    '$1 $2',
  )
}

function complianceClass(value: string) {
  if (
    [
      'Cleared',
      'OutOfCharge',
      'NocIssued',
      'NotRequired',
    ].includes(value)
  ) {
    return 'good'
  }

  if (
    [
      'Rejected',
      'Held',
      'NonConformance',
    ].includes(value)
  ) {
    return 'bad'
  }

  return 'pending'
}

async function loadReferenceData() {
  const [
    supplierResult,
    warehouseResult,
    productResult,
  ] = await Promise.all([
    apiFetch<PagedResponse<SupplierSummary>>(
      '/api/Supplier?isActive=true&page=1&pageSize=100',
    ),
    apiFetch<WarehouseSummary[]>(
      '/api/Warehouse?isActive=true',
    ),
    apiFetch<ProductSummary[]>(
      '/api/catalog/products?isActive=true',
    ),
  ])

  suppliers.value = supplierResult.items
  warehouses.value = warehouseResult
  products.value = productResult
}

async function loadShipments() {
  loading.value = true
  error.value = null

  try {
    const params = new URLSearchParams()
    const term = search.value.trim()

    if (term)
      params.set('searchTerm', term)

    const query = params.toString()

    shipments.value =
      await apiFetch<ShipmentSummary[]>(
        `/api/InboundShipment${query ? `?${query}` : ''}`,
      )

    if (
      page.value >
      Math.max(
        1,
        Math.ceil(
          filteredShipments.value.length /
            pageSize.value,
        ),
      )
    ) {
      page.value = 1
    }
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load inbound shipments.'
  } finally {
    loading.value = false
  }
}

async function loadShipment(id: string) {
  detailsLoading.value = true
  error.value = null

  try {
    selectedShipment.value =
      await apiFetch<ShipmentDetails>(
        `/api/InboundShipment/${id}`,
      )
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load shipment details.'
  } finally {
    detailsLoading.value = false
  }
}

function openCreate() {
  viewMode.value = 'create'
}

function showShipmentList() {
  viewMode.value = 'list'
}

async function handleShipmentCreated(
  shipmentId: string,
) {
  viewMode.value = 'list'
  success.value =
    'Inbound shipment created successfully.'

  await loadShipments()
  await loadShipment(shipmentId)
}

function remainingQuantity(
  line: ShipmentLine,
) {
  return Math.max(
    0,
    line.expectedQuantity -
      line.receivedQuantity,
  )
}

function openWarehouseDelivery() {
  if (!selectedShipment.value)
    return

  viewMode.value = 'delivery'
}

async function handleWarehouseDeliveryCreated() {
  if (!selectedShipment.value)
    return

  const shipmentId =
    selectedShipment.value.id

  viewMode.value = 'list'
  success.value =
    'Warehouse delivery planned successfully.'

  await loadShipment(shipmentId)
}

async function dispatchWarehouseDelivery(
  deliveryId: string,
) {
  if (!selectedShipment.value)
    return

  saving.value = true
  error.value = null

  try {
    await apiFetch(
      `/api/InboundShipment/${selectedShipment.value.id}/warehouse-deliveries/${deliveryId}/dispatch`,
      { method: 'PATCH' },
    )

    success.value =
      'Delivery marked in transit.'

    await loadShipment(
      selectedShipment.value.id,
    )
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to dispatch warehouse delivery.'
  } finally {
    saving.value = false
  }
}

async function markWarehouseDeliveryDelivered(
  deliveryId: string,
) {
  if (!selectedShipment.value)
    return

  saving.value = true
  error.value = null

  try {
    await apiFetch(
      `/api/InboundShipment/${selectedShipment.value.id}/warehouse-deliveries/${deliveryId}/deliver`,
      { method: 'PATCH' },
    )

    success.value =
      'Transport marked delivered. Stock is awaiting warehouse receipt.'

    await Promise.all([
      loadShipments(),
      loadShipment(
        selectedShipment.value.id,
      ),
    ])
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to mark transport delivered.'
  } finally {
    saving.value = false
  }
}

async function confirmWarehouseDelivery(
  deliveryId: string,
) {
  if (!selectedShipment.value)
    return

  saving.value = true
  error.value = null

  try {
    await apiFetch(
      `/api/InboundShipment/${selectedShipment.value.id}/warehouse-deliveries/${deliveryId}/receive`,
      { method: 'PATCH' },
    )

    success.value =
      'Warehouse delivery received and inventory updated.'

    await Promise.all([
      loadShipments(),
      loadShipment(
        selectedShipment.value.id,
      ),
    ])
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to receive warehouse delivery.'
  } finally {
    saving.value = false
  }
}

function openClearance() {
  if (!selectedShipment.value)
    return

  clearanceForm.authority = 'Customs'
  clearanceForm.status =
    selectedShipment.value.customsStatus
  clearanceForm.referenceNumber = ''
  clearanceForm.remarks = ''
  modalError.value = null
  clearanceVisible.value = true
}

async function saveClearanceEvent() {
  if (!selectedShipment.value)
    return

  saving.value = true
  modalError.value = null

  try {
    await apiFetch(
      `/api/InboundShipment/${selectedShipment.value.id}/clearance-events`,
      {
        method: 'POST',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify({
          authority:
            clearanceForm.authority,
          status: clearanceForm.status,
          referenceNumber:
            clearanceForm.referenceNumber.trim() ||
            null,
          remarks:
            clearanceForm.remarks.trim() ||
            null,
        }),
      },
    )

    clearanceVisible.value = false
    success.value =
      'Clearance status updated.'

    await Promise.all([
      loadShipments(),
      loadShipment(
        selectedShipment.value.id,
      ),
    ])
  } catch (exception) {
    modalError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to record clearance update.'
  } finally {
    saving.value = false
  }
}

function openDocument() {
  documentForm.type = 'BillOfLading'
  documentForm.documentName = ''
  documentForm.referenceNumber = ''
  documentForm.notes = ''
  modalError.value = null
  documentVisible.value = true
}

async function saveDocument() {
  if (!selectedShipment.value)
    return

  if (!documentForm.documentName.trim()) {
    modalError.value =
      'Document name is required.'
    return
  }

  saving.value = true
  modalError.value = null

  try {
    await apiFetch(
      `/api/InboundShipment/${selectedShipment.value.id}/documents`,
      {
        method: 'POST',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify({
          type: documentForm.type,
          documentName:
            documentForm.documentName.trim(),
          referenceNumber:
            documentForm.referenceNumber.trim() ||
            null,
          notes:
            documentForm.notes.trim() ||
            null,
        }),
      },
    )

    documentVisible.value = false
    success.value =
      'Document reference added.'

    await loadShipment(
      selectedShipment.value.id,
    )
  } catch (exception) {
    modalError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to add document reference.'
  } finally {
    saving.value = false
  }
}

async function transition(
  action: 'ship' | 'arrive',
) {
  if (!selectedShipment.value)
    return

  error.value = null

  try {
    await apiFetch(
      `/api/InboundShipment/${selectedShipment.value.id}/${action}`,
      { method: 'PATCH' },
    )

    success.value =
      action === 'ship'
        ? 'Shipment marked as shipped.'
        : 'Shipment marked as arrived.'

    await Promise.all([
      loadShipments(),
      loadShipment(
        selectedShipment.value.id,
      ),
    ])
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update shipment status.'
  }
}

onMounted(async () => {
  try {
    await loadReferenceData()
    await loadShipments()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load inbound logistics data.'
    loading.value = false
  }
})
</script>

<template>
  <InboundShipmentForm
    v-if="viewMode === 'create'"
    :suppliers="suppliers"
    :products="products"
    :can-enter-costs="isOwner"
    @cancel="showShipmentList"
    @created="handleShipmentCreated"
  />

  <WarehouseDeliveryForm
    v-else-if="
      viewMode === 'delivery' &&
      selectedShipment
    "
    :shipment="selectedShipment"
    :warehouses="warehouses"
    @cancel="showShipmentList"
    @created="handleWarehouseDeliveryCreated"
  />

  <section
    v-else
    class="page-stack users-page shipment-page"
  >
    <div class="page-heading catalog-page-heading">
      <div>
        <p class="eyebrow catalog-page-eyebrow">
          Inbound logistics
        </p>
        <h1>Inbound shipments</h1>
        <p>
          Track supplier consignments from booking through arrival, customs and regulatory clearance.
        </p>
      </div>

      <CButton
        color="primary"
        class="users-add-button"
        v-if="isOwner"
        @click="openCreate"
      >
        <i class="button-icon cil-plus" aria-hidden="true" />
        New shipment
      </CButton>
    </div>

    <div v-if="error" class="status-message status-error">
      {{ error }}
    </div>

    <div v-if="success" class="status-message status-success">
      {{ success }}
    </div>

    <div class="shipment-toolbar">
      <div class="search-field shipment-search">
        <i class="cil-search" aria-hidden="true" />
        <input
          v-model="search"
          class="form-control tenant-search"
          type="search"
          placeholder="Search reference, container, BL or Bill of Entry..."
        >
      </div>

      <select
        v-model="statusFilter"
        class="form-select shipment-status-filter"
      >
        <option value="">All statuses</option>
        <option value="Draft">Draft</option>
        <option value="Shipped">Shipped</option>
        <option value="Arrived">Arrived</option>
        <option value="Receiving">Receiving</option>
        <option value="PartiallyReceived">Partially received</option>
        <option value="Received">Received</option>
        <option value="Cancelled">Cancelled</option>
      </select>
    </div>

    <div v-if="loading" class="loading-state">
      <CSpinner />
      Loading inbound shipments…
    </div>

    <CCard
      v-else-if="filteredShipments.length === 0"
      class="empty-state"
    >
      <CCardBody>
        <h3>No inbound shipments found</h3>
        <p>
          Create the first shipment or change your search/filter.
        </p>
      </CCardBody>
    </CCard>

    <div v-else class="shipment-workspace">
      <section class="shipment-list-shell">
        <div class="table-responsive shipment-desktop-table">
          <table class="user-table shipment-table">
            <thead>
              <tr>
                <th>Shipment</th>
                <th>Supplier</th>
                <th>ETA</th>
                <th>Logistics</th>
                <th>Clearance</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="shipment in pagedShipments"
                :key="shipment.id"
                class="shipment-row"
                :class="{ selected: selectedShipment?.id === shipment.id }"
                tabindex="0"
                @click="loadShipment(shipment.id)"
                @keydown.enter="loadShipment(shipment.id)"
              >
                <td>
                  <div class="shipment-primary">
                    <strong>{{ shipment.trackingNumber }}</strong>
                    <small>
                      {{ shipment.containerNumber || shipment.billOfLadingNumber || countryName(shipment.originCountry) }}
                    </small>
                  </div>
                </td>
                <td>{{ supplierName(shipment.supplierId) }}</td>
                <td>{{ formatDate(shipment.expectedArrivalAtUtc) }}</td>
                <td>
                  <span class="shipment-status-pill">
                    {{ friendlyStatus(shipment.status) }}
                  </span>
                </td>
                <td>
                  <div class="shipment-clearance-mini">
                    <span :class="complianceClass(shipment.customsStatus)">
                      C {{ friendlyStatus(shipment.customsStatus) }}
                    </span>
                    <span :class="complianceClass(shipment.plantQuarantineStatus)">
                      PQ {{ friendlyStatus(shipment.plantQuarantineStatus) }}
                    </span>
                    <span :class="complianceClass(shipment.foodSafetyStatus)">
                      FS {{ friendlyStatus(shipment.foodSafetyStatus) }}
                    </span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="shipment-mobile-list">
          <button
            v-for="shipment in pagedShipments"
            :key="shipment.id"
            type="button"
            class="shipment-mobile-card"
            :class="{ selected: selectedShipment?.id === shipment.id }"
            @click="loadShipment(shipment.id)"
          >
            <span>
              <strong>{{ shipment.trackingNumber }}</strong>
              <small>
                {{ supplierName(shipment.supplierId) }} · {{ countryName(shipment.originCountry) }}
              </small>
            </span>
            <span class="shipment-status-pill">
              {{ friendlyStatus(shipment.status) }}
            </span>
          </button>
        </div>

        <TradingPartnerPager
          :page="page"
          :page-size="pageSize"
          :total-count="totalCount"
          :total-pages="totalPages"
          @update:page="page = $event"
          @update:page-size="pageSize = $event"
        />
      </section>

      <aside class="shipment-detail-shell">
        <div v-if="detailsLoading" class="loading-state compact">
          <CSpinner size="sm" />
          Loading shipment…
        </div>

        <div
          v-else-if="selectedShipment"
          class="shipment-detail-content"
        >
          <div class="shipment-detail-heading">
            <div>
              <p class="eyebrow">Shipment detail</p>
              <h2>{{ selectedShipment.trackingNumber }}</h2>
              <p>
                {{ supplierName(selectedShipment.supplierId) }}
                ·
                {{ countryName(selectedShipment.originCountry) }}

              </p>
            </div>

            <span class="shipment-status-pill prominent">
              {{ friendlyStatus(selectedShipment.status) }}
            </span>
          </div>

          <div class="shipment-actions">
            <CButton
              v-if="canOperate && selectedShipment.status === 'Draft'"
              color="primary"
              size="sm"
              @click="transition('ship')"
            >
              Mark shipped
            </CButton>
            <CButton
              v-if="canOperate && selectedShipment.status === 'Shipped'"
              color="primary"
              size="sm"
              @click="transition('arrive')"
            >
              Mark arrived
            </CButton>
          </div>

          <div class="shipment-summary-grid">
            <div>
              <small>ETA</small>
              <strong>{{ formatDate(selectedShipment.expectedArrivalAtUtc) }}</strong>
            </div>
            <div>
              <small>Container</small>
              <strong>{{ selectedShipment.containerNumber || '—' }}</strong>
            </div>
            <div>
              <small>Bill of Lading</small>
              <strong>{{ selectedShipment.billOfLadingNumber || '—' }}</strong>
            </div>
            <div>
              <small>Bill of Entry</small>
              <strong>{{ selectedShipment.billOfEntryNumber || '—' }}</strong>
            </div>
            <div>
              <small>Vessel / Voyage</small>
              <strong>
                {{ [selectedShipment.vesselName, selectedShipment.voyageNumber].filter(Boolean).join(' · ') || '—' }}
              </strong>
            </div>
            <div>
              <small>Route</small>
              <strong>
                {{ [selectedShipment.portOfLoading, selectedShipment.portOfDischarge].filter(Boolean).join(' → ') || '—' }}
              </strong>
            </div>
          </div>

          <section class="shipment-detail-section">
            <div class="section-heading">
              <div>
                <h3>Clearance</h3>
                <p>Current regulatory position</p>
              </div>
              <CButton
                color="secondary"
                variant="outline"
                size="sm"
                v-if="canOperate"
                @click="openClearance"
              >
                Update clearance
              </CButton>
            </div>

            <div class="clearance-cards">
              <div class="clearance-card">
                <small>Customs</small>
                <strong :class="complianceClass(selectedShipment.customsStatus)">
                  {{ friendlyStatus(selectedShipment.customsStatus) }}
                </strong>
              </div>
              <div class="clearance-card">
                <small>Plant Quarantine</small>
                <strong :class="complianceClass(selectedShipment.plantQuarantineStatus)">
                  {{ friendlyStatus(selectedShipment.plantQuarantineStatus) }}
                </strong>
              </div>
              <div class="clearance-card">
                <small>Food Safety</small>
                <strong :class="complianceClass(selectedShipment.foodSafetyStatus)">
                  {{ friendlyStatus(selectedShipment.foodSafetyStatus) }}
                </strong>
              </div>
            </div>
          </section>

          <section class="shipment-detail-section">
            <div class="section-heading">
              <div>
                <h3>Shipment lines</h3>
                <p>{{ selectedShipment.lines.length }} configured lines</p>
              </div>
            </div>

            <div class="shipment-lines-list">
              <div
                v-for="line in selectedShipment.lines"
                :key="line.id"
                class="shipment-line-row shipment-receive-row"
              >
                <div class="shipment-line-main">
                  <strong>{{ line.batchCode }}</strong>
                  <small>
                    {{ line.fruitsPerPack != null ? `${line.fruitsPerPack} fruits / pack` : `${line.packSizeKg} kg / pack` }}
                  </small>

                  <div v-if="canManageCosts" class="purchase-cost-summary">
                    <span>Purchase cost per crate: {{ line.unitCost == null ? 'Pending' : line.unitCost.toFixed(2) + ' ' + line.currency }}</span>
                    <CButton v-if="selectedShipment.status !== 'Cancelled'" color="secondary" variant="outline" size="sm" :disabled="saving" @click="openPurchaseCost(line)">
                      {{ line.unitCost == null ? 'Enter cost' : 'Edit cost' }}
                    </CButton>
                  </div>

                  <div class="shipment-receipt-progress">
                    <span>
                      Received
                      {{ line.receivedQuantity }}
                      / {{ line.expectedQuantity }}
                    </span>
                    <span>
                      {{ remainingQuantity(line) }}
                      remaining
                    </span>
                  </div>

                  <div class="shipment-progress-track">
                    <span
                      :style="{
                        width:
                          line.expectedQuantity > 0
                            ? `${Math.min(
                                100,
                                (line.receivedQuantity /
                                  line.expectedQuantity) *
                                  100,
                              )}%`
                            : '0%',
                      }"
                    />
                  </div>
                </div>

                <span
                  v-if="
                    line.receivedQuantity ===
                    line.expectedQuantity
                  "
                  class="shipment-received-complete"
                >
                  Complete
                </span>
              </div>
            </div>
          </section>

          <section class="shipment-detail-section">
            <div class="section-heading">
              <div>
                <h3>Port → Warehouse deliveries</h3>
                <p>Transport legs, allocations and warehouse receipt</p>
              </div>

              <CButton
                v-if="
                  canOperate && selectedShipment.isClearedForWarehouseDispatch &&
                  ['Arrived', 'Receiving', 'PartiallyReceived'].includes(
                    selectedShipment.status,
                  ) &&
                  selectedShipment.lines.some(
                    (line) => remainingQuantity(line) > 0,
                  )
                "
                color="primary"
                size="sm"
                @click="openWarehouseDelivery"
              >
                Allocate to warehouse
              </CButton>
            </div>

            <div
              v-if="
                selectedShipment.status === 'Arrived' &&
                !selectedShipment.isClearedForWarehouseDispatch
              "
              class="clearance-form-help"
            >
              <strong>Warehouse dispatch is locked</strong>
              <p>
                Complete Customs and any required Plant Quarantine / Food Safety
                clearance before stock can leave the port for a warehouse.
              </p>
            </div>

            <div
              v-if="selectedShipment.deliveries?.length"
              class="warehouse-delivery-list"
            >
              <article
                v-for="delivery in selectedShipment.deliveries"
                :key="delivery.id"
                class="warehouse-delivery-card"
              >
                <header>
                  <div>
                    <strong>
                      {{ warehouseName(delivery.warehouseId) }}
                    </strong>
                    <small>
                      {{ delivery.originLocation }}
                      →
                      {{ delivery.destinationLocation }}
                    </small>
                  </div>
                  <span class="shipment-status-pill">
                    {{ friendlyStatus(delivery.status) }}
                  </span>
                </header>

                <div class="warehouse-delivery-meta">
                  <span>
                    <small>Transporter</small>
                    <strong>{{ delivery.transportCompany }}</strong>
                  </span>
                  <span>
                    <small>Driver</small>
                    <strong>{{ delivery.driverName }}</strong>
                  </span>
                  <span>
                    <small>Phone</small>
                    <strong>{{ delivery.driverPhone }}</strong>
                  </span>
                  <span>
                    <small>Vehicle</small>
                    <strong>{{ delivery.vehicleNumber || '—' }}</strong>
                  </span>
                </div>

                <div class="warehouse-delivery-allocations">
                  <span
                    v-for="allocation in delivery.allocations"
                    :key="allocation.id"
                  >
                    {{
                      selectedShipment.lines.find(
                        (line) =>
                          line.id === allocation.shipmentLineId,
                      )?.batchCode || 'Line'
                    }}
                    ·
                    {{ allocation.quantity }} crates
                  </span>
                </div>

                <p
                  v-if="delivery.transportReference || delivery.remarks"
                  class="shipment-muted"
                >
                  {{ delivery.transportReference || '' }}
                  <span
                    v-if="
                      delivery.transportReference &&
                      delivery.remarks
                    "
                  >
                    ·
                  </span>
                  {{ delivery.remarks || '' }}
                </p>

                <div class="warehouse-delivery-actions">
                  <CButton
                    v-if="canOperate && delivery.status === 'Planned'"
                    color="primary"
                    variant="outline"
                    size="sm"
                    :disabled="saving"
                    @click="
                      dispatchWarehouseDelivery(
                        delivery.id,
                      )
                    "
                  >
                    Mark dispatched
                  </CButton>

                  <CButton
                    v-if="canOperate && delivery.status === 'InTransit'"
                    color="primary"
                    size="sm"
                    :disabled="saving"
                    @click="markWarehouseDeliveryDelivered(delivery.id)"
                  >
                    Mark delivered to warehouse
                  </CButton>

                  <CButton
                    v-if="canReceive(delivery.warehouseId) && delivery.status === 'DeliveredToWarehouse'"
                    color="primary"
                    size="sm"
                    :disabled="saving"
                    @click="
                      confirmWarehouseDelivery(
                        delivery.id,
                      )
                    "
                  >
                    Confirm warehouse receipt
                  </CButton>
                </div>
              </article>
            </div>

            <p v-else class="shipment-muted">
              No port-to-warehouse deliveries planned yet.
            </p>
          </section>

          <section class="shipment-detail-section">
            <div class="section-heading">
              <div>
                <h3>Clearance timeline</h3>
                <p>Operational authority history</p>
              </div>
            </div>

            <div
              v-if="selectedShipment.clearanceEvents?.length"
              class="shipment-timeline"
            >
              <div
                v-for="event in selectedShipment.clearanceEvents"
                :key="event.id"
                class="shipment-timeline-row"
              >
                <span class="timeline-dot" />
                <div>
                  <strong>
                    {{ event.authority }} · {{ friendlyStatus(event.status) }}
                  </strong>
                  <small>
                    {{ formatDate(event.occurredAtUtc) }}
                    <span v-if="event.referenceNumber">
                      · {{ event.referenceNumber }}
                    </span>
                  </small>
                  <p v-if="event.remarks">{{ event.remarks }}</p>
                </div>
              </div>
            </div>
            <p v-else class="shipment-muted">
              No clearance events recorded yet.
            </p>
          </section>

          <section class="shipment-detail-section">
            <div class="section-heading">
              <div>
                <h3>Documents</h3>
                <p>Reference register</p>
              </div>
              <CButton
                color="secondary"
                variant="outline"
                size="sm"
                v-if="canOperate"
                @click="openDocument"
              >
                Add reference
              </CButton>
            </div>

            <div
              v-if="selectedShipment.documents?.length"
              class="shipment-documents"
            >
              <div
                v-for="document in selectedShipment.documents"
                :key="document.id"
                class="shipment-document-row"
              >
                <div>
                  <strong>{{ friendlyStatus(document.type) }}</strong>
                  <small>{{ document.documentName }}</small>
                </div>
                <span>{{ document.referenceNumber || '—' }}</span>
              </div>
            </div>
            <p v-else class="shipment-muted">
              No document references recorded yet.
            </p>
          </section>
        </div>

        <div v-else class="shipment-detail-empty">
          <i class="cil-truck" aria-hidden="true" />
          <h3>Select a shipment</h3>
          <p>
            Choose a shipment to inspect its lines, logistics references and clearance state.
          </p>
        </div>
      </aside>
    </div>

    <CModal
      :visible="clearanceVisible"
      @close="clearanceVisible = false"
    >
      <CModalHeader>
        <CModalTitle>
          Record clearance update
        </CModalTitle>
      </CModalHeader>
      <CModalBody>
        <div
          v-if="modalError"
          class="status-message status-error"
        >
          {{ modalError }}
        </div>

        <div class="clearance-form-help">
          <strong>What are you recording?</strong>
          <p>
            Choose the authority currently handling the shipment, then record
            its latest status. Add the official reference number when one is
            available and use Remarks for operational notes.
          </p>
        </div>

        <div class="shipment-modal-fields">
          <label class="form-field">
            <span>Authority</span>
            <small class="form-help-text">
              Customs = import clearance · Plant Quarantine = agricultural inspection · Food Safety = food regulator.
            </small>
            <select
              v-model="clearanceForm.authority"
              class="form-select"
            >
              <option value="Customs">
                Customs
              </option>
              <option value="PlantQuarantine">
                Plant Quarantine
              </option>
              <option value="FoodSafety">
                Food Safety
              </option>
              <option value="Port">
                Port
              </option>
              <option value="Other">
                Other
              </option>
            </select>
          </label>

          <label class="form-field">
            <span>Status</span>
            <small class="form-help-text">
              Record the latest stage only; every update is kept in the clearance timeline.
            </small>
            <select
              v-model="clearanceForm.status"
              class="form-select"
            >
              <option
                v-for="status in clearanceStatuses[
                  clearanceForm.authority
                ]"
                :key="status"
                :value="status"
              >
                {{ friendlyStatus(status) }}
              </option>
            </select>
          </label>

          <label class="form-field">
            <span>Reference number</span>
            <input
              v-model="clearanceForm.referenceNumber"
              class="form-control"
              placeholder="e.g. Bill of Entry / NOC reference"
            >
          </label>

          <label class="form-field">
            <span>Remarks</span>
            <textarea
              v-model="clearanceForm.remarks"
              class="form-control"
              rows="3"
            />
          </label>
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="clearanceVisible = false"
        >
          Cancel
        </CButton>
        <CButton
          color="primary"
          :disabled="saving"
          @click="saveClearanceEvent"
        >
          Save update
        </CButton>
      </CModalFooter>
    </CModal>

    <CModal
      :visible="documentVisible"
      @close="documentVisible = false"
    >
      <CModalHeader>
        <CModalTitle>
          Add document reference
        </CModalTitle>
      </CModalHeader>
      <CModalBody>
        <div
          v-if="modalError"
          class="status-message status-error"
        >
          {{ modalError }}
        </div>

        <div class="shipment-modal-fields">
          <label class="form-field">
            <span>Document type</span>
            <select
              v-model="documentForm.type"
              class="form-select"
            >
              <option
                v-for="type in documentTypes"
                :key="type"
                :value="type"
              >
                {{ friendlyStatus(type) }}
              </option>
            </select>
          </label>

          <label class="form-field">
            <span>Document name *</span>
            <input
              v-model="documentForm.documentName"
              class="form-control"
            >
          </label>

          <label class="form-field">
            <span>Reference number</span>
            <input
              v-model="documentForm.referenceNumber"
              class="form-control"
            >
          </label>

          <label class="form-field">
            <span>Notes</span>
            <textarea
              v-model="documentForm.notes"
              class="form-control"
              rows="3"
            />
          </label>
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="documentVisible = false"
        >
          Cancel
        </CButton>
        <CButton
          color="primary"
          :disabled="saving"
          @click="saveDocument"
        >
          Add reference
        </CButton>
      </CModalFooter>
    </CModal>
  </section>
  <CModal :visible="costLineId !== null && canManageCosts" alignment="center" @close="!saving && (costLineId = null)">
    <CModalHeader><CModalTitle>Confidential purchase cost</CModalTitle></CModalHeader>
    <CModalBody>
      <form id="purchase-cost-form" class="admin-form admin-form-single" @submit.prevent="savePurchaseCost">
        <p v-if="costError" class="status-message status-error">{{ costError }}</p>
        <label class="form-field"><span>Purchase cost per crate</span><input v-model.number="costAmount" type="number" min="0" step="0.01" required class="form-control"></label>
        <label class="form-field"><span>Currency (ISO code)</span><input v-model.trim="costCurrency" maxlength="3" required class="form-control"></label>
      </form>
    </CModalBody>
    <CModalFooter>
      <CButton color="secondary" :disabled="saving" @click="costLineId = null">Cancel</CButton>
      <CButton color="primary" type="submit" form="purchase-cost-form" :disabled="saving">Save cost</CButton>
    </CModalFooter>
  </CModal>
</template>
