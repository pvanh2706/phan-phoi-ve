import { apiRequest } from './apiClient'
import type { UserRole, UserStatus } from './formatters'

export interface ProfileDto {
  id: number
  fullName: string
  email: string
  phoneNumber?: string | null
  role: UserRole
  status: UserStatus
  failedLoginCount: number
  lockedAtUtc?: string | null
  lockReason?: string | null
  lastLoginAtUtc?: string | null
  createdAtUtc: string
}

export interface UserPreferenceDto {
  themeMode: 'Dark' | 'Light' | 'System'
  language?: string | null
}

export interface UserSessionDto {
  id: number
  createdAtUtc: string
  expiresAtUtc: string
  createdByIp?: string | null
  lastUsedIp?: string | null
  userAgent?: string | null
  deviceName?: string | null
  lastUsedAtUtc?: string | null
  revokedAtUtc?: string | null
  revokeReason?: string | null
  isActive: boolean
  isCurrent: boolean
}

export function getProfile() {
  return apiRequest<ProfileDto>('/me/profile')
}

export function updateProfile(request: { fullName: string; phoneNumber?: string | null }) {
  return apiRequest<ProfileDto>('/me/profile', {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

export function uploadAvatar(file: File) {
  const formData = new FormData()
  formData.append('file', file)
  return apiRequest<{ avatarPath: string }>('/me/avatar', {
    method: 'POST',
    body: formData,
  })
}

export function changePassword(request: { currentPassword: string; newPassword: string }) {
  return apiRequest<null>('/me/change-password', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function listSessions() {
  return apiRequest<UserSessionDto[]>('/me/sessions')
}

export function revokeSession(sessionId: number) {
  return apiRequest<null>(`/me/sessions/${sessionId}/revoke`, { method: 'POST' })
}

export function revokeAllSessions() {
  return apiRequest<null>('/me/sessions/revoke-all', { method: 'POST' })
}

export function getPreferences() {
  return apiRequest<UserPreferenceDto>('/me/preferences')
}

export function updatePreferences(request: UserPreferenceDto) {
  return apiRequest<UserPreferenceDto>('/me/preferences', {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}
