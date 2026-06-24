<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import {
  childDebtCodes,
  childTopupCodes,
  parentParkCodes,
  type ParkCodeRecord,
  type ParkTab,
} from '../data/parkCodes'

const activeTab = ref<ParkTab>('ma')
const search = ref('')
const typeFilter = ref('')
const statusFilter = ref('')
const showModal = ref(false)
const showDelete = ref(false)
const editingId = ref<string | null>(null)
const deletingId = ref<string | null>(null)
const parentRecords = ref([...parentParkCodes])
const topupRecords = ref([...childTopupCodes])
const debtRecords = ref([...childDebtCodes])
const form = reactive<ParkCodeRecord>({
  id: '',
  code: '',
  name: '',
  type: 'Nạp tiền',
  parent: '',
  location: '',
  account: '',
  bank: '',
  status: 'Hoạt động',
})

const tabMeta = {
  ma: { label: 'Mã KVC', add: '+ Thêm KVC' },
  nap: { label: 'Danh mục KVC con nạp trước', add: '+ Thêm KVC con' },
  cn: { label: 'Danh mục KVC con công nợ', add: '+ Thêm KVC con' },
}

const currentRecords = computed(() => {
  if (activeTab.value === 'nap') {
    return topupRecords.value
  }
  if (activeTab.value === 'cn') {
    return debtRecords.value
  }
  return parentRecords.value
})

const filteredRecords = computed(() => {
  const keyword = search.value.trim().toLowerCase()
  return currentRecords.value.filter((record) => {
    const text = `${record.code} ${record.name} ${record.parent ?? ''} ${record.location}`.toLowerCase()
    return (
      (!keyword || text.includes(keyword)) &&
      (!typeFilter.value || record.type === typeFilter.value) &&
      (!statusFilter.value || record.status === statusFilter.value)
    )
  })
})

function switchTab(tab: ParkTab) {
  activeTab.value = tab
  search.value = ''
  typeFilter.value = ''
  statusFilter.value = ''
}

function openAdd() {
  editingId.value = null
  Object.assign(form, {
    id: '',
    code: '',
    name: '',
    type: activeTab.value === 'cn' ? 'Công nợ' : 'Nạp tiền',
    parent: '',
    location: '',
    account: '',
    bank: '',
    status: 'Hoạt động',
  })
  showModal.value = true
}

function openEdit(record: ParkCodeRecord) {
  editingId.value = record.id
  Object.assign(form, record)
  showModal.value = true
}

function saveRecord() {
  const target = activeTab.value === 'nap' ? topupRecords : activeTab.value === 'cn' ? debtRecords : parentRecords
  const record = { ...form, id: editingId.value ?? `${activeTab.value}-${Date.now()}` }
  const index = target.value.findIndex((item) => item.id === record.id)
  if (index >= 0) {
    target.value[index] = record
  } else {
    target.value.unshift(record)
  }
  showModal.value = false
}

function askDelete(id: string) {
  deletingId.value = id
  showDelete.value = true
}

function confirmDelete() {
  const target = activeTab.value === 'nap' ? topupRecords : activeTab.value === 'cn' ? debtRecords : parentRecords
  target.value = target.value.filter((record) => record.id !== deletingId.value)
  showDelete.value = false
}
</script>

