import { ref } from 'vue'

export type ToastKind = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: number
  kind: ToastKind
  message: string
  title?: string
}

export interface ToastOptions {
  title?: string
  /** Thời gian tự ẩn (ms). Truyền 0 để giữ cho tới khi người dùng đóng. */
  duration?: number
}

// State dùng chung ở cấp module (singleton) — giống cách useTheme quản lý trạng thái.
const toasts = ref<Toast[]>([])
const timers = new Map<number, number>()
let seq = 0

// Lỗi/cảnh báo ở lâu hơn để người dùng kịp đọc; thành công/thông tin ẩn nhanh.
const DEFAULT_DURATION: Record<ToastKind, number> = {
  success: 3000,
  info: 3500,
  warning: 5000,
  error: 6000,
}

function dismiss(id: number) {
  const timer = timers.get(id)
  if (timer !== undefined) {
    window.clearTimeout(timer)
    timers.delete(id)
  }
  toasts.value = toasts.value.filter((toast) => toast.id !== id)
}

function push(kind: ToastKind, message: string, options?: ToastOptions): number {
  const id = ++seq
  toasts.value = [...toasts.value, { id, kind, message, title: options?.title }]

  const duration = options?.duration ?? DEFAULT_DURATION[kind]
  if (duration > 0) {
    timers.set(id, window.setTimeout(() => dismiss(id), duration))
  }
  return id
}

export function useToast() {
  return {
    toasts,
    dismiss,
    success: (message: string, options?: ToastOptions) => push('success', message, options),
    error: (message: string, options?: ToastOptions) => push('error', message, options),
    warning: (message: string, options?: ToastOptions) => push('warning', message, options),
    info: (message: string, options?: ToastOptions) => push('info', message, options),
  }
}
