export type PaymentType = 'Prepaid' | 'Debt'
export type RecordStatus = 'Active' | 'Inactive'
export type SourceType = 'Api' | 'Manual'
export type UserRole = 'Admin' | 'Member' | 'Accountant'
export type UserStatus = 'Active' | 'Inactive' | 'Locked'
export type BankTransactionType = 'TopUp' | 'DebtPayment' | 'Refund' | 'Other'
export type ReconciliationStatus = 'Matched' | 'Variance' | 'MissingData' | 'Resolved'
export type JobRunStatus = 'Running' | 'Succeeded' | 'CompletedWithErrors' | 'Failed' | 'Canceled'
export type JobRunItemStatus = 'Pending' | 'Running' | 'Succeeded' | 'Failed' | 'Skipped' | 'ManualResolved'
export type NotificationType = 'SyncErrorSummary' | 'DailyReport' | 'ReconciliationVarianceAlert'

export function formatMoney(value?: number | null) {
  if (value === null || value === undefined) return '-'
  return `${new Intl.NumberFormat('vi-VN').format(value)} đ`
}

export function formatNumber(value?: number | null) {
  if (value === null || value === undefined) return '-'
  return new Intl.NumberFormat('vi-VN').format(value)
}

export function formatDate(value?: string | null) {
  if (!value) return '-'
  const date = new Date(`${value}T00:00:00`)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleDateString('vi-VN')
}

export function formatDateTime(value?: string | null) {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString('vi-VN')
}

export function paymentTypeLabel(value?: PaymentType | string | null) {
  if (value === 'Prepaid') return 'Nạp trước'
  if (value === 'Debt') return 'Công nợ'
  return '-'
}

export function recordStatusLabel(value?: RecordStatus | string | null) {
  if (value === 'Active') return 'Hoạt động'
  if (value === 'Inactive') return 'Ngừng sử dụng'
  return '-'
}

export function sourceTypeLabel(value?: SourceType | string | null) {
  if (value === 'Api') return 'API'
  if (value === 'Manual') return 'Nhập tay'
  return '-'
}

export function userRoleLabel(value?: UserRole | string | null) {
  if (value === 'Admin') return 'Quản trị'
  if (value === 'Accountant') return 'Kế toán'
  if (value === 'Member') return 'Thành viên'
  return '-'
}

export function userStatusLabel(value?: UserStatus | string | null) {
  if (value === 'Active') return 'Hoạt động'
  if (value === 'Inactive') return 'Vô hiệu hóa'
  if (value === 'Locked') return 'Bị khóa'
  return '-'
}

export function bankTransactionTypeLabel(value?: BankTransactionType | string | null) {
  if (value === 'TopUp') return 'Nạp tiền'
  if (value === 'DebtPayment') return 'Thanh toán công nợ'
  if (value === 'Refund') return 'Hoàn tiền'
  if (value === 'Other') return 'Khác'
  return '-'
}

export function reconciliationStatusLabel(value?: ReconciliationStatus | string | null) {
  if (value === 'Matched') return 'Khớp'
  if (value === 'Variance') return 'Có lệch'
  if (value === 'MissingData') return 'Thiếu dữ liệu'
  if (value === 'Resolved') return 'Đã xử lý'
  return '-'
}

export function jobRunStatusLabel(value?: JobRunStatus | string | null) {
  if (value === 'Running') return 'Đang chạy'
  if (value === 'Succeeded') return 'Thành công'
  if (value === 'CompletedWithErrors') return 'Hoàn tất có lỗi'
  if (value === 'Failed') return 'Lỗi'
  if (value === 'Canceled') return 'Đã hủy'
  return '-'
}

export function jobRunItemStatusLabel(value?: JobRunItemStatus | string | null) {
  if (value === 'Pending') return 'Chờ chạy'
  if (value === 'Running') return 'Đang chạy'
  if (value === 'Succeeded') return 'Thành công'
  if (value === 'Failed') return 'Cần xử lý'
  if (value === 'Skipped') return 'Bỏ qua'
  if (value === 'ManualResolved') return 'Đã nhập tay'
  return '-'
}

export function notificationTypeLabel(value?: NotificationType | string | null) {
  if (value === 'SyncErrorSummary') return 'Tổng hợp lỗi đồng bộ'
  if (value === 'DailyReport') return 'Báo cáo ngày'
  if (value === 'ReconciliationVarianceAlert') return 'Cảnh báo lệch đối soát'
  return '-'
}

export function auditActionLabel(value?: string | null) {
  const labels: Record<string, string> = {
    Create: 'Tạo mới',
    Update: 'Cập nhật',
    SetInactive: 'Ngừng sử dụng',
    Restore: 'Khôi phục',
    SoftDelete: 'Xóa mềm',
    RunJob: 'Chạy job',
    ManualEntry: 'Nhập tay',
    ResolveVariance: 'Xử lý lệch',
    Login: 'Đăng nhập',
    Logout: 'Đăng xuất',
    LockUser: 'Khóa tài khoản',
    UnlockUser: 'Mở khóa',
    ResetPassword: 'Đặt lại mật khẩu',
    RevokeSession: 'Đăng xuất thiết bị',
    ResetData: 'Xóa toàn bộ dữ liệu',
  }
  return value ? labels[value] ?? value : '-'
}

export function externalApiSourceLabel(value?: string | null) {
  const labels: Record<string, string> = {
    ParkBalance: 'Số dư KVC',
    TicketCost: 'Giá vốn vé',
    BankTransaction: 'Giao dịch ngân hàng',
  }
  return value ? labels[value] ?? value : '-'
}

export function badgeClassForStatus(value?: string | null) {
  if (!value) return 'badge-gray'
  if (['Active', 'Matched', 'Succeeded', 'ManualResolved'].includes(value)) return 'badge-green'
  if (['Inactive', 'Skipped', 'Resolved'].includes(value)) return 'badge-gray'
  if (['MissingData', 'CompletedWithErrors', 'Running', 'Pending'].includes(value)) return 'badge-amber'
  if (['Failed', 'Locked', 'Variance'].includes(value)) return 'badge-red'
  return 'badge-indigo'
}
