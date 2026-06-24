<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import KanbanBoard from '../components/ui/KanbanBoard.vue'
import PageHeader from '../components/ui/PageHeader.vue'
import ReportTableCard from '../components/ui/ReportTableCard.vue'
import { cloneColumns, topUpReportTabs, topUpWorkflow, type KanbanTask } from '../data/workflows'

const activeTab = ref('quytrinh')
const columns = ref(cloneColumns(topUpWorkflow))
const selectedTask = ref<KanbanTask | null>(null)
const showCreateTask = ref(false)
const createType = ref<'nap' | 'cn'>('nap')
const form = reactive({
  name: '',
  park: '',
  account: '',
  bank: '',
  amount: '',
  date: new Date().toISOString().slice(0, 10),
  note: '',
})

const currentReport = computed(() => topUpReportTabs.find((tab) => tab.id === activeTab.value) ?? topUpReportTabs[0])
const parkOptions = computed(() =>
  createType.value === 'nap'
    ? ['Vin Nha Trang', 'Vin Phú Quốc', 'Vin Nam Hội An', 'Bản Mòng', 'Sunworld', 'Đồi Rồng', 'Samten Hills Dalat', 'Delight']
    : ['Sealinks', 'TLTY', 'Sơn Tiên', 'Lumiere', 'Mikazuki', 'Mekong', 'Tà Cú', 'Hồ Tràm', 'Sightseeing HN'],
)

function moveTask(taskId: string, toColumnId: string) {
  const fromColumn = columns.value.find((column) => column.tasks.some((task) => task.id === taskId))
  const toColumn = columns.value.find((column) => column.id === toColumnId)
  if (!fromColumn || !toColumn || fromColumn.id === toColumn.id) {
    return
  }

  const index = fromColumn.tasks.findIndex((task) => task.id === taskId)
  const [task] = fromColumn.tasks.splice(index, 1)
  toColumn.tasks.push(task)
}

function openCreateTask() {
  form.name = ''
  form.park = ''
  form.account = ''
  form.bank = ''
  form.amount = ''
  form.date = new Date().toISOString().slice(0, 10)
  form.note = ''
  createType.value = 'nap'
  showCreateTask.value = true
}

function submitCreateTask() {
  if (!form.name || !form.park || !form.amount) {
    return
  }

  columns.value[0].tasks.unshift({
    id: `new-${Date.now()}`,
    title: form.name,
    park: form.park,
    owner: 'Anh Thảo',
    date: form.date.split('-').reverse().join('/'),
    amount: `${form.amount} đ`,
    status: 'Mới tạo',
    tone: 'blue',
    details: {
      'Số tài khoản': form.account || '—',
      'Ngân hàng': form.bank || '—',
      'Ghi chú': form.note || '—',
    },
  })
  showCreateTask.value = false
}
</script>

<template>
  <PageHeader title="Danh sách nạp tiền KVC theo ngày" subtitle="Quy trình tạo nhiệm vụ, duyệt và theo dõi lịch sử nạp tiền/công nợ" />

  <div class="tabs-bar">
    <button class="tab-btn" :class="{ active: activeTab === 'quytrinh' }" type="button" @click="activeTab = 'quytrinh'">
      Quy trình nạp tiền KVC
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'nap' }" type="button" @click="activeTab = 'nap'">
      Nạp tiền cho KVC nạp tiền
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'cn' }" type="button" @click="activeTab = 'cn'">
      Thanh toán KVC công nợ
    </button>
  </div>

  <section v-if="activeTab === 'quytrinh'" class="card">
    <div class="toolbar">
      <button class="btn-create-task" type="button" style="padding: 8px 16px" @click="openCreateTask">+ Tạo nhiệm vụ</button>
    </div>
    <KanbanBoard :columns="columns" @task-open="selectedTask = $event" @task-move="moveTask" />
  </section>
  <ReportTableCard v-else :tab="currentReport" />

  <aside v-if="selectedTask" class="task-panel">
    <div class="task-panel-head">
      <div class="task-panel-title">{{ selectedTask.title }}</div>
      <button class="modal-close" type="button" @click="selectedTask = null">✕</button>
    </div>
    <div class="task-panel-body">
      <div class="detail-row"><span class="detail-label">KVC</span><span>{{ selectedTask.park }}</span></div>
      <div class="detail-row"><span class="detail-label">Người phụ trách</span><span>{{ selectedTask.owner }}</span></div>
      <div class="detail-row"><span class="detail-label">Ngày thực hiện</span><span>{{ selectedTask.date }}</span></div>
      <div class="detail-row"><span class="detail-label">Số tiền</span><span class="task-money">{{ selectedTask.amount }}</span></div>
      <div class="detail-row"><span class="detail-label">Trạng thái</span><span class="badge" :class="`badge-${selectedTask.tone}`">{{ selectedTask.status }}</span></div>
      <div v-for="(value, key) in selectedTask.details" :key="key" class="detail-row">
        <span class="detail-label">{{ key }}</span>
        <span>{{ value }}</span>
      </div>
    </div>
  </aside>

  <div v-if="showCreateTask" class="ct-overlay" @click.self="showCreateTask = false">
    <div class="ct-modal">
      <div class="modal-header">
        <span class="modal-title">Tạo nhiệm vụ mới</span>
        <button class="modal-close" type="button" @click="showCreateTask = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label class="form-label">Tên nhiệm vụ</label>
          <input v-model="form.name" class="form-input" placeholder="VD: Nạp tiền tháng 6 - Vin Nha Trang" />
        </div>
        <div class="form-group">
          <label class="form-label">Loại nhiệm vụ</label>
          <div class="tabs-bar" style="margin-bottom: 0">
            <button class="tab-btn" :class="{ active: createType === 'nap' }" type="button" @click="createType = 'nap'">Nạp tiền</button>
            <button class="tab-btn" :class="{ active: createType === 'cn' }" type="button" @click="createType = 'cn'">Thanh toán Công nợ</button>
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Tên KVC</label>
          <select v-model="form.park" class="form-select">
            <option value="">-- Chọn KVC --</option>
            <option v-for="park in parkOptions" :key="park" :value="park">{{ park }}</option>
          </select>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Số tài khoản</label>
            <input v-model="form.account" class="form-input" placeholder="19139932758899" />
          </div>
          <div class="form-group">
            <label class="form-label">Ngân hàng</label>
            <input v-model="form.bank" class="form-input" placeholder="Techcombank" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Số tiền</label>
            <input v-model="form.amount" class="form-input" placeholder="50,000,000" />
          </div>
          <div class="form-group">
            <label class="form-label">Ngày thực hiện</label>
            <input v-model="form.date" class="form-input" type="date" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Ghi chú</label>
          <input v-model="form.note" class="form-input" placeholder="Ghi chú thêm" />
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showCreateTask = false">Hủy</button>
        <button class="btn-primary" type="button" @click="submitCreateTask">Tạo nhiệm vụ</button>
      </div>
    </div>
  </div>
</template>
