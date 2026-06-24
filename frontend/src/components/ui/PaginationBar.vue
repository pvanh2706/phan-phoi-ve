<script setup lang="ts">
const page = defineModel<number>('page', { required: true })

const props = defineProps<{
  total: number
  pageSize: number
}>()

function pages() {
  return Math.max(1, Math.ceil(props.total / props.pageSize))
}

function go(value: number) {
  page.value = Math.min(Math.max(1, value), pages())
}
</script>

<template>
  <div class="pagination">
    <span class="pg-info">Tổng {{ total }} dòng</span>
    <button class="pg-btn" type="button" :disabled="page <= 1" @click="go(page - 1)">‹</button>
    <button
      v-for="item in pages()"
      :key="item"
      class="pg-btn"
      :class="{ active: item === page }"
      type="button"
      @click="go(item)"
    >
      {{ item }}
    </button>
    <button class="pg-btn" type="button" :disabled="page >= pages()" @click="go(page + 1)">›</button>
  </div>
</template>
