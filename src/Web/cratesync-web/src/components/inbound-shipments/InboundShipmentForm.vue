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

interface SupplierSummary {
  id: string
  name: string
  isActive: boolean
}

interface ProductSummary {
  id: string
  name: string
  isActive: boolean
}

interface ProductVariety {
  id: string
  name: string
  isActive: boolean
}

interface ProductDetails {
  id: string
  name: string
  isActive: boolean
  varieties: ProductVariety[]
}

interface DraftLine {
  productId: string
  productVarietyId: string
  batchCode: string
  expectedQuantity: number
  packSizeKg: number
  unitCost: number
  currency: string
  varieties: ProductVariety[]
}

defineProps<{
  suppliers: SupplierSummary[]
  products: ProductSummary[]
}>()

const emit = defineEmits<{
  cancel: []
  created: [shipmentId: string]
}>()

const saving = ref(false)
const submitted = ref(false)
const error = ref<string | null>(null)
const freightExpanded = ref(true)
const shipmentCurrency = ref('USD')

const form = reactive({
  supplierId: '',
  trackingNumber: '',
  originCountry: '',
  expectedArrivalDate: '',
  containerNumber: '',
  billOfLadingNumber: '',
  portOfLoading: '',
  portOfDischarge: '',
  vesselName: '',
  voyageNumber: '',
})

const lines = ref<DraftLine[]>([
  {
    productId: '',
    productVarietyId: '',
    batchCode: '',
    expectedQuantity: 1,
    packSizeKg: 1,
    unitCost: 0,
    currency: 'USD',
    varieties: [],
  },
])

const totalDraftCrates = computed(() =>
  lines.value.reduce(
    (total, line) =>
      total +
      Math.max(
        0,
        Number(line.expectedQuantity) || 0,
      ),
    0,
  ),
)

const totalDraftWeightKg = computed(() =>
  lines.value.reduce(
    (total, line) =>
      total +
      Math.max(
        0,
        Number(line.expectedQuantity) || 0,
      ) *
        Math.max(
          0,
          Number(line.packSizeKg) || 0,
        ),
    0,
  ),
)

const totalDraftValue = computed(() =>
  lines.value.reduce(
    (total, line) =>
      total +
      Math.max(
        0,
        Number(line.expectedQuantity) || 0,
      ) *
        Math.max(
          0,
          Number(line.unitCost) || 0,
        ),
    0,
  ),
)

watch(shipmentCurrency, (currency) => {
  for (const line of lines.value)
    line.currency = currency
})

function addLine() {
  lines.value.push({
    productId: '',
    productVarietyId: '',
    batchCode: '',
    expectedQuantity: 1,
    packSizeKg: 1,
    unitCost: 0,
    currency: shipmentCurrency.value,
    varieties: [],
  })
}

function removeLine(index: number) {
  if (lines.value.length === 1)
    return

  lines.value.splice(index, 1)
}

function duplicateLastLine() {
  const source = lines.value.at(-1)

  if (!source) {
    addLine()
    return
  }

  lines.value.push({
    ...source,
    batchCode: '',
    varieties: [...source.varieties],
  })
}

function generateShipmentReference() {
  const year = new Date().getFullYear()
  const suffix =
    crypto.randomUUID()
      .replaceAll('-', '')
      .slice(0, 6)
      .toUpperCase()

  form.trackingNumber =
    `SHP-${year}-${suffix}`
}

function formatDraftMoney(value: number) {
  return new Intl.NumberFormat(
    undefined,
    {
      style: 'currency',
      currency: shipmentCurrency.value,
      maximumFractionDigits: 2,
    },
  ).format(value)
}

function currencySymbol() {
  switch (shipmentCurrency.value) {
    case 'USD':
      return '$'
    case 'EUR':
      return '€'
    case 'GBP':
      return '£'
    case 'INR':
      return '₹'
    default:
      return 'R'
  }
}

function lineFieldInvalid(
  line: DraftLine,
  field:
    | 'productId'
    | 'productVarietyId'
    | 'batchCode',
) {
  if (!submitted.value)
    return false

  if (field === 'batchCode')
    return !line.batchCode.trim()

  return !line[field]
}

