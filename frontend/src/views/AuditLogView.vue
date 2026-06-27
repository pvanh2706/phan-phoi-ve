<script setup lang="ts">
import { onMounted, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { listParkAuditLogs, type AuditLogDto } from '../services/auditApi'

const loading = ref(false)
const error = ref('')
const items = ref<AuditLogDto[]>([])

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await listParkAuditLogs()
    items.value = result.items
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Không tải được nhật ký.'
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <PageHeader title="Nhật ký thay đổi" subtitle="Theo dõi các thao tác trong menu Khu vui chơi" />
  <section class="card">
    <div class="toolbar">
      <button class="btn-secondary" type="button" @click="load">Tải lại</button>
    </div>
    <div v-if="error" class="notice notice-blue">{{ error }}</div>
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Thời điểm</th>
            <th>Người thao tác</th>
            <th>Đối tượng</th>
            <th>Hành động</th>
            <th>Dữ liệu sau</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="5">Đang tải...</td>
          </tr>
          <tr v-for="item in items" :key="item.id">
            <td>{{ new Date(item.occurredAtUtc).toLocaleString('vi-VN') }}</td>
            <td>{{ item.userEmailSnapshot ?? 'Hệ thống' }}</td>
            <td>{{ item.entityName }} #{{ item.entityId }}</td>
            <td><span class="badge badge-indigo">{{ item.action }}</span></td>
            <td class="cell-muted">{{ item.afterJson }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>
