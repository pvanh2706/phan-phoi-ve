<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { ApiClientError } from '../services/apiClient'
import { listParkAuditLogs, type AuditLogDto } from '../services/auditApi'
import { auditActionLabel, formatDateTime } from '../services/formatters'

const loading = ref(false)
const items = ref<AuditLogDto[]>([])
const toast = useToast()
const totalItems = ref(0)
const page = ref(1)

const filters = reactive({
  module: 'Park',
  entityName: '',
  action: '',
  fromDate: '',
  toDate: '',
})

function errorText(err: unknown, fallback: string) {
  return err instanceof ApiClientError ? err.message : fallback
}

function compactJson(value?: string | null) {
  if (!value) return '-'
  try {
    return JSON.stringify(JSON.parse(value))
  } catch {
    return value
  }
}

async function load() {
  loading.value = true
  try {
    const result = await listParkAuditLogs({
      page: page.value,
      module: filters.module,
      entityName: filters.entityName,
      action: filters.action,
      fromDate: filters.fromDate,
      toDate: filters.toDate,
    })
    items.value = result.items
    totalItems.value = result.totalItems
  } catch (err) {
    toast.error(errorText(err, 'Không tải được nhật ký.'))
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  filters.module = 'Park'
  filters.entityName = ''
  filters.action = ''
  filters.fromDate = ''
  filters.toDate = ''
  page.value = 1
  void load()
}

onMounted(load)
</script>

<template>
  <PageHeader title="Nhật ký thay đổi" subtitle="Theo dõi before/after các thao tác trong Khu vui chơi và job liên quan" />

  <section class="card">
    <div class="toolbar">
      <input v-model="filters.fromDate" class="tb-date" type="date" />
      <input v-model="filters.toDate" class="tb-date" type="date" />
      <select v-model="filters.module" class="tb-select">
        <option value="Park">Khu vui chơi</option>
        <option value="Jobs">Jobs</option>
        <option value="Settings">Cài đặt</option>
        <option value="Auth">Đăng nhập</option>
      </select>
      <select v-model="filters.entityName" class="tb-select">
        <option value="">Tất cả đối tượng</option>
        <option value="Park">KVC</option>
        <option value="ParkTicketType">Loại vé</option>
        <option value="DailyParkBalanceSnapshot">Số dư</option>
        <option value="DailyTicketCostSummary">Giá vốn</option>
        <option value="DailyBankTransactionSummary">Giao dịch ngân hàng</option>
        <option value="ParkReconciliation">Đối soát</option>
        <option value="JobRun">Job</option>
      </select>
      <select v-model="filters.action" class="tb-select">
        <option value="">Tất cả hành động</option>
        <option value="Create">Tạo mới</option>
        <option value="Update">Cập nhật</option>
        <option value="SetInactive">Ngừng sử dụng</option>
        <option value="SoftDelete">Xóa mềm</option>
        <option value="RunJob">Chạy job</option>
        <option value="ManualEntry">Nhập tay</option>
        <option value="ResolveVariance">Xử lý lệch</option>
      </select>
      <button class="btn-secondary" type="button" @click="load">Lọc</button>
      <button class="btn-secondary" type="button" @click="resetFilters">Xóa lọc</button>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Thời điểm</th>
            <th>Người thao tác</th>
            <th>Module</th>
            <th>Đối tượng</th>
            <th>Hành động</th>
            <th>Trước</th>
            <th>Sau</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="7">Đang tải...</td>
          </tr>
          <tr v-for="item in items" :key="item.id">
            <td>{{ formatDateTime(item.occurredAtUtc) }}</td>
            <td>{{ item.userEmailSnapshot ?? 'Hệ thống' }}</td>
            <td>{{ item.module }}</td>
            <td class="cell-strong">{{ item.entityName }} #{{ item.entityId || '-' }}</td>
            <td><span class="badge badge-indigo">{{ auditActionLabel(item.action) }}</span></td>
            <td class="cell-muted" style="max-width: 280px">{{ compactJson(item.beforeJson) }}</td>
            <td class="cell-muted" style="max-width: 280px">{{ compactJson(item.afterJson) }}</td>
          </tr>
          <tr v-if="!loading && items.length === 0">
            <td colspan="7" class="cell-muted" style="text-align: center">Không có nhật ký phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pg-info">Tổng {{ totalItems }} dòng</span>
      <button class="pg-btn" type="button" :disabled="page <= 1" @click="page -= 1; load()">‹</button>
      <button class="pg-btn active" type="button">{{ page }}</button>
      <button class="pg-btn" type="button" :disabled="page * 100 >= totalItems" @click="page += 1; load()">›</button>
    </div>
  </section>
</template>
