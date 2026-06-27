import { apiRequest, type PagedResult } from './apiClient'

export interface ParkReconciliationDto {
  id: number
  businessDate: string
  parkCode: string
  parkName: string
  paymentType: 'Prepaid' | 'Debt'
  previousBalance?: number | null
  additionalAmount?: number | null
  usedAmount?: number | null
  expectedBalance?: number | null
  actualBalance?: number | null
  varianceAmount?: number | null
  adjustmentAmount?: number | null
  status: 'Matched' | 'Variance' | 'MissingData' | 'Resolved'
  sourceChangedAfterResolved: boolean
}

export function listReconciliations(page = 1, businessDate?: string) {
  const query = new URLSearchParams({ page: String(page) })
  if (businessDate) query.set('businessDate', businessDate)
  return apiRequest<PagedResult<ParkReconciliationDto>>(`/reconciliations?${query}`)
}

export function buildReconciliation(businessDate: string) {
  return apiRequest('/reconciliations/build', {
    method: 'POST',
    body: JSON.stringify({ businessDate }),
  })
}
