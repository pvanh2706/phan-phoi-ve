import { apiRequest } from './apiClient'
import type { SourceType } from './formatters'

export interface AgencyBookingDto {
  id: number
  bookingCode: string
  bookingDate: string
  buyingAgentId?: number | null
  isAgencyMatched: boolean
  buyingAgentCode: string
  buyingAgentName?: string | null
  parentBuyingAgentCode?: string | null
  parentBuyingAgentName?: string | null
  sellingAgentCode?: string | null
  sellingAgentName?: string | null
  parkExternalCode?: string | null
  parkExternalName?: string | null
  ticketTypeCode?: string | null
  ticketTypeName?: string | null
  ticketGroupName?: string | null
  quantity: number
  unitPrice: number
  salesAmount: number
  costAmount: number
  subtotal: number
  discount: number
  sourceSystem: string
  sourceTransactionId: string
  sourceType: SourceType
  createdAtUtc: string
  updatedAtUtc?: string | null
  syncedAtUtc: string
}

export interface AgencyBookingListResult {
  items: AgencyBookingDto[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  totalAmount: number
}

export interface AgencyBookingSyncResult {
  businessDate: string
  totalLines: number
  matchedParent: number
  inserted: number
  updated: number
  skipped: number
  unmatchedAgency: number
  unmatchedAgencyCodes: string[]
  parentAgencyCode: string
}

export interface AgencyBookingFilters {
  page?: number
  dateFrom?: string
  dateTo?: string
  agencyId?: number | ''
  keyword?: string
}

export function listAgencyTaTransactions(filters: AgencyBookingFilters = {}) {
  const query = new URLSearchParams({ page: String(filters.page ?? 1) })
  if (filters.dateFrom) query.set('dateFrom', filters.dateFrom)
  if (filters.dateTo) query.set('dateTo', filters.dateTo)
  if (filters.agencyId !== undefined && filters.agencyId !== '') {
    query.set('agencyId', String(filters.agencyId))
  }
  if (filters.keyword?.trim()) {
    query.set('keyword', filters.keyword.trim())
  }
  return apiRequest<AgencyBookingListResult>(`/agency-ta-transactions?${query}`)
}

export function syncAgencyTaTransactions(businessDate: string) {
  return apiRequest<AgencyBookingSyncResult>('/agency-ta-transactions/sync', {
    method: 'POST',
    body: JSON.stringify({ businessDate }),
  })
}
