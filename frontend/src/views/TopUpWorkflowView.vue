<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { ApiClientError } from '../services/apiClient'
import {
  listBankTransactionDetails,
  syncBankTransactions,
  uploadBankStatement,
  type BankTransactionDetailDto,
} from '../services/summariesApi'
import { formatDate, formatNumber } from '../services/formatters'

type Tab = 'nap' | 'cn'

const toast = useToast()

const activeTab = ref<Tab>('nap')

// ── Transaction tables (Nạp tiền / Công nợ) ──
const loading = ref(false)
const rows = ref<BankTransactionDetailDto[]>([])
// Dòng đang xem chi tiết các giao dịch đã gộp (popup).
const txDetailRow = ref<BankTransactionDetailDto | null>(null)
const totalItems = ref(0)
const page = ref(1)

const filters = ref({ keyword: '', dateFrom: '', dateTo: '' })

const syncing = ref(false)
const uploading = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)

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
  loading.value = true
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
    toast.error(err instanceof ApiClientError ? err.message : 'Không tải được dữ liệu giao dịch.')
  } finally {
    loading.value = false
  }
}

function selectTab(tab: Tab) {
  if (activeTab.value === tab) return
  activeTab.value = tab
  page.value = 1
  rows.value = []
  void load()
}

function applyFilter() {
  page.value = 1
  void load()
}

async function syncFromEmail() {
  if (syncing.value) return
  syncing.value = true
  try {
    const result = await syncBankTransactions()
    let msg = `Đã nhập ${result.imported} dòng KVC (gộp từ ${result.transactionsParsed} giao dịch) từ ${result.mailsProcessed} email`
    if (result.skippedUnmatched > 0) {
      const accounts = result.unmatchedAccounts.slice(0, 5).join(', ')
      msg += `. Bỏ qua ${result.skippedUnmatched} giao dịch không khớp KVC`
      if (accounts) msg += ` (TK: ${accounts}${result.unmatchedAccounts.length > 5 ? '…' : ''})`
    }
    toast.success(msg + '.', { duration: 6000 })
    page.value = 1
    await load()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không lấy được dữ liệu từ email.')
  } finally {
    syncing.value = false
  }
}

function triggerUploadPicker() {
  fileInputRef.value?.click()
}

async function onStatementFileSelected(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return

  uploading.value = true
  try {
    const result = await uploadBankStatement(file)
    let msg = result.imported === 0
      ? 'Không có giao dịch nào được nhập từ file.'
      : `Đã nhập ${result.imported} dòng KVC (gộp từ ${result.transactionsParsed} giao dịch) từ file tải lên`
    if (result.skippedUnmatched > 0) {
      const accounts = result.unmatchedAccounts.slice(0, 5).join(', ')
      msg += `. Bỏ qua ${result.skippedUnmatched} giao dịch không khớp KVC`
      if (accounts) msg += ` (TK: ${accounts}${result.unmatchedAccounts.length > 5 ? '…' : ''})`
    }
    toast.success(msg + '.', { duration: 6000 })
    page.value = 1
    await load()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không xử lý được file sao kê.')
  } finally {
    uploading.value = false
    input.value = ''
  }
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
  filters.value = { keyword: '', dateFrom: '', dateTo: '' }
})

onMounted(() => {
  void load()
})
</script>

<template>
  <PageHeader
    title="Danh sách nạp tiền KVC theo ngày"
    subtitle="Lịch sử các giao dịch nạp tiền vào tài khoản khu vui chơi"
  />

  <div class="tabs-bar">
    <button class="tab-btn" :class="{ active: activeTab === 'nap' }" type="button" @click="selectTab('nap')">
      Nạp tiền cho KVC nạp tiền
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'cn' }" type="button" @click="selectTab('cn')">
      Thanh toán KVC công nợ
    </button>
  </div>

  <section class="card">
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
      <div class="tb-actions-right">
        <button class="btn-primary" type="button" :disabled="syncing" @click="syncFromEmail">
          {{ syncing ? 'Đang lấy...' : '⤓ Get API' }}
        </button>
        <div class="tb-upload-group">
          <input
            ref="fileInputRef"
            type="file"
            accept="application/pdf"
            style="display: none"
            @change="onStatementFileSelected"
          />
          <button
            class="btn-secondary"
            type="button"
            title="Dùng khi ngân hàng bị lỗi, không gửi được sao kê qua email: tải file PDF sao kê lên đây để nhập tay thay cho Get API."
            :disabled="uploading"
            @click="triggerUploadPicker"
          >
            {{ uploading ? 'Đang xử lý...' : '📤 Upload tay sao kê' }}
          </button>
          <p class="tb-hint">
            * Chỉ dùng khi ngân hàng gặp sự cố và không gửi được sao kê qua email cho "Get API".
          </p>
        </div>
      </div>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày giờ giao dịch</th>
            <th>Tên khu vui chơi</th>
            <th class="td-num">Ghi nợ</th>
            <th class="td-num">Ghi có</th>
            <th>Nội dung</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="5">Đang tải...</td></tr>
          <tr v-for="item in displayRows" :key="item.key" :class="{ 'date-separator': item.isSep }">
            <td v-if="item.isSep" colspan="5">📅 {{ formatDate(item.row.businessDate) }}</td>
            <template v-else>
              <td class="cell-muted">{{ formatTxTime(item.row.transactionAtUtc) }}</td>
              <td>{{ item.row.parkName ?? '—' }}</td>
              <td class="td-num amount amount-green">{{ formatNumber(item.row.debitAmount) }}</td>
              <td class="td-num cell-muted">{{ formatNumber(item.row.creditAmount) }}</td>
              <td class="content-cell">
                <button
                  v-if="item.row.lineItems && item.row.lineItems.length"
                  type="button"
                  class="tx-detail-link"
                  @click="txDetailRow = item.row"
                >{{ item.row.content }}</button>
                <template v-else>{{ item.row.content }}</template>
              </td>
            </template>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="5" class="cell-muted" style="text-align: center">Chưa có giao dịch phù hợp.</td>
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

  <div v-if="txDetailRow" class="modal-overlay">
    <div class="modal tx-detail-modal">
      <div class="modal-header">
        <div class="modal-title">Chi tiết giao dịch — {{ txDetailRow.parkName ?? '—' }}</div>
        <button class="modal-close" type="button" @click="txDetailRow = null">✕</button>
      </div>
      <div class="modal-body">
        <div class="notice notice-indigo" style="margin-bottom: 14px">
          {{ formatDate(txDetailRow.businessDate) }} · Gồm {{ txDetailRow.lineItems?.length ?? 0 }} giao dịch
        </div>
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
              <tr v-for="(line, idx) in txDetailRow.lineItems ?? []" :key="idx">
                <td class="cell-muted">{{ formatTxTime(line.transactionAtUtc) }}</td>
                <td class="td-num amount amount-green">{{ formatNumber(line.debitAmount) }}</td>
                <td class="td-num cell-muted">{{ formatNumber(line.creditAmount) }}</td>
                <td class="content-cell">{{ line.content }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="txDetailRow = null">Đóng</button>
      </div>
    </div>
  </div>
</template>
