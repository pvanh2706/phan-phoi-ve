import { apiRequest, type PagedResult } from './apiClient'

export type ExternalApiSource = 'ParkBalance' | 'TicketCost' | 'BankTransaction'

export interface ExternalApiLogDto {
  id: number
  source: ExternalApiSource
  businessDate?: string | null
  parkId?: number | null
  parkName?: string | null
  jobRunId?: number | null
  requestUrl?: string | null
  responseStatusCode?: number | null
  isSuccess: boolean
  errorMessage?: string | null
  durationMs?: number | null
  receivedAtUtc: string
}

export interface ExternalApiLogDetailDto extends ExternalApiLogDto {
  requestPayloadJson?: string | null
  responseBodyJson?: string | null
}

export interface ExternalApiLogFilters {
  page?: number
  source?: ExternalApiSource | ''
  isSuccess?: boolean | ''
  parkId?: number | ''
  keyword?: string
  fromDate?: string
  toDate?: string
}

function buildQuery(filters: Record<string, string | number | boolean | undefined | null>) {
  const query = new URLSearchParams()
  for (const [key, value] of Object.entries(filters)) {
    if (value !== undefined && value !== null && value !== '') {
      query.set(key, String(value))
    }
  }
  return query.toString()
}

export function listExternalApiLogs(filters: ExternalApiLogFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    source: filters.source,
    isSuccess: filters.isSuccess,
    parkId: filters.parkId,
    keyword: filters.keyword,
    fromDate: filters.fromDate,
    toDate: filters.toDate,
  })
  return apiRequest<PagedResult<ExternalApiLogDto>>(`/external-api-logs?${query}`)
}

export function getExternalApiLog(id: number) {
  return apiRequest<ExternalApiLogDetailDto>(`/external-api-logs/${id}`)
}
