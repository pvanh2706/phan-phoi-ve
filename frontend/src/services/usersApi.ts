import { apiRequest, type PagedResult } from './apiClient'
import type { UserRole, UserStatus } from './formatters'
import type { ProfileDto } from './meApi'

export interface UserFilters {
  page?: number
  keyword?: string
  role?: UserRole | ''
  status?: UserStatus | ''
}

export interface CreateUserRequest {
  fullName: string
  email: string
  phoneNumber?: string | null
  role: UserRole
  password: string
}

export interface UpdateUserRequest {
  fullName: string
  phoneNumber?: string | null
  role: UserRole
  status: UserStatus
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

export function listUsers(filters: UserFilters = {}) {
  const query = buildQuery({
    page: filters.page ?? 1,
    keyword: filters.keyword?.trim(),
    role: filters.role,
    status: filters.status,
  })
  return apiRequest<PagedResult<ProfileDto>>(`/users?${query}`)
}

export function createUser(request: CreateUserRequest) {
  return apiRequest<ProfileDto>('/users', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function updateUser(id: number, request: UpdateUserRequest) {
  return apiRequest<ProfileDto>(`/users/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

export function resetUserPassword(id: number, newPassword: string) {
  return apiRequest<null>(`/users/${id}/reset-password`, {
    method: 'POST',
    body: JSON.stringify({ newPassword }),
  })
}

export function unlockUser(id: number) {
  return apiRequest<null>(`/users/${id}/unlock`, { method: 'POST' })
}

export function disableUser(id: number) {
  return apiRequest<null>(`/users/${id}/disable`, { method: 'POST' })
}

export function enableUser(id: number) {
  return apiRequest<null>(`/users/${id}/enable`, { method: 'POST' })
}
