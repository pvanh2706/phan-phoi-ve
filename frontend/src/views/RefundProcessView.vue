<script setup lang="ts">
import { computed, ref } from 'vue'
import KanbanBoard from '../components/ui/KanbanBoard.vue'
import PageHeader from '../components/ui/PageHeader.vue'
import ReportTableCard from '../components/ui/ReportTableCard.vue'
import { reportPages } from '../data/reports'
import { cloneColumns, refundWorkflow, type KanbanTask } from '../data/workflows'

const activeTab = ref('quytrinh')
const columns = ref(cloneColumns(refundWorkflow))
const selectedTask = ref<KanbanTask | null>(null)
const showAdd = ref(false)

const refundReports = reportPages.parkRefunds.tabs
const currentReport = computed(() => refundReports.find((tab) => tab.id === activeTab.value) ?? refundReports[0])

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
</script>

<template>
  <PageHeader title="KVC hoàn tiền" subtitle="Quy trình hoàn tiền và danh sách yêu cầu theo từng nhóm KVC" />

  <div class="tabs-bar">
    <button class="tab-btn" :class="{ active: activeTab === 'quytrinh' }" type="button" @click="activeTab = 'quytrinh'">
      Quy trình hoàn tiền
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'nap' }" type="button" @click="activeTab = 'nap'">
      KVC nạp tiền
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'cn' }" type="button" @click="activeTab = 'cn'">
      KVC công nợ
    </button>
  </div>

  <section v-if="activeTab === 'quytrinh'" class="card">
    <KanbanBoard :columns="columns" @task-open="selectedTask = $event" @task-move="moveTask" />
  </section>
  <ReportTableCard v-else :tab="currentReport" @add="showAdd = true" />

  <aside v-if="selectedTask" class="task-panel">
    <div class="task-panel-head">
      <div class="task-panel-title">{{ selectedTask.title }}</div>
      <button class="modal-close" type="button" @click="selectedTask = null">✕</button>
    </div>
    <div class="task-panel-body">
      <div class="detail-row"><span class="detail-label">KVC</span><span>{{ selectedTask.park }}</span></div>
      <div class="detail-row"><span class="detail-label">Người phụ trách</span><span>{{ selectedTask.owner }}</span></div>
      <div class="detail-row"><span class="detail-label">Ngày yêu cầu</span><span>{{ selectedTask.date }}</span></div>
      <div class="detail-row"><span class="detail-label">Số tiền</span><span class="task-money">{{ selectedTask.amount }}</span></div>
      <div class="detail-row"><span class="detail-label">Trạng thái</span><span class="badge" :class="`badge-${selectedTask.tone}`">{{ selectedTask.status }}</span></div>
      <div v-for="(value, key) in selectedTask.details" :key="key" class="detail-row">
        <span class="detail-label">{{ key }}</span>
        <span>{{ value }}</span>
      </div>
    </div>
  </aside>

  <div v-if="showAdd" class="modal-overlay">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">{{ currentReport.addLabel?.replace('+ ', '') }}</span>
        <button class="modal-close" type="button" @click="showAdd = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mã đặt vé</label>
            <input class="form-input" placeholder="BK-240625-001" />
          </div>
          <div class="form-group">
            <label class="form-label">Tên KVC</label>
            <input class="form-input" placeholder="Bản Mòng" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Khách hàng</label>
            <input class="form-input" placeholder="Nguyễn Văn A" />
          </div>
          <div class="form-group">
            <label class="form-label">Số tiền</label>
            <input class="form-input" placeholder="1,250,000" />
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showAdd = false">Hủy</button>
        <button class="btn-primary" type="button" @click="showAdd = false">Lưu</button>
      </div>
    </div>
  </div>
</template>
