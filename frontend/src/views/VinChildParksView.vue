<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { useConfirm } from '../composables/useConfirm'

type DebtType = 'Prepaid' | 'Debt'

interface VinChildPark {
  id: number
  code: string
  name: string
  bankAccount: string
  debtType: DebtType
}

const toast = useToast()
const { confirm } = useConfirm()

// Dữ liệu demo (frontend-only, mất khi tải lại trang) — chưa có backend riêng cho KVC con Vin.
const rows = ref<VinChildPark[]>([
  { id: 1, code: '11810', name: 'Timescity', bankAccount: 'M368010106273448', debtType: 'Prepaid' },
  { id: 2, code: '11813', name: 'VinWonders Vũ Yên', bankAccount: '19010000888334', debtType: 'Prepaid' },
  { id: 3, code: '10288', name: 'Phú Quốc', bankAccount: '0091000593278', debtType: 'Prepaid' },
  { id: 4, code: '10064', name: 'Nam Hội An', bankAccount: 'M368020106273448', debtType: 'Prepaid' },
  { id: 5, code: '11732', name: 'CÔNG VIÊN GRAND PARK', bankAccount: '117002989969', debtType: 'Prepaid' },
])
let nextId = rows.value.length + 1

const keyword = ref('')
const filteredRows = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  if (!kw) return rows.value
  return rows.value.filter(
    (r) => r.code.toLowerCase().includes(kw) || r.name.toLowerCase().includes(kw) || r.bankAccount.toLowerCase().includes(kw),
  )
})

function debtTypeLabel(type: DebtType) {
  return type === 'Prepaid' ? 'Nạp trước' : 'Công nợ'
}

const showModal = ref(false)
const editingId = ref<number | null>(null)
const form = reactive({
  code: '',
  name: '',
  bankAccount: '',
  debtType: 'Prepaid' as DebtType,
})

function openAdd() {
  editingId.value = null
  Object.assign(form, { code: '', name: '', bankAccount: '', debtType: 'Prepaid' })
  showModal.value = true
}

function openEdit(row: VinChildPark) {
  editingId.value = row.id
  Object.assign(form, { code: row.code, name: row.name, bankAccount: row.bankAccount, debtType: row.debtType })
  showModal.value = true
}

function saveRow() {
  const code = form.code.trim()
  const name = form.name.trim()
  if (!code || !name) {
    toast.error('Vui lòng nhập đủ Mã KVC và Tên KVC.')
    return
  }

  if (editingId.value) {
    const row = rows.value.find((r) => r.id === editingId.value)
    if (row) {
      row.code = code
      row.name = name
      row.bankAccount = form.bankAccount.trim()
      row.debtType = form.debtType
    }
    toast.success('Đã cập nhật KVC con của Vin (demo, chưa lưu backend).')
  } else {
    rows.value.push({
      id: nextId++,
      code,
      name,
      bankAccount: form.bankAccount.trim(),
      debtType: form.debtType,
    })
    toast.success('Đã thêm KVC con của Vin (demo, chưa lưu backend).')
  }
  showModal.value = false
}

async function removeRow(row: VinChildPark) {
  const ok = await confirm({
    title: 'Xóa KVC con của Vin',
    message: `Bạn có chắc muốn xóa "${row.name}" (Mã ${row.code}) khỏi danh mục? Thao tác này không thể hoàn tác.`,
    confirmText: 'Xóa',
  })
  if (!ok) return
  rows.value = rows.value.filter((r) => r.id !== row.id)
  toast.success('Đã xóa KVC con của Vin (demo, chưa lưu backend).')
}
</script>

<template>
  <PageHeader
    title="Danh mục KVC con của Vin"
    subtitle="Các mã KVC con thuộc nhóm Vinpearl/VinWonders, dùng tài khoản ngân hàng và loại công nợ nạp trước"
  />

  <section class="card">
    <div class="toolbar">
      <input v-model="keyword" class="tb-input" placeholder="🔍  Tìm mã KVC, tên KVC..." />
      <button class="add-btn" type="button" @click="openAdd">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" d="M12 5v14M5 12h14" />
        </svg>
        Thêm KVC con
      </button>
    </div>

    <div class="table-wrap park-code-table-wrap">
      <table class="park-code-table">
        <thead>
          <tr>
            <th>Mã KVC</th>
            <th>TK Ngân hàng</th>
            <th>Tên KVC</th>
            <th>Loại Công nợ</th>
            <th style="width: 48px"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in filteredRows" :key="row.id">
            <td class="cell-muted">{{ row.code }}</td>
            <td class="cell-muted">{{ row.bankAccount || '-' }}</td>
            <td class="cell-strong">{{ row.name }}</td>
            <td>
              <span class="badge" :class="row.debtType === 'Prepaid' ? 'badge-teal' : 'badge-indigo'">
                {{ debtTypeLabel(row.debtType) }}
              </span>
            </td>
            <td>
              <button class="edit-btn" type="button" title="Sửa" @click="openEdit(row)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
              </button>
              <button class="delete-btn" type="button" title="Xóa" @click="removeRow(row)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </td>
          </tr>
          <tr v-if="filteredRows.length === 0">
            <td colspan="5" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <div v-if="showModal" class="modal-overlay park-code-modal-overlay">
    <div class="modal park-code-modal">
      <div class="modal-header">
        <span class="modal-title">{{ editingId ? 'Sửa KVC con của Vin' : 'Thêm KVC con của Vin' }}</span>
        <button class="modal-close" type="button" @click="showModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label class="form-label">Mã KVC <span class="required-mark">*</span></label>
          <input v-model="form.code" class="form-input" />
        </div>
        <div class="form-group">
          <label class="form-label">Tên KVC <span class="required-mark">*</span></label>
          <input v-model="form.name" class="form-input" />
        </div>
        <div class="form-group">
          <label class="form-label">TK Ngân hàng</label>
          <input v-model="form.bankAccount" class="form-input" />
        </div>
        <div class="form-group">
          <label class="form-label">Loại Công nợ</label>
          <select v-model="form.debtType" class="form-select">
            <option value="Prepaid">Nạp trước</option>
            <option value="Debt">Công nợ</option>
          </select>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-cancel" type="button" @click="showModal = false">Hủy</button>
        <button class="btn-save" type="button" @click="saveRow">Lưu thay đổi</button>
      </div>
    </div>
  </div>
</template>
