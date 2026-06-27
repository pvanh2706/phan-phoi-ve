<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { ApiClientError } from '../services/apiClient'
import { listParks, type ParkDto } from '../services/parksApi'
import {
  listBankTransactionSummaries,
  type DailyBankTransactionSummaryDto,
} from '../services/summariesApi'
import {
  badgeClassForStatus,
  bankTransactionTypeLabel,
  formatDate,
  formatMoney,
  formatNumber,
  paymentTypeLabel,
  sourceTypeLabel,
  type BankTransactionType,
  type PaymentType,
  type SourceType,
} from '../services/formatters'

const loading = ref(false)
const error = ref('')
const rows = ref<DailyBankTransactionSummaryDto[]>([])
const parks = ref<ParkDto[]>([])
const totalItems = ref(0)
const page = ref(1)

const filters = reactive({
  businessDate: '',
  parkId: '' as number | '',
  paymentType: '' as PaymentType | '',
  transactionType: '' as BankTransactionType | '',
  sourceType: '' as SourceType | '',
})

function errorText(err: unknown, fallback: string) {
  return err instanceof ApiClientError ? err.message : fallback
}

async function loadParks() {
  const result = await listParks({ page: 1 })
  parks.value = result.items
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await listBankTransactionSummaries({
      page: page.value,
      businessDate: filters.businessDate,
      parkId: filters.parkId,
      paymentType: filters.paymentType,
      transactionType: filters.transactionType,
      sourceType: filters.sourceType,
    })
    rows.value = result.items
    totalItems.value = result.totalItems
  } catch (err) {
    error.value = errorText(err, 'Không tải được dữ liệu nạp tiền/thanh toán công nợ.')
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  filters.businessDate = ''
  filters.parkId = ''
  filters.paymentType = ''
  filters.transactionType = ''
  filters.sourceType = ''
  page.value = 1
  void load()
}

watch(filters, () => {
  page.value = 1
  void load()
})

onMounted(async () => {
  await loadParks().catch(() => undefined)
  await load()
})
</script>

<template>
  <PageHeader
    title="Danh sách nạp tiền KVC theo ngày"
    subtitle="Tổng hợp giao dịch ngân hàng theo ngày + KVC + loại giao dịch, lấy từ API hoặc kế toán nhập tay khi job lỗi"
  />

  <section class="card">
    <div class="toolbar">
      <input v-model="filters.businessDate" class="tb-date" type="date" />
      <select v-model="filters.paymentType" class="tb-select">
        <option value="">Tất cả loại thanh toán</option>
        <option value="Prepaid">Nạp trước</option>
        <option value="Debt">Công nợ</option>
      </select>
      <select v-model="filters.transactionType" class="tb-select">
        <option value="">Tất cả loại giao dịch</option>
        <option value="TopUp">Nạp tiền</option>
        <option value="DebtPayment">Thanh toán công nợ</option>
        <option value="Refund">Hoàn tiền</option>
        <option value="Other">Khác</option>
      </select>
      <select v-model="filters.parkId" class="tb-select">
        <option value="">Tất cả KVC</option>
        <option v-for="park in parks" :key="park.id" :value="park.id">
          {{ park.code }} - {{ park.name }}
        </option>
      </select>
      <select v-model="filters.sourceType" class="tb-select">
        <option value="">Tất cả nguồn</option>
        <option value="Api">API</option>
        <option value="Manual">Nhập tay</option>
      </select>
      <button class="btn-secondary" type="button" @click="load">Tải lại</button>
      <button class="btn-secondary" type="button" @click="resetFilters">Xóa lọc</button>
    </div>

    <div v-if="error" class="notice notice-blue" style="margin-bottom: 14px">{{ error }}</div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày</th>
            <th>KVC</th>
            <th>Loại KVC</th>
            <th>Loại giao dịch</th>
            <th>Tiền vào</th>
            <th>Tiền ra</th>
            <th>Số giao dịch</th>
            <th>Nguồn</th>
            <th>Lý do nhập tay</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="9">Đang tải...</td>
          </tr>
          <tr v-for="row in rows" :key="row.id">
            <td>{{ formatDate(row.businessDate) }}</td>
            <td class="cell-strong">{{ row.parkCode }} - {{ row.parkName }}</td>
            <td>{{ paymentTypeLabel(row.paymentType) }}</td>
            <td><span class="badge badge-indigo">{{ bankTransactionTypeLabel(row.transactionType) }}</span></td>
            <td class="amount amount-green">{{ formatMoney(row.totalCreditAmount) }}</td>
            <td class="amount amount-red">{{ formatMoney(row.totalDebitAmount) }}</td>
            <td>{{ formatNumber(row.transactionCount) }}</td>
            <td><span class="badge" :class="badgeClassForStatus(row.sourceType)">{{ sourceTypeLabel(row.sourceType) }}</span></td>
            <td class="cell-muted">{{ row.manualReason || '-' }}</td>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="9" class="cell-muted" style="text-align: center">Chưa có giao dịch phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pg-info">Tổng {{ totalItems }} dòng</span>
      <button class="pg-btn" type="button" :disabled="page <= 1" @click="page -= 1; load()">‹</button>
      <button class="pg-btn active" type="button">{{ page }}</button>
      <button class="pg-btn" type="button" :disabled="page * 100 >= totalItems" @click="page += 1; load()">›</button>
    </div>
  </section>
</template>
