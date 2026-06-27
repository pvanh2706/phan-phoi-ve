import { reactive } from 'vue'
import { apiRequest, refreshAccessToken, setAccessToken } from './apiClient'

export interface CurrentUser {
  id: number
  fullName: string
  email: string
  phoneNumber?: string | null
  role: 'Admin' | 'Member' | 'Accountant'
  status: 'Active' | 'Inactive' | 'Locked'
  avatarPath?: string | null
}

interface TokenResponse {
  accessToken: string
  expiresAtUtc: string
  user: CurrentUser
}

export const authState = reactive({
  ready: false,
  user: null as CurrentUser | null,
})

let initPromise: Promise<void> | null = null

export function initializeAuth() {
  if (authState.ready) {
    return Promise.resolve()
  }
  if (initPromise) {
    return initPromise
  }

  initPromise = refreshAccessToken()
    .then((data) => {
      authState.user = (data?.user as CurrentUser | undefined) ?? null
    })
    .finally(() => {
      authState.ready = true
    })

  return initPromise
}

export async function login(email: string, password: string) {
  const data = await apiRequest<TokenResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  }, false)
  setAccessToken(data.accessToken)
  authState.user = data.user
  authState.ready = true
  return data.user
}

export async function logout() {
  try {
    await apiRequest('/auth/logout', { method: 'POST' }, false)
  } finally {
    setAccessToken(null)
    authState.user = null
    authState.ready = true
  }
}
