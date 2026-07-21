<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { ApiClientError } from '../services/apiClient'
import {
  buildReconciliation,
  listReconciliations,
  resolveReconciliation,
  type ParkReconciliationDto,
} from '../services/reconciliationApi'
import { authState } from '../services/authStore'
import { formatDate, formatNumber } from '../services/formatters'

const activeTab = ref<'nap' | 'cn'>('nap')
const page = ref(1)
const totalItems = ref(0)
const loading = ref(false)
const rows = ref<ParkReconciliationDto[]>([])

const toast = useToast()

const filters = reactive({
  keyword: '',
  dateFrom: '',
  dateTo: '',
  variance: '' as '' | 'lech' | 'ok',
})

const resolveModal = reactive({
  open: false,
  id: 0,
  title: '',
  adjustmentAmount: '',
  adjustmentNote: '',
})

const canResolve = computed(() => authState.user?.role === 'Admin' || authState.user?.role === 'Accountant')
const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / 100)))

function todayIso() {
  const now = new Date()
  return `${now.getFullYear()}-${`${now.getMonth() + 1}`.padStart(2, '0')}-${`${now.getDate()}`.padStart(2, '0')}`
}

async function load() {
  loading.value = true
  try {
    const result = await listReconciliations({
      page: page.value,
      paymentType: activeTab.value === 'nap' ? 'Prepaid' : 'Debt',
      dateFrom: filters.dateFrom,
      dateTo: filters.dateTo,
      keyword: filters.keyword,
      hasVariance: filters.variance === '' ? '' : filters.variance === 'lech',
    })
    rows.value = result.items
    totalItems.value = result.totalItems
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không tải được dữ liệu.')
  } finally {
    loading.value = false
  }
}

