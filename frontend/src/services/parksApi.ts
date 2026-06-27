import { apiRequest, type PagedResult } from './apiClient'

export interface ParkDto {
  id: number
  code: string
  name: string
  paymentType: 'Prepaid' | 'Debt'
  bankAccount?: string | null
  bankName?: string | null
  status: 'Active' | 'Inactive'
}

export interface ParkTicketTypeDto {
  id: number
  parkId: number
  parkCode: string
  parkName: string
  paymentType: 'Prepaid' | 'Debt'
  code: string
  ticketTypeCode: string
  name: string
  costPrice: number
  status: 'Active' | 'Inactive'
}

export function listParks(page = 1, keyword = '') {
  const query = new URLSearchParams({ page: String(page) })
  if (keyword.trim()) query.set('keyword', keyword.trim())
  return apiRequest<PagedResult<ParkDto>>(`/parks?${query}`)
}

export function listParkTicketTypes(page = 1, parkId?: number) {
  const query = new URLSearchParams({ page: String(page) })
  if (parkId) query.set('parkId', String(parkId))
  return apiRequest<PagedResult<ParkTicketTypeDto>>(`/park-ticket-types?${query}`)
}
