<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { useConfirm } from '../composables/useConfirm'
import { ApiClientError } from '../services/apiClient'
import { authState } from '../services/authStore'
import {
  createAgency,
  deleteAgency,
  listAgencies,
  updateAgency,
  type AgencyDto,
  type AgencySource,
} from '../services/agenciesApi'

const toast = useToast()
const { confirm } = useConfirm()

const agencies = ref<AgencyDto[]>([])
const keyword = ref('')
const page = ref(1)
const pageSize = 20
const totalItems = ref(0)
const totalPages = ref(1)
const loading = ref(false)
const saving = ref(false)

const showModal = ref(false)
const editingId = ref<number | null>(null)
const form = reactive({
  code: '',
  name: '',
  parentCode: '',
  parentName: '',
  source: 'OneInventory' as AgencySource,
})

const isAdmin = computed(() => authState.user?.role === 'Admin')

function errorText(error: unknown, fallback: string) {
  return error instanceof ApiClientError ? error.message : fallback
}

async function loadAgencies() {
  loading.value = true
  try {
    const result = await listAgencies({ page: page.value, keyword: keyword.value })
    agencies.value = result.items
    totalItems.value = result.totalItems
    totalPages.value = Math.max(1, result.totalPages)
  } catch (error) {
    toast.error(errorText(error, 'Không tải được danh sách đại lý.'))
  } finally {
    loading.value = false
  }
}

async function goPage(next: number) {
  page.value = Math.min(Math.max(1, next), totalPages.value)
  await loadAgencies()
}

let searchTimer: ReturnType<typeof setTimeout> | undefined
function onSearch() {
  page.value = 1
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(loadAgencies, 300)
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

function openEdit(agency: AgencyDto) {
  editingId.value = agency.id
  form.code = agency.code
  form.name = agency.name
  form.parentCode = agency.parentCode ?? ''
  form.parentName = agency.parentName ?? ''
  form.source = agency.source
  showModal.value = true
}

async function saveAgency() {
  if (saving.value) return
  if (!form.code.trim() || !form.name.trim()) {
    toast.warning('Vui lòng nhập đủ Mã đại lý và Tên đại lý.')
    return
  }

  saving.value = true
  try {
    const payload = {
      code: form.code.trim(),
      name: form.name.trim(),
      parentCode: form.parentCode.trim() || null,
      parentName: form.parentName.trim() || null,
      source: form.source,
    }

    if (editingId.value !== null) {
      await updateAgency(editingId.value, payload)
      toast.success('Đã cập nhật đại lý.')
    } else {
      await createAgency(payload)
      toast.success('Đã thêm đại lý.')
    }
    showModal.value = false
    await loadAgencies()
  } catch (error) {
    toast.error(errorText(error, 'Không lưu được đại lý.'))
  } finally {
    saving.value = false
  }
}

async function removeAgency(agency: AgencyDto) {
  const ok = await confirm({
    title: 'Xóa đại lý',
    message: `Bạn có chắc muốn xóa đại lý "${agency.name}" (${agency.code})?`,
    confirmText: 'Xóa',
  })
  if (!ok) return
  try {
    await deleteAgency(agency.id)
    if (agencies.value.length === 1 && page.value > 1) page.value--
    await loadAgencies()
    toast.success('Đã xóa đại lý.')
  } catch (error) {
    toast.error(errorText(error, 'Không xóa được đại lý.'))
  }
}

onMounted(loadAgencies)
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
      <button v-if="isAdmin" class="add-btn" type="button" @click="openCreate">
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
          <tr v-if="loading">
            <td colspan="7" class="cell-muted" style="text-align: center">Đang tải...</td>
          </tr>
          <tr v-for="(agency, i) in agencies" v-else :key="agency.id">
            <td class="cell-muted">{{ (page - 1) * pageSize + i + 1 }}</td>
            <td class="cell-muted">{{ agency.code }}</td>
            <td class="cell-strong">{{ agency.name }}</td>
            <td class="cell-muted">{{ agency.parentCode }}</td>
            <td>{{ agency.parentName || '-' }}</td>
            <td><span class="badge badge-blue">{{ agency.source }}</span></td>
            <td v-if="isAdmin">
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
            <td v-else></td>
          </tr>
          <tr v-if="!loading && agencies.length === 0">
            <td colspan="7" class="cell-muted" style="text-align: center">Không có đại lý phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pg-info">Tổng {{ totalItems }} đại lý</span>
      <button class="pg-btn" type="button" :disabled="loading || page <= 1" @click="goPage(page - 1)">‹</button>
      <button class="pg-btn active" type="button">{{ page }}</button>
      <button class="pg-btn" type="button" :disabled="loading || page >= totalPages" @click="goPage(page + 1)">›</button>
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
            <label class="form-label">Mã đại lý <span class="required-mark">*</span></label>
            <input v-model="form.code" class="form-input" placeholder="VD: 7391" />
          </div>
          <div class="form-group">
            <label class="form-label">Tên đại lý <span class="required-mark">*</span></label>
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
        <button class="btn-primary" type="button" :disabled="saving" @click="saveAgency">
          {{ saving ? 'Đang lưu...' : 'Lưu' }}
        </button>
      </div>
    </div>
  </div>
</template>
