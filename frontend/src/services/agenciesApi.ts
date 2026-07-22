import { apiRequest, type PagedResult } from './apiClient'

export type AgencySource = 'OneInventory' | 'AR' | 'AR & OneInventory'

export interface AgencyDto {
  id: number
  code: string
  name: string
  parentCode?: string | null
  parentName?: string | null
  source: AgencySource
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface SaveAgencyRequest {
  code: string
  name: string
  parentCode?: string | null
  parentName?: string | null
  source: AgencySource
}

export interface AgencyListFilters {
  page?: number
  keyword?: string
}

export function listAgencies(filters: AgencyListFilters = {}) {
  const query = new URLSearchParams({ page: String(filters.page ?? 1) })
  if (filters.keyword?.trim()) {
    query.set('keyword', filters.keyword.trim())
  }
  return apiRequest<PagedResult<AgencyDto>>(`/agencies?${query}`)
}

export function createAgency(request: SaveAgencyRequest) {
  return apiRequest<AgencyDto>('/agencies', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function updateAgency(id: number, request: SaveAgencyRequest) {
  return apiRequest<AgencyDto>(`/agencies/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

export function deleteAgency(id: number) {
  return apiRequest<null>(`/agencies/${id}`, { method: 'DELETE' })
}