function lineValidationMessage(
  line: DraftLine,
  index: number,
) {
  const missing: string[] = []

  if (!line.productId)
    missing.push('product')

  if (!line.productVarietyId)
    missing.push('variety')

  if (!line.batchCode.trim())
    missing.push('batch code')

  return missing.length
    ? `Line ${index + 1}: select/enter ${missing.join(', ')}.`
    : null
}

async function productChanged(
  line: DraftLine,
) {
  line.productVarietyId = ''
  line.varieties = []

  if (!line.productId)
    return

  try {
    const product =
      await apiFetch<ProductDetails>(
        `/api/catalog/products/${line.productId}`,
      )

    line.varieties =
      product.varieties.filter(
        (item) => item.isActive,
      )

    if (line.varieties.length === 1) {
      line.productVarietyId =
        line.varieties[0].id
    }
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load product varieties.'
  }
}

async function createShipment() {
  submitted.value = true

  if (
    !form.supplierId ||
    !form.trackingNumber.trim() ||
    !form.originCountry ||
    !form.expectedArrivalDate
  ) {
    error.value =
      'Supplier, reference, origin and ETA are required.'
    return
  }

  const invalidLines =
    lines.value
      .map((line, index) =>
        lineValidationMessage(line, index),
      )
      .filter(
        (message): message is string =>
          message !== null,
      )

  if (invalidLines.length > 0) {
    error.value = invalidLines.join(' ')
    return
  }

  saving.value = true
  error.value = null

  try {
    const created = await apiFetch<{
      id: string
    }>('/api/InboundShipment', {
      method: 'POST',
      headers: {
        'Content-Type':
          'application/json',
      },
      body: JSON.stringify({
        supplierId: form.supplierId,
        trackingNumber:
          form.trackingNumber.trim(),
        originCountry:
          form.originCountry,
        expectedArrivalAtUtc:
          new Date(
            `${form.expectedArrivalDate}T12:00:00Z`,
          ).toISOString(),
      }),
    })

    for (const line of lines.value) {
      await apiFetch(
        `/api/InboundShipment/${created.id}/lines`,
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            productId: line.productId,
            productVarietyId:
              line.productVarietyId,
            batchCode:
              line.batchCode.trim(),
            expectedQuantity:
              Number(line.expectedQuantity),
            packSizeKg:
              Number(line.packSizeKg),
            unitCost:
              Number(line.unitCost),
            currency:
              shipmentCurrency.value,
          }),
        },
      )
    }

    if (
      form.containerNumber.trim() ||
      form.billOfLadingNumber.trim() ||
      form.portOfLoading.trim() ||
      form.portOfDischarge.trim() ||
      form.vesselName.trim() ||
      form.voyageNumber.trim()
    ) {
      await apiFetch(
        `/api/InboundShipment/${created.id}/references`,
        {
          method: 'PUT',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            containerNumber:
              form.containerNumber.trim() ||
              null,
            billOfLadingNumber:
              form.billOfLadingNumber.trim() ||
              null,
            portOfLoading:
              form.portOfLoading.trim() ||
              null,
            portOfDischarge:
              form.portOfDischarge.trim() ||
              null,
            vesselName:
              form.vesselName.trim() ||
              null,
            voyageNumber:
              form.voyageNumber.trim() ||
              null,
            customsBrokerName: null,
            billOfEntryNumber: null,
            billOfEntryDate: null,
            outOfChargeAtUtc: null,
          }),
        },
      )
    }

    emit('created', created.id)
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to create inbound shipment.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <section class="shipment-form-page">
    <header class="shipment-form-page-header">
      <div>
        <div class="shipment-create-title-row">
          <button
            type="button"
            class="shipment-back-button"
            aria-label="Back to shipments"
            @click="emit('cancel')"
          >
            ←
          </button>
          <h1>New inbound shipment</h1>
          <span class="shipment-draft-badge">
            Draft
          </span>
        </div>
        <p>
          Record incoming supplier stock and freight details
        </p>
      </div>

      <label class="shipment-currency-picker">
        <span>Currency</span>
        <select
          v-model="shipmentCurrency"
          class="form-select"
        >
          <option value="USD">USD ($)</option>
          <option value="EUR">EUR (€)</option>
          <option value="GBP">GBP (£)</option>
          <option value="ZAR">ZAR (R)</option>
          <option value="INR">INR (₹)</option>
        </select>
      </label>
    </header>

    <main class="shipment-form-page-body">
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
            <h3>Core Details</h3>
          </div>
          <span class="shipment-required-note">
            * Required fields
          </span>
        </div>

        <div class="shipment-core-grid">
          <label class="form-field">
            <span>
              Supplier
              <strong class="shipment-required">*</strong>
            </span>
            <select
              v-model="form.supplierId"
              class="form-select"
            >
              <option value="">
                Select supplier...
              </option>
              <option
                v-for="supplier in suppliers"
                :key="supplier.id"
                :value="supplier.id"
              >
                {{ supplier.name }}
              </option>
            </select>
          </label>

          <label class="form-field">
            <span class="shipment-reference-label">
              <span>
                Shipment reference
                <strong class="shipment-required">*</strong>
              </span>
              <button
                type="button"
                class="shipment-auto-reference"
                @click="generateShipmentReference"
              >
                <span aria-hidden="true">⚡</span>
                Auto-generate
              </button>
            </span>
            <input
              v-model="form.trackingNumber"
              class="form-control"
              placeholder="e.g. SHP-2026-089"
            >
          </label>

          <label class="form-field">
            <span>
              Origin country
              <strong class="shipment-required">*</strong>
            </span>
            <select
              v-model="form.originCountry"
              class="form-select"
            >
              <option value="">
                Select country...
              </option>
              <option
                v-for="country in countries"
                :key="country.code"
                :value="country.code"
              >
                {{ country.name }}
              </option>
            </select>
          </label>

          <label class="form-field">
            <span>
              Expected arrival
              <strong class="shipment-required">*</strong>
            </span>
            <input
              v-model="form.expectedArrivalDate"
              class="form-control"
              type="date"
            >
          </label>
        </div>
      </section>

      <section class="shipment-freight-card">
        <button
          type="button"
          class="shipment-freight-header"
          @click="freightExpanded = !freightExpanded"
        >
          <span class="shipment-section-title">
            <span class="shipment-section-number muted">
              2
            </span>
            <strong>Freight & Vessel Details</strong>
            <span class="shipment-optional-badge">
              Optional — can be added later
            </span>
          </span>
          <span
            class="shipment-freight-chevron"
            :class="{ collapsed: !freightExpanded }"
            aria-hidden="true"
          >
            ⌃
          </span>
        </button>

        <div
          v-if="freightExpanded"
          class="shipment-freight-grid"
        >
          <label class="form-field">
            <span>Container number</span>
            <input
              v-model="form.containerNumber"
              class="form-control"
              placeholder="e.g. MSKU1234567"
            >
          </label>

          <label class="form-field">
            <span>Bill of Lading</span>
            <input
              v-model="form.billOfLadingNumber"
              class="form-control"
              placeholder="e.g. MEDU9876543"
            >
          </label>

          <label class="form-field">
            <span>Vessel</span>
            <input
              v-model="form.vesselName"
              class="form-control"
              placeholder="e.g. MSC Isabella"
            >
          </label>

          <label class="form-field">
            <span>Voyage</span>
            <input
              v-model="form.voyageNumber"
              class="form-control"
              placeholder="e.g. 614N"
            >
          </label>

          <label class="form-field">
            <span>Port of loading</span>
            <input
              v-model="form.portOfLoading"
              class="form-control"
              placeholder="e.g. Cape Town (ZACPT)"
            >
          </label>

          <label class="form-field">
            <span>Port of discharge</span>
            <input
              v-model="form.portOfDischarge"
              class="form-control"
              placeholder="e.g. JNPT (INNSA)"
            >
          </label>
        </div>
      </section>

      <section class="shipment-create-section shipment-lines-section">
        <div class="shipment-create-section-heading">
          <div class="shipment-section-title">
            <span class="shipment-section-number">3</span>
            <h3>Shipment Lines</h3>
            <span class="shipment-section-description">
              — Product, variety, batch, and expected quantity
            </span>
          </div>
        </div>

        <div class="shipment-line-table-wrap shipment-lines-desktop">
          <div class="shipment-line-table shipment-line-table-head">
            <span>Product</span>
            <span>Variety</span>
            <span>Batch code</span>
            <span>Crates</span>
            <span>Pack kg</span>
            <span>Unit price</span>
            <span>Line total</span>
            <span />
          </div>

          <div
            v-for="(line, index) in lines"
            :key="index"
            class="shipment-line-table shipment-line-table-row"
          >
            <select
              v-model="line.productId"
              class="form-select"
              :class="{
                'is-invalid':
                  lineFieldInvalid(
                    line,
                    'productId',
                  ),
              }"
              @change="productChanged(line)"
            >
              <option value="" disabled>
                Select product...
              </option>
              <option
                v-for="product in products"
                :key="product.id"
                :value="product.id"
              >
                {{ product.name }}
              </option>
            </select>

            <select
              v-model="line.productVarietyId"
              class="form-select"
              :class="{
                'is-invalid':
                  lineFieldInvalid(
                    line,
                    'productVarietyId',
                  ),
              }"
              :disabled="!line.productId"
            >
              <option value="" disabled>
                Select variety...
              </option>
              <option
                v-for="variety in line.varieties"
                :key="variety.id"
                :value="variety.id"
              >
                {{ variety.name }}
              </option>
            </select>

            <input
              v-model="line.batchCode"
              class="form-control"
              :class="{
                'is-invalid':
                  lineFieldInvalid(
                    line,
                    'batchCode',
                  ),
              }"
              placeholder="BATCH-01"
            >

            <input
              v-model.number="line.expectedQuantity"
              class="form-control shipment-number-input"
              type="number"
              min="1"
              placeholder="0"
            >

            <input
              v-model.number="line.packSizeKg"
              class="form-control shipment-number-input"
              type="number"
              min="0.001"
              step="0.001"
              placeholder="0"
            >

            <div class="shipment-price-input">
              <span>{{ currencySymbol() }}</span>
              <input
                v-model.number="line.unitCost"
                class="form-control"
                type="number"
                min="0"
                step="0.01"
                placeholder="0.00"
              >
            </div>

            <strong class="shipment-line-total">
              {{
                formatDraftMoney(
                  Math.max(
                    0,
                    Number(line.expectedQuantity) || 0,
                  ) *
                    Math.max(
                      0,
                      Number(line.unitCost) || 0,
                    ),
                )
              }}
            </strong>

            <button
              type="button"
              class="shipment-line-remove"
              :disabled="lines.length === 1"
              aria-label="Remove line"
              @click="removeLine(index)"
            >
              <span aria-hidden="true">×</span>
            </button>
          </div>
        </div>

        <div class="shipment-lines-mobile">
          <article
            v-for="(line, index) in lines"
            :key="index"
            class="shipment-line-mobile-card"
          >
            <header>
              <strong>Line {{ index + 1 }}</strong>
              <button
                type="button"
                class="shipment-line-remove"
                :disabled="lines.length === 1"
                aria-label="Remove line"
                @click="removeLine(index)"
              >
                <span aria-hidden="true">×</span>
              </button>
            </header>

            <div class="shipment-line-mobile-grid">
              <label class="form-field shipment-line-mobile-wide">
                <span>Product</span>
                <select
                  v-model="line.productId"
                  class="form-select"
                  :class="{
                    'is-invalid':
                      lineFieldInvalid(
                        line,
                        'productId',
                      ),
                  }"
                  @change="productChanged(line)"
                >
                  <option value="" disabled>
                    Select product...
                  </option>
                  <option
                    v-for="product in products"
                    :key="product.id"
                    :value="product.id"
                  >
                    {{ product.name }}
                  </option>
                </select>
              </label>

              <label class="form-field shipment-line-mobile-wide">
                <span>Variety</span>
                <select
                  v-model="line.productVarietyId"
                  class="form-select"
                  :class="{
                    'is-invalid':
                      lineFieldInvalid(
                        line,
                        'productVarietyId',
                      ),
                  }"
                  :disabled="!line.productId"
                >
                  <option value="" disabled>
                    Select variety...
                  </option>
                  <option
                    v-for="variety in line.varieties"
                    :key="variety.id"
                    :value="variety.id"
                  >
                    {{ variety.name }}
                  </option>
                </select>
              </label>

              <label class="form-field shipment-line-mobile-wide">
                <span>Batch code</span>
                <input
                  v-model="line.batchCode"
                  class="form-control"
                  :class="{
                    'is-invalid':
                      lineFieldInvalid(
                        line,
                        'batchCode',
                      ),
                  }"
                  placeholder="BATCH-01"
                >
              </label>

              <label class="form-field">
                <span>Crates</span>
                <input
                  v-model.number="line.expectedQuantity"
                  class="form-control"
                  type="number"
                  min="1"
                >
              </label>

              <label class="form-field">
                <span>Pack kg</span>
                <input
                  v-model.number="line.packSizeKg"
                  class="form-control"
                  type="number"
                  min="0.001"
                  step="0.001"
                >
              </label>

              <label class="form-field shipment-line-mobile-wide">
                <span>Unit price</span>
                <div class="shipment-price-input">
                  <span>{{ currencySymbol() }}</span>
                  <input
                    v-model.number="line.unitCost"
                    class="form-control"
                    type="number"
                    min="0"
                    step="0.01"
                  >
                </div>
              </label>
            </div>

            <p
              v-if="
                submitted &&
                lineValidationMessage(
                  line,
                  index,
                )
              "
              class="shipment-line-validation"
            >
              {{
                lineValidationMessage(
                  line,
                  index,
                )
              }}
            </p>

            <footer>
              <span>Line total</span>
              <strong>
                {{
                  formatDraftMoney(
                    Math.max(
                      0,
                      Number(line.expectedQuantity) || 0,
                    ) *
                      Math.max(
                        0,
                        Number(line.unitCost) || 0,
                      ),
                  )
                }}
              </strong>
            </footer>
          </article>
        </div>

        <div class="shipment-lines-actions">
          <div>
            <CButton
              color="primary"
              variant="outline"
              size="sm"
              @click="addLine"
            >
              <i class="cil-plus me-1" aria-hidden="true" />
              Add line
            </CButton>

            <CButton
              color="secondary"
              variant="outline"
              size="sm"
              @click="duplicateLastLine"
            >
              <i class="cil-copy me-1" aria-hidden="true" />
              Duplicate last line
            </CButton>
          </div>

          <small>
            Add as many product / variety lines as required for this consignment.
          </small>
        </div>
      </section>
    </main>

    <footer class="shipment-form-page-footer">
      <div class="shipment-create-totals">
        <span>
          <strong>{{ lines.length }}</strong>
          {{ lines.length === 1 ? 'line' : 'lines' }}
        </span>
        <span class="shipment-summary-dot">•</span>
        <span>
          <strong>{{ totalDraftCrates }}</strong>
          crates
        </span>
        <span class="shipment-summary-dot">•</span>
        <span>
          <strong>
            {{ totalDraftWeightKg.toLocaleString() }}
          </strong>
          kg
        </span>
        <span class="shipment-summary-dot">•</span>
        <span class="shipment-total-value">
          Total:
          <strong>
            {{ formatDraftMoney(totalDraftValue) }}
          </strong>
        </span>
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
          @click="createShipment"
        >
          <CSpinner
            v-if="saving"
            size="sm"
            class="me-2"
          />
          Create shipment
          <i
            v-if="!saving"
            class="cil-check-circle ms-2"
            aria-hidden="true"
          />
        </CButton>
      </div>
    </footer>
  </section>
</template>
