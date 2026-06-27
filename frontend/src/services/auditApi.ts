import { apiRequest, type PagedResult } from './apiClient'

export interface AuditLogDto {
  id: number
  occurredAtUtc: string
  userId?: number | null
  userEmailSnapshot?: string | null
  userRoleSnapshot?: string | null
  module: string
  entityName: string
  entityId?: string | null
  action: string
  beforeJson?: string | null
  afterJson?: string | null
  ipAddress?: string | null
  userAgent?: string | null
  correlationId?: string | null
}

export interface AuditLogFilters {
  page?: number
  module?: string
  entityName?: string
  entityId?: string
  action?: string
  userId?: number | ''
  fromDate?: string
  toDate?: string
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

export function listParkAuditLogs(filters: AuditLogFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    module: filters.module ?? 'Park',
    entityName: filters.entityName,
    entityId: filters.entityId,
    action: filters.action,
    userId: filters.userId,
    fromDate: filters.fromDate,
    toDate: filters.toDate,
  })
  return apiRequest<PagedResult<AuditLogDto>>(`/audit-logs?${query}`)
}
