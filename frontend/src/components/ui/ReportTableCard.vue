<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import type { ReportTabConfig } from '../../data/reports'
import DataTable from './DataTable.vue'
import PaginationBar from './PaginationBar.vue'

const props = defineProps<{
  tab: ReportTabConfig
}>()

const emit = defineEmits<{
  add: []
}>()

const search = ref('')
const fromDate = ref('')
const toDate = ref('')
const page = ref(1)
const filterValues = reactive<Record<string, string>>({})

const filteredRows = computed(() => {
  const keyword = search.value.trim().toLowerCase()

  return props.tab.rows.filter((row) => {
    if (keyword && !row.search.toLowerCase().includes(keyword)) {
      return false
    }

    if (fromDate.value && row.date && row.date < fromDate.value) {
      return false
    }

    if (toDate.value && row.date && row.date > toDate.value) {
      return false
    }

    return (props.tab.filters ?? []).every((filter) => {
      const value = filterValues[filter.key]
      return !value || row.filters?.[filter.key] === value
    })
  })
})

const pageSize = computed(() => props.tab.pageSize ?? 8)
const visibleRows = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filteredRows.value.slice(start, start + pageSize.value)
})

function reset() {
  search.value = ''
  fromDate.value = ''
  toDate.value = ''
  page.value = 1
  for (const key of Object.keys(filterValues)) {
    delete filterValues[key]
  }
}

watch(() => props.tab.id, reset)

function addButtonText(label: string) {
  return label.replace(/^\+\s*/, '')
}
</script>

<template>
  <div class="card">
    <div class="toolbar">
      <input
        v-model="search"
        type="text"
        class="tb-input"
        :placeholder="tab.searchPlaceholder ?? '🔍  Tìm kiếm...'"
        @input="page = 1"
      />
      <template v-if="tab.dateFilter">
        <input v-model="fromDate" class="tb-date" type="date" @change="page = 1" />
        <input v-model="toDate" class="tb-date" type="date" @change="page = 1" />
      </template>
      <select
        v-for="filter in tab.filters ?? []"
        :key="filter.key"
        v-model="filterValues[filter.key]"
        class="tb-select"
        @change="page = 1"
      >
        <option value="">{{ filter.label }}</option>
        <option v-for="option in filter.options" :key="option.value" :value="option.value">
          {{ option.label }}
        </option>
      </select>
      <button v-if="tab.addLabel" class="add-btn" type="button" @click="emit('add')">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" d="M12 5v14M5 12h14" />
        </svg>
        {{ addButtonText(tab.addLabel) }}
      </button>
      <slot name="actions" />
    </div>

    <DataTable :columns="tab.columns" :rows="visibleRows" />
    <PaginationBar v-model:page="page" :total="filteredRows.length" :page-size="pageSize" />
  </div>
</template>
