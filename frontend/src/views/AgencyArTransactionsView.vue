<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { ApiClientError } from '../services/apiClient'
import { authState } from '../services/authStore'
import {
  listAgencyArTransactions,
  syncAgencyArTransactions,
  type AgencyArTransactionDto,
} from '../services/agencyArTransactionsApi'
import { formatDate, formatDateTime, formatMoney } from '../services/formatters'

const page = ref(1)
const totalItems = ref(0)
const totalAmount = ref(0)
const loading = ref(false)
const syncing = ref(false)
const rows = ref<AgencyArTransactionDto[]>([])

const toast = useToast()

// Chỉ Admin/Kế toán được chạy đồng bộ tay (§15); ẩn nút với vai trò khác.
const canSync = computed(() => authState.user?.role === 'Admin' || authState.user?.role === 'Accountant')

const filters = reactive({
  keyword: '',
  dateFrom: '',
  dateTo: '',
})

const syncModal = reactive({
  open: false,
  businessDate: '',
})

function todayIso() {
  const now = new Date()
  return `${now.getFullYear()}-${`${now.getMonth() + 1}`.padStart(2, '0')}-${`${now.getDate()}`.padStart(2, '0')}`
}

function openSyncModal() {
  syncModal.businessDate = todayIso()
  syncModal.open = true
}

const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / 100)))

async function load() {
  loading.value = true
  try {
    const result = await listAgencyArTransactions({
      page: page.value,
      dateFrom: filters.dateFrom,
      dateTo: filters.dateTo,
      keyword: filters.keyword,
    })
    rows.value = result.items
    totalItems.value = result.totalItems
    totalAmount.value = result.totalAmount
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không tải được dữ liệu.')
  } finally {
    loading.value = false
  }
}

async function runSync() {
  if (!syncModal.businessDate) {
    toast.error('Vui lòng chọn ngày cần lấy dữ liệu.')
    return
  }
  syncModal.open = false
  syncing.value = true
  try {
    const result = await syncAgencyArTransactions(syncModal.businessDate)
    const date = formatDate(result.businessDate)
    if (result.validBookingTransactions === 0) {
      toast.info(`Không có giao dịch 'Thanh toán tiền cho booking' cho ngày ${date}.`, { duration: 6000 })
    } else {
      let msg = `Đã đồng bộ ngày ${date}: ${result.inserted} thêm mới, ${result.updated} cập nhật, ${result.unchanged} không đổi (từ ${result.validBookingTransactions} giao dịch booking hợp lệ).`
      if (result.errorRows > 0) {
        msg += ` ${result.errorRows} dòng lỗi bị bỏ qua.`
      }
      toast.success(msg, { duration: 7000 })
    }
    page.value = 1
    await load()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không lấy được giao dịch đại lý trên AR.')
  } finally {
    syncing.value = false
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

onMounted(load)
</script>

<template>
  <PageHeader
    title="Giao dịch của các đại lý trên AR"
    subtitle="Danh sách giao dịch trừ tiền của các đại lý trên hệ thống AR (ar.ezcloud.vn), đồng bộ tự động"
  />

  <section class="card">
    <div class="toolbar">
      <input
        v-model="filters.keyword"
        class="tb-input"
        placeholder="🔍  Tìm mã booking, tên/mã đại lý..."
        @input="onSearchInput"
        @keyup.enter="applyFilter"
      />
      <span class="tb-label">Từ ngày</span>
      <input v-model="filters.dateFrom" class="tb-date" type="date" @change="applyFilter" />
      <span class="tb-label">Đến ngày</span>
      <input v-model="filters.dateTo" class="tb-date" type="date" @change="applyFilter" />
      <button
        v-if="canSync"
        class="add-btn"
        type="button"
        :disabled="syncing || loading"
        @click="openSyncModal"
      >
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 3l14 9-14 9V3z" />
        </svg>
        {{ syncing ? 'Đang lấy...' : 'Lấy dữ liệu' }}
      </button>
    </div>

    <p class="tb-total">
      Tổng <strong>{{ totalItems }}</strong> giao dịch · Tổng số tiền
      <strong class="amount-red">{{ formatMoney(totalAmount) }}</strong>
    </p>

    <div v-if="syncModal.open" class="modal-overlay">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Lấy giao dịch đại lý trên AR</span>
          <button class="modal-close" type="button" @click="syncModal.open = false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Ngày cần lấy dữ liệu <span class="required-mark">*</span></label>
            <input v-model="syncModal.businessDate" class="form-input" type="date" :max="todayIso()" />
          </div>
          <div class="notice notice-indigo">
            Hệ thống sẽ đăng nhập AR, tải file Excel giao dịch của ngày này, lọc các dòng "Thanh toán tiền
            cho booking" và upsert vào DB. Bản ghi trùng theo khóa định danh sẽ không bị tạo lại.
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn-secondary" type="button" @click="syncModal.open = false">Hủy</button>
          <button class="btn-primary" type="button" :disabled="syncing" @click="runSync">Lấy dữ liệu</button>
        </div>
      </div>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Mã booking</th>
            <th>Tên đại lý</th>
            <th>Ngày tạo giờ</th>
            <th style="text-align: right">Số tiền</th>
            <th>Mã đại lý</th>
            <th>Mã TK công nợ</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="6" class="cell-muted" style="text-align: center">Đang tải...</td></tr>
          <tr v-for="row in rows" :key="row.id">
            <td class="cell-strong mono">{{ row.bookingId }}</td>
            <td>{{ row.travelAgentName || '-' }}</td>
            <td class="cell-muted">{{ formatDateTime(row.transactionDate) }}</td>
            <td class="amount amount-red" style="text-align: right">{{ formatMoney(row.amount) }}</td>
            <td class="mono">{{ row.travelAgentCode || '-' }}</td>
            <td class="mono">{{ row.receivableAccountCode || '-' }}</td>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="6" class="cell-muted" style="text-align: center">Chưa có giao dịch AR phù hợp.</td>
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
</template>

<style scoped>
.tb-total {
  margin: 10px 2px 0;
  font-size: 13px;
  color: var(--text-muted, #6b7280);
}
.tb-total .amount-red {
  color: #dc2626;
}
</style>
