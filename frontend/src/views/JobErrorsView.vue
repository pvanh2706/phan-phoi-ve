<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { ApiClientError } from '../services/apiClient'
import { authState } from '../services/authStore'
import {
  listJobErrors,
  runBankTransactions,
  runParkBalances,
  runTicketCosts,
  sendSyncErrorSummary,
  type ExternalApiSource,
  type JobRunItemDto,
} from '../services/jobsApi'
import { listParks, type ParkDto } from '../services/parksApi'
import {
  saveManualBankTransactionSummary,
  saveManualTicketCostSummary,
} from '../services/summariesApi'
import {
  badgeClassForStatus,
  bankTransactionTypeLabel,
  formatDate,
  formatDateTime,
  jobRunItemStatusLabel,
  type BankTransactionType,
} from '../services/formatters'

const businessDate = ref('')
const source = ref<ExternalApiSource | ''>('')
const status = ref<'Failed' | 'ManualResolved' | ''>('Failed')
const loading = ref(false)
const actionLoading = ref(false)
const items = ref<JobRunItemDto[]>([])
const parks = ref<ParkDto[]>([])

const toast = useToast()

const manualModal = reactive({
  open: false,
  itemId: null as number | null,
  source: '' as ExternalApiSource | '',
  businessDate: '',
  parkId: '' as number | '',
  totalTicketCost: '',
  totalSalesAmount: '',
  totalQuantity: '',
  transactionType: 'TopUp' as BankTransactionType,
  totalDebitAmount: '',
  totalCreditAmount: '',
  transactionCount: '',
  manualReason: '',
})

const canManual = computed(() => authState.user?.role === 'Admin' || authState.user?.role === 'Accountant')

function todayIso() {
  const now = new Date()
  return `${now.getFullYear()}-${`${now.getMonth() + 1}`.padStart(2, '0')}-${`${now.getDate()}`.padStart(2, '0')}`
}

function errorText(err: unknown, fallback: string) {
  return err instanceof ApiClientError ? err.message : fallback
}

function toNumber(value: string) {
  return Number(value.trim().replace(/[,. ]/g, '')) || 0
}

function toNullableNumber(value: string) {
  const trimmed = value.trim().replace(/[,. ]/g, '')
  return trimmed ? Number(trimmed) : null
}

function sourceLabel(value?: ExternalApiSource | null) {
  if (value === 'ParkBalance') return 'Số dư KVC'
  if (value === 'TicketCost') return 'Giá vốn vé bán'
  if (value === 'BankTransaction') return 'Giao dịch ngân hàng'
  return '-'
}

async function loadParks() {
  const result = await listParks({ page: 1 })
  parks.value = result.items
}

async function load() {
  loading.value = true
  try {
    const result = await listJobErrors({
      page: 1,
      businessDate: businessDate.value,
      source: source.value,
      status: status.value,
    })
    items.value = result.items
  } catch (err) {
    toast.error(errorText(err, 'Không tải được lỗi đồng bộ.'))
  } finally {
    loading.value = false
  }
}

async function runJob(kind: ExternalApiSource) {
  actionLoading.value = true
  const request = { businessDate: businessDate.value || todayIso() }
  businessDate.value = request.businessDate
  try {
    if (kind === 'ParkBalance') await runParkBalances(request)
    if (kind === 'TicketCost') await runTicketCosts(request)
    if (kind === 'BankTransaction') await runBankTransactions(request)
    toast.success(`Đã chạy job ${sourceLabel(kind)} cho ngày ${formatDate(request.businessDate)}.`)
    await load()
  } catch (err) {
    toast.error(errorText(err, 'Không chạy được job.'))
  } finally {
    actionLoading.value = false
  }
}

async function sendEmailSummary() {
  actionLoading.value = true
  const request = { businessDate: businessDate.value || todayIso() }
  businessDate.value = request.businessDate
  try {
    const result = await sendSyncErrorSummary(request)
    toast.success(result.message || 'Đã chạy gửi email tổng hợp lỗi.')
  } catch (err) {
    toast.error(errorText(err, 'Không gửi được email tổng hợp lỗi.'))
  } finally {
    actionLoading.value = false
  }
}

