<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { ApiClientError, type PagedResult } from '../services/apiClient'
import { authState } from '../services/authStore'
import {
  listParks,
  type ParkDto,
} from '../services/parksApi'
import {
  listParkBalances,
  listTicketCostSummaries,
  type DailyParkBalanceSnapshotDto,
  type DailyTicketCostSummaryDto,
} from '../services/summariesApi'
import {
  buildReconciliation,
  listReconciliations,
  resolveReconciliation,
  type ParkReconciliationDto,
} from '../services/reconciliationApi'
import {
  badgeClassForStatus,
  formatDate,
  formatMoney,
  formatNumber,
  paymentTypeLabel,
  reconciliationStatusLabel,
  recordStatusLabel,
  sourceTypeLabel,
  type PaymentType,
  type ReconciliationStatus,
  type SourceType,
} from '../services/formatters'
import type { ReportPageKey } from '../data/reports'

const props = defineProps<{
  pageKey: ReportPageKey
}>()

type ApiRow = ParkDto | DailyParkBalanceSnapshotDto | DailyTicketCostSummaryDto | ParkReconciliationDto

const page = ref(1)
const totalItems = ref(0)
const loading = ref(false)
const error = ref('')
const message = ref('')
const rows = ref<ApiRow[]>([])
const parkOptions = ref<ParkDto[]>([])

