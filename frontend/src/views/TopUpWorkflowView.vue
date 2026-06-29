<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import KanbanBoard from '../components/ui/KanbanBoard.vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { useConfirm } from '../composables/useConfirm'
import { ApiClientError } from '../services/apiClient'
import {
  listBankTransactionDetails,
  syncBankTransactions,
  type BankTransactionDetailDto,
} from '../services/summariesApi'
import {
  getWorkflowBoard,
  createWorkflowTask,
  updateWorkflowTask,
  moveWorkflowTask,
  deleteWorkflowTask,
  updateWorkflowColumnSettings,
  type WorkflowBoardDto,
  type WorkflowUserOption,
  type WorkflowTaskDto,
  type SaveWorkflowTaskRequest,
} from '../services/workflowApi'
import { listParks, type ParkDto } from '../services/parksApi'
import { formatDate, formatMoney, formatNumber } from '../services/formatters'
import { topUpColumnDecor, type KanbanColumn, type KanbanTask, type KanbanTone } from '../data/workflows'

type Tab = 'quytrinh' | 'nap' | 'cn'

const toast = useToast()
const { confirm } = useConfirm()

const activeTab = ref<Tab>('quytrinh')

// ── Kanban (Quy trình nạp tiền KVC) ──
const boardDto = ref<WorkflowBoardDto | null>(null)
const columns = ref<KanbanColumn[]>([])
const boardUsers = ref<WorkflowUserOption[]>([])
const selectedTask = ref<KanbanTask | null>(null)

function mapBoard(board: WorkflowBoardDto): KanbanColumn[] {
  return board.columns.map((col) => ({
    id: col.columnKey,
    dbId: col.id,
    title: col.title,
    icon: '',
    tone: col.headTone as KanbanTone,
    avatars: topUpColumnDecor[col.columnKey]?.avatars,
    stats: topUpColumnDecor[col.columnKey]?.stats,
    visibleFields: col.visibleFields,
    permittedUserIds: col.permittedUserIds,
    tasks: col.tasks.map((task) => ({
      id: `task-${task.id}`,
      dbId: task.id,
      title: task.title,
      park: task.paymentType === 'Prepaid' ? 'Nạp tiền' : 'Công nợ',
      owner: task.bankName ?? '—',
      date: task.executeDate ? formatDate(task.executeDate) : '—',
      amount: formatMoney(task.amount),
      status: col.cardStatusLabel,
      tone: col.cardTone as KanbanTask['tone'],
      details: {
        'Số tài khoản': task.bankAccount ?? '—',
        'Ngân hàng': task.bankName ?? '—',
        'Ghi chú': task.note ?? '',
      },
    })),
  }))
}

async function loadBoard() {
  try {
    const board = await getWorkflowBoard()
    boardDto.value = board
    boardUsers.value = board.users
    columns.value = mapBoard(board)
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không tải được quy trình.')
  }
}

async function moveTask(taskId: string, toColumnId: string) {
  const fromColumn = columns.value.find((c) => c.tasks.some((t) => t.id === taskId))
  const toColumn = columns.value.find((c) => c.id === toColumnId)
  if (!fromColumn || !toColumn || fromColumn.id === toColumn.id) return
  const task = fromColumn.tasks.find((t) => t.id === taskId)
  if (!task?.dbId || !toColumn.dbId) return
  try {
    await moveWorkflowTask(task.dbId, toColumn.dbId)
    await loadBoard()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không chuyển được nhiệm vụ.')
    await loadBoard()
  }
}

async function onSettingsChange(payload: { columnId: number; visibleFields: string[]; permittedUserIds: number[] }) {
  try {
    await updateWorkflowColumnSettings(payload.columnId, {
      visibleFields: payload.visibleFields,
      permittedUserIds: payload.permittedUserIds,
    })
    toast.success('Đã lưu cấu hình cột.')
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không lưu được cấu hình cột.')
  }
}

// ── Create / edit task modal ──
const parks = ref<ParkDto[]>([])
const showTaskModal = ref(false)
const editingTaskId = ref<number | null>(null)
const saving = ref(false)

const emptyForm = (): SaveWorkflowTaskRequest => ({
  title: '',
  paymentType: 'Prepaid',
  parkId: null,
  bankAccount: '',
  bankName: '',
  amount: 0,
  executeDate: '',
  note: '',
})
const taskForm = ref<SaveWorkflowTaskRequest>(emptyForm())

async function loadParks() {
  try {
    const result = await listParks({ status: 'Active' })
    parks.value = result.items
  } catch {
    // dropdown chỉ là tiện ích — bỏ qua lỗi tải danh sách KVC
  }
}

