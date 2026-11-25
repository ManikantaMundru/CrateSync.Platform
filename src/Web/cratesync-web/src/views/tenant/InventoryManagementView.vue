<script setup lang="ts">
import {
  computed,
  onMounted,
  ref,
  watch,
} from 'vue'
import {
  CButton,
  CCard,
  CCardBody,
  CSpinner,
  CModal, CModalHeader, CModalTitle, CModalBody, CModalFooter,
} from '@coreui/vue'
import { apiFetch } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { countries } from '@/data/countries'
import TradingPartnerPager from '@/components/trading-partners/TradingPartnerPager.vue'
import WarehouseTransferForm from '@/components/inventory/WarehouseTransferForm.vue'

interface WarehouseSummary {
  id: string
  name: string
  code: string
  city: string
  country: string
  isActive: boolean
}

interface StockSummary {
  id: string
  warehouseId: string
  productVarietyId: string
  productId: string
  productName: string
  varietyName: string
  warehouseName: string
  warehouseCode: string
  originCountry: string
  batchId: string
  batchCode: string | null
  packWeightKg: number | null
  fruitsPerPack: number | null
  onHand: number
  reserved: number
  available: number
  updatedAtUtc: string
  sellingPrice: number | null
  sellingCurrency: string | null
  sellingPriceUpdatedAtUtc: string | null
  originalReceivedAtUtc: string | null
}

interface StockMovement {
  id: string
  type: string
  onHandChange: number
  reservedChange: number
  onHandAfter: number
  reservedAfter: number
  referenceType: string
  referenceId: string
  reason: string | null
  occurredAtUtc: string
}

interface TransferRow { id: string; sourceWarehouseId: string; destinationWarehouseId: string; productVarietyId: string; batchCode: string | null; originCountry: string; packWeightKg: number | null; fruitsPerPack: number | null; quantity: number; status: string; transportCompany: string; driverName: string; driverPhone: string; vehicleNumber: string | null; transferReference: string | null; accepted: number | null; damaged: number | null; missing: number | null; receivingRemarks: string | null }
const transfers = ref<TransferRow[]>([])
const transferError = ref('')
const transferBusy = ref(false)
const receipt = ref<TransferRow | null>(null)
const accepted = ref(0), damaged = ref(0), missing = ref(0), receiptRemarks = ref('')
const viewMode = ref<'list' | 'transfer'>('list')
function canAdmin(id: string) { return tenantOperations.value || auth.me?.currentTenant?.warehouses.some(w => w.warehouseId === id && w.role === 'WarehouseAdmin') === true }
function warehouseName(id: string) { return warehouses.value.find(w => w.id === id)?.name ?? transferDestinations.value.find(w => w.id === id)?.name ?? id }
async function loadTransfers() {
  try { transfers.value = await apiFetch<TransferRow[]>('/api/inventory/transfers') }
  catch (e) { transferError.value = e instanceof Error ? e.message : 'Unable to load transfers.' }
}
function openReceipt(t: TransferRow) { receipt.value = t; accepted.value = t.quantity; damaged.value = 0; missing.value = 0; receiptRemarks.value = ''; transferError.value = '' }
const validReceipt = computed(() => !!receipt.value && [accepted.value, damaged.value, missing.value].every(n => Number.isInteger(n) && n >= 0) && accepted.value + damaged.value + missing.value === receipt.value.quantity && (!(damaged.value || missing.value) || !!receiptRemarks.value.trim()))
async function transitionTransfer(t: TransferRow, action: string) {
  transferBusy.value = true; transferError.value = ''
  try {
    await apiFetch(`/api/inventory/transfers/${t.id}/${action}`, { method: 'PATCH', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(action === 'receive' ? { accepted: accepted.value, damaged: damaged.value, missing: missing.value, remarks: receiptRemarks.value.trim() || null } : {}) })
    receipt.value = null; selectedStock.value = null; movements.value = []
    await Promise.all([loadStock(), loadTransfers()])
  } catch (e) { transferError.value = e instanceof Error ? e.message : 'Unable to update transfer.' }
  finally { transferBusy.value = false }
}

