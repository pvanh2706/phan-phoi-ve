<script setup lang="ts">
import { computed, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import PaginationBar from '../components/ui/PaginationBar.vue'
import { formatDate, formatMoney } from '../services/formatters'

const SAMPLE_AGENCIES = [
  'Oneinventory_Hanh BANA',
  'Oneinventory_Anh Thư',
  'Oneinventory_AGSAPA',
  'Oneinventory_Sao Mai Sa Pa',
  'Oneinventory_Phương Lan',
  'Oneinventory_Cát Bà Vi Vu',
  'Oneinventory_NhatKimYenTicket',
]

function hashSeed(str: string): number {
  let h = 2166136261
  for (let i = 0; i < str.length; i++) {
    h ^= str.charCodeAt(i)
    h = Math.imul(h, 16777619)
  }
  return h >>> 0
}

function seededFloat(str: string): number {
  return (hashSeed(str) % 10000) / 10000
}

function daysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate()
}

interface MonthlyRow {
  id: string
  date: string
  agency: string
  openingMonth: number
  topup: number
  used: number
  running: number
  closingMonth: number
}

function buildMonthlyRows(yearMonth: string): MonthlyRow[] {
  const [yearStr, monthStr] = yearMonth.split('-')
  const year = Number(yearStr)
  const month = Number(monthStr)
  if (!year || !month) return []

  const total = daysInMonth(year, month)
  const rows: MonthlyRow[] = []

  for (const agency of SAMPLE_AGENCIES) {
    const opening = 5_000_000 + Math.floor(seededFloat(`${agency}-${yearMonth}-opening`) * 20) * 1_000_000
    let running = opening
    const dayRows: Omit<MonthlyRow, 'openingMonth' | 'closingMonth'>[] = []

    for (let day = 1; day <= total; day++) {
      const dateStr = `${yearStr}-${monthStr}-${String(day).padStart(2, '0')}`
      const topupOccurs = seededFloat(`${agency}-${dateStr}-topupChance`) < 0.18
      const topup = topupOccurs
        ? 5_000_000 + Math.floor(seededFloat(`${agency}-${dateStr}-topupAmt`) * 16) * 1_000_000
        : 0
      const usedRaw = Math.floor(seededFloat(`${agency}-${dateStr}-used`) * 8) * 250_000
      const used = Math.min(usedRaw, running + topup)
      running = running + topup - used

      dayRows.push({ id: `${agency}-${dateStr}`, date: dateStr, agency, topup, used, running })
    }

    const closing = running
    for (const row of dayRows) {
      rows.push({ ...row, openingMonth: opening, closingMonth: closing })
    }
  }

  return rows
}

const today = new Date()
const selectedMonth = ref(`${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}`)
const selectedAgency = ref('')
const keyword = ref('')
const page = ref(1)
const pageSize = 15

const allRows = computed(() => buildMonthlyRows(selectedMonth.value))

const filteredRows = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  return allRows.value.filter((row) => {
    if (selectedAgency.value && row.agency !== selectedAgency.value) return false
    if (kw && !row.agency.toLowerCase().includes(kw)) return false
    return true
  })
})

const visibleRows = computed(() => {
  const start = (page.value - 1) * pageSize
  return filteredRows.value.slice(start, start + pageSize)
})

function onFilterChange() {
  page.value = 1
}
</script>

<template>
  <PageHeader
    title="Số dư theo ngày của các đại lý (tự tính)"
    subtitle="Số dư từng ngày trong tháng được tự tính = số dư đầu tháng + nạp thêm − đã dùng lũy kế"
  />

  <section class="card">
    <div class="toolbar">
      <span class="tb-label">Tháng</span>
      <input v-model="selectedMonth" class="tb-date" type="month" @change="onFilterChange" />
      <select v-model="selectedAgency" class="tb-select" @change="onFilterChange">
        <option value="">Tất cả đại lý</option>
        <option v-for="agency in SAMPLE_AGENCIES" :key="agency" :value="agency">{{ agency }}</option>
      </select>
      <input
        v-model="keyword"
        class="tb-input"
        placeholder="🔍  Tìm tên đại lý..."
        @input="onFilterChange"
      />
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày</th>
            <th>Đại lý</th>
            <th>Số dư ngày 01 đầu tháng</th>
            <th>Số nạp thêm</th>
            <th>Số đã dùng</th>
            <th>Số dư mới tự tính</th>
            <th>Số dư ngày cuối tháng</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in visibleRows" :key="row.id">
            <td class="cell-muted">{{ formatDate(row.date) }}</td>
            <td class="cell-strong">{{ row.agency }}</td>
            <td class="cell-muted">{{ formatMoney(row.openingMonth) }}</td>
            <td class="amount" :class="row.topup > 0 ? 'amount-green' : ''">{{ row.topup > 0 ? formatMoney(row.topup) : '0 đ' }}</td>
            <td class="amount" :class="row.used > 0 ? 'amount-red' : ''">{{ row.used > 0 ? formatMoney(row.used) : '0 đ' }}</td>
            <td class="amount amount-green">{{ formatMoney(row.running) }}</td>
            <td class="cell-muted">{{ formatMoney(row.closingMonth) }}</td>
          </tr>
          <tr v-if="visibleRows.length === 0">
            <td colspan="7" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <PaginationBar v-model:page="page" :total="filteredRows.length" :page-size="pageSize" />
  </section>
</template>
