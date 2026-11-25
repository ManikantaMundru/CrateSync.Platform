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

interface WarehouseSummary {
  id: string
  name: string
  code: string
  isActive: boolean
}

interface ShipmentLine {
  id: string
  batchCode: string
  expectedQuantity: number
  receivedQuantity: number
}

interface DeliveryAllocation {
  shipmentLineId: string
  quantity: number
  damagedQuantity?: number | null
  missingQuantity?: number | null
}

interface ShipmentDelivery {
  id: string
  status: string
  allocations: DeliveryAllocation[]
}

interface ShipmentForDelivery {
  id: string
  trackingNumber: string
  portOfDischarge: string | null
  lines: ShipmentLine[]
  deliveries: ShipmentDelivery[] | null
}

const props = defineProps<{
  shipment: ShipmentForDelivery
  warehouses: WarehouseSummary[]
}>()

const emit = defineEmits<{
  cancel: []
  created: [deliveryId: string]
}>()

const saving = ref(false)
const error = ref<string | null>(null)

const form = reactive({
  warehouseId: '',
  transportCompany: '',
  driverName: '',
  driverPhone: '',
  vehicleNumber: '',
  originLocation:
    props.shipment.portOfDischarge ||
    'Port / clearance location',
  destinationLocation: '',
  transportReference: '',
  remarks: '',
})

const quantities = reactive<Record<string, number>>(
  Object.fromEntries(
    props.shipment.lines.map((line) => [
      line.id,
      0,
    ]),
  ),
)

const warehouseById = computed(
  () =>
    new Map(
      props.warehouses.map((warehouse) => [
        warehouse.id,
        warehouse,
      ]),
    ),
)

const selectedWarehouse = computed(() =>
  warehouseById.value.get(form.warehouseId),
)

const alreadyAllocated = (lineId: string) =>
  (props.shipment.deliveries ?? [])
    .filter((delivery) =>
      ['Planned', 'InTransit', 'DeliveredToWarehouse'].includes(
        delivery.status,
      ),
    )
    .flatMap((delivery) => delivery.allocations)
    .filter(
      (allocation) =>
        allocation.shipmentLineId === lineId,
    )
    .reduce(
      (total, allocation) =>
        total + allocation.quantity,
      0,
    )

const availableForAllocation = (
  line: ShipmentLine,
) =>
  Math.max(
    0,
    line.expectedQuantity -
      line.receivedQuantity -
      alreadyAllocated(line.id) - (props.shipment.deliveries ?? []).flatMap(d => d.allocations).filter(a => a.shipmentLineId === line.id).reduce((total, a) => total + (a.damagedQuantity ?? 0) + (a.missingQuantity ?? 0), 0),
  )

const allocations = computed(() =>
  props.shipment.lines
    .map((line) => ({
      shipmentLineId: line.id,
      quantity:
        Number(quantities[line.id]) || 0,
    }))
    .filter((line) => line.quantity > 0),
)

const totalAllocated = computed(() =>
  allocations.value.reduce(
    (total, line) =>
      total + line.quantity,
    0,
  ),
)

function warehouseChanged() {
  const warehouse =
    selectedWarehouse.value

  if (
    warehouse &&
    !form.destinationLocation.trim()
  ) {
    form.destinationLocation =
      `${warehouse.name} (${warehouse.code})`
  }
}

function generateReference() {
  const suffix =
    crypto.randomUUID()
      .replaceAll('-', '')
      .slice(0, 6)
      .toUpperCase()

  form.transportReference =
    `DLV-${new Date().getFullYear()}-${suffix}`
}

