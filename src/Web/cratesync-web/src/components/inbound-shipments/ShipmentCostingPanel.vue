<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { CButton } from '@coreui/vue'
import { apiFetch } from '@/services/api'
const props = defineProps<{ shipmentId: string; status: string; lines: { id: string; batchCode: string }[]; deliveries: { id: string; status: string; transportReference?: string | null }[]; canEdit: boolean }>()
interface CostLine { lineId: string; batchCode: string; expected: number; accepted: number; sampled: number; damaged: number; missing: number; purchaseCost: number | null; allocatedExpenses: number; totalCost: number | null; effectiveCostPerAcceptedPackage: number | null }
interface Expense { id: string; category: string; amount: number; currency: string; exchangeRate: number; deliveryId: string | null; lineId: string | null; description: string; reference: string | null; reversedAtUtc: string | null; reversalReason: string | null }
interface Costing { currency: string | null; isComplete: boolean; totalPurchaseCost: number | null; totalExpenses: number; totalLandedCost: number | null; warnings: string[]; lines: CostLine[]; expenses: Expense[]; exchangeRates: { currency: string; rate: number }[] }
const data = ref<Costing | null>(null)
const busy = ref(false), error = ref('')
const currency = ref('INR'), foreign = ref('USD'), rate = ref<number | string>('')
const category = ref('OceanFreight'), amount = ref<number | string>(''), expenseCurrency = ref('INR'), expenseRate = ref<number | string>(1)
const scope = ref('shipment'), target = ref(''), reference = ref(''), description = ref('')
const expenseId = ref(crypto.randomUUID())
const reversalId = ref(''), reversalReason = ref('')
const editable = computed(() => props.canEdit && props.status !== 'Cancelled')
const categories = ['OceanFreight','Insurance','CustomsDuty','Clearance','PortCharges','RegulatoryTesting','InlandTransport','Handling','Storage','Other']
const money = (n: number | null | undefined) => n == null ? 'Pending' : (data.value?.currency ?? '') + ' ' + n.toLocaleString(undefined,{minimumFractionDigits:2,maximumFractionDigits:2})
async function load() {
  const id = props.shipmentId
  error.value = ''
  try {
    const result = await apiFetch<Costing>(`/api/InboundShipment/${id}/costing`)
    if (id !== props.shipmentId) return
    data.value = result
    currency.value = result.currency ?? 'INR'
    expenseCurrency.value = result.currency ?? 'INR'
    expenseRate.value = 1
  } catch (e) { if (id === props.shipmentId) error.value = e instanceof Error ? e.message : 'Unable to load costing.' }
}
watch(() => props.shipmentId, () => { data.value = null; expenseId.value = crypto.randomUUID(); amount.value = ''; description.value = ''; reference.value = ''; target.value = ''; reversalId.value = ''; void load() }, { immediate: true })
watch(scope, () => { target.value = '' })
watch(expenseCurrency, value => { expenseRate.value = value.toUpperCase() === data.value?.currency ? 1 : data.value?.exchangeRates.find(r => r.currency === value.toUpperCase())?.rate ?? '' })
async function submit(action: string, payload: object) {
  if (!editable.value || busy.value) return false
  busy.value = true; error.value = ''
  try {
    await apiFetch(`/api/InboundShipment/${props.shipmentId}/costing/${action}`, {method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(payload)})
    await load()
    return true
  } catch (e) { error.value = e instanceof Error ? e.message : 'Unable to update costing.'; return false }
  finally { busy.value = false }
}
async function saveExpense() {
  if (await submit('expense',{expenseId:expenseId.value,category:category.value,amount:Number(amount.value),currency:expenseCurrency.value,rate:Number(expenseRate.value),lineId:scope.value==='line'?target.value:null,deliveryId:scope.value==='delivery'?target.value:null,reference:reference.value||null,description:description.value})) {
    expenseId.value=crypto.randomUUID(); amount.value=''; description.value=''; reference.value=''
  }
}
async function reverse() {
  if (await submit('reverse',{expenseId:reversalId.value,description:reversalReason.value})) { reversalId.value=''; reversalReason.value='' }
}
</script>
<template>
  <section class="shipment-detail-section">
    <div class="section-heading"><div><h3>Confidential shipment costing</h3><p>Purchase costs, expenses and effective cost per accepted package</p></div><CButton size="sm" variant="outline" @click="load">Refresh</CButton></div>
    <p v-if="error" class="text-danger">{{ error }}</p>
    <template v-if="data">
      <div class="border rounded p-2 mb-2">
        <strong>{{ data.isComplete ? 'Calculated from completed receipt' : 'Provisional / incomplete' }}</strong>
        <div>Purchase: {{ money(data.totalPurchaseCost) }}</div><div>Expenses: {{ money(data.totalExpenses) }}</div><div>Landed cost: {{ money(data.totalLandedCost) }}</div>
        <small v-for="warning in data.warnings" :key="warning" class="d-block text-warning">{{ warning }}</small>
      </div>
      <div v-if="editable" class="border rounded p-2 mb-2">
        <label class="form-field">Costing currency<input v-model.trim="currency" maxlength="3" class="form-control" :disabled="data.expenses.length > 0 || data.exchangeRates.length > 0"></label>
        <CButton size="sm" :disabled="busy || currency.length !== 3 || data.expenses.length > 0 || data.exchangeRates.length > 0" @click="submit('currency',{currency})">Set currency</CButton>
        <template v-if="data.currency">
          <label class="form-field mt-2">Purchase currency<input v-model.trim="foreign" maxlength="3" class="form-control"></label>
          <label class="form-field">Exchange rate ({{ data.currency }} per unit of foreign currency)<input v-model.number="rate" type="number" min="0.00000001" step="0.00000001" class="form-control"></label>
          <CButton size="sm" :disabled="busy || Number(rate)<=0 || foreign.length!==3" @click="submit('rate',{currency:foreign,rate:Number(rate)})">Save rate</CButton>
        </template>
      </div>
      <div v-for="r in data.exchangeRates" :key="r.currency" class="small">1 {{ r.currency }} = {{ r.rate }} {{ data.currency }}</div>
      <h4 class="mt-3">Batch landed costs</h4>
      <div v-for="line in data.lines" :key="line.lineId" class="border rounded p-2 mb-2">
        <strong>{{ line.batchCode }}</strong><div>{{ line.expected }} imported · {{ line.accepted }} accepted · {{ line.sampled }} sampled · {{ line.damaged }} damaged · {{ line.missing }} missing</div>
        <small class="d-block">Purchase: {{ money(line.purchaseCost) }} · Expenses: {{ money(line.allocatedExpenses) }}</small>
        <div>Total: {{ money(line.totalCost) }}</div><strong>Per accepted package: {{ money(line.effectiveCostPerAcceptedPackage) }}</strong>
      </div>
      <h4 class="mt-3">Expenses</h4>
      <form v-if="editable && data.currency" class="border rounded p-2 mb-2" @submit.prevent="saveExpense">
        <label class="form-field">Category<select v-model="category" class="form-select"><option v-for="c in categories" :key="c">{{ c }}</option></select></label>
        <label class="form-field">Amount<input v-model.number="amount" type="number" min="0.01" step="0.01" required class="form-control"></label>
        <label class="form-field">Currency<input v-model.trim="expenseCurrency" maxlength="3" required class="form-control"></label>
        <label class="form-field">Exchange rate to {{ data.currency }}<input v-model.number="expenseRate" type="number" min="0.00000001" step="0.00000001" required class="form-control"></label>
        <label class="form-field">Allocation scope<select v-model="scope" class="form-select"><option value="shipment">Entire shipment</option><option value="line">Specific batch</option><option value="delivery">Specific port delivery</option></select></label>
        <label v-if="scope==='line'" class="form-field">Batch<select v-model="target" required class="form-select"><option value="">Select</option><option v-for="l in lines" :key="l.id" :value="l.id">{{ l.batchCode }}</option></select></label>
        <label v-if="scope==='delivery'" class="form-field">Delivery<select v-model="target" required class="form-select"><option value="">Select</option><option v-for="d in deliveries.filter(x=>x.status!=='Cancelled')" :key="d.id" :value="d.id">{{ d.transportReference || d.id }}</option></select></label>
        <label class="form-field">Invoice/reference<input v-model="reference" maxlength="100" class="form-control"></label>
        <label class="form-field">Description<textarea v-model="description" required maxlength="500" class="form-control"/></label>
        <CButton type="submit" size="sm" :disabled="busy">Record expense</CButton>
      </form>
      <div v-for="e in data.expenses" :key="e.id" class="border rounded p-2 mb-2">
        <strong>{{ e.category }} · {{ e.currency }} {{ e.amount.toFixed(2) }}</strong><div>{{ e.description }} · {{ e.reference || 'No reference' }}</div>
        <small>{{ money(e.amount*e.exchangeRate) }} · {{ e.lineId ? 'Batch' : e.deliveryId ? 'Delivery' : 'Entire shipment' }}</small>
        <div v-if="e.reversedAtUtc" class="text-warning">Reversed · {{ e.reversalReason }}</div>
        <CButton v-else-if="editable" size="sm" variant="outline" color="warning" @click="reversalId=e.id; reversalReason=''">Reverse expense</CButton>
        <div v-if="reversalId===e.id"><textarea v-model="reversalReason" maxlength="500" placeholder="Reversal reason" class="form-control"/><CButton size="sm" :disabled="busy||!reversalReason.trim()" @click="reverse">Confirm reversal</CButton><CButton size="sm" variant="outline" @click="reversalId=''">Cancel</CButton></div>
      </div>
    </template>
  </section>
</template>
