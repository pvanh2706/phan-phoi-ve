<script setup lang="ts">
import type { TableCell, TableColumn, TableRow } from '../../data/reports'
import AppIcon from './AppIcon.vue'

defineProps<{
  columns: TableColumn[]
  rows: TableRow[]
}>()

function cellClass(cell: TableCell) {
  if (typeof cell === 'string') {
    return ''
  }

  if (cell.kind === 'amount') {
    return `amount amount-${cell.tone}`
  }

  if (cell.kind === 'strong') {
    return 'cell-strong'
  }

  if (cell.kind === 'muted') {
    return 'cell-muted'
  }

  return ''
}

function actionIcon(label: string) {
  if (label.includes('Sửa')) return 'edit'
  if (label.includes('Xoá') || label.includes('Xóa')) return 'trash'
  return 'eye'
}
</script>

<template>
  <div class="table-wrap">
    <table>
      <thead>
        <tr>
          <th v-for="column in columns" :key="column.key" :style="{ textAlign: column.align ?? 'left' }">
            {{ column.label }}
          </th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="row in rows" :key="row.id">
          <td v-for="(cell, index) in row.cells" :key="`${row.id}-${index}`" :style="{ textAlign: columns[index]?.align ?? 'left' }">
            <span v-if="typeof cell === 'string'">{{ cell }}</span>
            <span v-else-if="cell.kind === 'badge'" class="badge" :class="`badge-${cell.tone}`">{{ cell.text }}</span>
            <span v-else-if="cell.kind === 'actions'">
              <button v-for="action in cell.actions" :key="action.label" class="act-btn" type="button" :title="action.label">
                <AppIcon :name="actionIcon(action.label)" :size="14" />
              </button>
            </span>
            <span v-else :class="cellClass(cell)">{{ cell.text }}</span>
          </td>
        </tr>
        <tr v-if="rows.length === 0">
          <td :colspan="columns.length" style="text-align: center; color: #6b7280">Không có dữ liệu phù hợp</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
