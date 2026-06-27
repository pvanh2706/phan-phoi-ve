import { apiRequest, type PagedResult } from './apiClient'

export interface AuditLogDto {
  id: number
  occurredAtUtc: string
  userEmailSnapshot?: string | null
  userRoleSnapshot?: string | null
  module: string
  entityName: string
  entityId?: string | null
  action: string
  beforeJson?: string | null
  afterJson?: string | null
}

export function listParkAuditLogs(page = 1) {
  return apiRequest<PagedResult<AuditLogDto>>(`/audit-logs?page=${page}&module=Park`)
}