const warehouses = ref<WarehouseSummary[]>([])
const auth = useAuthStore()
const transferDestinations = ref<WarehouseSummary[]>([])
const tenantOperations = computed(() => auth.me?.currentTenant?.accessMode === 'SuperAdmin' || auth.me?.currentTenant?.roles.some(r => r === 'TenantOwner' || r === 'TenantAdmin') === true)
const sourceWarehouses = computed(() => warehouses.value.filter(w => tenantOperations.value || auth.me?.currentTenant?.warehouses.some(a => a.warehouseId === w.id && a.role === 'WarehouseAdmin')))
const canTransfer = computed(() => sourceWarehouses.value.some(w => w.isActive))
const stock = ref<StockSummary[]>([])
const movements = ref<StockMovement[]>([])
const selectedStock = ref<StockSummary | null>(null)
interface PriceChange { id: string; previousPrice: number | null; previousCurrency: string | null; price: number; currency: string; userId: string; reason: string; changedAtUtc: string }
const priceHistory = ref<PriceChange[]>([])
const priceVisible = ref(false)
const priceBusy = ref(false)
const priceError = ref('')
const priceTarget = ref<StockSummary | null>(null)
const priceAmount = ref<number | string>('')
const priceCurrency = ref('INR')
const priceReason = ref('')
function openPrice(item: StockSummary) {
  if (!canAdmin(item.warehouseId) || item.onHand <= 0) return
  priceTarget.value = item
  priceAmount.value = item.sellingPrice ?? ''
  priceCurrency.value = item.sellingCurrency ?? 'INR'
  priceReason.value = ''
  priceError.value = ''
  priceVisible.value = true
}
async function loadPriceHistory(item: StockSummary) {
  const rows = await apiFetch<PriceChange[]>(`/api/inventory/stock/${item.id}/selling-price/history?warehouseId=${item.warehouseId}`)
  if (selectedStock.value?.id === item.id) priceHistory.value = rows
}
async function savePrice() {
  const item = priceTarget.value
  if (!item || !canAdmin(item.warehouseId)) return
  const amount = Number(priceAmount.value)
  const currency = priceCurrency.value.trim().toUpperCase()
  if (!Number.isFinite(amount) || amount <= 0 || !/^[A-Z]{3}$/.test(currency) || !priceReason.value.trim()) {
    priceError.value = 'Enter a positive price, currency and reason.'
    return
  }
  priceBusy.value = true
  priceError.value = ''
  try {
    await apiFetch(`/api/inventory/stock/${item.id}/selling-price`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ warehouseId: item.warehouseId, price: amount, currency, reason: priceReason.value.trim() }) })
    priceVisible.value = false
    await loadStock()
    const updated = stock.value.find(s => s.id === item.id)
    if (updated && selectedStock.value?.id === item.id) await selectStock(updated)
  } catch (e) { priceError.value = e instanceof Error ? e.message : 'Unable to save selling price.' }
  finally { priceBusy.value = false }
}

const loading = ref(true)
const detailsLoading = ref(false)
const error = ref<string | null>(null)
const warehouseFilter = ref('')
const originFilter = ref('')
const ageSort = ref('default')
function ageDays(item: StockSummary) {
  return item.originalReceivedAtUtc ? Math.max(0, Math.floor((Date.now() - Date.parse(item.originalReceivedAtUtc)) / 86400000)) : null
}
function ageLabel(item: StockSummary) {
  const days = ageDays(item)
  return days === null ? 'Unknown' : days === 0 ? 'Today' : days + ' days'
}
watch(ageSort, () => { page.value = 1 })
interface FifoPlan { requested: number; allocated: number; shortfall: number; lines: { balanceId: string; batchId: string; batchCode: string | null; originalReceivedAtUtc: string; quantity: number; sellingPrice: number; currency: string }[] }
const fifoQuantity = ref<number | string>('')
const fifoPlan = ref<FifoPlan | null>(null)
const fifoError = ref('')
const fifoBusy = ref(false)
watch(selectedStock, () => { fifoPlan.value = null; fifoError.value = ''; fifoQuantity.value = '' })
async function previewFifo() {
  const item = selectedStock.value
  const quantity = Number(fifoQuantity.value)
  if (!item || !item.sellingCurrency || !Number.isInteger(quantity) || quantity <= 0) return
  fifoBusy.value = true; fifoError.value = ''; fifoPlan.value = null
  try {
    const params = new URLSearchParams({ warehouseId: item.warehouseId, productVarietyId: item.productVarietyId, originCountry: item.originCountry, currency: item.sellingCurrency, quantity: String(quantity) })
    if (item.fruitsPerPack != null) params.set('fruitsPerPack', String(item.fruitsPerPack))
    else if (item.packWeightKg != null) params.set('packWeightKg', String(item.packWeightKg))
    const plan = await apiFetch<FifoPlan>(`/api/inventory/fifo-plan?${params}`)
    if (selectedStock.value?.id === item.id) fifoPlan.value = plan
  } catch (e) { fifoError.value = e instanceof Error ? e.message : 'Unable to plan allocation.' }
  finally { fifoBusy.value = false }
}

