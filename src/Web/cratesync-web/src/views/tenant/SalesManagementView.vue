<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { CButton, CCard, CCardBody } from '@coreui/vue'
import { apiFetch } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
interface Warehouse { id: string; name: string }
interface Customer { id: string; name: string; isActive?: boolean; status?: string }
interface Stock { id: string; productVarietyId: string; productName: string; varietyName: string; batchId: string; batchCode: string | null; originCountry: string; fruitsPerPack: number | null; packWeightKg: number | null; available: number; sellingPrice: number | null; sellingCurrency: string | null; originalReceivedAtUtc: string | null }
interface Order { id: string; orderNumber: string; customerId: string; warehouseId: string; status: string | number; totalAmount: number; currency: string | null; reservationFailureReason: string | null }
interface Line { id: string; productVarietyId: string; batchId: string; batchCode: string | null; quantity: number; packSize: number; fruitsPerPack: number | null; unitPrice: number; currency: string; lineTotal: number; originCountry: string }
interface Detail extends Order { lines: Line[]; addressLine1: string; city: string; country: string }
const auth = useAuthStore()
const tenant = computed(() => auth.me?.currentTenant)
const canAdmin = (id: string) => tenant.value?.accessMode === 'SuperAdmin' || tenant.value?.roles.includes('TenantOwner') || tenant.value?.roles.includes('TenantAdmin') || tenant.value?.warehouses.some(w => w.warehouseId === id && w.role === 'WarehouseAdmin')
const warehouses = ref<Warehouse[]>([]), customers = ref<Customer[]>([]), orders = ref<Order[]>([]), stock = ref<Stock[]>([])
const selected = ref<Detail | null>(null), creating = ref(false), busy = ref(false), error = ref(''), filter = ref('')
const form = reactive({ customerId:'', warehouseId:'', orderNumber:'', addressLine1:'', addressLine2:'', city:'', province:'', postalCode:'', country:'India' })
const stockId = ref(''), quantity = ref<number | string>(1)
const status = (v: string | number) => typeof v === 'number' ? ['','Draft','Confirmed','Allocated','Dispatched','Fulfilled','Cancelled'][v] ?? String(v) : v
const filtered = computed(() => orders.value.filter(o => !filter.value || status(o.status) === filter.value))
const eligible = computed(() => stock.value.filter(s => s.available > 0 && s.batchCode && s.sellingPrice != null && s.sellingPrice > 0 && s.sellingCurrency && (s.fruitsPerPack != null || s.packWeightKg != null) && (!selected.value?.currency || selected.value.currency === s.sellingCurrency) && !selected.value?.lines.some(l=>l.batchId===s.batchId && l.productVarietyId===s.productVarietyId && l.originCountry===s.originCountry)).sort((a,b)=>(a.originalReceivedAtUtc ?? '9999').localeCompare(b.originalReceivedAtUtc ?? '9999') || a.id.localeCompare(b.id)))
const chosen = computed(() => eligible.value.find(s=>s.id===stockId.value))
const money = (n: number, c: string | null) => (c ?? '') + ' ' + n.toLocaleString(undefined,{minimumFractionDigits:2,maximumFractionDigits:2})
const customerName = (id: string) => customers.value.find(c=>c.id===id)?.name ?? id
const warehouseName = (id: string) => warehouses.value.find(w=>w.id===id)?.name ?? id
async function loadOrders() { orders.value=await apiFetch<Order[]>('/api/sales-orders') }
async function selectOrder(id: string) {
  const detail=await apiFetch<Detail>(`/api/sales-orders/${id}`)
  selected.value=detail; creating.value=false; stockId.value=''; quantity.value=1
  stock.value=status(detail.status)==='Draft' && canAdmin(detail.warehouseId) ? await apiFetch<Stock[]>(`/api/inventory?warehouseId=${detail.warehouseId}`) : []
}
async function run(work: ()=>Promise<void>) {
  if(busy.value)return
  busy.value=true;error.value=''
  try{await work()}catch(e){error.value=e instanceof Error?e.message:'Unable to complete request.'}finally{busy.value=false}
}
onMounted(()=>void run(async()=>{
  warehouses.value=await apiFetch<Warehouse[]>('/api/Warehouse')
  const all: Customer[]=[]
  for(let page=1;page<=100;page++){
    const result=await apiFetch<{items:Customer[];totalCount:number}>(`/api/Customer?page=${page}&pageSize=50`)
    all.push(...result.items)
    if(!result.items.length||all.length>=result.totalCount)break
  }
  customers.value=all
  await loadOrders()
}))
function newOrder(){selected.value=null;creating.value=true;Object.assign(form,{customerId:'',warehouseId:'',orderNumber:'SO-'+new Date().getFullYear()+'-'+crypto.randomUUID().slice(0,8).toUpperCase(),addressLine1:'',addressLine2:'',city:'',province:'',postalCode:'',country:'India'})}
async function create(){await run(async()=>{const result=await apiFetch<string>('/api/sales-orders',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(form)});await loadOrders();await selectOrder(result)})}
async function addLine(){const s=chosen.value;const o=selected.value;const q=Number(quantity.value);if(!s||!o||!Number.isInteger(q)||q<=0||q>s.available)return;await run(async()=>{await apiFetch(`/api/sales-orders/${o.id}/batch-lines`,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({productVarietyId:s.productVarietyId,originCountry:s.originCountry,batchId:s.batchId,quantity:q})});await loadOrders();await selectOrder(o.id)})}
async function removeLine(line:Line){const id=selected.value?.id;if(!id)return;await run(async()=>{await apiFetch(`/api/sales-orders/${id}/lines/${line.id}`,{method:'DELETE'});await loadOrders();await selectOrder(id)})}
async function transition(operation:string){const id=selected.value?.id;if(!id)return;await run(async()=>{await apiFetch(`/api/sales-orders/${id}/${operation}`,{method:'PATCH'});await loadOrders();await selectOrder(id)})}
</script>
<template>
  <section class="p-2">
    <div class="d-flex justify-content-between align-items-center mb-3"><div><h2>Sales orders</h2><p>Customer orders, batch allocation and dispatch</p></div><CButton v-if="warehouses.some(w=>canAdmin(w.id))" color="primary" @click="newOrder">New order</CButton></div>
    <p v-if="error" class="text-danger">{{error}}</p>
    <div class="row g-3">
      <div class="col-lg-7"><CCard><CCardBody>
        <label class="form-field">Status<select v-model="filter" class="form-select mb-2"><option value="">All statuses</option><option v-for="s in ['Draft','Confirmed','Allocated','Dispatched','Fulfilled','Cancelled']" :key="s">{{s}}</option></select></label>
        <div class="table-responsive"><table class="table table-hover"><thead><tr><th>Order</th><th>Customer</th><th>Warehouse</th><th>Status</th><th>Total</th></tr></thead><tbody><tr v-for="o in filtered" :key="o.id" :class="{'table-active':selected?.id===o.id}"><td><button class="btn btn-link p-0" :disabled="busy" @click="run(()=>selectOrder(o.id))">{{o.orderNumber}}</button></td><td>{{customerName(o.customerId)}}</td><td>{{warehouseName(o.warehouseId)}}</td><td>{{status(o.status)}}</td><td>{{money(o.totalAmount,o.currency)}}</td></tr></tbody></table></div>
        <p v-if="!filtered.length">No orders found.</p>
      </CCardBody></CCard></div>
      <div class="col-lg-5"><CCard><CCardBody>
        <form v-if="creating" @submit.prevent="create">
          <h3>New draft order</h3>
          <label class="form-field">Customer<select v-model="form.customerId" required class="form-select"><option value="">Select customer</option><option v-for="c in customers.filter(c=>c.isActive!==false && c.status!=='Inactive')" :key="c.id" :value="c.id">{{c.name}}</option></select></label>
          <label class="form-field">Warehouse<select v-model="form.warehouseId" required class="form-select"><option value="">Select warehouse</option><option v-for="w in warehouses.filter(w=>canAdmin(w.id))" :key="w.id" :value="w.id">{{w.name}}</option></select></label>
          <label class="form-field">Order number<input v-model="form.orderNumber" required maxlength="50" class="form-control"></label>
          <label class="form-field">Delivery address<input v-model="form.addressLine1" required maxlength="200" class="form-control"></label>
          <label class="form-field">Address line 2<input v-model="form.addressLine2" maxlength="200" class="form-control"></label>
          <label class="form-field">City<input v-model="form.city" required maxlength="100" class="form-control"></label>
          <label class="form-field">Province<input v-model="form.province" maxlength="100" class="form-control"></label>
          <label class="form-field">Postal code<input v-model="form.postalCode" maxlength="20" class="form-control"></label>
          <label class="form-field">Country<input v-model="form.country" required maxlength="100" class="form-control"></label>
          <CButton type="submit" color="primary" :disabled="busy">Create draft</CButton> <CButton color="secondary" variant="outline" @click="creating=false">Cancel</CButton>
        </form>
        <template v-else-if="selected">
          <h3>{{selected.orderNumber}}</h3><p>{{customerName(selected.customerId)}} · {{warehouseName(selected.warehouseId)}} · {{status(selected.status)}}</p>
          <p>{{selected.addressLine1}}, {{selected.city}}, {{selected.country}}</p>
          <p v-if="selected.reservationFailureReason" class="text-danger">{{selected.reservationFailureReason}}</p>
          <h4>Order lines</h4>
          <div v-for="l in selected.lines" :key="l.id" class="border rounded p-2 mb-2"><strong>{{l.batchCode || 'Legacy'}} · {{l.originCountry}}</strong><div>{{l.fruitsPerPack != null ? l.fruitsPerPack+' fruits/pack' : l.packSize+' kg/pack'}} · {{l.quantity}} packages</div><div>{{money(l.unitPrice,l.currency)}} / package · {{money(l.lineTotal,l.currency)}}</div><CButton v-if="status(selected.status)==='Draft' && canAdmin(selected.warehouseId)" size="sm" color="danger" variant="outline" :disabled="busy" @click="removeLine(l)">Remove</CButton></div>
          <template v-if="status(selected.status)==='Draft' && canAdmin(selected.warehouseId)">
            <label class="form-field">Available priced batch (oldest known first)<select v-model="stockId" class="form-select"><option value="">Select batch</option><option v-for="s in eligible" :key="s.id" :value="s.id">{{s.productName}} / {{s.varietyName}} · {{s.batchCode}} · {{s.originCountry}} · {{s.fruitsPerPack ?? s.packWeightKg}} {{s.fruitsPerPack != null?'fruits':'kg'}} · {{s.available}} available · {{money(s.sellingPrice!,s.sellingCurrency)}}</option></select></label>
            <label class="form-field">Packages<input v-model.number="quantity" type="number" min="1" :max="chosen?.available" step="1" class="form-control"></label>
            <CButton size="sm" :disabled="busy||!chosen||Number(quantity)<=0||Number(quantity)>(chosen?.available??0)" @click="addLine">Add batch</CButton>
          </template>
          <hr><strong>Total: {{money(selected.totalAmount,selected.currency)}}</strong>
          <div v-if="canAdmin(selected.warehouseId)" class="d-flex flex-wrap gap-2 mt-3">
            <CButton v-if="status(selected.status)==='Draft'" :disabled="busy||!selected.lines.length" @click="transition('confirm')">Confirm and reserve</CButton>
            <CButton v-if="status(selected.status)==='Allocated'" :disabled="busy" @click="transition('dispatch')">Dispatch</CButton>
            <CButton v-if="status(selected.status)==='Dispatched'" :disabled="busy" @click="transition('fulfil')">Complete order</CButton>
            <CButton v-if="['Draft','Confirmed','Allocated'].includes(status(selected.status))" color="danger" variant="outline" :disabled="busy" @click="transition('cancel')">Cancel order</CButton>
          </div>
        </template>
        <p v-else>Select an order or create a new draft.</p>
      </CCardBody></CCard></div>
    </div>
  </section>
</template>