function openManual(item: JobRunItemDto) {
  manualModal.open = true
  manualModal.itemId = item.id
  manualModal.source = item.source ?? ''
  manualModal.businessDate = (item.businessDate ?? businessDate.value) || todayIso()
  manualModal.parkId = item.parkId ?? ''
  manualModal.totalTicketCost = ''
  manualModal.totalSalesAmount = ''
  manualModal.totalQuantity = ''
  manualModal.transactionType = item.source === 'BankTransaction' ? 'TopUp' : 'TopUp'
  manualModal.totalDebitAmount = ''
  manualModal.totalCreditAmount = ''
  manualModal.transactionCount = ''
  manualModal.manualReason = ''
}

async function saveManual() {
  if (!manualModal.source || !manualModal.parkId) return

  actionLoading.value = true

  try {
    if (manualModal.source === 'TicketCost') {
      await saveManualTicketCostSummary({
        businessDate: manualModal.businessDate,
        parkId: Number(manualModal.parkId),
        totalTicketCost: toNumber(manualModal.totalTicketCost),
        totalSalesAmount: toNullableNumber(manualModal.totalSalesAmount),
        totalQuantity: toNullableNumber(manualModal.totalQuantity),
        manualReason: manualModal.manualReason,
        jobRunItemId: manualModal.itemId,
      })
    }

    if (manualModal.source === 'BankTransaction') {
      await saveManualBankTransactionSummary({
        businessDate: manualModal.businessDate,
        parkId: Number(manualModal.parkId),
        transactionType: manualModal.transactionType,
        totalDebitAmount: toNumber(manualModal.totalDebitAmount),
        totalCreditAmount: toNumber(manualModal.totalCreditAmount),
        transactionCount: toNumber(manualModal.transactionCount),
        manualReason: manualModal.manualReason,
        jobRunItemId: manualModal.itemId,
      })
    }

    toast.success('Đã lưu dữ liệu nhập tay và đánh dấu lỗi đã xử lý.')
    manualModal.open = false
    status.value = ''
    await load()
  } catch (err) {
    toast.error(errorText(err, 'Không lưu được dữ liệu nhập tay.'))
  } finally {
    actionLoading.value = false
  }
}

onMounted(async () => {
  await loadParks().catch(() => undefined)
  await load()
})
</script>

