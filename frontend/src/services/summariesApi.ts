import { apiRequest, type PagedResult } from './apiClient'
import type { BankTransactionType, PaymentType, SourceType } from './formatters'

export interface SummaryFilters {
  page?: number
  businessDate?: string
  parkId?: number | ''
  paymentType?: PaymentType | ''
  sourceType?: SourceType | ''
}

export interface BankSummaryFilters extends SummaryFilters {
  transactionType?: BankTransactionType | ''
}

export interface DailyParkBalanceSnapshotDto {
  id: number
  businessDate: string
  parkId: number
  parkCode: string
  parkName: string
  paymentType: PaymentType
  availableBalance: number
  currentDebt?: number | null
  bankAccountSnapshot?: string | null
  sourceType: SourceType
  sourceJobRunId?: number | null
  sourceJobRunItemId?: number | null
  rawResponseId?: number | null
  manualReason?: string | null
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface DailyTicketCostSummaryDto {
  id: number
  businessDate: string
  parkId: number
  parkCode: string
  parkName: string
  paymentType: PaymentType
  totalTicketCost: number
  totalSalesAmount?: number | null
  totalQuantity?: number | null
  sourceType: SourceType
  sourceJobRunId?: number | null
  sourceJobRunItemId?: number | null
  rawResponseId?: number | null
  manualReason?: string | null
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface DailyBankTransactionSummaryDto {
  id: number
  businessDate: string
  parkId: number
  parkCode: string
  parkName: string
  paymentType: PaymentType
  transactionType: BankTransactionType
  totalDebitAmount: number
  totalCreditAmount: number
  transactionCount: number
  sourceType: SourceType
  sourceJobRunId?: number | null
  sourceJobRunItemId?: number | null
  rawResponseId?: number | null
  manualReason?: string | null
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface ManualParkBalanceRequest {
  businessDate: string
  parkId: number
  availableBalance: number
  currentDebt?: number | null
  bankAccountSnapshot?: string | null
  manualReason: string
  jobRunItemId?: number | null
}

export interface ManualTicketCostSummaryRequest {
  businessDate: string
  parkId: number
  totalTicketCost: number
  totalSalesAmount?: number | null
  totalQuantity?: number | null
  manualReason: string
  jobRunItemId?: number | null
}

export interface ManualBankTransactionSummaryRequest {
  businessDate: string
  parkId: number
  transactionType: BankTransactionType
  totalDebitAmount: number
  totalCreditAmount: number
  transactionCount: number
  manualReason: string
  jobRunItemId?: number | null
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

export function listParkBalances(filters: SummaryFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    businessDate: filters.businessDate,
    parkId: filters.parkId,
    paymentType: filters.paymentType,
    sourceType: filters.sourceType,
  })
  return apiRequest<PagedResult<DailyParkBalanceSnapshotDto>>(`/park-balances?${query}`)
}

export function saveManualParkBalance(request: ManualParkBalanceRequest) {
  return apiRequest<DailyParkBalanceSnapshotDto>('/park-balances/manual', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function listTicketCostSummaries(filters: SummaryFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    businessDate: filters.businessDate,
    parkId: filters.parkId,
    paymentType: filters.paymentType,
    sourceType: filters.sourceType,
  })
  return apiRequest<PagedResult<DailyTicketCostSummaryDto>>(`/ticket-cost-summaries?${query}`)
}

export function saveManualTicketCostSummary(request: ManualTicketCostSummaryRequest) {
  return apiRequest<DailyTicketCostSummaryDto>('/ticket-cost-summaries/manual', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function listBankTransactionSummaries(filters: BankSummaryFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    businessDate: filters.businessDate,
    parkId: filters.parkId,
    paymentType: filters.paymentType,
    sourceType: filters.sourceType,
    transactionType: filters.transactionType,
  })
  return apiRequest<PagedResult<DailyBankTransactionSummaryDto>>(`/bank-transaction-summaries?${query}`)
}

export function saveManualBankTransactionSummary(request: ManualBankTransactionSummaryRequest) {
  return apiRequest<DailyBankTransactionSummaryDto>('/bank-transaction-summaries/manual', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
