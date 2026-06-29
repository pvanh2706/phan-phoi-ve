<script setup lang="ts">
import { useToast, type ToastKind } from '../../composables/useToast'

const { toasts, dismiss } = useToast()

const ICON: Record<ToastKind, string> = {
  success: '✓',
  error: '✕',
  warning: '!',
  info: 'i',
}
</script>

<template>
  <Teleport to="body">
    <div class="toast-host" aria-live="polite" aria-atomic="false">
      <TransitionGroup name="toast">
        <div
          v-for="toast in toasts"
          :key="toast.id"
          class="toast"
          :class="`toast-${toast.kind}`"
          role="status"
        >
          <span class="toast-icon">{{ ICON[toast.kind] }}</span>
          <div class="toast-content">
            <div v-if="toast.title" class="toast-title">{{ toast.title }}</div>
            <div class="toast-text">{{ toast.message }}</div>
          </div>
          <button class="toast-close" type="button" aria-label="Đóng" @click="dismiss(toast.id)">✕</button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>
