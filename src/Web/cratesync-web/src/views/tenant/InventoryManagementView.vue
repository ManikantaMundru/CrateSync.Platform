<script setup lang="ts">
import {
  computed,
  onMounted,
  ref,
  watch,
} from 'vue'
import {
  CCard,
  CCardBody,
  CSpinner,
} from '@coreui/vue'
import { apiFetch } from '@/services/api'
import TradingPartnerPager from '@/components/trading-partners/TradingPartnerPager.vue'

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
  onHand: number
  reserved: number
  available: number
  updatedAtUtc: string
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

const warehouses = ref<WarehouseSummary[]>([])
const stock = ref<StockSummary[]>([])
const movements = ref<StockMovement[]>([])
const selectedStock = ref<StockSummary | null>(null)
const loading = ref(true)
const detailsLoading = ref(false)
const error = ref<string | null>(null)
const warehouseFilter = ref('')
const search = ref('')
const page = ref(1)
const pageSize = ref(10)

const warehouseById = computed(
  () =>
    new Map(
      warehouses.value.map((warehouse) => [
        warehouse.id,
        warehouse,
      ]),
    ),
)

const filteredStock = computed(() => {
  const term =
    search.value.trim().toLowerCase()

  if (!term)
    return stock.value

  return stock.value.filter((item) => {
    return [
      item.productName,
      item.varietyName,
      item.warehouseName,
      item.warehouseCode,
    ]
      .filter(Boolean)
      .some((value) =>
        String(value)
          .toLowerCase()
          .includes(term),
      )
  })
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

watch(warehouseFilter, async () => {
  page.value = 1
  selectedStock.value = null
  movements.value = []
  await loadStock()
})

function warehouseName(id: string) {
  const warehouse =
    warehouseById.value.get(id)

  return warehouse
    ? warehouse.name
    : 'Warehouse'
}

function warehouseCode(id: string) {
  return (
    warehouseById.value.get(id)?.code ??
    '—'
  )
}

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
  detailsLoading.value = true
  error.value = null

  try {
    const params =
      new URLSearchParams({
        warehouseId: item.warehouseId,
        productVarietyId:
          item.productVarietyId,
      })

    movements.value =
      await apiFetch<StockMovement[]>(
        `/api/inventory/movements?${params.toString()}`,
      )
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
    await loadStock()
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
  <section class="page-stack inventory-page">
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
        <small>warehouse / variety balances</small>
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

      <label class="search-field inventory-search">
        <i
          class="cil-search"
          aria-hidden="true"
        />
        <input
          v-model="search"
          class="form-control tenant-search"
          type="search"
          placeholder="Search product, variety or warehouse..."
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
                <th>Variety</th>
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
                  {{ item.varietyName }}
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
                  {{ item.varietyName }}
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
                {{ selectedStock.varietyName }}
              </h2>
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
              <div>
                <h3>Recent movements</h3>
                <p>
                  Audit trail for this warehouse / variety balance
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
</template>
