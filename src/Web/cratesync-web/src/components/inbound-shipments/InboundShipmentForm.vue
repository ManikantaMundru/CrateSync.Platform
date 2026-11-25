<script setup lang="ts">
import {
  computed,
  reactive,
  ref,
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
  packType: 'Weight' | 'Count'
  packValue: number
  unitCost: number | string
  varieties: ProductVariety[]
}

const props = defineProps<{
  suppliers: SupplierSummary[]
  products: ProductSummary[]
  canEnterCosts: boolean
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
    packType: 'Weight',
    packValue: 1,
    unitCost: '',
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

const totalDraftWeightKg = computed(() => lines.value.filter(l => l.packType === 'Weight').reduce((total, l) => total + Number(l.expectedQuantity || 0) * Number(l.packValue || 0), 0))
const totalDraftFruits = computed(() => lines.value.filter(l => l.packType === 'Count').reduce((total, l) => total + Number(l.expectedQuantity || 0) * Number(l.packValue || 0), 0))
const totalDraftValue = computed(() => lines.value.reduce((total, l) => total + Number(l.expectedQuantity || 0) * Number(l.unitCost || 0), 0))
const pendingCosts = computed(() => lines.value.some(l => l.unitCost === ''))

function addLine() {
  lines.value.push({
    productId: '',
    productVarietyId: '',
    batchCode: '',
    expectedQuantity: 1,
    packType: 'Weight',
    packValue: 1,
    unitCost: '',
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

  if (!Number.isInteger(Number(line.expectedQuantity)) || Number(line.expectedQuantity) <= 0) missing.push('positive whole pack quantity')
  if (!Number.isFinite(Number(line.packValue)) || Number(line.packValue) <= 0 || (line.packType === 'Count' && !Number.isInteger(Number(line.packValue)))) missing.push('valid pack size')
  if (props.canEnterCosts && line.unitCost !== '' && (!Number.isFinite(Number(line.unitCost)) || Number(line.unitCost) < 0)) missing.push('valid purchase cost')

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
            packSizeKg: line.packType === 'Weight' ? Number(line.packValue) : null,
            fruitsPerPack: line.packType === 'Count' ? Number(line.packValue) : null,
            ...(props.canEnterCosts && line.unitCost !== '' ? { unitCost: Number(line.unitCost), currency: shipmentCurrency.value } : {}),
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
          Record incoming supplier stock and freight details.
        </p>
      </div>


      <label v-if="canEnterCosts" class="shipment-currency-picker">
        <span>Purchase currency</span>
        <select v-model="shipmentCurrency" class="form-select">
          <option v-for="currency in ['USD', 'EUR', 'GBP', 'ZAR', 'INR', 'NZD', 'CLP', 'PLN']" :key="currency" :value="currency">{{ currency }}</option>
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
          <div class="shipment-line-table shipment-line-table-head operational-line-table" :class="{ costing: canEnterCosts }">
            <span>Product</span>
            <span>Variety</span>
            <span>Batch code</span>
            <span>Crates / boxes</span>
            <span>Pack type</span>
            <span>Per pack</span>
            <span v-if="canEnterCosts">Cost / pack</span>
            <span v-if="canEnterCosts">Line value</span>
            <span />
          </div>

          <div
            v-for="(line, index) in lines"
            :key="index"
            class="shipment-line-table shipment-line-table-row operational-line-table" :class="{ costing: canEnterCosts }"
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

            <select v-model="line.packType" class="form-select" aria-label="Pack quantity type">
              <option value="Weight">Kilograms</option><option value="Count">Fruit count</option>
            </select>
            <input v-model.number="line.packValue" class="form-control" type="number" :min="line.packType === 'Count' ? 1 : 0.001" :step="line.packType === 'Count' ? 1 : 0.001" aria-label="Quantity per pack">
            <input v-if="canEnterCosts" v-model.number="line.unitCost" class="form-control" type="number" min="0" step="0.01" placeholder="Pending" aria-label="Purchase cost per pack">
            <strong v-if="canEnterCosts">{{ line.unitCost === '' ? 'Pending' : shipmentCurrency + ' ' + (Number(line.unitCost) * Number(line.expectedQuantity)).toFixed(2) }}</strong>

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

              <label class="form-field"><span>Pack type</span><select v-model="line.packType" class="form-select"><option value="Weight">Kilograms</option><option value="Count">Fruit count</option></select></label>
              <label class="form-field"><span>Quantity per pack</span><input v-model.number="line.packValue" type="number" class="form-control" :min="line.packType === 'Count' ? 1 : 0.001" :step="line.packType === 'Count' ? 1 : 0.001"></label>
              <label v-if="canEnterCosts" class="form-field"><span>Purchase cost / pack ({{ shipmentCurrency }})</span><input v-model.number="line.unitCost" type="number" min="0" step="0.01" placeholder="Pending" class="form-control"></label>
              <span v-if="canEnterCosts">Line value: {{ line.unitCost === '' ? 'Pending' : shipmentCurrency + ' ' + (Number(line.unitCost) * Number(line.expectedQuantity)).toFixed(2) }}</span>


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
          crates / boxes
        </span>
        <span class="shipment-summary-dot">•</span>
        <span>
          <strong>
            {{ totalDraftWeightKg.toLocaleString() }}
          </strong>
          kg (weight-based packs)
        </span>
        <span v-if="totalDraftFruits">{{ totalDraftFruits.toLocaleString() }} fruits (count-based packs)</span>
        <span v-if="canEnterCosts">{{ pendingCosts ? 'Recorded subtotal' : 'Purchase total' }}: {{ shipmentCurrency }} {{ totalDraftValue.toFixed(2) }}</span>

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

<style scoped>
.operational-line-table { grid-template-columns: minmax(110px, 1.3fr) minmax(110px, 1.3fr) minmax(100px, 1fr) minmax(80px, .7fr) minmax(110px, 1fr) minmax(80px, .7fr) 40px; }
.operational-line-table.costing { grid-template-columns: minmax(110px, 1.3fr) minmax(110px, 1.3fr) minmax(100px, 1fr) minmax(80px, .7fr) minmax(110px, 1fr) minmax(80px, .7fr) minmax(100px, .9fr) minmax(110px, 1fr) 40px; min-width: 930px; }
</style>
