<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import type { KanbanColumn, KanbanTask } from '../../data/workflows'

interface BoardUser {
  id: number
  fullName: string
  initials: string
}

const props = defineProps<{
  columns: KanbanColumn[]
  users?: BoardUser[]
}>()

const emit = defineEmits<{
  taskOpen: [task: KanbanTask]
  taskMove: [taskId: string, toColumnId: string]
  settingsChange: [payload: { columnId: number; visibleFields: string[]; permittedUserIds: number[] }]
}>()

let draggingTaskId = ''

function dragStart(taskId: string) {
  draggingTaskId = taskId
}

function drop(toColumnId: string) {
  if (draggingTaskId) {
    emit('taskMove', draggingTaskId, toColumnId)
    draggingTaskId = ''
  }
}

function count(column: KanbanColumn) {
  return props.columns.find((item) => item.id === column.id)?.tasks.length ?? 0
}

function formatHours(hours: number) {
  return `${hours.toFixed(2)}h`
}

// ── Column settings (per column) ──
const FIELD_DEFS = [
  { id: 'title', label: 'Tiêu đề nhiệm vụ' },
  { id: 'desc', label: 'Mô tả / Ghi chú' },
  { id: 'amount', label: 'Số tiền' },
  { id: 'date', label: 'Ngày thực hiện' },
  { id: 'tag', label: 'Loại topup (tag)' },
  { id: 'bank', label: 'Ngân hàng / Số TK' },
  { id: 'person', label: 'Người phụ trách' },
]
const DEFAULT_VISIBLE = ['title', 'desc', 'amount', 'date', 'tag']
const AVATAR_PALETTE = ['#6366f1', '#14b8a6', '#f59e0b', '#f43f5e', '#0ea5e9', '#8b5cf6', '#10b981']

const localSettings = reactive<Record<string, { visibleFields: string[]; permittedUserIds: number[] }>>({})

function settingsFor(column: KanbanColumn) {
  if (!localSettings[column.id]) {
    localSettings[column.id] = {
      visibleFields: column.visibleFields ? [...column.visibleFields] : [...DEFAULT_VISIBLE],
      permittedUserIds: column.permittedUserIds ? [...column.permittedUserIds] : [],
    }
  }
  return localSettings[column.id]
}

function fieldOn(column: KanbanColumn, id: string) {
  return settingsFor(column).visibleFields.includes(id)
}

function toggleField(column: KanbanColumn, id: string, event: Event) {
  const checked = (event.target as HTMLInputElement).checked
  const setting = settingsFor(column)
  setting.visibleFields = checked
    ? [...new Set([...setting.visibleFields, id])]
    : setting.visibleFields.filter((x) => x !== id)
  persist(column)
}

function permOn(column: KanbanColumn, userId: number) {
  return settingsFor(column).permittedUserIds.includes(userId)
}

function togglePerm(column: KanbanColumn, userId: number, event: Event) {
  const checked = (event.target as HTMLInputElement).checked
  const setting = settingsFor(column)
  setting.permittedUserIds = checked
    ? [...new Set([...setting.permittedUserIds, userId])]
    : setting.permittedUserIds.filter((x) => x !== userId)
  persist(column)
}

function persist(column: KanbanColumn) {
  if (column.dbId == null) return
  const setting = settingsFor(column)
  emit('settingsChange', {
    columnId: column.dbId,
    visibleFields: [...setting.visibleFields],
    permittedUserIds: [...setting.permittedUserIds],
  })
}

function taskDesc(task: KanbanTask) {
  return task.details['Ghi chú'] ?? ''
}

function taskBank(task: KanbanTask) {
  const account = task.details['Số tài khoản']
  const bank = task.details['Ngân hàng']
  if (!account && !bank) return ''
  return `TK: ${account ?? ''}${account && bank ? ' · ' : ''}${bank ?? ''}`
}

// ── Settings popover ──
const openColId = ref<string | null>(null)
const colTab = ref<0 | 1>(0)
const popStyle = reactive({ top: '0px', left: '0px' })

const openColumn = computed(() => props.columns.find((c) => c.id === openColId.value) ?? null)

function toggleSettings(columnId: string, event: MouseEvent) {
  event.stopPropagation()
  if (openColId.value === columnId) {
    openColId.value = null
    return
  }
  openColId.value = columnId
  colTab.value = 0

  const button = event.currentTarget as HTMLElement
  const rect = button.getBoundingClientRect()
  const popWidth = 272
  let left = rect.right - popWidth
  if (left < 8) left = 8
  if (left + popWidth > window.innerWidth - 8) left = window.innerWidth - popWidth - 8
  popStyle.top = `${rect.bottom + 6}px`
  popStyle.left = `${left}px`
}

function closeSettings() {
  openColId.value = null
}

function onDocumentClick() {
  if (openColId.value) closeSettings()
}

onMounted(() => document.addEventListener('click', onDocumentClick))
onBeforeUnmount(() => document.removeEventListener('click', onDocumentClick))
</script>