const filters = reactive({
  keyword: '',
  businessDate: '',
  parkId: '' as number | '',
  paymentType: '' as PaymentType | '',
  sourceType: '' as SourceType | '',
  recordStatus: '' as 'Active' | 'Inactive' | '',
  reconciliationStatus: '' as ReconciliationStatus | '',
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

const config = computed(() => {
  if (props.pageKey === 'parkList') {
    return {
      title: 'Danh sách khu vui chơi',
      subtitle: 'Danh sách KVC lấy từ API danh mục, dùng cho vận hành và đối soát.',
      empty: 'Chưa có khu vui chơi phù hợp.',
    }
  }

  if (props.pageKey === 'dailyBalances') {
    return {
      title: 'Số dư khu vui chơi hằng ngày',
      subtitle: 'Snapshot số dư theo ngày + KVC, gồm dữ liệu API và dữ liệu kế toán nhập tay.',
      empty: 'Chưa có snapshot số dư phù hợp.',
    }
  }

  if (props.pageKey === 'ticketCosts') {
    return {
      title: 'Chi tiết giá vốn vé bán',
      subtitle: 'Tổng giá vốn vé bán đã xử lý theo ngày + KVC; raw response được lưu riêng trong backend.',
      empty: 'Chưa có dữ liệu giá vốn phù hợp.',
    }
  }

  if (props.pageKey === 'reconciliation') {
    return {
      title: 'Đối soát Khu vui chơi',
      subtitle: 'So sánh số dư T, số dư T-1, nạp/thanh toán và giá vốn để phát hiện lệch.',
      empty: 'Chưa có dữ liệu đối soát phù hợp.',
    }
  }

  return {
    title: props.pageKey === 'parkRefunds' ? 'KVC hoàn tiền' : 'Trạng thái hoàn tiền cho khách hàng',
    subtitle: 'Phần này chưa có API backend trong phạm vi đã triển khai.',
    empty: 'Chưa có API cho màn này.',
  }
})

const isUnsupported = computed(() => !['parkList', 'dailyBalances', 'ticketCosts', 'reconciliation'].includes(props.pageKey))
const needsDate = computed(() => ['dailyBalances', 'ticketCosts', 'reconciliation'].includes(props.pageKey))
const needsSource = computed(() => ['dailyBalances', 'ticketCosts'].includes(props.pageKey))
const needsPayment = computed(() => ['parkList', 'dailyBalances', 'ticketCosts', 'reconciliation'].includes(props.pageKey))

function todayIso() {
  const now = new Date()
  const year = now.getFullYear()
  const month = `${now.getMonth() + 1}`.padStart(2, '0')
  const day = `${now.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

function errorText(err: unknown, fallback: string) {
  return err instanceof ApiClientError ? err.message : fallback
}

function setPagedResult<T extends ApiRow>(result: PagedResult<T>) {
  rows.value = result.items
  totalItems.value = result.totalItems
}

async function loadParkOptions() {
  const result = await listParks({ page: 1 })
  parkOptions.value = result.items
}

async function load() {
  if (isUnsupported.value) return
  loading.value = true
  error.value = ''

  try {
    if (props.pageKey === 'parkList') {
      const result = await listParks({
        page: page.value,
        keyword: filters.keyword,
        paymentType: filters.paymentType,
        status: filters.recordStatus,
      })
      setPagedResult(result)
    }

    if (props.pageKey === 'dailyBalances') {
      const result = await listParkBalances({
        page: page.value,
        businessDate: filters.businessDate,
        parkId: filters.parkId,
        paymentType: filters.paymentType,
        sourceType: filters.sourceType,
      })
      setPagedResult(result)
    }

    if (props.pageKey === 'ticketCosts') {
      const result = await listTicketCostSummaries({
        page: page.value,
        businessDate: filters.businessDate,
        parkId: filters.parkId,
        paymentType: filters.paymentType,
        sourceType: filters.sourceType,
      })
      setPagedResult(result)
    }

    if (props.pageKey === 'reconciliation') {
      const result = await listReconciliations({
        page: page.value,
        businessDate: filters.businessDate,
        parkId: filters.parkId,
        paymentType: filters.paymentType,
        status: filters.reconciliationStatus,
      })
      setPagedResult(result)
    }
  } catch (err) {
    error.value = errorText(err, 'Không tải được dữ liệu.')
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  filters.keyword = ''
  filters.businessDate = ''
  filters.parkId = ''
  filters.paymentType = ''
  filters.sourceType = ''
  filters.recordStatus = ''
  filters.reconciliationStatus = ''
  page.value = 1
  void load()
}

function goPage(nextPage: number) {
  page.value = Math.min(Math.max(1, nextPage), totalPages.value)
  void load()
}

async function runBuild() {
  const businessDate = filters.businessDate || todayIso()
  filters.businessDate = businessDate
  loading.value = true
  error.value = ''
  message.value = ''
  try {
    const result = await buildReconciliation(businessDate)
    message.value = `Đã build đối soát ${formatDate(result.businessDate)}: ${result.totalItems} KVC, ${result.varianceCount} lệch, ${result.missingDataCount} thiếu dữ liệu.`
    await load()
  } catch (err) {
    error.value = errorText(err, 'Không build được đối soát.')
  } finally {
    loading.value = false
  }
}

function openResolve(row: ParkReconciliationDto) {
  resolveModal.open = true
  resolveModal.id = row.id
  resolveModal.title = `${row.parkCode} - ${row.parkName} (${formatDate(row.businessDate)})`
  resolveModal.adjustmentAmount = row.adjustmentAmount?.toString() ?? ''
  resolveModal.adjustmentNote = row.adjustmentNote ?? ''
}

async function saveResolve() {
  loading.value = true
  error.value = ''
  message.value = ''
  try {
    const amount = Number(resolveModal.adjustmentAmount.trim().replace(/[,. ]/g, '')) || 0
    await resolveReconciliation(resolveModal.id, amount, resolveModal.adjustmentNote)
    message.value = 'Đã xử lý dòng đối soát.'
    resolveModal.open = false
    await load()
  } catch (err) {
    error.value = errorText(err, 'Không xử lý được dòng đối soát.')
  } finally {
    loading.value = false
  }
}

function asPark(row: ApiRow) {
  return row as ParkDto
}

function asBalance(row: ApiRow) {
  return row as DailyParkBalanceSnapshotDto
}

function asTicketCost(row: ApiRow) {
  return row as DailyTicketCostSummaryDto
}

function asReconciliation(row: ApiRow) {
  return row as ParkReconciliationDto
}

watch(
  () => props.pageKey,
  () => {
    rows.value = []
    totalItems.value = 0
    message.value = ''
    error.value = ''
    resetFilters()
  },
)

watch(
  () => [filters.paymentType, filters.sourceType, filters.recordStatus, filters.reconciliationStatus, filters.parkId],
  () => {
    page.value = 1
    void load()
  },
)

onMounted(async () => {
  if (needsDate.value && props.pageKey === 'reconciliation') {
    filters.businessDate = todayIso()
  }
  await loadParkOptions().catch(() => undefined)
  await load()
})
</script>

<template>
  <PageHeader :title="config.title" :subtitle="config.subtitle" />

  <div v-if="isUnsupported" class="notice notice-indigo">
    Màn này đang được giữ lại để nâng cấp sau. Backend hiện chưa có API cho nghiệp vụ này trong phạm vi Khu vui chơi v1.
  </div>

  <section v-else class="card">
    <div class="toolbar">
      <input
        v-if="props.pageKey === 'parkList'"
        v-model="filters.keyword"
        class="tb-input"
        placeholder="🔍  Tìm khu vui chơi..."
        @keyup.enter="load"
      />
      <input v-if="needsDate" v-model="filters.businessDate" class="tb-date" type="date" @change="page = 1; load()" />
      <select v-if="needsPayment" v-model="filters.paymentType" class="tb-select">
        <option value="">Tất cả loại thanh toán</option>
        <option value="Prepaid">Nạp trước</option>
        <option value="Debt">Công nợ</option>
      </select>
      <select v-if="props.pageKey !== 'parkList'" v-model="filters.parkId" class="tb-select">
        <option value="">Tất cả KVC</option>
        <option v-for="park in parkOptions" :key="park.id" :value="park.id">
          {{ park.code }} - {{ park.name }}
        </option>
      </select>
      <select v-if="needsSource" v-model="filters.sourceType" class="tb-select">
        <option value="">Tất cả nguồn</option>
        <option value="Api">API</option>
        <option value="Manual">Nhập tay</option>
      </select>
      <select v-if="props.pageKey === 'parkList'" v-model="filters.recordStatus" class="tb-select">
        <option value="">Tất cả trạng thái</option>
        <option value="Active">Hoạt động</option>
        <option value="Inactive">Ngừng sử dụng</option>
      </select>
      <select v-if="props.pageKey === 'reconciliation'" v-model="filters.reconciliationStatus" class="tb-select">
        <option value="">Tất cả trạng thái</option>
        <option value="Matched">Khớp</option>
        <option value="Variance">Có lệch</option>
        <option value="MissingData">Thiếu dữ liệu</option>
        <option value="Resolved">Đã xử lý</option>
      </select>
      <button class="btn-secondary" type="button" @click="load">Tải lại</button>
      <button class="btn-secondary" type="button" @click="resetFilters">Xóa lọc</button>
      <button v-if="props.pageKey === 'reconciliation'" class="add-btn" type="button" @click="runBuild">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" d="M12 5v14M5 12h14" />
        </svg>
        Build đối soát
      </button>
    </div>

    <div v-if="message" class="notice notice-indigo" style="margin-bottom: 14px">{{ message }}</div>
    <div v-if="error" class="notice notice-blue" style="margin-bottom: 14px">{{ error }}</div>

    <div class="table-wrap report-table-wrap">
      <table v-if="props.pageKey === 'parkList'">
        <thead>
          <tr>
            <th>Mã KVC</th>
            <th>Tên khu vui chơi</th>
            <th>Loại</th>
            <th>Địa điểm</th>
            <th>Tài khoản</th>
            <th>Hạn mức</th>
            <th>Trạng thái</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="7">Đang tải...</td></tr>
          <tr v-for="row in rows" :key="asPark(row).id">
            <td class="cell-muted">{{ asPark(row).code }}</td>
            <td class="cell-strong">{{ asPark(row).name }}</td>
            <td><span class="badge badge-indigo">{{ paymentTypeLabel(asPark(row).paymentType) }}</span></td>
            <td>{{ asPark(row).location || '-' }}</td>
            <td class="cell-muted">{{ asPark(row).bankName || '-' }} {{ asPark(row).bankAccount ? `- ${asPark(row).bankAccount}` : '' }}</td>
            <td class="amount">{{ formatMoney(asPark(row).creditLimit) }}</td>
            <td><span class="badge" :class="badgeClassForStatus(asPark(row).status)">{{ recordStatusLabel(asPark(row).status) }}</span></td>
          </tr>
        </tbody>
      </table>

      <table v-if="props.pageKey === 'dailyBalances'">
        <thead>
          <tr>
            <th>Ngày</th>
            <th>KVC</th>
            <th>Loại</th>
            <th>Số dư khả dụng</th>
            <th>Công nợ</th>
            <th>Tài khoản</th>
            <th>Nguồn</th>
            <th>Lý do nhập tay</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="8">Đang tải...</td></tr>
          <tr v-for="row in rows" :key="asBalance(row).id">
            <td>{{ formatDate(asBalance(row).businessDate) }}</td>
            <td class="cell-strong">{{ asBalance(row).parkCode }} - {{ asBalance(row).parkName }}</td>
            <td>{{ paymentTypeLabel(asBalance(row).paymentType) }}</td>
            <td class="amount">{{ formatMoney(asBalance(row).availableBalance) }}</td>
            <td class="amount">{{ formatMoney(asBalance(row).currentDebt) }}</td>
            <td class="cell-muted">{{ asBalance(row).bankAccountSnapshot || '-' }}</td>
            <td><span class="badge badge-blue">{{ sourceTypeLabel(asBalance(row).sourceType) }}</span></td>
            <td class="cell-muted">{{ asBalance(row).manualReason || '-' }}</td>
          </tr>
        </tbody>
      </table>

      <table v-if="props.pageKey === 'ticketCosts'">
        <thead>
          <tr>
            <th>Ngày</th>
            <th>KVC</th>
            <th>Loại</th>
            <th>Tổng giá vốn</th>
            <th>Tổng bán</th>
            <th>Số lượng</th>
            <th>Nguồn</th>
            <th>Lý do nhập tay</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="8">Đang tải...</td></tr>
          <tr v-for="row in rows" :key="asTicketCost(row).id">
            <td>{{ formatDate(asTicketCost(row).businessDate) }}</td>
            <td class="cell-strong">{{ asTicketCost(row).parkCode }} - {{ asTicketCost(row).parkName }}</td>
            <td>{{ paymentTypeLabel(asTicketCost(row).paymentType) }}</td>
            <td class="amount">{{ formatMoney(asTicketCost(row).totalTicketCost) }}</td>
            <td class="amount">{{ formatMoney(asTicketCost(row).totalSalesAmount) }}</td>
            <td>{{ formatNumber(asTicketCost(row).totalQuantity) }}</td>
            <td><span class="badge badge-blue">{{ sourceTypeLabel(asTicketCost(row).sourceType) }}</span></td>
            <td class="cell-muted">{{ asTicketCost(row).manualReason || '-' }}</td>
          </tr>
        </tbody>
      </table>

      <table v-if="props.pageKey === 'reconciliation'">
        <thead>
          <tr>
            <th>Ngày</th>
            <th>KVC</th>
            <th>Số dư T-1</th>
            <th>Tăng</th>
            <th>Đã dùng</th>
            <th>Lý thuyết</th>
            <th>Thực tế</th>
            <th>Lệch</th>
            <th>Trạng thái</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="10">Đang tải...</td></tr>
          <tr v-for="row in rows" :key="asReconciliation(row).id">
            <td>{{ formatDate(asReconciliation(row).businessDate) }}</td>
            <td class="cell-strong">
              {{ asReconciliation(row).parkCode }} - {{ asReconciliation(row).parkName }}
              <span v-if="asReconciliation(row).sourceChangedAfterResolved" class="badge badge-amber" style="margin-left: 6px">Nguồn đổi</span>
            </td>
            <td class="amount">{{ formatMoney(asReconciliation(row).previousBalance) }}</td>
            <td class="amount">{{ formatMoney(asReconciliation(row).additionalAmount) }}</td>
            <td class="amount">{{ formatMoney(asReconciliation(row).usedAmount) }}</td>
            <td class="amount">{{ formatMoney(asReconciliation(row).expectedBalance) }}</td>
            <td class="amount">{{ formatMoney(asReconciliation(row).actualBalance) }}</td>
            <td class="amount" :class="{ 'amount-red': Number(asReconciliation(row).varianceAmount ?? 0) !== 0 }">
              {{ formatMoney(asReconciliation(row).varianceAmount) }}
            </td>
            <td>
              <span class="badge" :class="badgeClassForStatus(asReconciliation(row).status)">
                {{ reconciliationStatusLabel(asReconciliation(row).status) }}
              </span>
            </td>
            <td>
              <button
                v-if="canResolve && asReconciliation(row).status !== 'Matched'"
                class="act-btn"
                type="button"
                @click="openResolve(asReconciliation(row))"
              >
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                Xử lý
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <table v-if="!loading && rows.length === 0 && !isUnsupported">
        <tbody>
          <tr><td class="cell-muted" style="text-align: center">{{ config.empty }}</td></tr>
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

  <div v-if="resolveModal.open" class="modal-overlay" @click.self="resolveModal.open = false">
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