const search = ref('')
const page = ref(1)
const pageSize = ref(10)

const countryByCode = new Map(
  countries.map((country) => [
    country.code,
    country.name,
  ]),
)

function originName(code: string) {
  return countryByCode.get(code) ?? code
}

function countryFlag(code: string) {
  if (!/^[A-Z]{2}$/.test(code) || code === 'ZZ')
    return '🌐'

  return String.fromCodePoint(
    ...[...code].map(
      (character) =>
        127397 + character.charCodeAt(0),
    ),
  )
}

const originOptions = computed(() =>
  [...new Set(
    stock.value
      .map((item) => item.originCountry)
      .filter(Boolean),
  )].sort((a, b) =>
    originName(a).localeCompare(originName(b)),
  ),
)

const filteredStock = computed(() => {
  const term =
    search.value.trim().toLowerCase()

  const rows = stock.value.filter((item) => {
    if (
      originFilter.value &&
      item.originCountry !== originFilter.value
    ) {
      return false
    }

    if (!term)
      return true

    return [
      item.productName,
      item.varietyName,
      item.batchCode,
      item.warehouseName,
      item.warehouseCode,
      item.originCountry,
      originName(item.originCountry),
    ]
      .filter(Boolean)
      .some((value) =>
        String(value)
          .toLowerCase()
          .includes(term),
      )
  })
  if (ageSort.value !== 'default') rows.sort((a,b) => {
    if (!a.originalReceivedAtUtc) return b.originalReceivedAtUtc ? 1 : 0
    if (!b.originalReceivedAtUtc) return -1
    return (Date.parse(a.originalReceivedAtUtc) - Date.parse(b.originalReceivedAtUtc)) * (ageSort.value === 'oldest' ? 1 : -1)
  })
  return rows
})

const totalCount = computed(
  () => filteredStock.value.length,
)

const totalPages = computed(() =>
  Math.ceil(
    totalCount.value / pageSize.value,
  ),
)

const pagedStock = computed(() => {
  const start =
    (page.value - 1) * pageSize.value

  return filteredStock.value.slice(
    start,
    start + pageSize.value,
  )
})

const totalOnHand = computed(() =>
  filteredStock.value.reduce(
    (total, item) =>
      total + item.onHand,
    0,
  ),
)

const totalReserved = computed(() =>
  filteredStock.value.reduce(
    (total, item) =>
      total + item.reserved,
    0,
  ),
)

const totalAvailable = computed(() =>
  filteredStock.value.reduce(
    (total, item) =>
      total + item.available,
    0,
  ),
)

watch(search, () => {
  page.value = 1
})

watch(pageSize, () => {
  page.value = 1
})

watch(originFilter, () => {
  page.value = 1
})

watch(warehouseFilter, async () => {
  page.value = 1
  originFilter.value = ''
  selectedStock.value = null
  movements.value = []
  await loadStock()
})

function stockStatus(item: StockSummary) {
  if (
    item.onHand === 0 &&
    item.reserved === 0
  ) {
    return {
      label: 'Out of stock',
      className: 'out',
    }
  }

  if (
    item.onHand > 0 &&
    item.available === 0
  ) {
    return {
      label: 'Fully reserved',
      className: 'reserved',
    }
  }

  return {
    label: 'Available',
    className: 'available',
  }
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat(
    undefined,
    {
      dateStyle: 'medium',
      timeStyle: 'short',
    },
  ).format(new Date(value))
}

function friendly(value: string) {
  return value
    .replace(
      /([a-z])([A-Z])/g,
      '$1 $2',
    )
    .replaceAll('_', ' ')
}