async function createDelivery() {
  if (
    !form.warehouseId ||
    !form.transportCompany.trim() ||
    !form.driverName.trim() ||
    !form.driverPhone.trim() ||
    !form.originLocation.trim() ||
    !form.destinationLocation.trim()
  ) {
    error.value =
      'Warehouse, transporter, driver, phone, origin and destination are required.'
    return
  }

  if (allocations.value.length === 0) {
    error.value =
      'Allocate at least one shipment line to this delivery.'
    return
  }

  for (const line of props.shipment.lines) {
    const quantity =
      Number(quantities[line.id]) || 0

    if (
      quantity >
      availableForAllocation(line)
    ) {
      error.value =
        `Batch ${line.batchCode} has only ${availableForAllocation(line)} crates available for allocation.`
      return
    }
  }

  saving.value = true
  error.value = null

  try {
    const result =
      await apiFetch<{ id: string }>(
        `/api/InboundShipment/${props.shipment.id}/warehouse-deliveries`,
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            warehouseId:
              form.warehouseId,
            transportCompany:
              form.transportCompany.trim(),
            driverName:
              form.driverName.trim(),
            driverPhone:
              form.driverPhone.trim(),
            vehicleNumber:
              form.vehicleNumber.trim() ||
              null,
            originLocation:
              form.originLocation.trim(),
            destinationLocation:
              form.destinationLocation.trim(),
            transportReference:
              form.transportReference.trim() ||
              null,
            remarks:
              form.remarks.trim() || null,
            allocations:
              allocations.value,
          }),
        },
      )

    emit('created', result.id)
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to create warehouse delivery.'
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
            aria-label="Back to shipment"
            @click="emit('cancel')"
          >
            ←
          </button>
          <h1>Allocate to warehouse</h1>
        </div>
        <p>
          Plan one port-to-warehouse truck leg for
          {{ shipment.trackingNumber }}.
        </p>
      </div>
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
            <h3>Destination & transport</h3>
          </div>
        </div>

        <div class="shipment-core-grid">
          <label class="form-field">
            <span>Destination warehouse *</span>
            <select
              v-model="form.warehouseId"
              class="form-select"
              @change="warehouseChanged"
            >
              <option value="" disabled>
                Select warehouse...
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

          <label class="form-field">
            <span>Transport company *</span>
            <input
              v-model="form.transportCompany"
              class="form-control"
              placeholder="e.g. ABC Logistics"
            >
          </label>

          <label class="form-field">
            <span>Driver name *</span>
            <input
              v-model="form.driverName"
              class="form-control"
            >
          </label>

          <label class="form-field">
            <span>Driver phone *</span>
            <input
              v-model="form.driverPhone"
              class="form-control"
              type="tel"
            >
          </label>

          <label class="form-field">
            <span>Vehicle / truck number</span>
            <input
              v-model="form.vehicleNumber"
              class="form-control"
              placeholder="e.g. CA 123-456"
            >
          </label>

          <label class="form-field">
            <span class="shipment-reference-label">
              <span>Transport reference</span>
              <button
                type="button"
                class="shipment-auto-reference"
                @click="generateReference"
              >
                ⚡ Auto-generate
              </button>
            </span>
            <input
              v-model="form.transportReference"
              class="form-control"
              placeholder="e.g. DLV-2026-A1B2C3"
            >
          </label>

          <label class="form-field">
            <span>From *</span>
            <input
              v-model="form.originLocation"
              class="form-control"
              placeholder="Port / cold store / clearance depot"
            >
          </label>

          <label class="form-field">
            <span>To *</span>
            <input
              v-model="form.destinationLocation"
              class="form-control"
              placeholder="Warehouse / delivery address"
            >
          </label>
        </div>
      </section>

      <section class="shipment-create-section">
        <div class="shipment-create-section-heading">
          <div class="shipment-section-title">
            <span class="shipment-section-number">2</span>
            <h3>Allocate shipment lines</h3>
          </div>
        </div>

        <div class="delivery-allocation-list">
          <article
            v-for="line in shipment.lines"
            :key="line.id"
            class="delivery-allocation-row"
          >
            <div>
              <strong>{{ line.batchCode }}</strong>
              <small>
                {{ line.receivedQuantity }} received ·
                {{ alreadyAllocated(line.id) }} already allocated ·
                {{ availableForAllocation(line) }} available
              </small>
            </div>

            <label>
              <span>Crates</span>
              <input
                v-model.number="quantities[line.id]"
                class="form-control"
                type="number"
                min="0"
                :max="availableForAllocation(line)"
              >
            </label>
          </article>
        </div>
      </section>

      <section class="shipment-create-section">
        <div class="shipment-create-section-heading">
          <div class="shipment-section-title">
            <span class="shipment-section-number">3</span>
            <h3>Notes</h3>
          </div>
        </div>

        <label class="form-field">
          <span>Remarks</span>
          <textarea
            v-model="form.remarks"
            class="form-control"
            rows="3"
            maxlength="500"
            placeholder="Loading instructions, seal number, transport notes..."
          />
        </label>
      </section>
    </main>

    <footer class="shipment-form-page-footer">
      <div class="inventory-transfer-footer-summary">
        <span>
          {{ selectedWarehouse?.name || 'No warehouse selected' }}
        </span>
        <strong v-if="totalAllocated > 0">
          {{ totalAllocated.toLocaleString() }}
          crates allocated
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
          @click="createDelivery"
        >
          <CSpinner
            v-if="saving"
            size="sm"
            class="me-2"
          />
          Create delivery
        </CButton>
      </div>
    </footer>
  </section>
</template>
