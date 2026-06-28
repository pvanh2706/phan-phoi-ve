import { apiRequest, type PagedResult } from './apiClient'
import type { PaymentType, ReconciliationStatus } from './formatters'

export interface ParkReconciliationDto {
  id: number
  businessDate: string
  previousBusinessDate?: string | null
  parkId: number
  parkCode: string
  parkName: string
  bankAccount?: string | null
  paymentType: PaymentType
  previousBalance?: number | null
  additionalAmount?: number | null
  usedAmount?: number | null
  expectedBalance?: number | null
  actualBalance?: number | null
  varianceAmount?: number | null
  adjustmentAmount?: number | null
  adjustmentNote?: string | null
  status: ReconciliationStatus
  missingPreviousBalance: boolean
  missingActualBalance: boolean
  missingTicketCost: boolean
  missingBankTransaction: boolean
  resolvedByUserId?: number | null
  resolvedAtUtc?: string | null
  sourceChangedAfterResolved: boolean
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface BuildReconciliationResultDto {
  jobRunId: number
  businessDate: string
  totalItems: number
  matchedCount: number
  varianceCount: number
  missingDataCount: number
  resolvedPreservedCount: number
}

export interface ReconciliationFilters {
  page?: number
  businessDate?: string
  dateFrom?: string
  dateTo?: string
  parkId?: number | ''
  paymentType?: PaymentType | ''
  status?: ReconciliationStatus | ''
  hasVariance?: boolean | ''
  keyword?: string
}

function buildQuery(filters: Record<string, string | number | undefined | null>) {
  const query = new URLSearchParams()
  for (const [key, value] of Object.entries(filters)) {
    if (value !== undefined && value !== null && value !== '') {
      query.set(key, String(value))
    }
  }
  return query.toString()
}

export function listReconciliations(filters: ReconciliationFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    businessDate: filters.businessDate,
    dateFrom: filters.dateFrom,
    dateTo: filters.dateTo,
    parkId: filters.parkId,
    paymentType: filters.paymentType,
    status: filters.status,
    hasVariance: typeof filters.hasVariance === 'boolean' ? String(filters.hasVariance) : undefined,
    keyword: filters.keyword,
  })
  return apiRequest<PagedResult<ParkReconciliationDto>>(`/reconciliations?${query}`)
}

export function buildReconciliation(businessDate: string) {
  return apiRequest<BuildReconciliationResultDto>('/reconciliations/build', {
    method: 'POST',
    body: JSON.stringify({ businessDate }),
  })
}

export function resolveReconciliation(id: number, adjustmentAmount: number, adjustmentNote: string) {
  return apiRequest<ParkReconciliationDto>(`/reconciliations/${id}/resolve`, {
    method: 'POST',
    body: JSON.stringify({ adjustmentAmount, adjustmentNote }),
  })
}