<template>
  <PageHeader
    title="Lỗi đồng bộ cần xử lý"
    subtitle="Các lỗi API theo từng KVC; kế toán nhập tay dữ liệu đúng ngày để không lệch sang ngày hôm sau"
  />

  <section class="card">
    <div class="toolbar">
      <input v-model="businessDate" class="tb-date" type="date" />
      <select v-model="source" class="tb-select">
        <option value="">Tất cả nguồn</option>
        <option value="ParkBalance">Số dư KVC</option>
        <option value="TicketCost">Giá vốn vé bán</option>
        <option value="BankTransaction">Giao dịch ngân hàng</option>
      </select>
      <select v-model="status" class="tb-select">
        <option value="">Tất cả trạng thái</option>
        <option value="Failed">Cần xử lý</option>
        <option value="ManualResolved">Đã nhập tay</option>
      </select>
      <button class="btn-secondary" type="button" @click="load">Lọc</button>
      <button class="btn-secondary" type="button" :disabled="actionLoading" @click="runJob('ParkBalance')">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 3l14 9-14 9V3z" />
        </svg>
        Chạy số dư
      </button>
      <button class="btn-secondary" type="button" :disabled="actionLoading" @click="runJob('TicketCost')">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 3l14 9-14 9V3z" />
        </svg>
        Chạy giá vốn
      </button>
      <button class="btn-secondary" type="button" :disabled="actionLoading" @click="runJob('BankTransaction')">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 3l14 9-14 9V3z" />
        </svg>
        Chạy ngân hàng
      </button>
      <button v-if="canManual" class="add-btn" type="button" :disabled="actionLoading" @click="sendEmailSummary">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <rect x="3" y="5" width="18" height="14" rx="2" />
          <path stroke-linecap="round" stroke-linejoin="round" d="M3 7l9 6 9-6" />
        </svg>
        Gửi email lỗi
      </button>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày</th>
            <th>KVC</th>
            <th>Nguồn</th>
            <th>Trạng thái</th>
            <th>Số lần gọi</th>
            <th>Thời điểm lỗi</th>
            <th>Lỗi</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="8">Đang tải...</td>
          </tr>
          <tr v-for="item in items" :key="item.id">
            <td>{{ formatDate(item.businessDate) }}</td>
            <td class="cell-strong">{{ item.parkCode || '-' }} - {{ item.parkName || '-' }}</td>
            <td>{{ sourceLabel(item.source) }}</td>
            <td><span class="badge" :class="badgeClassForStatus(item.status)">{{ jobRunItemStatusLabel(item.status) }}</span></td>
            <td>{{ item.attemptCount }}</td>
            <td>{{ formatDateTime(item.finishedAtUtc) }}</td>
            <td>{{ item.errorMessage || item.errorCode || '-' }}</td>
            <td>
              <button
                v-if="canManual && item.status === 'Failed' && item.source && item.source !== 'ParkBalance'"
                class="act-btn"
                type="button"
                @click="openManual(item)"
              >
                <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <rect x="3" y="5" width="18" height="14" rx="2" />
                  <path stroke-linecap="round" d="M7 9h.01M11 9h.01M15 9h.01M7 13h10" />
                </svg>
                Nhập tay
              </button>
            </td>
          </tr>
          <tr v-if="!loading && items.length === 0">
            <td colspan="8" class="cell-muted" style="text-align: center">Không có lỗi phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <div v-if="manualModal.open" class="modal-overlay">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">Nhập tay {{ sourceLabel(manualModal.source || null) }}</span>
        <button class="modal-close" type="button" @click="manualModal.open = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Ngày dữ liệu</label>
            <input v-model="manualModal.businessDate" class="form-input" type="date" />
          </div>
          <div class="form-group">
            <label class="form-label">KVC <span class="required-mark">*</span></label>
            <select v-model="manualModal.parkId" class="form-select">
              <option value="">Chọn KVC</option>
              <option v-for="park in parks" :key="park.id" :value="park.id">
                {{ park.code }} - {{ park.name }}
              </option>
            </select>
          </div>
        </div>
        <template v-if="manualModal.source === 'TicketCost'">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Tổng giá vốn</label>
              <input v-model="manualModal.totalTicketCost" class="form-input" inputmode="numeric" />
            </div>
            <div class="form-group">
              <label class="form-label">Tổng tiền bán</label>
              <input v-model="manualModal.totalSalesAmount" class="form-input" inputmode="numeric" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Tổng số lượng vé</label>
            <input v-model="manualModal.totalQuantity" class="form-input" inputmode="numeric" />
          </div>
        </template>

        <template v-if="manualModal.source === 'BankTransaction'">
          <div class="form-group">
            <label class="form-label">Loại giao dịch</label>
            <select v-model="manualModal.transactionType" class="form-select">
              <option value="TopUp">{{ bankTransactionTypeLabel('TopUp') }}</option>
              <option value="DebtPayment">{{ bankTransactionTypeLabel('DebtPayment') }}</option>
              <option value="Refund">{{ bankTransactionTypeLabel('Refund') }}</option>
              <option value="Other">{{ bankTransactionTypeLabel('Other') }}</option>
            </select>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Tổng tiền vào</label>
              <input v-model="manualModal.totalCreditAmount" class="form-input" inputmode="numeric" />
            </div>
            <div class="form-group">
              <label class="form-label">Tổng tiền ra</label>
              <input v-model="manualModal.totalDebitAmount" class="form-input" inputmode="numeric" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Số giao dịch</label>
            <input v-model="manualModal.transactionCount" class="form-input" inputmode="numeric" />
          </div>
        </template>

        <div class="form-group">
          <label class="form-label">Lý do nhập tay <span class="required-mark">*</span></label>
          <textarea v-model="manualModal.manualReason" class="form-textarea"></textarea>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="manualModal.open = false">Hủy</button>
        <button class="btn-primary" type="button" :disabled="actionLoading" @click="saveManual">Lưu nhập tay</button>
      </div>
    </div>
  </div>
</template>
