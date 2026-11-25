<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  role: string
  warehouseIds: string[]
  warehouses: { id: string; name: string; code: string; city: string; isActive: boolean }[]
  canAssignFinancial: boolean
  locked?: boolean
}>()
const emit = defineEmits<{
  'update:role': [value: string]
  'update:warehouseIds': [value: string[]]
}>()
const options = [
  { id: 'TenantOwner', label: 'Tenant Owner', description: 'Full tenant operations and financial control.' },
  { id: 'TenantAdmin', label: 'Tenant Administrator', description: 'Tenant-wide operations and operational user management. No confidential costs or margins.' },
  { id: 'Finance', label: 'Finance', description: 'Financial access and read-only operational data.' },
  { id: 'WarehouseAdmin', label: 'Warehouse Administrator', description: 'Assigned warehouse stock, outbound transfers, shipment receipt, customers and sales. No purchase costs.' },
  { id: 'WarehouseUser', label: 'Warehouse User', description: 'Limited warehouse access; no warehouse administration.' },
  { id: 'Viewer', label: 'Viewer', description: 'Read-only tenant-wide operational data.' },
]
const available = computed(() => options.filter(o => props.canAssignFinancial || !['TenantOwner', 'Finance'].includes(o.id)))
const selected = computed(() => options.find(o => o.id === props.role))
const scoped = computed(() => ['WarehouseAdmin', 'WarehouseUser'].includes(props.role))
function changeRole(event: Event) {
  const value = (event.target as HTMLSelectElement).value
  if (!['WarehouseAdmin', 'WarehouseUser'].includes(value)) emit('update:warehouseIds', [])
  emit('update:role', value)
}
function toggle(id: string, event: Event) {
  const checked = (event.target as HTMLInputElement).checked
  emit('update:warehouseIds', checked ? [...new Set([...props.warehouseIds, id])] : props.warehouseIds.filter(x => x !== id))
}
</script>

<template>
  <div class="access-form">
    <label class="form-field">
      <span>Primary role *</span>
      <select class="form-select" :value="role" :disabled="locked" required @change="changeRole">
        <option value="" disabled>Select a role</option>
        <option v-for="option in available" :key="option.id" :value="option.id">{{ option.label }}</option>
      </select>
    </label>
    <p v-if="selected" class="role-description">{{ selected.description }}</p>
    <p v-if="locked" class="role-description">Your own access must be changed by another authorized administrator.</p>
    <template v-if="scoped">
      <strong>Warehouse assignments *</strong>
      <small>Select at least one active warehouse.</small>
      <p v-if="!warehouses.some(w => w.isActive)">No active warehouses available.</p>
      <label v-for="warehouse in warehouses.filter(w => w.isActive)" :key="warehouse.id" class="warehouse-option">
        <input type="checkbox" :checked="warehouseIds.includes(warehouse.id)" :disabled="locked" @change="toggle(warehouse.id, $event)">
        <span><strong>{{ warehouse.name }}</strong><small>{{ warehouse.code }} · {{ warehouse.city }}</small></span>
      </label>
    </template>
    <p v-else-if="selected" class="role-description"><strong>Warehouse scope:</strong> {{ ['Finance', 'Viewer'].includes(role) ? 'All warehouses — read-only' : 'All warehouses — operational control' }}</p>
    <div v-if="selected" class="permission-preview">
      <strong>Access summary</strong>
      <span>Confidential costs and margins: {{ ['TenantOwner', 'Finance'].includes(role) ? 'Allowed' : 'Restricted' }}</span>
      <span>User management: {{ ['TenantOwner', 'TenantAdmin'].includes(role) ? 'Allowed' : 'Restricted' }}</span>
      <span>Warehouse operations: {{ scoped ? 'Assigned warehouses' : ['Finance', 'Viewer'].includes(role) ? 'Read-only' : 'Tenant-wide' }}</span>
    </div>
  </div>
</template>

<style scoped>
.access-form { display: grid; gap: .75rem; }
.role-description, .permission-preview { padding: .8rem; border: 1px solid var(--cui-border-color, #d1d5db); border-radius: .5rem; margin: 0; }
.permission-preview { display: grid; gap: .35rem; font-size: .875rem; }
.warehouse-option { display: flex; align-items: center; gap: .75rem; border: 1px solid var(--cui-border-color, #d1d5db); border-radius: .5rem; padding: .75rem; }
.warehouse-option span { display: grid; gap: .2rem; }
</style>
