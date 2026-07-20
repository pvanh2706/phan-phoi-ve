<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { useConfirm } from '../composables/useConfirm'
import { initialAgencies, type AgencyRecord } from '../data/agencies'

const toast = useToast()
const { confirm } = useConfirm()

const agencies = ref<AgencyRecord[]>(initialAgencies.map((a) => ({ ...a })))
let nextId = Math.max(0, ...agencies.value.map((a) => a.id)) + 1

const keyword = ref('')
const page = ref(1)
const pageSize = 20

const showModal = ref(false)
const editingId = ref<number | null>(null)
const form = reactive({
  code: '',
  name: '',
  parentCode: '',
  parentName: '',
  source: 'OneInventory',
})

const filteredAgencies = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  if (!kw) return agencies.value
  return agencies.value.filter(
    (a) => a.code.toLowerCase().includes(kw) || a.name.toLowerCase().includes(kw),
  )
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredAgencies.value.length / pageSize)))
const visibleAgencies = computed(() => {
  const start = (page.value - 1) * pageSize
  return filteredAgencies.value.slice(start, start + pageSize)
})

function goPage(next: number) {
  page.value = Math.min(Math.max(1, next), totalPages.value)
}

function onSearch() {
  page.value = 1
}

function resetForm() {
  form.code = ''
  form.name = ''
  form.parentCode = '5129'
  form.parentName = 'Đại Lý ezCloud Mua_PL'
  form.source = 'OneInventory'
}

function openCreate() {
  editingId.value = null
  resetForm()
  showModal.value = true
}

function openEdit(agency: AgencyRecord) {
  editingId.value = agency.id
  form.code = agency.code
  form.name = agency.name
  form.parentCode = agency.parentCode
  form.parentName = agency.parentName
  form.source = agency.source
  showModal.value = true
}

function saveAgency() {
  if (!form.code.trim() || !form.name.trim()) {
    toast.warning('Vui lòng nhập đủ Mã đại lý và Tên đại lý.')
    return
  }

  if (editingId.value) {
    const agency = agencies.value.find((a) => a.id === editingId.value)
    if (agency) {
      agency.code = form.code.trim()
      agency.name = form.name.trim()
      agency.parentCode = form.parentCode.trim()
      agency.parentName = form.parentName.trim()
      agency.source = form.source.trim()
    }
    toast.success('Đã cập nhật đại lý.')
  } else {
    agencies.value.unshift({
      id: nextId++,
      code: form.code.trim(),
      name: form.name.trim(),
      parentCode: form.parentCode.trim(),
      parentName: form.parentName.trim(),
      source: form.source.trim(),
    })
    toast.success('Đã thêm đại lý.')
  }
  showModal.value = false
}

async function removeAgency(agency: AgencyRecord) {
  const ok = await confirm({
    title: 'Xóa đại lý',
    message: `Bạn có chắc muốn xóa đại lý "${agency.name}" (${agency.code})?`,
    confirmText: 'Xóa',
  })
  if (!ok) return
  agencies.value = agencies.value.filter((a) => a.id !== agency.id)
  toast.success('Đã xóa đại lý.')
}
</script>

<template>
  <PageHeader
    title="Danh sách các đại lý"
    subtitle="Đại lý thuộc nhóm mua cấp trên 5129 (Đại Lý ezCloud Mua_PL) — nạp trước một khoản tiền và lấy vé dần cho đến khi hết số dư"
  />

  <section class="card">
    <div class="toolbar">
      <input
        v-model="keyword"
        class="tb-input"
        placeholder="🔍  Tìm mã hoặc tên đại lý..."
        @input="onSearch"
      />
      <button class="add-btn" type="button" @click="openCreate">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" d="M12 5v14M5 12h14" />
        </svg>
        Thêm đại lý
      </button>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th style="width: 56px">#</th>
            <th>Mã đại lý</th>
            <th>Tên đại lý</th>
            <th>Mã đại lý mua cấp trên</th>
            <th>Tên đại lý mua cấp trên</th>
            <th>Nguồn dữ liệu</th>
            <th style="width: 90px"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(agency, i) in visibleAgencies" :key="agency.id">
            <td class="cell-muted">{{ (page - 1) * pageSize + i + 1 }}</td>
            <td class="cell-muted">{{ agency.code }}</td>
            <td class="cell-strong">{{ agency.name }}</td>
            <td class="cell-muted">{{ agency.parentCode }}</td>
            <td>{{ agency.parentName }}</td>
            <td><span class="badge badge-blue">{{ agency.source }}</span></td>
            <td>
              <button class="edit-btn" type="button" title="Sửa" @click="openEdit(agency)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
              </button>
              <button class="delete-btn" type="button" title="Xóa" @click="removeAgency(agency)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </td>
          </tr>
          <tr v-if="visibleAgencies.length === 0">
            <td colspan="7" class="cell-muted" style="text-align: center">Không có đại lý phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pg-info">Tổng {{ filteredAgencies.length }} đại lý</span>
      <button class="pg-btn" type="button" :disabled="page <= 1" @click="goPage(page - 1)">‹</button>
      <button class="pg-btn active" type="button">{{ page }}</button>
      <button class="pg-btn" type="button" :disabled="page >= totalPages" @click="goPage(page + 1)">›</button>
    </div>
  </section>

  <div v-if="showModal" class="modal-overlay">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">{{ editingId ? 'Sửa đại lý' : 'Thêm đại lý' }}</span>
        <button class="modal-close" type="button" @click="showModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mã đại lý *</label>
            <input v-model="form.code" class="form-input" placeholder="VD: 7391" />
          </div>
          <div class="form-group">
            <label class="form-label">Tên đại lý *</label>
            <input v-model="form.name" class="form-input" placeholder="VD: Oneinventory_API_Klook" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mã đại lý mua cấp trên</label>
            <input v-model="form.parentCode" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Tên đại lý mua cấp trên</label>
            <input v-model="form.parentName" class="form-input" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Nguồn dữ liệu</label>
          <select v-model="form.source" class="form-select">
            <option value="OneInventory">OneInventory</option>
            <option value="AR">AR</option>
            <option value="AR & OneInventory">AR & OneInventory</option>
          </select>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showModal = false">Hủy</button>
        <button class="btn-primary" type="button" @click="saveAgency">Lưu</button>
      </div>
    </div>
  </div>
</template>
