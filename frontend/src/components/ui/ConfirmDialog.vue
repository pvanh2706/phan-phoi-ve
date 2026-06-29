<script setup lang="ts">
import { onBeforeUnmount, watch } from 'vue'
import { useConfirm } from '../../composables/useConfirm'

const { state, accept, cancel } = useConfirm()

function onKeydown(event: KeyboardEvent) {
  if (!state.value.open) return
  if (event.key === 'Escape') cancel()
  else if (event.key === 'Enter') accept()
}

watch(
  () => state.value.open,
  (open) => {
    if (open) document.addEventListener('keydown', onKeydown)
    else document.removeEventListener('keydown', onKeydown)
  },
)

onBeforeUnmount(() => document.removeEventListener('keydown', onKeydown))
</script>

<template>
  <Teleport to="body">
    <Transition name="confirm">
      <div v-if="state.open" class="modal-overlay confirm-overlay">
        <div class="modal confirm-modal" role="alertdialog" aria-modal="true">
          <div class="confirm-main">
            <span class="confirm-icon" :class="`confirm-icon-${state.tone}`">
              {{ state.tone === 'danger' ? '⚠' : '?' }}
            </span>
            <div class="confirm-texts">
              <div v-if="state.title" class="confirm-title">{{ state.title }}</div>
              <div class="confirm-message">{{ state.message }}</div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-secondary" type="button" @click="cancel">{{ state.cancelText }}</button>
            <button
              :class="state.tone === 'danger' ? 'btn-danger' : 'btn-primary'"
              type="button"
              @click="accept"
            >{{ state.confirmText }}</button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
