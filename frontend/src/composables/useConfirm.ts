import { ref } from 'vue'

export interface ConfirmOptions {
  message: string
  title?: string
  confirmText?: string
  cancelText?: string
  /** 'danger' cho hành động xóa/ngừng dùng; 'primary' cho xác nhận thông thường. */
  tone?: 'danger' | 'primary'
}

interface ConfirmState extends Required<Omit<ConfirmOptions, 'title'>> {
  open: boolean
  title?: string
}

const state = ref<ConfirmState>({
  open: false,
  message: '',
  confirmText: 'Xác nhận',
  cancelText: 'Hủy',
  tone: 'danger',
})

let resolver: ((value: boolean) => void) | null = null

/** Mở hộp thoại xác nhận, trả về Promise<boolean> theo lựa chọn của người dùng. */
function confirm(options: ConfirmOptions): Promise<boolean> {
  state.value = {
    open: true,
    message: options.message,
    title: options.title,
    confirmText: options.confirmText ?? 'Xác nhận',
    cancelText: options.cancelText ?? 'Hủy',
    tone: options.tone ?? 'danger',
  }
  return new Promise<boolean>((resolve) => {
    resolver = resolve
  })
}

function settle(result: boolean) {
  state.value = { ...state.value, open: false }
  if (resolver) {
    resolver(result)
    resolver = null
  }
}

export function useConfirm() {
  return {
    state,
    confirm,
    accept: () => settle(true),
    cancel: () => settle(false),
  }
}
