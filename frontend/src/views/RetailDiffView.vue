<script setup lang="ts">
import { computed, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import ReportTableCard from '../components/ui/ReportTableCard.vue'
import { useToast } from '../composables/useToast'
import { reportPages, type ReportPageKey, type ReportTabConfig, type TableCell, type TableColumn } from '../data/reports'
import { formatMoney } from '../services/formatters'

const toast = useToast()
const building = ref(false)

const props = withDefaults(
  defineProps<{
    taPageKey?: ReportPageKey
    bankPageKey?: ReportPageKey
    entityLabel?: string
    title?: string
  }>(),
  {
    taPageKey: 'retailTaBookings',
    bankPageKey: 'retailBankInflows',
    entityLabel: 'Tên khách hàng',
    title: 'Đối soát booking TA với tiền về ngân hàng',
  },
)

function cellText(cell: TableCell): string {
  if (typeof cell === 'string') return cell
  return cell.kind === 'actions' ? '' : cell.text
}

function parseAmount(text: string): number {
  return Number(text.replace(/[^0-9-]/g, '')) || 0
}

const diffColumns = computed<TableColumn[]>(() => [
  { key: 'booking', label: 'Mã booking' },
  { key: 'customer', label: props.entityLabel },
  { key: 'taAmount', label: 'Số tiền trên TA' },
  { key: 'bankAmount', label: 'Số tiền về ngân hàng' },
  { key: 'diff', label: 'Chênh lệch' },
  { key: 'status', label: 'Trạng thái' },
])

const taRows = computed(() => reportPages[props.taPageKey].tabs[0].rows)
const bankRows = computed(() => reportPages[props.bankPageKey].tabs[0].rows)

// Lấy vị trí cột theo "key" thay vì cố định theo index, vì các nguồn TA
// (Khách lẻ: 5 cột có SĐT / Đại lý API: 4 cột không có SĐT) có số cột khác nhau.
const taColumns = computed(() => reportPages[props.taPageKey].tabs[0].columns)
const taBookingIdx = computed(() => taColumns.value.findIndex((c) => c.key === 'booking'))
const taCustomerIdx = computed(() =>
  taColumns.value.findIndex((c) => c.key === 'customer' || c.key === 'agency'),
)
const taAmountIdx = computed(() => taColumns.value.findIndex((c) => c.key === 'amount'))

const bankColumns = computed(() => reportPages[props.bankPageKey].tabs[0].columns)
const bankDescriptionIdx = computed(() => bankColumns.value.findIndex((c) => c.key === 'description'))
const bankCreditIdx = computed(() => bankColumns.value.findIndex((c) => c.key === 'credit'))

const bankByBooking = computed(() => {
  const map = new Map<string, number>()
  for (const row of bankRows.value) {
    const description = cellText(row.cells[bankDescriptionIdx.value])
    const match = description.match(/BK(\d+)/)
    if (!match) continue
    map.set(match[1], parseAmount(cellText(row.cells[bankCreditIdx.value])))
  }
  return map
})

const page = computed(() => {
  let unmatched = 0
  let mismatched = 0

  const rows = taRows.value.map((row) => {
    const code = cellText(row.cells[taBookingIdx.value])
    const customer = cellText(row.cells[taCustomerIdx.value])
    const taAmount = parseAmount(cellText(row.cells[taAmountIdx.value]))
    const bankAmount = bankByBooking.value.get(code) ?? null
    const diff = bankAmount === null ? null : taAmount - bankAmount

    let statusCell: TableCell
    let diffCell: TableCell
    if (bankAmount === null) {
      unmatched++
      statusCell = { kind: 'badge', text: 'Chưa về ngân hàng', tone: 'amber' }
      diffCell = { kind: 'muted', text: '—' }
    } else if (diff === 0) {
      statusCell = { kind: 'badge', text: 'Đã khớp', tone: 'green' }
      diffCell = { kind: 'muted', text: '0 đ' }
    } else {
      mismatched++
      statusCell = { kind: 'badge', text: 'Lệch số tiền', tone: 'red' }
      diffCell = { kind: 'amount', text: formatMoney(diff), tone: 'red' }
    }

    return {
      id: `diff-${props.taPageKey}-${code}`,
      search: `${code} ${customer}`,
      date: row.date,
      cells: [
        row.cells[taBookingIdx.value],
        customer,
        row.cells[taAmountIdx.value],
        bankAmount === null ? { kind: 'muted', text: '—' } : formatMoney(bankAmount),
        diffCell,
        statusCell,
      ] as TableCell[],
    }
  })

  const tab: ReportTabConfig = {
    id: 'booking-bank-diff',
    label: 'Đối soát TA - Ngân hàng',
    columns: diffColumns.value,
    searchPlaceholder: `🔍  Tìm mã booking, ${props.entityLabel.toLowerCase()}...`,
    dateFilter: true,
    rows,
  }

  return {
    title: props.title,
    subtitle: `VLOOKUP theo Mã booking (parse từ Diễn giải sao kê ngân hàng): ${unmatched} booking chưa thấy tiền về, ${mismatched} booking lệch số tiền`,
    tab,
  }
})

async function runBuildReconciliation() {
  if (building.value) return
  building.value = true
  try {
    await new Promise((resolve) => setTimeout(resolve, 800))
    toast.success(`Đã build đối soát (demo, chưa nối API thật) — ${page.value.tab.rows.length} dòng.`, { duration: 6000 })
  } finally {
    building.value = false
  }
}
</script>

<template>
  <PageHeader :title="page.title" :subtitle="page.subtitle" />
  <ReportTableCard :tab="page.tab">
    <template #actions>
      <div class="tb-actions-right">
        <button class="add-btn" type="button" :disabled="building" @click="runBuildReconciliation">
          <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
            <path stroke-linecap="round" d="M12 5v14M5 12h14" />
          </svg>
          {{ building ? 'Đang build...' : 'Build đối soát' }}
        </button>
      </div>
    </template>
  </ReportTableCard>
</template>
