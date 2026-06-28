<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import KanbanBoard from '../components/ui/KanbanBoard.vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { ApiClientError } from '../services/apiClient'
import { listBankTransactionDetails, type BankTransactionDetailDto } from '../services/summariesApi'
import { formatDate, formatNumber } from '../services/formatters'
import { cloneColumns, topUpWorkflow, type KanbanTask } from '../data/workflows'

type Tab = 'quytrinh' | 'nap' | 'cn'

const activeTab = ref<Tab>('quytrinh')

// ── Kanban (Quy trình nạp tiền KVC) ──
const columns = ref(cloneColumns(topUpWorkflow))
const selectedTask = ref<KanbanTask | null>(null)

function moveTask(taskId: string, toColumnId: string) {
  const fromColumn = columns.value.find((column) => column.tasks.some((task) => task.id === taskId))
  const toColumn = columns.value.find((column) => column.id === toColumnId)
  if (!fromColumn || !toColumn || fromColumn.id === toColumn.id) return
  const index = fromColumn.tasks.findIndex((task) => task.id === taskId)
  const [task] = fromColumn.tasks.splice(index, 1)
  toColumn.tasks.push(task)
}

// ── Transaction tables (Nạp tiền / Công nợ) ──
const loading = ref(false)
const error = ref('')
const rows = ref<BankTransactionDetailDto[]>([])
const totalItems = ref(0)
const page = ref(1)

const filters = ref({ keyword: '', dateFrom: '', dateTo: '' })

const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / 100)))

const displayRows = computed(() => {
  const out: { key: string; isSep: boolean; row: BankTransactionDetailDto }[] = []
  let lastDate: string | null = null
  for (const row of rows.value) {
    if (row.businessDate !== lastDate) {
      out.push({ key: `sep-${row.businessDate}`, isSep: true, row })
      lastDate = row.businessDate
    }
    out.push({ key: `row-${row.id}`, isSep: false, row })
  }
  return out
})

function formatTxTime(iso: string) {
  const [datePart, timeRaw = ''] = iso.split('T')
  const [y, m, d] = datePart.split('-')
  const time = timeRaw.replace('Z', '').split('.')[0]
  const dmy = `${d}/${m}/${y}`
  return !time || time === '00:00:00' ? dmy : `${dmy} ${time}`
}

async function load() {
  if (activeTab.value === 'quytrinh') return
  loading.value = true
  error.value = ''
  try {
    const result = await listBankTransactionDetails({
      page: page.value,
      paymentType: activeTab.value === 'nap' ? 'Prepaid' : 'Debt',
      dateFrom: filters.value.dateFrom,
      dateTo: filters.value.dateTo,
      keyword: filters.value.keyword,
    })
    rows.value = result.items
    totalItems.value = result.totalItems
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Không tải được dữ liệu giao dịch.'
  } finally {
    loading.value = false
  }
}

function selectTab(tab: Tab) {
  if (activeTab.value === tab) return
  activeTab.value = tab
  if (tab !== 'quytrinh') {
    page.value = 1
    rows.value = []
    void load()
  }
}

function applyFilter() {
  page.value = 1
  void load()
}

let searchTimer: ReturnType<typeof setTimeout> | undefined
function onSearchInput() {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(applyFilter, 350)
}

function goPage(nextPage: number) {
  page.value = Math.min(Math.max(1, nextPage), totalPages.value)
  void load()
}

watch(activeTab, () => {
  // reset filters when moving between the two transaction tabs
  filters.value = { keyword: '', dateFrom: '', dateTo: '' }
})
</script>

<template>
  <PageHeader
    title="Danh sách nạp tiền KVC theo ngày"
    subtitle="Lịch sử các giao dịch nạp tiền vào tài khoản khu vui chơi"
  />

  <div class="tabs-bar">
    <button class="tab-btn" :class="{ active: activeTab === 'quytrinh' }" type="button" @click="selectTab('quytrinh')">
      Quy trình nạp tiền KVC
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'nap' }" type="button" @click="selectTab('nap')">
      Nạp tiền cho KVC nạp tiền
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'cn' }" type="button" @click="selectTab('cn')">
      Thanh toán KVC công nợ
    </button>
  </div>

  <section v-if="activeTab === 'quytrinh'" class="card">
    <KanbanBoard :columns="columns" @task-open="selectedTask = $event" @task-move="moveTask" />
  </section>

  <section v-else class="card">
    <div class="toolbar">
      <input
        v-model="filters.keyword"
        class="tb-input"
        placeholder="🔍  Tìm nội dung..."
        @input="onSearchInput"
        @keyup.enter="applyFilter"
      />
      <span class="tb-label">Từ ngày</span>
      <input v-model="filters.dateFrom" class="tb-date" type="date" @change="applyFilter" />
      <span class="tb-label">Đến ngày</span>
      <input v-model="filters.dateTo" class="tb-date" type="date" @change="applyFilter" />
    </div>

    <div v-if="error" class="notice notice-blue" style="margin-bottom: 14px">{{ error }}</div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày giờ giao dịch</th>
            <th class="td-num">Ghi nợ</th>
            <th class="td-num">Ghi có</th>
            <th>Nội dung</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="4">Đang tải...</td></tr>
          <tr v-for="item in displayRows" :key="item.key" :class="{ 'date-separator': item.isSep }">
            <td v-if="item.isSep" colspan="4">📅 {{ formatDate(item.row.businessDate) }}</td>
            <template v-else>
              <td class="cell-muted">{{ formatTxTime(item.row.transactionAtUtc) }}</td>
              <td class="td-num amount amount-green">{{ formatNumber(item.row.debitAmount) }}</td>
              <td class="td-num cell-muted">{{ formatNumber(item.row.creditAmount) }}</td>
              <td class="content-cell">{{ item.row.content }}</td>
            </template>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="4" class="cell-muted" style="text-align: center">Chưa có giao dịch phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pg-info">Tổng {{ totalItems }} dòng</span>
      <button class="pg-btn" type="button" :disabled="page <= 1" @click="goPage(page - 1)">‹</button>
      <button class="pg-btn active" type="button">{{ page }}</button>
      <button class="pg-btn" type="button" :disabled="page >= totalPages" @click="goPage(page + 1)">›</button>
    </div>
  </section>

  <aside v-if="selectedTask" class="task-panel">
    <div class="task-panel-head">
      <div class="task-panel-title">{{ selectedTask.title }}</div>
      <button class="modal-close" type="button" @click="selectedTask = null">✕</button>
    </div>
    <div class="task-panel-body">
      <div class="detail-row"><span class="detail-label">Loại</span><span>{{ selectedTask.park }}</span></div>
      <div class="detail-row"><span class="detail-label">Ngày</span><span>{{ selectedTask.date }}</span></div>
      <div class="detail-row"><span class="detail-label">Số tiền</span><span class="task-money">{{ selectedTask.amount }}</span></div>
      <div class="detail-row"><span class="detail-label">Trạng thái</span><span class="badge" :class="`badge-${selectedTask.tone}`">{{ selectedTask.status }}</span></div>
      <div v-for="(value, key) in selectedTask.details" :key="key" class="detail-row">
        <span class="detail-label">{{ key }}</span>
        <span>{{ value }}</span>
      </div>
    </div>
  </aside>
</template>
