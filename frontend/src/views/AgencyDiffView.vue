<script setup lang="ts">
import { computed } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import ReportTableCard from '../components/ui/ReportTableCard.vue'
import { reportPages, type ReportTabConfig, type TableCell, type TableColumn } from '../data/reports'

const props = defineProps<{
  direction: 'ar-ta' | 'ta-ar'
}>()

function cellText(cell: TableCell): string {
  if (typeof cell === 'string') return cell
  return cell.kind === 'actions' ? '' : cell.text
}

const diffColumns: TableColumn[] = [
  { key: 'booking', label: 'Mã booking' },
  { key: 'agency', label: 'Tên đại lý' },
  { key: 'time', label: 'Ngày giờ giao dịch' },
  { key: 'amount', label: 'Số tiền' },
  { key: 'status', label: 'Trạng thái' },
]

const arRows = reportPages.agencyArTransactions.tabs[0].rows
const taRows = reportPages.agencyTaTransactions.tabs[0].rows

const page = computed(() => {
  const isArBase = props.direction === 'ar-ta'
  const baseRows = isArBase ? arRows : taRows
  const otherCodes = new Set((isArBase ? taRows : arRows).map((row) => cellText(row.cells[0])))
  const missingLabel = isArBase ? 'Không có trên TA' : 'Không có trên AR'

  const rows = baseRows
    .filter((row) => !otherCodes.has(cellText(row.cells[0])))
    .map((row) => ({
      id: row.id,
      search: row.search,
      date: row.date,
      cells: [
        row.cells[0],
        row.cells[1],
        row.cells[2],
        row.cells[3],
        { kind: 'badge', text: missingLabel, tone: 'amber' } as TableCell,
      ],
    }))

  const tab: ReportTabConfig = {
    id: props.direction,
    label: isArBase ? 'AR → TA' : 'TA → AR',
    columns: diffColumns,
    searchPlaceholder: '🔍  Tìm mã booking, tên đại lý...',
    dateFilter: true,
    rows,
  }

  return {
    title: isArBase ? 'Đối soát AR - TA' : 'Đối soát TA - AR',
    subtitle: isArBase
      ? `VLOOKUP theo Mã booking: ${rows.length} booking có trên hệ thống AR nhưng không tìm thấy trên TA`
      : `VLOOKUP theo Mã booking: ${rows.length} booking có trên hệ thống TA nhưng không tìm thấy trên AR`,
    tab,
  }
})
</script>

<template>
  <PageHeader :title="page.title" :subtitle="page.subtitle" />
  <ReportTableCard :tab="page.tab" />
</template>
