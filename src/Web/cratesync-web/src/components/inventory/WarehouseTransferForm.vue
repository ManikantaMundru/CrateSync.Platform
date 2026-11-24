<script setup lang="ts">
import {
  computed,
  reactive,
  ref,
  watch,
} from 'vue'
import {
  CButton,
  CSpinner,
} from '@coreui/vue'
import { apiFetch } from '@/services/api'
import { countries } from '@/data/countries'
import { countries } from '@/data/countries'

interface WarehouseSummary {
  id: string
  name: string
  code: string
  isActive: boolean
}

interface StockSummary {
  id: string
  warehouseId: string
  productVarietyId: string
  countryOfOrigin: string
  productName: string
  varietyName: string
  warehouseName: string
  warehouseCode: string
  onHand: number
  reserved: number
  available: number
}

const props = defineProps<{
  warehouses: WarehouseSummary[]
  stock: StockSummary[]
}>()

const emit = defineEmits<{
  cancel: []
  completed: [
    destinationWarehouseId: string,
    productVarietyId: string,
    countryOfOrigin: string,
  ]
}>()

const saving = ref(false)
const error = ref<string | null>(null)

const form = reactive({
  transferId: crypto.randomUUID(),
  sourceWarehouseId: '',
  destinationWarehouseId: '',
  stockPositionId: '',
  quantity: 0,
  transferReference: '',
  remarks: '',
})

const sourcePositions = computed(() =>
  props.stock
    .filter(
      (item) =>
        item.warehouseId ===
          form.sourceWarehouseId &&
        item.available > 0,
    )
    .sort((a, b) =>
      `${a.productName} ${a.varietyName} ${a.countryOfOrigin}`
        .localeCompare(
          `${b.productName} ${b.varietyName} ${b.countryOfOrigin}`,
        ),
    ),
)

const selectedPosition = computed(() =>
  sourcePositions.value.find(
    (item) =>
      item.id ===
      form.stockPositionId,
  ) ?? null,
)

const destinationWarehouses = computed(() =>
  props.warehouses.filter(
    (warehouse) =>
      warehouse.isActive &&
      warehouse.id !== form.sourceWarehouseId,
  ),
)

watch(
  () => form.sourceWarehouseId,
  () => {
    form.stockPositionId = ''
    form.destinationWarehouseId = ''
    form.quantity = 0
  },
)

watch(
  () => form.stockPositionId,
  () => {
    form.quantity = 0
  },
)

function countryName(code: string) {
  if (code === 'ZZ')
    return 'Unknown origin'

  return (
    countries.find(
      (country) => country.code === code,
    )?.name ?? code
  )
}

function generateReference() {
  const year = new Date().getFullYear()
  const suffix =
    crypto.randomUUID()
      .replaceAll('-', '')
      .slice(0, 6)
      .toUpperCase()

  form.transferReference =
    `TRF-${year}-${suffix}`
}

