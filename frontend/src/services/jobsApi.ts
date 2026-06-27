import { apiRequest, type PagedResult } from './apiClient'

export interface JobRunListItemDto {
  id: number
  jobName: string
  businessDate?: string | null
  status: string
  totalItems: number
  successItems: number
  failedItems: number
  startedAtUtc: string
  finishedAtUtc?: string | null
}

export interface JobRunItemDto {
  id: number
  jobRunId: number
  businessDate?: string | null
  parkCode?: string | null
  parkName?: string | null
  source?: string | null
  status: string
  errorMessage?: string | null
}

export function listJobRuns(page = 1) {
  return apiRequest<PagedResult<JobRunListItemDto>>(`/jobs/runs?page=${page}`)
}

export function listJobErrors(page = 1, businessDate?: string) {
  const query = new URLSearchParams({ page: String(page) })
  if (businessDate) query.set('businessDate', businessDate)
  return apiRequest<PagedResult<JobRunItemDto>>(`/jobs/errors?${query}`)
}
