<script setup lang="ts">
import AppShell from './components/layout/AppShell.vue'
import ToastHost from './components/ui/ToastHost.vue'
import ConfirmDialog from './components/ui/ConfirmDialog.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <RouterView v-if="route.meta.public" />
  <AppShell v-else>
    <RouterView />
  </AppShell>
  <ToastHost />
  <ConfirmDialog />
</template>