<template>
  <div class="kanban-root">
    <div class="kanban-wrap">
      <div class="kanban-board">
        <section
          v-for="column in columns"
          :key="column.id"
          class="kanban-col"
          @dragover.prevent
          @drop="drop(column.id)"
        >
          <div class="kanban-head" :class="column.tone ? `kanban-head-${column.tone}` : ''">
            <div class="kanban-head-row">
              <div class="kanban-title">{{ column.title }}</div>
              <span class="kanban-count">{{ count(column) }}</span>
              <button
                class="kanban-settings-btn"
                type="button"
                title="Cài đặt cột"
                @click="toggleSettings(column.id, $event)"
              >⚙</button>
            </div>

            <div v-if="column.avatars?.length" class="kanban-avatars">
              <span
                v-for="(avatar, index) in column.avatars"
                :key="index"
                class="kanban-av"
                :class="avatar.tone ? `kanban-av-${avatar.tone}` : ''"
                :title="avatar.name ?? avatar.initials"
              >{{ avatar.initials }}</span>
            </div>

            <div v-if="column.stats" class="kanban-stats">
              <span
                class="kanban-stat"
                :class="column.stats.nvTone ? `kanban-stat-${column.stats.nvTone}` : ''"
              >{{ column.stats.done }}/{{ column.stats.total }} NV</span>
              <span class="kanban-stat-sep">·</span>
              <span class="kanban-stat" :class="{ 'kanban-stat-red': column.stats.overdue > 0 }">
                {{ column.stats.overdue }} Q.hạn
              </span>
              <template v-if="column.stats.avgHours !== undefined">
                <span class="kanban-stat-sep">·</span>
                <span class="kanban-stat">⏱ {{ formatHours(column.stats.avgHours) }}</span>
              </template>
              <template v-if="column.stats.workItems !== undefined">
                <span class="kanban-stat-sep">·</span>
                <span class="kanban-stat">📋 {{ column.stats.workItems }} CV</span>
              </template>
            </div>
          </div>

          <div class="kanban-list">
            <article
              v-for="task in column.tasks"
              :key="task.id"
              class="task-card"
              draggable="true"
              @dragstart="dragStart(task.id)"
              @click="emit('taskOpen', task)"
            >
              <span v-if="fieldOn(column, 'tag')" class="task-tag" :class="`task-tag-${task.tone}`">{{ task.park }}</span>
              <div v-if="fieldOn(column, 'title')" class="task-name">{{ task.title }}</div>
              <div v-if="fieldOn(column, 'desc') && taskDesc(task)" class="task-desc">{{ taskDesc(task) }}</div>
              <div v-if="fieldOn(column, 'bank') && taskBank(task)" class="task-desc">{{ taskBank(task) }}</div>
              <div class="task-meta">
                <span v-if="fieldOn(column, 'person')">{{ task.owner }}</span>
                <span v-if="fieldOn(column, 'date')">📅 {{ task.date }}</span>
                <span v-if="fieldOn(column, 'amount')" class="task-money">{{ task.amount }}</span>
              </div>
              <span class="badge task-priority" :class="`badge-${task.tone}`">{{ task.status }}</span>
            </article>
          </div>
        </section>
      </div>
    </div>

    <div v-if="openColumn" class="col-pop open" :style="popStyle" @click.stop>
      <div class="col-pop-tabs">
        <div class="col-pop-tab" :class="{ active: colTab === 0 }" @click="colTab = 0">Trường thông tin</div>
        <div class="col-pop-tab" :class="{ active: colTab === 1 }" @click="colTab = 1">Phân quyền</div>
      </div>
      <div class="col-pop-body">
        <template v-if="colTab === 0">
          <div class="col-pop-ttl">Trường hiển thị trên card</div>
          <label v-for="field in FIELD_DEFS" :key="field.id" class="col-check">
            <input
              type="checkbox"
              :checked="fieldOn(openColumn, field.id)"
              @change="toggleField(openColumn, field.id, $event)"
            />
            <span class="col-check-lbl">{{ field.label }}</span>
          </label>
        </template>
        <template v-else>
          <div class="col-pop-ttl">Ai được chuyển task ở bước này?</div>
          <div class="col-pop-sub">Bước: <strong>{{ openColumn.title }}</strong></div>
          <div v-if="!users?.length" class="col-pop-sub">Chưa có người dùng để phân quyền.</div>
          <div v-for="(user, index) in users ?? []" :key="user.id" class="col-user-row">
            <div class="col-user-av" :style="{ background: AVATAR_PALETTE[index % AVATAR_PALETTE.length] }">{{ user.initials }}</div>
            <span class="col-user-name">{{ user.fullName }}</span>
            <input
              type="checkbox"
              class="col-user-chk"
              :checked="permOn(openColumn, user.id)"
              @change="togglePerm(openColumn, user.id, $event)"
            />
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
