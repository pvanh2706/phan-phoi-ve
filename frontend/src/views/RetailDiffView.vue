<script setup lang="ts">
import { computed } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import ReportTableCard from '../components/ui/ReportTableCard.vue'
import { reportPages, type ReportPageKey, type ReportTabConfig, type TableCell, type TableColumn } from '../data/reports'
import { formatMoney } from '../services/formatters'

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

const bankByBooking = computed(() => {
  const map = new Map<string, number>()
  for (const row of bankRows.value) {
    const description = cellText(row.cells[8])
    const match = description.match(/BK(\d+)/)
    if (!match) continue
    map.set(match[1], parseAmount(cellText(row.cells[5])))
  }
  return map
})

const page = computed(() => {
  let unmatched = 0
  let mismatched = 0

  const rows = taRows.value.map((row) => {
    const code = cellText(row.cells[0])
    const customer = cellText(row.cells[1])
    const taAmount = parseAmount(cellText(row.cells[4]))
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
        row.cells[0],
        customer,
        row.cells[4],
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
</script>

<template>
  <PageHeader :title="page.title" :subtitle="page.subtitle" />
  <ReportTableCard :tab="page.tab" />
</template>
