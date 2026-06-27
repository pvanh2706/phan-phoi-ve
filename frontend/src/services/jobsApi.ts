import { apiRequest, type PagedResult } from './apiClient'
import type { JobRunItemStatus, JobRunStatus } from './formatters'

export type ExternalApiSource = 'ParkBalance' | 'TicketCost' | 'BankTransaction'
export type JobTriggerType = 'Schedule' | 'Manual' | 'System'

export interface JobRunListItemDto {
  id: number
  jobName: string
  businessDate?: string | null
  triggeredBy: JobTriggerType
  triggeredByUserId?: number | null
  startedAtUtc: string
  finishedAtUtc?: string | null
  status: JobRunStatus
  totalItems: number
  successItems: number
  failedItems: number
  skippedItems: number
  errorMessage?: string | null
}

export interface JobRunItemDto {
  id: number
  jobRunId: number
  businessDate?: string | null
  parkId?: number | null
  parkCode?: string | null
  parkName?: string | null
  source?: ExternalApiSource | null
  status: JobRunItemStatus
  attemptCount: number
  startedAtUtc?: string | null
  finishedAtUtc?: string | null
  durationMs?: number | null
  errorCode?: string | null
  errorMessage?: string | null
  rawResponseId?: number | null
  resolvedByUserId?: number | null
  resolvedAtUtc?: string | null
  manualResolutionNote?: string | null
}

export interface JobRunDetailDto extends JobRunListItemDto {
  summaryJson?: string | null
  items: JobRunItemDto[]
}

export interface RunJobRequest {
  businessDate?: string | null
}

export interface SendSyncErrorSummaryResultDto {
  jobRunId: number
  businessDate: string
  errorCount: number
  recipientCount: number
  emailAttempted: boolean
  emailSent: boolean
  message?: string | null
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

export function listJobRuns(filters: { page?: number; businessDate?: string; jobName?: string; status?: JobRunStatus | '' } = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    businessDate: filters.businessDate,
    jobName: filters.jobName,
    status: filters.status,
  })
  return apiRequest<PagedResult<JobRunListItemDto>>(`/jobs/runs?${query}`)
}

export function getJobRun(id: number) {
  return apiRequest<JobRunDetailDto>(`/jobs/runs/${id}`)
}

export function listJobErrors(filters: { page?: number; businessDate?: string; source?: ExternalApiSource | ''; status?: JobRunItemStatus | '' } = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    businessDate: filters.businessDate,
    source: filters.source,
    status: filters.status,
  })
  return apiRequest<PagedResult<JobRunItemDto>>(`/jobs/errors?${query}`)
}

export function runParkBalances(request: RunJobRequest) {
  return apiRequest<JobRunDetailDto>('/jobs/sync-park-balances/run', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function runTicketCosts(request: RunJobRequest) {
  return apiRequest<JobRunDetailDto>('/jobs/sync-ticket-costs/run', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function runBankTransactions(request: RunJobRequest) {
  return apiRequest<JobRunDetailDto>('/jobs/sync-bank-transactions/run', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function sendSyncErrorSummary(request: RunJobRequest) {
  return apiRequest<SendSyncErrorSummaryResultDto>('/jobs/send-sync-error-summary/run', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