function movementChange(
  movement: StockMovement,
) {
  if (movement.onHandChange !== 0)
    return movement.onHandChange

  return movement.reservedChange
}

function movementChangeClass(
  movement: StockMovement,
) {
  const change = movementChange(movement)

  if (change > 0)
    return 'positive'

  if (change < 0)
    return 'negative'

  return 'neutral'
}

function movementReference(
  movement: StockMovement,
) {
  const shortId =
    movement.referenceId
      .replaceAll('-', '')
      .slice(0, 8)
      .toUpperCase()

  return `${friendly(
    movement.referenceType,
  )} · ${shortId}`
}

async function openTransfer() {
  if (!canTransfer.value) return
  try {
    transferDestinations.value = await apiFetch<WarehouseSummary[]>('/api/inventory/transfer-destinations')
    viewMode.value = 'transfer'
  } catch (exception) {
    error.value = exception instanceof Error ? exception.message : 'Unable to load transfer destinations.'
  }
}

function showInventory() {
  viewMode.value = 'list'
}

async function handleTransferCompleted(
  destinationWarehouseId: string,
  productVarietyId: string,
  originCountry: string,
  batchId: string,
) {
  viewMode.value = 'list'
  await Promise.all([loadStock(), loadTransfers()])

  const destination =
    stock.value.find(
      (item) =>
        item.warehouseId ===
          destinationWarehouseId &&
        item.productVarietyId ===
          productVarietyId &&
        item.originCountry ===
          originCountry && item.batchId === batchId,
    )

  if (destination)
    await selectStock(destination)
}

async function loadReferenceData() {
  warehouses.value =
    await apiFetch<WarehouseSummary[]>(
      '/api/Warehouse',
    )
}

