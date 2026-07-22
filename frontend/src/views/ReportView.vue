<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
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
import { runParkBalances } from '../services/jobsApi'
import {
  badgeClassForStatus,
  formatDate,
  formatMoney,
  formatNumber,
  paymentTypeLabel,
  reconciliationStatusLabel,
  recordStatusLabel,
  sourceTypeLabel,
  VN_TIME_ZONE,
  type PaymentType,
  type ReconciliationStatus,
  type SourceType,
} from '../services/formatters'
import type { ReportPageKey } from '../data/reports'
import { isHiddenParkCode } from '../data/hiddenParks'

const props = defineProps<{
  pageKey: ReportPageKey
}>()

type ApiRow = ParkDto | DailyParkBalanceSnapshotDto | DailyTicketCostSummaryDto | ParkReconciliationDto

const page = ref(1)
const totalItems = ref(0)
const loading = ref(false)
const rows = ref<ApiRow[]>([])
const toast = useToast()
const parkOptions = ref<ParkDto[]>([])
const balanceTab = ref<'nap' | 'cn'>('nap')

const balanceDisplayRows = computed(() => {
  const out: { key: string; isSep: boolean; row: DailyParkBalanceSnapshotDto }[] = []
  let lastDate: string | null = null
  for (const row of rows.value as DailyParkBalanceSnapshotDto[]) {
    if (row.businessDate !== lastDate) {
      out.push({ key: `sep-${row.businessDate}`, isSep: true, row })
      lastDate = row.businessDate
    }
    out.push({ key: `row-${row.id}`, isSep: false, row })
  }
  return out
})

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
      subtitle: 'Theo dõi số dư và công nợ các khu vui chơi theo ngày',
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
    title: 'Đang phát triển',
    subtitle: 'Phần này chưa có API backend trong phạm vi đã triển khai.',
    empty: 'Chưa có API cho màn này.',
  }
})

const isUnsupported = computed(() => !['parkList', 'dailyBalances', 'ticketCosts', 'reconciliation'].includes(props.pageKey))
const needsDate = computed(() => ['dailyBalances', 'ticketCosts', 'reconciliation'].includes(props.pageKey))
const needsSource = computed(() => props.pageKey === 'ticketCosts')
const needsPayment = computed(() => ['parkList', 'dailyBalances', 'ticketCosts', 'reconciliation'].includes(props.pageKey))

