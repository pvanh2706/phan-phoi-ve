<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import DataTable from '../components/ui/DataTable.vue'
import PageHeader from '../components/ui/PageHeader.vue'
import PaginationBar from '../components/ui/PaginationBar.vue'
import { reportPages, type ReportPageKey } from '../data/reports'

const props = defineProps<{
  pageKey: ReportPageKey
}>()

const pageConfig = computed(() => reportPages[props.pageKey])
const activeTab = ref(pageConfig.value.tabs[0].id)
const search = ref('')
const fromDate = ref('')
const toDate = ref('')
const page = ref(1)
const filterValues = reactive<Record<string, string>>({})
const showAddModal = ref(false)

const currentTab = computed(() => pageConfig.value.tabs.find((tab) => tab.id === activeTab.value) ?? pageConfig.value.tabs[0])

const filteredRows = computed(() => {
  const keyword = search.value.trim().toLowerCase()
  return currentTab.value.rows.filter((row) => {
    if (keyword && !row.search.toLowerCase().includes(keyword)) {
      return false
    }

    if (fromDate.value && row.date && row.date < fromDate.value) {
      return false
    }

    if (toDate.value && row.date && row.date > toDate.value) {
      return false
    }

    return (currentTab.value.filters ?? []).every((filter) => {
      const value = filterValues[filter.key]
      return !value || row.filters?.[filter.key] === value
    })
  })
})

const pageSize = computed(() => currentTab.value.pageSize ?? 8)
const visibleRows = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filteredRows.value.slice(start, start + pageSize.value)
})

function switchTab(tabId: string) {
  activeTab.value = tabId
  resetFilters()
}

function resetFilters() {
  search.value = ''
  fromDate.value = ''
  toDate.value = ''
  page.value = 1
  for (const key of Object.keys(filterValues)) {
    delete filterValues[key]
  }
}

watch(
  () => props.pageKey,
  () => {
    activeTab.value = pageConfig.value.tabs[0].id
    resetFilters()
  },
)
</script>

<template>
  <PageHeader :title="pageConfig.title" :subtitle="pageConfig.subtitle" />

  <div v-if="pageConfig.stats" class="stat-grid">
    <div v-for="stat in pageConfig.stats" :key="stat.label" class="stat-card">
      <div class="stat-label">{{ stat.label }}</div>
      <div class="stat-value" :class="stat.tone">{{ stat.value }}</div>
    </div>
  </div>

  <div v-if="pageConfig.tabs.length > 1" class="tabs-bar">
    <button
      v-for="tab in pageConfig.tabs"
      :key="tab.id"
      class="tab-btn"
      :class="{ active: tab.id === activeTab }"
      type="button"
      @click="switchTab(tab.id)"
    >
      {{ tab.label }}
    </button>
  </div>

  <div class="card">
    <div class="toolbar">
      <input
        v-model="search"
        type="text"
        class="tb-input"
        :placeholder="currentTab.searchPlaceholder ?? '🔍  Tìm kiếm...'"
        @input="page = 1"
      />
      <template v-if="currentTab.dateFilter">
        <input v-model="fromDate" class="tb-date" type="date" @change="page = 1" />
        <input v-model="toDate" class="tb-date" type="date" @change="page = 1" />
      </template>
      <select
        v-for="filter in currentTab.filters ?? []"
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
      <button v-if="currentTab.addLabel" class="add-btn" type="button" @click="showAddModal = true">
        {{ currentTab.addLabel }}
      </button>
    </div>

    <DataTable :columns="currentTab.columns" :rows="visibleRows" />
    <PaginationBar v-model:page="page" :total="filteredRows.length" :page-size="pageSize" />
  </div>

  <div v-if="showAddModal" class="modal-overlay" @click.self="showAddModal = false">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">{{ currentTab.addLabel?.replace('+ ', '') }}</span>
        <button class="modal-close" type="button" @click="showAddModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mã đặt vé</label>
            <input class="form-input" placeholder="BK-240625-001" />
          </div>
          <div class="form-group">
            <label class="form-label">Tên KVC</label>
            <input class="form-input" placeholder="Bản Mòng" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Khách hàng</label>
            <input class="form-input" placeholder="Nguyễn Văn A" />
          </div>
          <div class="form-group">
            <label class="form-label">Số tiền</label>
            <input class="form-input" placeholder="1,250,000" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Ghi chú</label>
          <textarea class="form-textarea" placeholder="Thông tin xử lý hoàn tiền"></textarea>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showAddModal = false">Hủy</button>
        <button class="btn-primary" type="button" @click="showAddModal = false">Lưu</button>
      </div>
    </div>
  </div>
</template>
