<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { ApiClientError } from '../services/apiClient'
import { listTicketCostDetails, type TicketSaleCostDetailDto } from '../services/summariesApi'
import { formatDate, formatNumber } from '../services/formatters'

const activeTab = ref<'nap' | 'cn'>('nap')
const page = ref(1)
const totalItems = ref(0)
const loading = ref(false)
const error = ref('')
const rows = ref<TicketSaleCostDetailDto[]>([])

const filters = reactive({
  keyword: '',
  dateFrom: '',
  dateTo: '',
})

const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / 100)))

const displayRows = computed(() => {
  const out: { key: string; isSep: boolean; row: TicketSaleCostDetailDto }[] = []
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

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await listTicketCostDetails({
      page: page.value,
      paymentType: activeTab.value === 'nap' ? 'Prepaid' : 'Debt',
      dateFrom: filters.dateFrom,
      dateTo: filters.dateTo,
      keyword: filters.keyword,
    })
    rows.value = result.items
    totalItems.value = result.totalItems
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Không tải được dữ liệu.'
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

onMounted(load)
</script>

<template>
  <PageHeader
    title="Chi tiết giá vốn vé bán"
    subtitle="Danh sách chi tiết vé bán kèm giá vốn theo từng đơn đặt hàng"
  />

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
        placeholder="🔍  Tìm tên KVC, đại lý, loại vé..."
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
            <th>Mã đặt vé</th>
            <th style="text-align: right">Đơn giá</th>
            <th>Tên loại vé</th>
            <th>Tên nhóm loại vé</th>
            <th style="text-align: right">Tiền bán</th>
            <th style="text-align: right">Tiền vốn</th>
            <th>Mã ĐL bán</th>
            <th style="text-align: right">SL vé</th>
            <th>Mã ĐL mua</th>
            <th>Tên đại lý mua</th>
            <th>Mã KVC</th>
            <th>Tên Khu vui chơi</th>
            <th style="text-align: right">Tạm tính</th>
            <th>ID</th>
            <th>Ngày đặt</th>
            <th>Tên đại lý bán</th>
            <th>Mã loại vé</th>
            <th>Tên ĐL mua cấp trên</th>
            <th>Mã ĐL mua cấp trên</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="19">Đang tải...</td></tr>
          <tr v-for="item in displayRows" :key="item.key" :class="{ 'date-separator': item.isSep }">
            <td v-if="item.isSep" colspan="19">📅 {{ formatDate(item.row.businessDate) }}</td>
            <template v-else>
              <td class="mono">{{ item.row.bookingCode }}</td>
              <td class="mono" style="text-align: right">{{ formatNumber(item.row.unitPrice) }}</td>
              <td>{{ item.row.ticketTypeName }}</td>
              <td>{{ item.row.ticketGroupName || '-' }}</td>
              <td class="amount amount-green" style="text-align: right">{{ formatNumber(item.row.salesAmount) }}</td>
              <td class="amount amount-blue" style="text-align: right">{{ formatNumber(item.row.costAmount) }}</td>
              <td class="mono">{{ item.row.sellingAgentCode || '-' }}</td>
              <td style="text-align: right">{{ item.row.quantity }}</td>
              <td class="mono">{{ item.row.buyingAgentCode || '-' }}</td>
              <td>{{ item.row.buyingAgentName || '-' }}</td>
              <td class="mono">{{ item.row.parkCodeSnapshot }}</td>
              <td>{{ item.row.parkNameSnapshot }}</td>
              <td class="amount amount-green" style="text-align: right">{{ formatNumber(item.row.subtotal) }}</td>
              <td class="mono">{{ item.row.externalLineId || '-' }}</td>
              <td class="cell-muted">{{ formatDate(item.row.businessDate) }}</td>
              <td>{{ item.row.sellingAgentName || '-' }}</td>
              <td class="mono">{{ item.row.ticketTypeCode || '-' }}</td>
              <td>{{ item.row.parentBuyingAgentName || '-' }}</td>
              <td class="mono">{{ item.row.parentBuyingAgentCode || '-' }}</td>
            </template>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="19" class="cell-muted" style="text-align: center">Chưa có dữ liệu giá vốn phù hợp.</td>
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
