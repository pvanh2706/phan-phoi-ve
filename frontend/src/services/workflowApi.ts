import { apiRequest } from './apiClient'
import type { PaymentType, UserRole } from './formatters'

export interface WorkflowUserOption {
  id: number
  fullName: string
  initials: string
  role: UserRole
}

export interface WorkflowTaskDto {
  id: number
  title: string
  paymentType: PaymentType
  parkId?: number | null
  parkName?: string | null
  bankAccount?: string | null
  bankName?: string | null
  amount: number
  executeDate?: string | null
  note?: string | null
  columnId: number
  sortOrder: number
}

export interface WorkflowColumnDto {
  id: number
  columnKey: string
  title: string
  headTone: string
  cardStatusLabel: string
  cardTone: string
  sortOrder: number
  visibleFields: string[]
  permittedUserIds: number[]
  tasks: WorkflowTaskDto[]
}

export interface WorkflowBoardDto {
  columns: WorkflowColumnDto[]
  users: WorkflowUserOption[]
}

export interface SaveWorkflowTaskRequest {
  title: string
  paymentType: PaymentType
  parkId?: number | null
  bankAccount?: string | null
  bankName?: string | null
  amount: number
  executeDate?: string | null
  note?: string | null
  columnId?: number | null
}

export interface WorkflowColumnSettingsRequest {
  visibleFields: string[]
  permittedUserIds: number[]
}

export function getWorkflowBoard() {
  return apiRequest<WorkflowBoardDto>('/workflow/board')
}

export function createWorkflowTask(request: SaveWorkflowTaskRequest) {
  return apiRequest<WorkflowTaskDto>('/workflow/tasks', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function updateWorkflowTask(id: number, request: SaveWorkflowTaskRequest) {
  return apiRequest<WorkflowTaskDto>(`/workflow/tasks/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

export function moveWorkflowTask(id: number, columnId: number) {
  return apiRequest<WorkflowTaskDto>(`/workflow/tasks/${id}/move`, {
    method: 'PUT',
    body: JSON.stringify({ columnId }),
  })
}

export function deleteWorkflowTask(id: number) {
  return apiRequest<null>(`/workflow/tasks/${id}`, { method: 'DELETE' })
}

export function updateWorkflowColumnSettings(id: number, request: WorkflowColumnSettingsRequest) {
  return apiRequest<WorkflowColumnDto>(`/workflow/columns/${id}/settings`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}
