<script setup lang="ts">
import { onMounted, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { listJobErrors, type JobRunItemDto } from '../services/jobsApi'

const businessDate = ref('')
const loading = ref(false)
const error = ref('')
const items = ref<JobRunItemDto[]>([])

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await listJobErrors(1, businessDate.value || undefined)
    items.value = result.items
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Không tải được lỗi đồng bộ.'
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <PageHeader title="Lỗi đồng bộ cần xử lý" subtitle="Các KVC lỗi API để kế toán nhập tay dữ liệu đúng ngày" />
  <section class="card">
    <div class="toolbar">
      <input v-model="businessDate" class="tb-date" type="date" />
      <button class="btn-secondary" type="button" @click="load">Lọc</button>
    </div>
    <div v-if="error" class="notice notice-blue">{{ error }}</div>
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Ngày</th>
            <th>KVC</th>
            <th>Nguồn</th>
            <th>Trạng thái</th>
            <th>Lỗi</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="5">Đang tải...</td>
          </tr>
          <tr v-for="item in items" :key="item.id">
            <td>{{ item.businessDate }}</td>
            <td class="cell-strong">{{ item.parkCode }} - {{ item.parkName }}</td>
            <td>{{ item.source }}</td>
            <td><span class="badge" :class="item.status === 'ManualResolved' ? 'badge-green' : 'badge-red'">{{ item.status }}</span></td>
            <td>{{ item.errorMessage }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>
