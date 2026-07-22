import { apiRequest } from './apiClient'

export interface AgencyArTransactionDto {
  id: number
  bookingId: string
  travelAgentName?: string | null
  transactionDate: string
  amount: number
  travelAgentCode?: string | null
  receivableAccountCode?: string | null
  description: string
  businessDate: string
  syncedAtUtc: string
}

export interface AgencyArTransactionListResult {
  items: AgencyArTransactionDto[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  totalAmount: number
}

export interface AgencyArTransactionSyncResult {
  businessDate: string
  totalRows: number
  skippedHeaderRows: number
  rowsWithDescription: number
  validBookingTransactions: number
  skippedNonBookingRows: number
  errorRows: number
  inserted: number
  updated: number
  unchanged: number
  warnings: string[]
}

export interface AgencyArTransactionFilters {
  page?: number
  dateFrom?: string
  dateTo?: string
  travelAgentCode?: string
  keyword?: string
}

export function listAgencyArTransactions(filters: AgencyArTransactionFilters = {}) {
  const query = new URLSearchParams({ page: String(filters.page ?? 1) })
  if (filters.dateFrom) query.set('dateFrom', filters.dateFrom)
  if (filters.dateTo) query.set('dateTo', filters.dateTo)
  if (filters.travelAgentCode?.trim()) query.set('travelAgentCode', filters.travelAgentCode.trim())
  if (filters.keyword?.trim()) query.set('keyword', filters.keyword.trim())
  return apiRequest<AgencyArTransactionListResult>(`/agency-ar-transactions?${query}`)
}

export function syncAgencyArTransactions(businessDate: string) {
  return apiRequest<AgencyArTransactionSyncResult>('/agency-ar-transactions/sync', {
    method: 'POST',
    body: JSON.stringify({ businessDate }),
  })
}