function openCreate() {
  editingTaskId.value = null
  taskForm.value = emptyForm()
  showTaskModal.value = true
}

function openEdit(task: KanbanTask) {
  if (!task.dbId || !boardDto.value) return
  let dto: WorkflowTaskDto | undefined
  for (const column of boardDto.value.columns) {
    dto = column.tasks.find((t) => t.id === task.dbId)
    if (dto) break
  }
  if (!dto) return
  editingTaskId.value = dto.id
  taskForm.value = {
    title: dto.title,
    paymentType: dto.paymentType,
    parkId: dto.parkId ?? null,
    bankAccount: dto.bankAccount ?? '',
    bankName: dto.bankName ?? '',
    amount: dto.amount,
    executeDate: dto.executeDate ?? '',
    note: dto.note ?? '',
  }
  selectedTask.value = null
  showTaskModal.value = true
}

function onParkChange() {
  const park = parks.value.find((p) => p.id === taskForm.value.parkId)
  if (!park) return
  taskForm.value.paymentType = park.paymentType
  if (park.bankAccount) taskForm.value.bankAccount = park.bankAccount
  if (park.bankName) taskForm.value.bankName = park.bankName
}

async function saveTask() {
  if (!taskForm.value.title.trim()) {
    toast.warning('Vui lòng nhập tên nhiệm vụ.')
    return
  }
  saving.value = true
  try {
    const payload: SaveWorkflowTaskRequest = {
      ...taskForm.value,
      title: taskForm.value.title.trim(),
      executeDate: taskForm.value.executeDate || null,
    }
    if (editingTaskId.value) {
      await updateWorkflowTask(editingTaskId.value, payload)
      toast.success('Đã cập nhật nhiệm vụ.')
    } else {
      await createWorkflowTask(payload)
      toast.success('Đã tạo nhiệm vụ.')
    }
    showTaskModal.value = false
    await loadBoard()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không lưu được nhiệm vụ.')
  } finally {
    saving.value = false
  }
}

async function removeTask(task: KanbanTask) {
  if (!task.dbId) return
  const ok = await confirm({
    title: 'Xóa nhiệm vụ',
    message: `Bạn có chắc muốn xóa nhiệm vụ "${task.title}"?`,
    confirmText: 'Xóa',
  })
  if (!ok) return
  try {
    await deleteWorkflowTask(task.dbId)
    selectedTask.value = null
    toast.success('Đã xóa nhiệm vụ.')
    await loadBoard()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không xóa được nhiệm vụ.')
  }
}

// ── Transaction tables (Nạp tiền / Công nợ) ──
const loading = ref(false)
const rows = ref<BankTransactionDetailDto[]>([])
// Dòng đang xem chi tiết các giao dịch đã gộp (popup).
const txDetailRow = ref<BankTransactionDetailDto | null>(null)
const totalItems = ref(0)
const page = ref(1)

const filters = ref({ keyword: '', dateFrom: '', dateTo: '' })

const syncing = ref(false)

const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / 100)))

