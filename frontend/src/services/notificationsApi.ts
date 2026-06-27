import { apiRequest, type PagedResult } from './apiClient'
import type { NotificationType } from './formatters'

export interface NotificationRecipientDto {
  id: number
  notificationType: NotificationType
  email: string
  displayName?: string | null
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface SaveNotificationRecipientRequest {
  notificationType: NotificationType
  email: string
  displayName?: string | null
  isActive: boolean
}

function buildQuery(filters: Record<string, string | boolean | number | undefined | null>) {
  const query = new URLSearchParams()
  for (const [key, value] of Object.entries(filters)) {
    if (value !== undefined && value !== null && value !== '') {
      query.set(key, String(value))
    }
  }
  return query.toString()
}

export function listNotificationRecipients(filters: { page?: number; notificationType?: NotificationType | ''; active?: boolean | '' } = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    notificationType: filters.notificationType,
    active: filters.active,
  })
  return apiRequest<PagedResult<NotificationRecipientDto>>(`/notification-recipients?${query}`)
}

export function createNotificationRecipient(request: SaveNotificationRecipientRequest) {
  return apiRequest<NotificationRecipientDto>('/notification-recipients', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function updateNotificationRecipient(id: number, request: SaveNotificationRecipientRequest) {
  return apiRequest<NotificationRecipientDto>(`/notification-recipients/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

export function deactivateNotificationRecipient(id: number) {
  return apiRequest<null>(`/notification-recipients/${id}`, { method: 'DELETE' })
}