function selectTab(tab: 'nap' | 'cn') {
  if (activeTab.value === tab) return
  activeTab.value = tab
  page.value = 1
  void load()
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

function variance(row: ParkReconciliationDto) {
  return Number(row.varianceAmount ?? 0)
}

// Dữ liệu (1)-(5) không có (park mới, ngày chưa có giao dịch...) hiển thị là 0
// thay vì để trống, tránh gây hiểu lầm là công thức (4)=(1)+(2)-(3) không tính được.
function amt(value?: number | null) {
  return Number(value ?? 0)
}

function formatVariance(row: ParkReconciliationDto) {
  const n = variance(row)
  if (n === 0) return '0'
  return n > 0 ? `+${formatNumber(Math.abs(n))}` : `-${formatNumber(Math.abs(n))}`
}

function varianceClass(row: ParkReconciliationDto) {
  const n = variance(row)
  return n > 0 ? 'lech-pos' : n < 0 ? 'lech-neg' : 'lech-zero'
}

function cbDisabled(row: ParkReconciliationDto) {
  return variance(row) === 0 || row.status === 'Resolved' || !canResolve.value
}

function onCbClick(row: ParkReconciliationDto) {
  if (cbDisabled(row)) return
  resolveModal.open = true
  resolveModal.id = row.id
  resolveModal.title = `${row.parkCode} - ${row.parkName} (${formatDate(row.businessDate)})`
  resolveModal.adjustmentAmount = (row.varianceAmount ?? 0).toString()
  resolveModal.adjustmentNote = ''
}

async function saveResolve() {
  loading.value = true
  try {
    const amount = Number(resolveModal.adjustmentAmount.trim().replace(/[,. ]/g, '')) || 0
    await resolveReconciliation(resolveModal.id, amount, resolveModal.adjustmentNote)
    toast.success('Đã đánh dấu xử lý dòng đối soát.')
    resolveModal.open = false
    await load()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không xử lý được dòng đối soát.')
  } finally {
    loading.value = false
  }
}

async function runBuild() {
  const businessDate = filters.dateTo || filters.dateFrom || todayIso()
  loading.value = true
  try {
    const result = await buildReconciliation(businessDate)
    toast.success(
      `Đã build đối soát ${formatDate(result.businessDate)}: ${result.totalItems} KVC, ${result.varianceCount} lệch, ${result.missingDataCount} thiếu dữ liệu.`,
      { duration: 6000 },
    )
    await load()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không build được đối soát.')
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <PageHeader title="Đối soát Khu vui chơi" subtitle="Đối soát số dư tài khoản khu vui chơi theo ngày" />

  <div class="tabs-bar">
    <button class="tab-btn" :class="{ active: activeTab === 'nap' }" type="button" @click="selectTab('nap')">
      KVC nạp tiền
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'cn' }" type="button" @click="selectTab('cn')">
      KVC công nợ
    </button>
  </div>

  <section class="card">
    <div class="toolbar">
      <input
        v-model="filters.keyword"
        class="tb-input"
        placeholder="🔍  Tìm mã KVC, tên KVC..."
        @input="onSearchInput"
        @keyup.enter="applyFilter"
      />
      <span class="tb-label">Từ ngày</span>
      <input v-model="filters.dateFrom" class="tb-date" type="date" @change="applyFilter" />
      <span class="tb-label">Đến ngày</span>
      <input v-model="filters.dateTo" class="tb-date" type="date" @change="applyFilter" />
      <select v-model="filters.variance" class="tb-select" @change="applyFilter">
        <option value="">Tất cả</option>
        <option value="lech">Có lệch</option>
        <option value="ok">Khớp</option>
      </select>
      <button class="add-btn" type="button" @click="runBuild">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" d="M12 5v14M5 12h14" />
        </svg>
        Build đối soát
      </button>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày T-1</th>
            <th>Ngày T</th>
            <th>TK Ngân hàng</th>
            <th>Mã KVC</th>
            <th>Khu vui chơi</th>
            <th class="th-num">(1) Số dư Ngày T-1</th>
            <th class="th-num">(2) Số nạp thêm</th>
            <th class="th-num">(3) Số đã dùng</th>
            <th class="th-num">(4)=(1)+(2)-(3) Số dư mới tự tính</th>
            <th class="th-num">(5) Số dư Ngày T</th>
            <th class="th-num">(5)-(4) Lệch</th>
            <th class="td-center">Cảnh báo</th>
            <th class="td-center">Đã xử lý</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="13">Đang tải...</td></tr>
          <tr v-for="row in rows" :key="row.id" :class="{ 'row-handled': row.status === 'Resolved' }">
            <td class="mono">{{ row.previousBusinessDate ? formatDate(row.previousBusinessDate) : '-' }}</td>
            <td class="mono">{{ formatDate(row.businessDate) }}</td>
            <td class="mono">{{ row.bankAccount || '-' }}</td>
            <td>{{ row.parkCode }}</td>
            <td class="cell-strong">{{ row.parkName }}</td>
            <td class="td-num amount amount-blue">{{ formatNumber(amt(row.previousBalance)) }}</td>
            <td class="td-num amount amount-green">{{ formatNumber(amt(row.additionalAmount)) }}</td>
            <td class="td-num amount amount-red">{{ formatNumber(amt(row.usedAmount)) }}</td>
            <td class="td-num amount amount-blue">{{ formatNumber(amt(row.expectedBalance)) }}</td>
            <td class="td-num amount amount-blue">{{ formatNumber(amt(row.actualBalance)) }}</td>
            <td class="td-num" :class="varianceClass(row)">{{ formatVariance(row) }}</td>
            <td class="td-center">
              <span v-if="variance(row) < 0" class="warn-cell warn-neg">⚠ Thiếu {{ formatNumber(Math.abs(variance(row))) }} ₫</span>
              <span v-else-if="variance(row) > 0" class="warn-cell warn-pos">⚠ Dư {{ formatNumber(Math.abs(variance(row))) }} ₫</span>
              <span v-else class="warn-cell warn-ok">—</span>
            </td>
            <td class="cb-wrap">
              <input
                type="checkbox"
                class="cb-done"
                :checked="row.status === 'Resolved'"
                :disabled="cbDisabled(row)"
                :title="variance(row) !== 0 ? 'Đánh dấu đã xử lý cảnh báo' : 'Không có cảnh báo'"
                @click.prevent="onCbClick(row)"
              />
            </td>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="13" class="cell-muted" style="text-align: center">Chưa có dữ liệu đối soát phù hợp.</td>
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

  <div v-if="resolveModal.open" class="modal-overlay">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">Xử lý lệch đối soát</span>
        <button class="modal-close" type="button" @click="resolveModal.open = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="notice notice-indigo" style="margin-bottom: 14px">{{ resolveModal.title }}</div>
        <div class="form-group">
          <label class="form-label">Số điều chỉnh</label>
          <input v-model="resolveModal.adjustmentAmount" class="form-input" inputmode="numeric" />
        </div>
        <div class="form-group">
          <label class="form-label">Ghi chú xử lý</label>
          <textarea v-model="resolveModal.adjustmentNote" class="form-textarea"></textarea>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="resolveModal.open = false">Hủy</button>
        <button class="btn-primary" type="button" @click="saveResolve">Lưu xử lý</button>
      </div>
    </div>
  </div>
</template>
