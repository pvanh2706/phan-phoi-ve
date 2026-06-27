<script setup lang="ts">
import type { KanbanColumn, KanbanTask } from '../../data/workflows'
import AppIcon from './AppIcon.vue'

const props = defineProps<{
  columns: KanbanColumn[]
}>()

const emit = defineEmits<{
  taskOpen: [task: KanbanTask]
  taskMove: [taskId: string, toColumnId: string]
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
</script>

<template>
  <div class="kanban-wrap">
    <div class="kanban-board">
      <section
        v-for="column in columns"
        :key="column.id"
        class="kanban-col"
        @dragover.prevent
        @drop="drop(column.id)"
      >
        <div class="kanban-head">
          <div class="kanban-title">
            <span class="kanban-title-icon"><AppIcon :name="column.icon" :size="15" /></span>
            {{ column.title }}
          </div>
          <span class="kanban-count">{{ count(column) }}</span>
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
            <div class="task-name">{{ task.title }}</div>
            <div class="task-meta">
              <span>{{ task.park }}</span>
              <span>{{ task.owner }} · {{ task.date }}</span>
              <span class="task-money">{{ task.amount }}</span>
            </div>
            <span class="badge task-priority" :class="`badge-${task.tone}`">{{ task.status }}</span>
          </article>
        </div>
      </section>
    </div>
  </div>
</template>