const displayRows = computed(() => {
  const out: { key: string; isSep: boolean; row: BankTransactionDetailDto }[] = []
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

function formatTxTime(iso: string) {
  const [datePart, timeRaw = ''] = iso.split('T')
  const [y, m, d] = datePart.split('-')
  const time = timeRaw.replace('Z', '').split('.')[0]
  const dmy = `${d}/${m}/${y}`
  return !time || time === '00:00:00' ? dmy : `${dmy} ${time}`
}

async function load() {
  if (activeTab.value === 'quytrinh') return
  loading.value = true
  try {
    const result = await listBankTransactionDetails({
      page: page.value,
      paymentType: activeTab.value === 'nap' ? 'Prepaid' : 'Debt',
      dateFrom: filters.value.dateFrom,
      dateTo: filters.value.dateTo,
      keyword: filters.value.keyword,
    })
    rows.value = result.items
    totalItems.value = result.totalItems
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không tải được dữ liệu giao dịch.')
  } finally {
    loading.value = false
  }
}

function selectTab(tab: Tab) {
  if (activeTab.value === tab) return
  activeTab.value = tab
  if (tab !== 'quytrinh') {
    page.value = 1
    rows.value = []
    void load()
  }
}

function applyFilter() {
  page.value = 1
  void load()
}

async function syncFromEmail() {
  if (syncing.value) return
  syncing.value = true
  try {
    const result = await syncBankTransactions()
    let msg = `Đã nhập ${result.imported} dòng KVC (gộp từ ${result.transactionsParsed} giao dịch) từ ${result.mailsProcessed} email`
    if (result.skippedUnmatched > 0) {
      const accounts = result.unmatchedAccounts.slice(0, 5).join(', ')
      msg += `. Bỏ qua ${result.skippedUnmatched} giao dịch không khớp KVC`
      if (accounts) msg += ` (TK: ${accounts}${result.unmatchedAccounts.length > 5 ? '…' : ''})`
    }
    toast.success(msg + '.', { duration: 6000 })
    page.value = 1
    await load()
  } catch (err) {
    toast.error(err instanceof ApiClientError ? err.message : 'Không lấy được dữ liệu từ email.')
  } finally {
    syncing.value = false
  }
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

watch(activeTab, (tab) => {
  if (tab !== 'quytrinh') {
    // reset filters when moving between the two transaction tabs
    filters.value = { keyword: '', dateFrom: '', dateTo: '' }
  }
})

onMounted(() => {
  void loadBoard()
  void loadParks()
})
</script>

<template>
  <PageHeader
    title="Danh sách nạp tiền KVC theo ngày"
    subtitle="Lịch sử các giao dịch nạp tiền vào tài khoản khu vui chơi"
  />

  <div class="tabs-bar">
    <button class="tab-btn" :class="{ active: activeTab === 'quytrinh' }" type="button" @click="selectTab('quytrinh')">
      Quy trình nạp tiền KVC
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'nap' }" type="button" @click="selectTab('nap')">
      Nạp tiền cho KVC nạp tiền
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'cn' }" type="button" @click="selectTab('cn')">
      Thanh toán KVC công nợ
    </button>
  </div>

  <template v-if="activeTab === 'quytrinh'">
    <div class="kb-toolbar">
      <button class="btn-primary" type="button" @click="openCreate">＋ Tạo nhiệm vụ</button>
    </div>
    <section class="card">
      <KanbanBoard
        :columns="columns"
        :users="boardUsers"
        @task-open="selectedTask = $event"
        @task-move="moveTask"
        @settings-change="onSettingsChange"
      />
    </section>
  </template>

  <section v-else class="card">
    <div class="toolbar">
      <input
        v-model="filters.keyword"
        class="tb-input"
        placeholder="🔍  Tìm nội dung..."
        @input="onSearchInput"
        @keyup.enter="applyFilter"
      />
      <span class="tb-label">Từ ngày</span>
      <input v-model="filters.dateFrom" class="tb-date" type="date" @change="applyFilter" />
      <span class="tb-label">Đến ngày</span>
      <input v-model="filters.dateTo" class="tb-date" type="date" @change="applyFilter" />
      <button class="btn-primary tb-sync" type="button" :disabled="syncing" @click="syncFromEmail">
        {{ syncing ? 'Đang lấy...' : '⤓ Get API' }}
      </button>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày giờ giao dịch</th>
            <th>Tên khu vui chơi</th>
            <th class="td-num">Ghi nợ</th>
            <th class="td-num">Ghi có</th>
            <th>Nội dung</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="5">Đang tải...</td></tr>
          <tr v-for="item in displayRows" :key="item.key" :class="{ 'date-separator': item.isSep }">
            <td v-if="item.isSep" colspan="5">📅 {{ formatDate(item.row.businessDate) }}</td>
            <template v-else>
              <td class="cell-muted">{{ formatTxTime(item.row.transactionAtUtc) }}</td>
              <td>{{ item.row.parkName ?? '—' }}</td>
              <td class="td-num amount amount-green">{{ formatNumber(item.row.debitAmount) }}</td>
              <td class="td-num cell-muted">{{ formatNumber(item.row.creditAmount) }}</td>
              <td class="content-cell">
                <button
                  v-if="item.row.lineItems && item.row.lineItems.length"
                  type="button"
                  class="tx-detail-link"
                  @click="txDetailRow = item.row"
                >{{ item.row.content }}</button>
                <template v-else>{{ item.row.content }}</template>
              </td>
            </template>
          </tr>
          <tr v-if="!loading && rows.length === 0">
            <td colspan="5" class="cell-muted" style="text-align: center">Chưa có giao dịch phù hợp.</td>
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

  <aside v-if="selectedTask" class="task-panel">
    <div class="task-panel-head">
      <div class="task-panel-title">{{ selectedTask.title }}</div>
      <button class="modal-close" type="button" @click="selectedTask = null">✕</button>
    </div>
    <div class="task-panel-body">
      <div class="detail-row"><span class="detail-label">Loại</span><span>{{ selectedTask.park }}</span></div>
      <div class="detail-row"><span class="detail-label">Ngày</span><span>{{ selectedTask.date }}</span></div>
      <div class="detail-row"><span class="detail-label">Số tiền</span><span class="task-money">{{ selectedTask.amount }}</span></div>
      <div class="detail-row"><span class="detail-label">Trạng thái</span><span class="badge" :class="`badge-${selectedTask.tone}`">{{ selectedTask.status }}</span></div>
      <div v-for="(value, key) in selectedTask.details" :key="key" class="detail-row">
        <span class="detail-label">{{ key }}</span>
        <span>{{ value }}</span>
      </div>
      <div v-if="selectedTask.dbId" class="task-panel-actions">
        <button class="btn-secondary" type="button" @click="openEdit(selectedTask)">✏️ Sửa</button>
        <button class="btn-danger" type="button" @click="removeTask(selectedTask)">🗑️ Xóa</button>
      </div>
    </div>
  </aside>

  <div v-if="txDetailRow" class="modal-overlay">
    <div class="modal tx-detail-modal">
      <div class="modal-header">
        <div class="modal-title">Chi tiết giao dịch — {{ txDetailRow.parkName ?? '—' }}</div>
        <button class="modal-close" type="button" @click="txDetailRow = null">✕</button>
      </div>
      <div class="modal-body">
        <div class="notice notice-indigo" style="margin-bottom: 14px">
          {{ formatDate(txDetailRow.businessDate) }} · Gồm {{ txDetailRow.lineItems?.length ?? 0 }} giao dịch
        </div>
        <div class="table-wrap report-table-wrap">
          <table>
            <thead>
              <tr>
                <th>Ngày giờ giao dịch</th>
                <th class="td-num">Ghi nợ</th>
                <th class="td-num">Ghi có</th>
                <th>Nội dung</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(line, idx) in txDetailRow.lineItems ?? []" :key="idx">
                <td class="cell-muted">{{ formatTxTime(line.transactionAtUtc) }}</td>
                <td class="td-num amount amount-green">{{ formatNumber(line.debitAmount) }}</td>
                <td class="td-num cell-muted">{{ formatNumber(line.creditAmount) }}</td>
                <td class="content-cell">{{ line.content }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="txDetailRow = null">Đóng</button>
      </div>
    </div>
  </div>

  <div v-if="showTaskModal" class="modal-overlay">
    <div class="modal">
      <div class="modal-header">
        <div class="modal-title">{{ editingTaskId ? 'Sửa nhiệm vụ' : 'Tạo nhiệm vụ mới' }}</div>
        <button class="modal-close" type="button" @click="showTaskModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label class="form-label">Tên nhiệm vụ *</label>
          <input v-model="taskForm.title" class="form-input" placeholder="VD: Nạp tiền tháng 6 - Vin Nha Trang" />
        </div>
        <div class="form-row">
          <div class="form-group" style="margin-bottom: 0">
            <label class="form-label">Loại</label>
            <select v-model="taskForm.paymentType" class="form-select">
              <option value="Prepaid">Nạp tiền</option>
              <option value="Debt">Thanh toán Công nợ</option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom: 0">
            <label class="form-label">Khu vui chơi</label>
            <select v-model="taskForm.parkId" class="form-select" @change="onParkChange">
              <option :value="null">— Chọn KVC —</option>
              <option v-for="park in parks" :key="park.id" :value="park.id">{{ park.name }}</option>
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group" style="margin-bottom: 0">
            <label class="form-label">Số tài khoản</label>
            <input v-model="taskForm.bankAccount" class="form-input" />
          </div>
          <div class="form-group" style="margin-bottom: 0">
            <label class="form-label">Ngân hàng</label>
            <input v-model="taskForm.bankName" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group" style="margin-bottom: 0">
            <label class="form-label">Số tiền</label>
            <input v-model.number="taskForm.amount" type="number" min="0" class="form-input" />
          </div>
          <div class="form-group" style="margin-bottom: 0">
            <label class="form-label">Ngày thực hiện</label>
            <input v-model="taskForm.executeDate" type="date" class="form-input" />
          </div>
        </div>
        <div class="form-group" style="margin-bottom: 0">
          <label class="form-label">Ghi chú</label>
          <textarea v-model="taskForm.note" class="form-textarea" rows="2"></textarea>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showTaskModal = false">Hủy</button>
        <button class="btn-primary" type="button" :disabled="saving" @click="saveTask">
          {{ saving ? 'Đang lưu...' : 'Lưu' }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.kb-toolbar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}

.task-panel-actions {
  display: flex;
  gap: 10px;
  margin-top: 18px;
}

.btn-danger {
  padding: 7px 14px;
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: 8px;
  background: rgba(239, 68, 68, 0.12);
  color: #f87171;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

.btn-danger:hover {
  background: rgba(239, 68, 68, 0.2);
}
</style>
