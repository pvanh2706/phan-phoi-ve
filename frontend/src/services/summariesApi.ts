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
  bankAccountSnapshot?: string | null
  sourceType: SourceType
  sourceJobRunId?: number | null
  sourceJobRunItemId?: number | null
  rawResponseId?: number | null
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

export interface TicketSaleCostDetailDto {
  id: number
  businessDate: string
  parkId?: number | null
  paymentType: PaymentType
  bookingCode: string
  unitPrice: number
  ticketTypeName: string
  ticketGroupName?: string | null
  salesAmount: number
  costAmount: number
  sellingAgentCode?: string | null
  quantity: number
  buyingAgentCode?: string | null
  buyingAgentName?: string | null
  parkCodeSnapshot: string
  parkNameSnapshot: string
  subtotal: number
  externalLineId?: string | null
  sellingAgentName?: string | null
  ticketTypeCode?: string | null
  parentBuyingAgentName?: string | null
  parentBuyingAgentCode?: string | null
  sourceType: SourceType
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface TicketCostDetailFilters {
  page?: number
  dateFrom?: string
  dateTo?: string
  parkId?: number | ''
  paymentType?: PaymentType | ''
  keyword?: string
}

export interface BankTransactionDetailDto {
  id: number
  businessDate: string
  transactionAtUtc: string
  paymentType: PaymentType
  debitAmount: number
  creditAmount: number
  content: string
  bankAccount?: string | null
  parkId?: number | null
  parkName?: string | null
  sourceType: SourceType
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface BankStatementSyncResult {
  mailsProcessed: number
  transactionsParsed: number
  imported: number
  skippedUnmatched: number
  overwrittenDates: number
  unmatchedAccounts: string[]
}

export interface BankTransactionDetailFilters {
  page?: number
  dateFrom?: string
  dateTo?: string
  paymentType?: PaymentType | ''
  keyword?: string
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

export interface TicketCostSyncResult {
  businessDate: string
  totalLines: number
  imported: number
  skippedUnmatched: number
  unmatchedParkCodes: string[]
}

export function syncTicketCostDetails() {
  return apiRequest<TicketCostSyncResult>('/ticket-cost-details/sync', {
    method: 'POST',
  })
}

export function listTicketCostDetails(filters: TicketCostDetailFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    dateFrom: filters.dateFrom,
    dateTo: filters.dateTo,
    parkId: filters.parkId,
    paymentType: filters.paymentType,
    keyword: filters.keyword,
  })
  return apiRequest<PagedResult<TicketSaleCostDetailDto>>(`/ticket-cost-details?${query}`)
}

export function listBankTransactionDetails(filters: BankTransactionDetailFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    dateFrom: filters.dateFrom,
    dateTo: filters.dateTo,
    paymentType: filters.paymentType,
    keyword: filters.keyword,
  })
  return apiRequest<PagedResult<BankTransactionDetailDto>>(`/bank-transaction-details?${query}`)
}

export function syncBankTransactions() {
  return apiRequest<BankStatementSyncResult>('/bank-transaction-details/sync', {
    method: 'POST',
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