<template>
  <PageHeader title="Mã khu vui chơi" subtitle="Quản lý mã KVC cha và danh mục KVC con theo hình thức thanh toán" />

  <div class="tabs-bar">
    <button
      v-for="(meta, key) in tabMeta"
      :key="key"
      class="tab-btn"
      :class="{ active: activeTab === key }"
      type="button"
      @click="switchTab(key as ParkTab)"
    >
      {{ meta.label }}
    </button>
  </div>

  <div class="card">
    <div class="toolbar">
      <input v-model="search" class="tb-input" placeholder="🔍  Tìm mã hoặc tên KVC..." />
      <select v-if="activeTab === 'ma'" v-model="typeFilter" class="tb-select">
        <option value="">Tất cả loại KVC</option>
        <option value="Nạp tiền">Nạp tiền</option>
        <option value="Công nợ">Công nợ</option>
      </select>
      <select v-model="statusFilter" class="tb-select">
        <option value="">Tất cả trạng thái</option>
        <option value="Hoạt động">Hoạt động</option>
        <option value="Tạm dừng">Tạm dừng</option>
      </select>
      <button class="add-btn" type="button" @click="openAdd">{{ tabMeta[activeTab].add }}</button>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Mã KVC</th>
            <th>Tên khu vui chơi</th>
            <th v-if="activeTab !== 'ma'">KVC cha</th>
            <th>Loại</th>
            <th>Địa điểm</th>
            <th>Tài khoản</th>
            <th>Trạng thái</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="record in filteredRecords" :key="record.id">
            <td class="cell-muted">{{ record.code }}</td>
            <td class="cell-strong">{{ record.name }}</td>
            <td v-if="activeTab !== 'ma'">{{ record.parent }}</td>
            <td><span class="badge" :class="record.type === 'Nạp tiền' ? 'badge-green' : 'badge-amber'">{{ record.type }}</span></td>
            <td>{{ record.location }}</td>
            <td><span class="cell-muted">{{ record.bank }} · {{ record.account }}</span></td>
            <td><span class="badge" :class="record.status === 'Hoạt động' ? 'badge-green' : 'badge-gray'">● {{ record.status }}</span></td>
            <td>
              <button class="act-btn" type="button" title="Sửa" @click="openEdit(record)">✏️</button>
              <button class="act-btn" type="button" title="Xoá" @click="askDelete(record.id)">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">{{ editingId ? 'Chỉnh sửa' : tabMeta[activeTab].add.replace('+ ', '') }}</span>
        <button class="modal-close" type="button" @click="showModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mã KVC</label>
            <input v-model="form.code" class="form-input" placeholder="KVC-001" />
          </div>
          <div class="form-group">
            <label class="form-label">Tên khu vui chơi</label>
            <input v-model="form.name" class="form-input" placeholder="Bản Mòng" />
          </div>
        </div>
        <div class="form-row">
          <div v-if="activeTab !== 'ma'" class="form-group">
            <label class="form-label">KVC cha</label>
            <input v-model="form.parent" class="form-input" placeholder="Sun Group" />
          </div>
          <div class="form-group">
            <label class="form-label">Loại</label>
            <select v-model="form.type" class="form-select">
              <option value="Nạp tiền">Nạp tiền</option>
              <option value="Công nợ">Công nợ</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Trạng thái</label>
            <select v-model="form.status" class="form-select">
              <option value="Hoạt động">Hoạt động</option>
              <option value="Tạm dừng">Tạm dừng</option>
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Địa điểm</label>
            <input v-model="form.location" class="form-input" placeholder="Đà Nẵng" />
          </div>
          <div class="form-group">
            <label class="form-label">Ngân hàng</label>
            <input v-model="form.bank" class="form-input" placeholder="Techcombank" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Số tài khoản</label>
          <input v-model="form.account" class="form-input" placeholder="19139932758899" />
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showModal = false">Hủy</button>
        <button class="btn-primary" type="button" @click="saveRecord">Lưu</button>
      </div>
    </div>
  </div>

  <div v-if="showDelete" class="modal-overlay" @click.self="showDelete = false">
    <div class="modal" style="width: 380px">
      <div class="modal-header">
        <span class="modal-title">Xác nhận xoá</span>
        <button class="modal-close" type="button" @click="showDelete = false">✕</button>
      </div>
      <div class="modal-body">Bạn có chắc muốn xoá mã KVC này?</div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showDelete = false">Hủy</button>
        <button class="btn-danger" type="button" @click="confirmDelete">Xoá</button>
      </div>
    </div>
  </div>
</template>