async function submitTransfer() {
  if (
    !form.sourceWarehouseId ||
    !form.destinationWarehouseId ||
    !form.stockPositionId
  ) {
    error.value =
      'Source warehouse, destination warehouse and stock item are required.'
    return
  }

  if (
    !selectedPosition.value ||
    form.quantity <= 0 ||
    form.quantity >
      selectedPosition.value.available
  ) {
    error.value =
      `Enter a quantity between 1 and ${selectedPosition.value?.available ?? 0}.`
    return
  }

  saving.value = true
  error.value = null

  try {
    await apiFetch(
      '/api/inventory/transfers',
      {
        method: 'POST',
        headers: {
          'Content-Type':
            'application/json',
        },
        body: JSON.stringify({
          transferId: form.transferId,
          sourceWarehouseId:
            form.sourceWarehouseId,
          destinationWarehouseId:
            form.destinationWarehouseId,
          productVarietyId:
            selectedPosition.value!.productVarietyId,
          countryOfOrigin:
            selectedPosition.value!.countryOfOrigin,
          quantity: Number(form.quantity),
          transferReference:
            form.transferReference.trim() ||
            null,
          remarks:
            form.remarks.trim() || null,
        }),
      },
    )

    emit(
      'completed',
      form.destinationWarehouseId,
      selectedPosition.value!.productVarietyId,
      selectedPosition.value!.countryOfOrigin,
    )
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to transfer stock.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <section class="inventory-transfer-page">
    <header class="shipment-form-page-header">
      <div>
        <div class="shipment-create-title-row">
          <button
            type="button"
            class="shipment-back-button"
            aria-label="Back to inventory"
            @click="emit('cancel')"
          >
            ←
          </button>
          <h1>Warehouse transfer</h1>
        </div>
        <p>
          Move available stock between warehouses and keep both inventory movements linked.
        </p>
      </div>
    </header>

    <main class="inventory-transfer-body">
      <div
        v-if="error"
        class="status-message status-error"
      >
        {{ error }}
      </div>

      <section class="shipment-create-section">
        <div class="shipment-create-section-heading">
          <div class="shipment-section-title">
            <span class="shipment-section-number">1</span>
            <h3>Transfer route</h3>
          </div>
        </div>

        <div class="shipment-core-grid">
          <label class="form-field">
            <span>From warehouse *</span>
            <select
              v-model="form.sourceWarehouseId"
              class="form-select"
            >
              <option value="" disabled>
                Select source warehouse...
              </option>
              <option
                v-for="warehouse in warehouses.filter(x => x.isActive)"
                :key="warehouse.id"
                :value="warehouse.id"
              >
                {{ warehouse.name }} ({{ warehouse.code }})
              </option>
            </select>
          </label>

          <label class="form-field">
            <span>To warehouse *</span>
            <select
              v-model="form.destinationWarehouseId"
              class="form-select"
              :disabled="!form.sourceWarehouseId"
            >
              <option value="" disabled>
                Select destination warehouse...
              </option>
              <option
                v-for="warehouse in destinationWarehouses"
                :key="warehouse.id"
                :value="warehouse.id"
              >
                {{ warehouse.name }} ({{ warehouse.code }})
              </option>
            </select>
          </label>
        </div>
      </section>

      <section class="shipment-create-section">
        <div class="shipment-create-section-heading">
          <div class="shipment-section-title">
            <span class="shipment-section-number">2</span>
            <h3>Stock</h3>
          </div>
        </div>

        <div class="shipment-core-grid">
          <label class="form-field">
            <span>Product / variety *</span>
            <select
              v-model="form.stockPositionId"
              class="form-select"
              :disabled="!form.sourceWarehouseId"
            >
              <option value="" disabled>
                Select available stock...
              </option>
              <option
                v-for="item in sourcePositions"
                :key="item.id"
                :value="item.id"
              >
                {{ item.productName }} / {{ item.varietyName }}
                · {{ countryName(item.countryOfOrigin) }}
                — {{ item.available.toLocaleString() }} available
              </option>
            </select>
            <small
              v-if="
                form.sourceWarehouseId &&
                sourcePositions.length === 0
              "
              class="form-help-text"
            >
              This warehouse has no available stock to transfer.
            </small>
          </label>

          <label class="form-field">
            <span>Quantity *</span>
            <input
              v-model.number="form.quantity"
              class="form-control"
              type="number"
              min="1"
              :max="selectedPosition?.available ?? undefined"
              :disabled="!selectedPosition"
              placeholder="0"
            >
            <small
              v-if="selectedPosition"
              class="form-help-text"
            >
              {{ selectedPosition.available.toLocaleString() }}
              available ·
              {{ selectedPosition.reserved.toLocaleString() }}
              reserved
            </small>
          </label>
        </div>

        <div
          v-if="selectedPosition"
          class="inventory-transfer-balance-preview"
        >
          <article>
            <small>Source on hand</small>
            <strong>
              {{ selectedPosition.onHand.toLocaleString() }}
            </strong>
          </article>
          <article>
            <small>Reserved</small>
            <strong>
              {{ selectedPosition.reserved.toLocaleString() }}
            </strong>
          </article>
          <article>
            <small>Available</small>
            <strong>
              {{ selectedPosition.available.toLocaleString() }}
            </strong>
          </article>
          <article>
            <small>After transfer</small>
            <strong>
              {{
                Math.max(
                  0,
                  selectedPosition.onHand -
                    (Number(form.quantity) || 0),
                ).toLocaleString()
              }}
            </strong>
          </article>
        </div>
      </section>

      <section class="shipment-create-section">
        <div class="shipment-create-section-heading">
          <div class="shipment-section-title">
            <span class="shipment-section-number">3</span>
            <h3>Reference & notes</h3>
          </div>
        </div>

        <div class="shipment-core-grid">
          <label class="form-field">
            <span class="shipment-reference-label">
              <span>Transfer reference</span>
              <button
                type="button"
                class="shipment-auto-reference"
                @click="generateReference"
              >
                ⚡ Auto-generate
              </button>
            </span>
            <input
              v-model="form.transferReference"
              class="form-control"
              maxlength="100"
              placeholder="e.g. TRF-2026-A1B2C3"
            >
          </label>

          <label class="form-field">
            <span>Remarks</span>
            <textarea
              v-model="form.remarks"
              class="form-control"
              rows="3"
              maxlength="250"
              placeholder="Reason or operational note..."
            />
          </label>
        </div>
      </section>
    </main>

    <footer class="shipment-form-page-footer">
      <div class="inventory-transfer-footer-summary">
        <span v-if="selectedPosition">
          {{ selectedPosition.productName }}
          /
          {{ selectedPosition.varietyName }}
          ·
          {{ countryName(selectedPosition.countryOfOrigin) }}
        </span>
        <strong v-if="form.quantity > 0">
          {{ Number(form.quantity).toLocaleString() }}
          crates
        </strong>
      </div>

      <div class="shipment-create-footer-actions">
        <CButton
          color="secondary"
          variant="ghost"
          :disabled="saving"
          @click="emit('cancel')"
        >
          Cancel
        </CButton>

        <CButton
          color="primary"
          :disabled="saving"
          @click="submitTransfer"
        >
          <CSpinner
            v-if="saving"
            size="sm"
            class="me-2"
          />
          Confirm transfer
        </CButton>
      </div>
    </footer>
  </section>
</template>