async function loadStock() {
  loading.value = true
  error.value = null

  try {
    const params =
      new URLSearchParams()

    if (warehouseFilter.value) {
      params.set(
        'warehouseId',
        warehouseFilter.value,
      )
    }


    const query = params.toString()

    stock.value =
      await apiFetch<StockSummary[]>(
        `/api/inventory${query ? `?${query}` : ''}`,
      )

    if (
      page.value >
      Math.max(
        1,
        Math.ceil(
          filteredStock.value.length /
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
        : 'Unable to load inventory.'
  } finally {
    loading.value = false
  }
}

async function selectStock(
  item: StockSummary,
) {
  selectedStock.value = item
  priceHistory.value = []
  detailsLoading.value = true
  error.value = null

  try {
    const params =
      new URLSearchParams({
        warehouseId: item.warehouseId,
        productVarietyId:
          item.productVarietyId,
        originCountry:
          item.originCountry,
        batchId: item.batchId,
      })

    movements.value =
      await apiFetch<StockMovement[]>(
        `/api/inventory/movements?${params.toString()}`,
      )
    await loadPriceHistory(item)
  } catch (exception) {
    movements.value = []
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load stock movements.'
  } finally {
    detailsLoading.value = false
  }
}

onMounted(async () => {
  try {
    await loadReferenceData()
    if (canTransfer.value) transferDestinations.value = await apiFetch<WarehouseSummary[]>('/api/inventory/transfer-destinations')
    await Promise.all([loadStock(), loadTransfers()])
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load inventory data.'
    loading.value = false
  }
})
</script>

<template>
  <WarehouseTransferForm
    v-if="viewMode === 'transfer'"
    :warehouses="sourceWarehouses"
    :destinations="transferDestinations"
    :stock="stock"
    @cancel="showInventory"
    @completed="handleTransferCompleted"
  />

  <section
    v-else
    class="page-stack inventory-page"
  >
    <div class="page-heading catalog-page-heading">
      <div>
        <p class="eyebrow catalog-page-eyebrow">
          Inventory
        </p>
        <h1>Warehouse stock</h1>
        <p>
          See current stock availability and trace every inventory movement.
        </p>
      </div>

      <CButton
        color="primary"
        class="users-add-button"
        v-if="canTransfer"
        @click="openTransfer"
      >
        <i class="cil-swap-horizontal me-1" aria-hidden="true" />
        Transfer stock
      </CButton>
    </div>

    <div
      v-if="error"
      class="status-message status-error"
    >
      {{ error }}
    </div>

    <section class="inventory-kpis">
      <article>
        <span>On hand</span>
        <strong>
          {{ totalOnHand.toLocaleString() }}
        </strong>
        <small>crates across filtered stock</small>
      </article>

      <article>
        <span>Reserved</span>
        <strong>
          {{ totalReserved.toLocaleString() }}
        </strong>
        <small>committed to sales orders</small>
      </article>

      <article>
        <span>Available</span>
        <strong>
          {{ totalAvailable.toLocaleString() }}
        </strong>
        <small>ready for allocation</small>
      </article>

      <article>
        <span>Stock positions</span>
        <strong>
          {{ totalCount.toLocaleString() }}
        </strong>
        <small>warehouse / variety / origin balances</small>
      </article>
    </section>

    <section class="inventory-toolbar">
      <label class="inventory-warehouse-filter">
        <span>Warehouse</span>
        <select
          v-model="warehouseFilter"
          class="form-select"
        >
          <option value="">
            All accessible warehouses
          </option>
          <option
            v-for="warehouse in warehouses"
            :key="warehouse.id"
            :value="warehouse.id"
          >
            {{ warehouse.name }}
            ({{ warehouse.code }})
          </option>
        </select>
      </label>

      <label class="inventory-warehouse-filter">
        <span>Origin</span>
        <select
          v-model="originFilter"
          class="form-select"
        >
          <option value="">
            All origins
          </option>
          <option
            v-for="code in originOptions"
            :key="code"
            :value="code"
          >
            {{ countryFlag(code) }}
            {{ originName(code) }}
          </option>
        </select>
      </label>

      <label class="inventory-warehouse-filter"><span>Stock age</span><select v-model="ageSort" class="form-select"><option value="default">Default order</option><option value="oldest">Oldest first</option><option value="newest">Newest first</option></select></label>
      <label class="search-field inventory-search">
        <i
          class="cil-search"
          aria-hidden="true"
        />
        <input
          v-model="search"
          class="form-control tenant-search"
          type="search"
          placeholder="Search product, variety, origin or warehouse..."
        >
      </label>
    </section>

    <div
      v-if="loading"
      class="loading-state"
    >
      <CSpinner />
      Loading inventory…
    </div>

    <CCard
      v-else-if="filteredStock.length === 0"
      class="empty-state"
    >
      <CCardBody>
        <h3>No inventory found</h3>
        <p>
          Receive stock into a warehouse or change the current filters.
        </p>
      </CCardBody>
    </CCard>

    <div
      v-else
      class="inventory-workspace"
    >
      <section class="inventory-list-shell">
        <div class="inventory-desktop-table table-responsive">
          <table class="user-table inventory-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Variety / batch / pack</th>
                <th>Origin</th>
                <th>Warehouse</th>
                <th>On hand</th>
                <th>Reserved</th>
                <th>Available</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="item in pagedStock"
                :key="item.id"
                class="inventory-row"
                :class="{
                  selected:
                    selectedStock?.id === item.id,
                }"
                tabindex="0"
                @click="selectStock(item)"
                @keydown.enter="selectStock(item)"
              >
                <td>
                  <strong>
                    {{ item.productName }}
                  </strong>
                </td>
                <td>
                  {{ item.varietyName }} · {{ item.batchCode || 'Legacy / unclassified' }} · {{ item.fruitsPerPack != null ? item.fruitsPerPack + ' fruits' : item.packWeightKg != null ? item.packWeightKg + ' kg' : 'Unknown pack' }}<small class="d-block">Age: {{ ageLabel(item) }}</small>
                </td>
                <td>
                  <div class="inventory-origin-cell">
                    <strong>
                      {{ countryFlag(item.originCountry) }}
                      {{ originName(item.originCountry) }}
                    </strong>
                    <small>{{ item.originCountry }}</small>
                  </div>
                </td>
                <td>
                  <div class="inventory-warehouse-cell">
                    <strong>
                      {{ item.warehouseName }}
                    </strong>
                    <small>
                      {{ item.warehouseCode }}
                    </small>
                  </div>
                </td>
                <td class="inventory-number">
                  {{ item.onHand.toLocaleString() }}
                </td>
                <td class="inventory-number">
                  {{ item.reserved.toLocaleString() }}
                </td>
                <td class="inventory-number inventory-available-number">
                  {{ item.available.toLocaleString() }}
                </td>
                <td>
                  <span
                    class="inventory-stock-status"
                    :class="
                      stockStatus(item).className
                    "
                  >
                    {{ stockStatus(item).label }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="inventory-mobile-list">
          <button
            v-for="item in pagedStock"
            :key="item.id"
            type="button"
            class="inventory-mobile-card"
            :class="{
              selected:
                selectedStock?.id === item.id,
            }"
            @click="selectStock(item)"
          >
            <div class="inventory-mobile-card-heading">
              <div>
                <strong>
                  {{ item.productName }}
                </strong>
                <span>
                  {{ item.varietyName }} · {{ item.batchCode || 'Legacy / unclassified' }} · Age: {{ ageLabel(item) }}
                </span>
              </div>

              <span
                class="inventory-stock-status"
                :class="
                  stockStatus(item).className
                "
              >
                {{ stockStatus(item).label }}
              </span>
            </div>

            <div class="inventory-mobile-origin">
              Origin:
              <strong>
                {{ countryFlag(item.originCountry) }}
                {{ originName(item.originCountry) }}
              </strong>
            </div>

            <div class="inventory-mobile-warehouse">
              {{ item.warehouseName }}
              ·
              {{ item.warehouseCode }}
            </div>

            <div class="inventory-mobile-values">
              <span>
                <small>On hand</small>
                <strong>
                  {{ item.onHand.toLocaleString() }}
                </strong>
              </span>
              <span>
                <small>Reserved</small>
                <strong>
                  {{ item.reserved.toLocaleString() }}
                </strong>
              </span>
              <span>
                <small>Available</small>
                <strong>
                  {{ item.available.toLocaleString() }}
                </strong>
              </span>
            </div>
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

      <aside class="inventory-detail-shell">
        <div
          v-if="detailsLoading"
          class="loading-state compact"
        >
          <CSpinner size="sm" />
          Loading movements…
        </div>

        <div
          v-else-if="selectedStock"
          class="inventory-detail-content"
        >
          <header class="inventory-detail-heading">
            <div>
              <p class="eyebrow">
                Stock position
              </p>
              <h2>
                {{ selectedStock.productName }}
                /
                {{ selectedStock.varietyName }} · {{ selectedStock.batchCode || 'Legacy / unclassified' }} · {{ selectedStock.fruitsPerPack != null ? selectedStock.fruitsPerPack + ' fruits' : selectedStock.packWeightKg != null ? selectedStock.packWeightKg + ' kg' : 'Unknown pack' }}
              </h2>
              <p class="inventory-detail-origin">
                Origin:
                <strong>
                  {{ countryFlag(selectedStock.originCountry) }}
                  {{ originName(selectedStock.originCountry) }}
                </strong>
              </p>
              <p>
                {{ selectedStock.warehouseName }}
                ·
                {{ selectedStock.warehouseCode }}
              </p>
            </div>

            <span
              class="inventory-stock-status prominent"
              :class="
                stockStatus(selectedStock).className
              "
            >
              {{ stockStatus(selectedStock).label }}
            </span>
          </header>

          <section class="inventory-movement-section"><h3>Stock age</h3><strong>{{ ageLabel(selectedStock) }}</strong><p>Original receipt: {{ selectedStock.originalReceivedAtUtc ? formatDateTime(selectedStock.originalReceivedAtUtc) : 'Unknown' }}</p><small>Age is measured from original warehouse receipt, not harvest or expiry.</small></section>
          <section v-if="selectedStock.sellingCurrency && selectedStock.batchCode && (selectedStock.packWeightKg != null || selectedStock.fruitsPerPack != null)" class="inventory-movement-section">
            <h3>FIFO allocation preview</h3><p>Find oldest available, priced batches matching this warehouse, variety, origin, pack and currency. No stock is reserved.</p>
            <label class="form-field">Requested packages<input v-model.number="fifoQuantity" type="number" min="1" step="1" class="form-control"></label>
            <CButton size="sm" :disabled="fifoBusy || Number(fifoQuantity)<=0" @click="previewFifo">Preview allocation</CButton>
            <p v-if="fifoError" class="text-danger">{{ fifoError }}</p>
            <div v-if="fifoPlan" class="mt-2"><strong>{{ fifoPlan.allocated }} allocated in preview · {{ fifoPlan.shortfall }} shortfall</strong><div v-for="line in fifoPlan.lines" :key="line.balanceId" class="border rounded p-2 mt-1">{{ line.batchCode }} · {{ line.quantity }} packages · {{ line.currency }} {{ line.sellingPrice.toFixed(2) }} / pack<br><small>Original receipt: {{ formatDateTime(line.originalReceivedAtUtc) }}</small></div></div>
          </section>

          <section class="inventory-detail-kpis">
            <article>
              <small>On hand</small>
              <strong>
                {{ selectedStock.onHand.toLocaleString() }}
              </strong>
            </article>
            <article>
              <small>Reserved</small>
              <strong>
                {{ selectedStock.reserved.toLocaleString() }}
              </strong>
            </article>
            <article>
              <small>Available</small>
              <strong>
                {{ selectedStock.available.toLocaleString() }}
              </strong>
            </article>
          </section>

          <section class="inventory-movement-section">
            <div class="section-heading">
              <div><h3>Selling price</h3><p>Per package · specific to this warehouse and batch</p></div>
              <CButton v-if="canAdmin(selectedStock.warehouseId) && selectedStock.onHand > 0" size="sm" color="primary" variant="outline" @click="openPrice(selectedStock)">{{ selectedStock.sellingPrice == null ? 'Set price' : 'Change price' }}</CButton>
            </div>
            <strong>{{ selectedStock.sellingPrice == null ? 'Not priced' : selectedStock.sellingCurrency + ' ' + selectedStock.sellingPrice.toFixed(2) }}</strong>
            <small v-if="selectedStock.sellingPriceUpdatedAtUtc" class="d-block">{{ formatDateTime(selectedStock.sellingPriceUpdatedAtUtc) }}</small>
            <h4 class="mt-3">Price history</h4>
            <div v-for="change in priceHistory" :key="change.id" class="border rounded p-2 mb-2">
              <strong>{{ change.previousPrice == null ? 'Not priced' : change.previousCurrency + ' ' + change.previousPrice.toFixed(2) }} → {{ change.currency }} {{ change.price.toFixed(2) }}</strong>
              <div>{{ change.reason }}</div><small>{{ formatDateTime(change.changedAtUtc) }} · User {{ change.userId }}</small>
            </div>
            <p v-if="!priceHistory.length" class="shipment-muted">No selling price changes recorded.</p>
          </section>

          <section class="inventory-movement-section">
            <div class="section-heading">
              <div>
                <h3>Recent movements</h3>
                <p>
                  Audit trail for this warehouse / variety / origin balance
                </p>
              </div>
            </div>

            <div
              v-if="movements.length"
              class="inventory-movement-list"
            >
              <article
                v-for="movement in movements"
                :key="movement.id"
                class="inventory-movement-row"
              >
                <div
                  class="inventory-movement-change"
                  :class="
                    movementChangeClass(
                      movement,
                    )
                  "
                >
                  {{
                    movementChange(movement) > 0
                      ? '+'
                      : ''
                  }}{{ movementChange(movement) }}
                </div>

                <div class="inventory-movement-copy">
                  <div>
                    <strong>
                      {{ friendly(movement.type) }}
                    </strong>
                    <time>
                      {{
                        formatDateTime(
                          movement.occurredAtUtc,
                        )
                      }}
                    </time>
                  </div>

                  <small>
                    {{ movementReference(movement) }}
                  </small>

                  <p v-if="movement.reason">
                    {{ movement.reason }}
                  </p>

                  <div class="inventory-movement-after">
                    <span>
                      On hand
                      <strong>
                        {{ movement.onHandAfter }}
                      </strong>
                    </span>
                    <span>
                      Reserved
                      <strong>
                        {{ movement.reservedAfter }}
                      </strong>
                    </span>
                  </div>
                </div>
              </article>
            </div>

            <p
              v-else
              class="shipment-muted"
            >
              No stock movements recorded yet.
            </p>
          </section>
        </div>

        <div
          v-else
          class="shipment-detail-empty"
        >
          <i
            class="cil-storage"
            aria-hidden="true"
          />
          <h3>Select a stock position</h3>
          <p>
            Choose a product variety to inspect availability and movement history.
          </p>
        </div>
      </aside>
    </div>
  </section>
  <section v-if="viewMode === 'list'" class="mt-3">
    <h3>Warehouse transfers</h3>
    <p>Planned transfers reserve stock; dispatched transfers remain in transit until received.</p>
    <p v-if="transferError" class="text-danger">{{ transferError }}</p>
    <div v-for="t in transfers" :key="t.id" class="border rounded p-3 mb-2">
      <strong>{{ t.transferReference || t.id.slice(0,8) }} · {{ friendly(t.status) }}</strong>
      <div>{{ warehouseName(t.sourceWarehouseId) }} → {{ warehouseName(t.destinationWarehouseId) }}</div>
      <div>{{ t.batchCode || 'Legacy / unclassified' }} · {{ t.originCountry }} · {{ t.quantity }} packages · {{ t.fruitsPerPack != null ? t.fruitsPerPack + ' fruits/pack' : t.packWeightKg != null ? t.packWeightKg + ' kg/pack' : 'Unknown pack' }}</div>
      <div>{{ t.transportCompany }} · {{ t.driverName }} · {{ t.driverPhone }} · {{ t.vehicleNumber || 'No vehicle number' }}</div>
      <div v-if="t.accepted != null">{{ t.accepted }} accepted · {{ t.damaged }} damaged · {{ t.missing }} missing · {{ t.receivingRemarks }}</div>
      <CButton v-if="t.status === 'Planned' && canAdmin(t.sourceWarehouseId)" size="sm" :disabled="transferBusy" @click="transitionTransfer(t,'dispatch')">Dispatch</CButton>
      <CButton v-if="t.status === 'Planned' && canAdmin(t.sourceWarehouseId)" size="sm" color="secondary" :disabled="transferBusy" @click="transitionTransfer(t,'cancel')">Cancel</CButton>
      <CButton v-if="t.status === 'InTransit' && canAdmin(t.destinationWarehouseId)" size="sm" :disabled="transferBusy" @click="openReceipt(t)">Confirm receipt</CButton>
    </div>
    <p v-if="!transfers.length">No transfers recorded.</p>
  </section>
  <CModal :visible="receipt !== null" @close="!transferBusy && (receipt = null)">
    <CModalHeader><CModalTitle>Inspect transferred packages</CModalTitle></CModalHeader>
    <CModalBody>
      <p>{{ receipt?.quantity }} expected packages</p>
      <p v-if="transferError" class="text-danger">{{ transferError }}</p>
      <label class="form-field">Accepted<input v-model.number="accepted" type="number" min="0" step="1" class="form-control"></label>
      <label class="form-field">Damaged<input v-model.number="damaged" type="number" min="0" step="1" class="form-control"></label>
      <label class="form-field">Missing<input v-model.number="missing" type="number" min="0" step="1" class="form-control"></label>
      <label class="form-field">Remarks (required for discrepancies)<textarea v-model="receiptRemarks" maxlength="500" class="form-control"/></label>
    </CModalBody>
    <CModalFooter><CButton color="secondary" :disabled="transferBusy" @click="receipt = null">Cancel</CButton><CButton :disabled="transferBusy || !validReceipt" @click="receipt && transitionTransfer(receipt,'receive')">Confirm receipt</CButton></CModalFooter>
  </CModal>
  <CModal :visible="priceVisible" @close="!priceBusy && (priceVisible = false)">
    <CModalHeader><CModalTitle>Warehouse selling price</CModalTitle></CModalHeader>
    <CModalBody>
      <p>{{ priceTarget?.warehouseName }} · {{ priceTarget?.batchCode || 'Legacy / unclassified' }}</p>
      <p v-if="priceError" class="text-danger">{{ priceError }}</p>
      <label class="form-field">Selling price per package<input v-model.number="priceAmount" type="number" min="0.01" step="0.01" class="form-control"></label>
      <label class="form-field">Currency<input v-model.trim="priceCurrency" maxlength="3" class="form-control"></label>
      <label class="form-field">Reason (required)<textarea v-model="priceReason" maxlength="500" class="form-control" rows="3"/></label>
    </CModalBody>
    <CModalFooter><CButton color="secondary" :disabled="priceBusy" @click="priceVisible = false">Cancel</CButton><CButton color="primary" :disabled="priceBusy || !priceReason.trim()" @click="savePrice">Save selling price</CButton></CModalFooter>
  </CModal>
</template>
