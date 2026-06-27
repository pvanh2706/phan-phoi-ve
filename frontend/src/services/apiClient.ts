export interface ApiErrorItem {
  field?: string
  message: string
}

export interface ApiResponse<T> {
  success: boolean
  data: T
  message: string
  errors: ApiErrorItem[]
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export class ApiClientError extends Error {
  status: number
  errors: ApiErrorItem[]

  constructor(status: number, message: string, errors: ApiErrorItem[] = []) {
    super(message)
    this.status = status
    this.errors = errors
  }
}

let accessToken: string | null = null

const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL ??
  (window.location.port === '5173' ? 'http://localhost:5052/api' : '/api')

export function setAccessToken(token: string | null) {
  accessToken = token
}

export function getAccessToken() {
  return accessToken
}

export async function apiRequest<T>(path: string, init: RequestInit = {}, retry = true): Promise<T> {
  const headers = new Headers(init.headers)
  if (init.body && !headers.has('Content-Type') && !(init.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers,
    credentials: 'include',
  })

  if (response.status === 401 && retry && path !== '/auth/refresh') {
    const refreshed = await refreshAccessToken()
    if (refreshed) {
      return apiRequest<T>(path, init, false)
    }
  }

  const payload = (await response.json().catch(() => null)) as ApiResponse<T> | null

  if (!response.ok || !payload?.success) {
    throw new ApiClientError(
      response.status,
      payload?.message ?? 'Có lỗi xảy ra, vui lòng thử lại.',
      payload?.errors ?? [],
    )
  }

  return payload.data
}

export async function refreshAccessToken() {
  try {
    const data = await apiRequest<{ accessToken: string; user: unknown }>('/auth/refresh', { method: 'POST' }, false)
    setAccessToken(data.accessToken)
    return data
  } catch {
    setAccessToken(null)
    return null
  }
}
