export interface TradingPartnerSummary {
  id: string
  name: string
  commercialIdentifier: string | null
  isActive: boolean
  addressLine1: string | null
  city: string | null
  stateOrProvince: string | null
  country: string | null
  contactName: string | null
  email: string | null
  phone: string | null
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
