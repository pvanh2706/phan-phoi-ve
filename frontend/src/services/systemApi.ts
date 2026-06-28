import { apiRequest } from './apiClient'

export interface ResetDataResultDto {
  keptAdminCount: number
  totalDeleted: number
  deletedByTable: Record<string, number>
}

/** Cụm từ người dùng phải gõ để xác nhận xóa toàn bộ dữ liệu. */
export const RESET_CONFIRM_PHRASE = 'XÓA TOÀN BỘ DỮ LIỆU'

export function resetAllData(confirmText: string) {
  return apiRequest<ResetDataResultDto>('/system/reset-data', {
    method: 'POST',
    body: JSON.stringify({ confirmText }),
  })
}
