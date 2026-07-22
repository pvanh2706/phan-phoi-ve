<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import PaginationBar from '../components/ui/PaginationBar.vue'
import { useToast } from '../composables/useToast'
import { reportPages, type TableCell } from '../data/reports'

const props = defineProps<{
  direction: 'ar-ta' | 'ta-ar'
}>()

const toast = useToast()
const building = ref(false)

function cellText(cell: TableCell): string {
  if (typeof cell === 'string') return cell
  return cell.kind === 'actions' ? '' : cell.text
}

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
      booking: cellText(row.cells[0]),
      agency: cellText(row.cells[1]),
      time: cellText(row.cells[2]),
      amount: cellText(row.cells[3]),
    }))

  return {
    title: isArBase ? 'Đối soát AR - TA' : 'Đối soát TA - AR',
    subtitle: isArBase
      ? `VLOOKUP theo Mã booking: ${rows.length} booking có trên hệ thống AR nhưng không tìm thấy trên TA`
      : `VLOOKUP theo Mã booking: ${rows.length} booking có trên hệ thống TA nhưng không tìm thấy trên AR`,
    missingLabel,
    rows,
  }
})

const keyword = ref('')
const dateFrom = ref('')
const dateTo = ref('')
const pageIndex = ref(1)
const pageSize = 8

const filteredRows = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  return page.value.rows.filter((row) => {
    if (kw && !row.search.toLowerCase().includes(kw)) return false
    if (dateFrom.value && row.date && row.date < dateFrom.value) return false
    if (dateTo.value && row.date && row.date > dateTo.value) return false
    return true
  })
})

const visibleRows = computed(() => {
  const start = (pageIndex.value - 1) * pageSize
  return filteredRows.value.slice(start, start + pageSize)
})

watch(() => props.direction, () => {
  keyword.value = ''
  dateFrom.value = ''
  dateTo.value = ''
  pageIndex.value = 1
})

watch(filteredRows, () => {
  pageIndex.value = 1
})

// Trạng thái xử lý (mock, phía frontend) — key theo id dòng, mất khi tải lại trang.
const resolvedMap = reactive<Record<string, { method: string; reason: string }>>({})

function isResolved(id: string) {
  return Boolean(resolvedMap[id])
}

const resolveModal = reactive({
  open: false,
  id: '',
  title: '',
  method: '',
  reason: '',
})

function onCbClick(row: { id: string; booking: string; agency: string }) {
  if (isResolved(row.id)) return
  resolveModal.open = true
  resolveModal.id = row.id
  resolveModal.title = `${row.booking} - ${row.agency}`
  resolveModal.method = ''
  resolveModal.reason = ''
}

function saveResolve() {
  if (!resolveModal.method.trim() || !resolveModal.reason.trim()) {
    toast.error('Vui lòng nhập đủ Cách thức xử lý và Lý do xử lý.')
    return
  }
  resolvedMap[resolveModal.id] = {
    method: resolveModal.method.trim(),
    reason: resolveModal.reason.trim(),
  }
  resolveModal.open = false
  toast.success('Đã đánh dấu xử lý dòng đối soát (demo, chưa lưu backend).')
}

async function runBuildReconciliation() {
  if (building.value) return
  building.value = true
  try {
    await new Promise((resolve) => setTimeout(resolve, 800))
    toast.success(`Đã build đối soát (demo, chưa nối API thật) — ${page.value.rows.length} dòng.`, { duration: 6000 })
  } finally {
    building.value = false
  }
}
</script>

<template>
  <PageHeader :title="page.title" :subtitle="page.subtitle" />

  <section class="card">
    <div class="toolbar">
      <input v-model="keyword" class="tb-input" placeholder="🔍  Tìm mã booking, tên đại lý..." />
      <span class="tb-label">Từ ngày</span>
      <input v-model="dateFrom" class="tb-date" type="date" />
      <span class="tb-label">Đến ngày</span>
      <input v-model="dateTo" class="tb-date" type="date" />
      <div class="tb-actions-right">
        <button class="add-btn" type="button" :disabled="building" @click="runBuildReconciliation">
          <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
            <path stroke-linecap="round" d="M12 5v14M5 12h14" />
          </svg>
          {{ building ? 'Đang build...' : 'Build đối soát' }}
        </button>
      </div>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Mã booking</th>
            <th>Tên đại lý</th>
            <th>Ngày giờ giao dịch</th>
            <th>Số tiền</th>
            <th>Trạng thái</th>
            <th class="td-center">Đã xử lý</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in visibleRows" :key="row.id" :class="{ 'row-handled': isResolved(row.id) }">
            <td class="cell-strong">{{ row.booking }}</td>
            <td>{{ row.agency }}</td>
            <td class="cell-muted">{{ row.time }}</td>
            <td class="amount amount-red">{{ row.amount }}</td>
            <td><span class="badge badge-amber">{{ page.missingLabel }}</span></td>
            <td class="cb-wrap">
              <input
                type="checkbox"
                class="cb-done"
                :checked="isResolved(row.id)"
                :disabled="isResolved(row.id)"
                title="Đánh dấu đã xử lý"
                @click.prevent="onCbClick(row)"
              />
            </td>
          </tr>
          <tr v-if="visibleRows.length === 0">
            <td colspan="6" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>

    <PaginationBar v-model:page="pageIndex" :total="filteredRows.length" :page-size="pageSize" />
  </section>

  <div v-if="resolveModal.open" class="modal-overlay">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">Xử lý chênh lệch đối soát</span>
        <button class="modal-close" type="button" @click="resolveModal.open = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="notice notice-indigo" style="margin-bottom: 14px">{{ resolveModal.title }}</div>
        <div class="form-group">
          <label class="form-label">Cách thức xử lý <span class="required-mark">*</span></label>
          <input v-model="resolveModal.method" class="form-input" placeholder="VD: Bổ sung booking thủ công lên hệ thống thiếu" />
        </div>
        <div class="form-group">
          <label class="form-label">Lý do xử lý <span class="required-mark">*</span></label>
          <textarea v-model="resolveModal.reason" class="form-textarea" placeholder="VD: Booking bị lỗi đồng bộ do timeout API"></textarea>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="resolveModal.open = false">Hủy</button>
        <button class="btn-primary" type="button" @click="saveResolve">Lưu xử lý</button>
      </div>
    </div>
  </div>
</template>
