import { apiRequest, type PagedResult } from './apiClient'
import type { PaymentType, RecordStatus } from './formatters'

export interface ParkDto {
  id: number
  code: string
  name: string
  paymentType: PaymentType
  searchCode?: string | null
  location?: string | null
  bankAccount?: string | null
  bankName?: string | null
  creditLimit?: number | null
  apiSiteId?: string | null
  apiProfileId?: string | null
  balanceTransformRule?: string | null
  apiNote?: string | null
  status: RecordStatus
  isDeleted: boolean
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface ParkTicketTypeDto {
  id: number
  parkId: number
  parkCode: string
  parkName: string
  paymentType: PaymentType
  code: string
  ticketTypeCode: string
  name: string
  ticketGroupName?: string | null
  costPrice: number
  status: RecordStatus
  isDeleted: boolean
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface ParkListFilters {
  page?: number
  keyword?: string
  paymentType?: PaymentType | ''
  status?: RecordStatus | ''
}

export interface ParkTicketTypeListFilters extends ParkListFilters {
  parkId?: number | ''
}

export interface SaveParkRequest {
  code?: string
  name: string
  paymentType: PaymentType
  searchCode?: string | null
  location?: string | null
  bankAccount?: string | null
  bankName?: string | null
  creditLimit?: number | null
  apiSiteId?: string | null
  apiProfileId?: string | null
  balanceTransformRule?: string | null
  apiNote?: string | null
  status: RecordStatus
}

export interface SaveParkTicketTypeRequest {
  parkId: number
  code: string
  ticketTypeCode: string
  name: string
  ticketGroupName?: string | null
  costPrice: number
  status: RecordStatus
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

export function listParks(filters: ParkListFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    keyword: filters.keyword?.trim(),
    paymentType: filters.paymentType,
    status: filters.status,
  })
  return apiRequest<PagedResult<ParkDto>>(`/parks?${query}`)
}

export function createPark(request: SaveParkRequest) {
  return apiRequest<ParkDto>('/parks', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function updatePark(id: number, request: SaveParkRequest) {
  return apiRequest<ParkDto>(`/parks/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

export function setParkInactive(id: number) {
  return apiRequest<ParkDto>(`/parks/${id}/set-inactive`, { method: 'POST' })
}

export function deletePark(id: number) {
  return apiRequest<null>(`/parks/${id}`, { method: 'DELETE' })
}

export function listParkTicketTypes(filters: ParkTicketTypeListFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    parkId: filters.parkId,
    keyword: filters.keyword?.trim(),
    paymentType: filters.paymentType,
    status: filters.status,
  })
  return apiRequest<PagedResult<ParkTicketTypeDto>>(`/park-ticket-types?${query}`)
}

export function createParkTicketType(request: SaveParkTicketTypeRequest) {
  return apiRequest<ParkTicketTypeDto>('/park-ticket-types', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function updateParkTicketType(id: number, request: SaveParkTicketTypeRequest) {
  return apiRequest<ParkTicketTypeDto>(`/park-ticket-types/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

export function setParkTicketTypeInactive(id: number) {
  return apiRequest<ParkTicketTypeDto>(`/park-ticket-types/${id}/set-inactive`, { method: 'POST' })
}

export function deleteParkTicketType(id: number) {
  return apiRequest<null>(`/park-ticket-types/${id}`, { method: 'DELETE' })
}
