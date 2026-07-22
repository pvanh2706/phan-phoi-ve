<script setup lang="ts">
import { computed, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { reportPages, type TableCell } from '../data/reports'
import { formatMoney } from '../services/formatters'

function cellText(cell: TableCell): string {
  if (typeof cell === 'string') return cell
  return cell.kind === 'actions' ? '' : cell.text
}

function parseAmount(text: string): number {
  return Number(text.replace(/[^0-9-]/g, '')) || 0
}

// "dd/MM/yyyy HH:mm:ss" (giao dịch AR) -> "yyyy-MM" để gộp theo tháng.
function toMonthKey(text: string): string | null {
  const datePart = text.trim().split(' ')[0]
  const [d, m, y] = datePart.split('/')
  if (!d || !m || !y) return null
  return `${y}-${m}`
}

function monthLabel(key: string): string {
  const [y, m] = key.split('-')
  return `Tháng ${m}/${y}`
}

interface MonthlyAgencyTotal {
  id: string
  monthKey: string
  agency: string
  bookingCount: number
  totalUsed: number
}

// Lấy toàn bộ giao dịch trên AR (mỗi dòng = 1 booking bị trừ tiền) và cộng dồn theo (đại lý, tháng).
// Đây là phép tính tự động (computed) trên dữ liệu AR hiện có — mỗi khi có giao dịch AR mới trong ngày,
// tổng tháng tương ứng sẽ tự cập nhật theo mà không cần thao tác build thủ công.
const monthlyTotals = computed<MonthlyAgencyTotal[]>(() => {
  const arRows = reportPages.agencyArTransactions.tabs[0].rows
  const totals = new Map<string, MonthlyAgencyTotal>()

  for (const row of arRows) {
    const agency = cellText(row.cells[1])
    const monthKey = toMonthKey(cellText(row.cells[2]))
    if (!monthKey) continue
    const amount = parseAmount(cellText(row.cells[3]))

    const key = `${monthKey}__${agency}`
    const existing = totals.get(key)
    if (existing) {
      existing.bookingCount += 1
      existing.totalUsed += amount
    } else {
      totals.set(key, { id: key, monthKey, agency, bookingCount: 1, totalUsed: amount })
    }
  }

  return [...totals.values()].sort((a, b) =>
    b.monthKey === a.monthKey ? b.totalUsed - a.totalUsed : b.monthKey.localeCompare(a.monthKey),
  )
})

const monthOptions = computed(() => {
  const keys = new Set(monthlyTotals.value.map((row) => row.monthKey))
  return [...keys].sort().reverse()
})

const agencyOptions = computed(() => {
  const names = new Set(monthlyTotals.value.map((row) => row.agency))
  return [...names].sort()
})

const selectedMonth = ref('')
const selectedAgency = ref('')
const keyword = ref('')

const filteredRows = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  return monthlyTotals.value.filter((row) => {
    if (selectedMonth.value && row.monthKey !== selectedMonth.value) return false
    if (selectedAgency.value && row.agency !== selectedAgency.value) return false
    if (kw && !row.agency.toLowerCase().includes(kw)) return false
    return true
  })
})

const grandTotal = computed(() => filteredRows.value.reduce((sum, row) => sum + row.totalUsed, 0))
const grandTotalBookings = computed(() => filteredRows.value.reduce((sum, row) => sum + row.bookingCount, 0))
</script>

<template>
  <PageHeader
    title="Tổng tiền các đại lý đã dùng theo tháng"
    subtitle="Tự động cộng dồn hàng ngày từ toàn bộ booking trừ tiền trên AR, gộp theo từng đại lý và từng tháng"
  />

  <section class="card">
    <div class="toolbar">
      <select v-model="selectedMonth" class="tb-select">
        <option value="">Tất cả các tháng</option>
        <option v-for="m in monthOptions" :key="m" :value="m">{{ monthLabel(m) }}</option>
      </select>
      <select v-model="selectedAgency" class="tb-select">
        <option value="">Tất cả đại lý</option>
        <option v-for="a in agencyOptions" :key="a" :value="a">{{ a }}</option>
      </select>
      <input v-model="keyword" class="tb-input" placeholder="🔍  Tìm tên đại lý..." />
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Tháng</th>
            <th>Đại lý</th>
            <th class="td-num">Số booking</th>
            <th class="td-num">Tổng tiền đã dùng (trừ trên AR)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in filteredRows" :key="row.id">
            <td class="cell-muted">{{ monthLabel(row.monthKey) }}</td>
            <td class="cell-strong">{{ row.agency }}</td>
            <td class="td-num cell-muted">{{ row.bookingCount }}</td>
            <td class="td-num amount amount-red">{{ formatMoney(row.totalUsed) }}</td>
          </tr>
          <tr v-if="filteredRows.length === 0">
            <td colspan="4" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp.</td>
          </tr>
        </tbody>
        <tfoot v-if="filteredRows.length > 0">
          <tr>
            <td colspan="2" class="cell-strong">Tổng cộng</td>
            <td class="td-num cell-strong">{{ grandTotalBookings }}</td>
            <td class="td-num amount amount-red cell-strong">{{ formatMoney(grandTotal) }}</td>
          </tr>
        </tfoot>
      </table>
    </div>
  </section>
</template>
