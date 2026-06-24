import { computed, onMounted, ref } from 'vue'

export type ThemeMode = 'dark' | 'light' | 'system'

const storageKey = 'ppv-theme'
const mode = ref<ThemeMode>('dark')

function resolveTheme(value: ThemeMode) {
  if (value === 'system') {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  }

  return value
}

function paint(value: ThemeMode) {
  document.documentElement.setAttribute('data-theme', resolveTheme(value))
}

export function useTheme() {
  function setMode(value: ThemeMode) {
    mode.value = value
    localStorage.setItem(storageKey, value)
    paint(value)
  }

  function toggleTheme() {
    setMode(document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark')
  }

  onMounted(() => {
    const saved = localStorage.getItem(storageKey) as ThemeMode | null
    mode.value = saved ?? 'dark'
    paint(mode.value)

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
      if (mode.value === 'system') {
        paint('system')
      }
    })
  })

  return {
    mode,
    modeLabel: computed(() => (mode.value === 'system' ? 'Hệ thống' : mode.value === 'dark' ? 'Tối' : 'Sáng')),
    setMode,
    toggleTheme,
  }
}
