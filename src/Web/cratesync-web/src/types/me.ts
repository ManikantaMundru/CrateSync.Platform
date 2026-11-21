export interface WarehouseAccess {
  warehouseId: string
  role: string
}

export interface TenantAccess {
  tenantId: string
  name: string
  status: string
  roles: string[]
  warehouses: WarehouseAccess[]
  path: string
}

export interface MeResponse {
  userId: string
  displayName: string
  email: string | null
  platform: {
    role: string | null
    canAccessAdminPortal: boolean
    adminPath: string
  }
  currentTenantId: string | null
  actingAsPlatformAdmin: boolean
  tenants: TenantAccess[]
}
