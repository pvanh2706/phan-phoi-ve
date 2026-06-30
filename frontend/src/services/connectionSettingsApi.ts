import { apiRequest } from './apiClient'

export interface MailConnection {
  host: string
  port: number
  useSsl: boolean
  username: string
  password: string
  mailbox: string
  fromFilter: string
  pdfPassword: string
}

export interface ParkBalanceConnection {
  endpoint: string
  timeoutSeconds: number
}

export interface OneInventoryConnection {
  baseUrl: string
  username: string
  password: string
  timeoutSeconds: number
}

export interface JobScheduleConnection {
  parkBalanceTime: string
  ticketCostTime: string
  bankScanStart: string
  bankScanEnd: string
  bankScanIntervalMinutes: number
}

export interface ConnectionSettings {
  mail: MailConnection
  parkBalance: ParkBalanceConnection
  oneInventory: OneInventoryConnection
  jobSchedule: JobScheduleConnection
}

export interface ConnectionTestResult {
  success: boolean
  message: string
  durationMs: number
}

export function getConnectionSettings() {
  return apiRequest<ConnectionSettings>('/connection-settings')
}

export function saveConnectionSettings(payload: ConnectionSettings) {
  return apiRequest<ConnectionSettings>('/connection-settings', {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
}

export function testMailConnection(payload: MailConnection) {
  return apiRequest<ConnectionTestResult>('/connection-settings/test/mail', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export function testParkBalanceConnection(payload: ParkBalanceConnection) {
  return apiRequest<ConnectionTestResult>('/connection-settings/test/park-balance', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export function testOneInventoryConnection(payload: OneInventoryConnection) {
  return apiRequest<ConnectionTestResult>('/connection-settings/test/oneinventory', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}
