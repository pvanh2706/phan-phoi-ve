<script setup lang="ts">
import type { TableCell, TableColumn, TableRow } from '../../data/reports'

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

function actionClass(label: string) {
  const icon = actionIcon(label)
  if (icon === 'edit') return 'edit-btn'
  if (icon === 'trash') return 'delete-btn'
  return 'view-btn'
}
</script>

<template>
  <div class="table-wrap report-table-wrap">
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
              <button v-for="action in cell.actions" :key="action.label" :class="actionClass(action.label)" type="button" :title="action.label">
                <svg v-if="actionIcon(action.label) === 'edit'" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                <svg v-else-if="actionIcon(action.label) === 'trash'" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
                <svg v-else width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
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