function todayIso() {
  const now = new Date()
  const year = now.getFullYear()
  const month = `${now.getMonth() + 1}`.padStart(2, '0')
  const day = `${now.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

function formatTime(value?: string | null) {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleTimeString('vi-VN', { hour12: false, timeZone: VN_TIME_ZONE })
}

function selectBalanceTab(tab: 'nap' | 'cn') {
  if (balanceTab.value === tab) return
  balanceTab.value = tab
  filters.paymentType = tab === 'nap' ? 'Prepaid' : 'Debt'
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

  try {
    if (props.pageKey === 'parkList') {
      const result = await listParks({
        page: page.value,
        keyword: filters.keyword,
        paymentType: filters.paymentType,
        status: filters.recordStatus,
      })
      setPagedResult({ ...result, items: result.items.filter((item) => !isHiddenParkCode(item.code)) })
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
    toast.error(errorText(err, 'Không tải được dữ liệu.'))
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
  try {
    const result = await buildReconciliation(businessDate)
    toast.success(
      `Đã build đối soát ${formatDate(result.businessDate)}: ${result.totalItems} KVC, ${result.varianceCount} lệch, ${result.missingDataCount} thiếu dữ liệu.`,
      { duration: 6000 },
    )
    await load()
  } catch (err) {
    toast.error(errorText(err, 'Không build được đối soát.'))
  } finally {
    loading.value = false
  }
}

async function runBalanceApiTest() {
  const businessDate = filters.businessDate || todayIso()
  filters.businessDate = businessDate
  page.value = 1
  loading.value = true

  try {
    const result = await runParkBalances({ businessDate })
    toast.success(
      `Đã gọi API số dư ${formatDate(result.businessDate)}: ${result.successItems}/${result.totalItems} KVC thành công, ${result.failedItems} lỗi. Dữ liệu cùng ngày đã được ghi đè.`,
      { duration: 6000 },
    )
    await load()
  } catch (err) {
    toast.error(errorText(err, 'Không gọi được API số dư KVC.'))
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
  try {
    const amount = Number(resolveModal.adjustmentAmount.trim().replace(/[,. ]/g, '')) || 0
    await resolveReconciliation(resolveModal.id, amount, resolveModal.adjustmentNote)
    toast.success('Đã xử lý dòng đối soát.')
    resolveModal.open = false
    await load()
  } catch (err) {
    toast.error(errorText(err, 'Không xử lý được dòng đối soát.'))
  } finally {
    loading.value = false
  }
}

function asPark(row: ApiRow) {
  return row as ParkDto
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
    balanceTab.value = 'nap'
    resetFilters()
    if (props.pageKey === 'dailyBalances') {
      filters.paymentType = 'Prepaid'
    }
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
  if (props.pageKey === 'dailyBalances') {
    filters.paymentType = 'Prepaid'
  }
  await loadParkOptions().catch(() => undefined)
  await load()
})
</script>

<template>
  <PageHeader :title="config.title" :subtitle="config.subtitle" />

  <div v-if="props.pageKey === 'dailyBalances'" class="tabs-bar">
    <button class="tab-btn" :class="{ active: balanceTab === 'nap' }" type="button" @click="selectBalanceTab('nap')">
      Số dư KVC nạp tiền
    </button>
    <button class="tab-btn" :class="{ active: balanceTab === 'cn' }" type="button" @click="selectBalanceTab('cn')">
      Số dư KVC công nợ
    </button>
  </div>

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
      <select v-if="needsPayment && props.pageKey !== 'dailyBalances'" v-model="filters.paymentType" class="tb-select">
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
      <button v-if="props.pageKey === 'dailyBalances'" class="add-btn" type="button" :disabled="loading" @click="runBalanceApiTest">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 3l14 9-14 9V3z" />
        </svg>
        Gọi API test
      </button>
      <button v-if="props.pageKey === 'reconciliation'" class="add-btn" type="button" @click="runBuild">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" d="M12 5v14M5 12h14" />
        </svg>
        Build đối soát
      </button>
    </div>

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

      <table v-if="props.pageKey === 'dailyBalances' && balanceTab === 'nap'">
        <thead>
          <tr>
            <th>Ngày VN</th>
            <th>Giờ</th>
            <th>Loại topup</th>
            <th>Tên KVC</th>
            <th>Số dư khả dụng</th>
            <th>Mã KVC</th>
            <th>Mã Ngân hàng</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="7">Đang tải...</td></tr>
          <tr v-for="item in balanceDisplayRows" :key="item.key" :class="{ 'date-separator': item.isSep }">
            <td v-if="item.isSep" colspan="7">📅 {{ formatDate(item.row.businessDate) }}</td>
            <template v-else>
              <td>{{ formatDate(item.row.businessDate) }}</td>
              <td class="cell-muted">{{ formatTime(item.row.createdAtUtc) }}</td>
              <td><span class="badge badge-teal">{{ paymentTypeLabel(item.row.paymentType) }}</span></td>
              <td class="cell-strong">{{ item.row.parkName }}</td>
              <td class="amount" :class="Number(item.row.availableBalance) < 0 ? 'amount-red' : 'amount-green'">
                {{ formatMoney(item.row.availableBalance) }}
              </td>
              <td class="mono">{{ item.row.parkCode }}</td>
              <td class="mono">{{ item.row.bankAccountSnapshot || '-' }}</td>
            </template>
          </tr>
        </tbody>
      </table>

      <table v-if="props.pageKey === 'dailyBalances' && balanceTab === 'cn'">
        <thead>
          <tr>
            <th>Ngày VN</th>
            <th>Giờ</th>
            <th>Loại topup</th>
            <th>Tên KVC</th>
            <th>Số dư khả dụng</th>
            <th>Mã KVC</th>
            <th>TK Ngân hàng</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="7">Đang tải...</td></tr>
          <tr v-for="item in balanceDisplayRows" :key="item.key" :class="{ 'date-separator': item.isSep }">
            <td v-if="item.isSep" colspan="7">📅 {{ formatDate(item.row.businessDate) }}</td>
            <template v-else>
              <td>{{ formatDate(item.row.businessDate) }}</td>
              <td class="cell-muted">{{ formatTime(item.row.createdAtUtc) }}</td>
              <td><span class="badge badge-indigo">{{ paymentTypeLabel(item.row.paymentType) }}</span></td>
              <td class="cell-strong">{{ item.row.parkName }}</td>
              <td class="amount" :class="Number(item.row.availableBalance) < 0 ? 'amount-red' : 'amount-green'">
                {{ formatMoney(item.row.availableBalance) }}
              </td>
              <td class="mono">{{ item.row.parkCode }}</td>
              <td class="mono">{{ item.row.bankAccountSnapshot || '-' }}</td>
            </template>
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
          <label class="form-label">Ghi chú xử lý <span class="required-mark">*</span></label>
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
