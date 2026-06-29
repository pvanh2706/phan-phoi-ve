<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useToast } from '../composables/useToast'
import { useConfirm } from '../composables/useConfirm'
import { ApiClientError } from '../services/apiClient'
import { authState } from '../services/authStore'
import {
  createPark,
  createParkTicketType,
  deletePark,
  deleteParkTicketType,
  listParks,
  listParkTicketTypes,
  setParkInactive,
  setParkTicketTypeInactive,
  updatePark,
  updateParkTicketType,
  type ParkDto,
  type ParkTicketTypeDto,
} from '../services/parksApi'
import {
  badgeClassForStatus,
  formatMoney,
  paymentTypeLabel,
  recordStatusLabel,
  type PaymentType,
  type RecordStatus,
} from '../services/formatters'

type TabKey = 'parks' | 'nap' | 'cn'

const toast = useToast()
const { confirm } = useConfirm()

const activeTab = ref<TabKey>('parks')
const loading = ref(false)
const saving = ref(false)

const parkPage = ref(1)
const ticketPage = ref(1)
const parkTotal = ref(0)
const ticketTotal = ref(0)
const parks = ref<ParkDto[]>([])
const ticketTypes = ref<ParkTicketTypeDto[]>([])
const parkOptions = ref<ParkDto[]>([])

const filters = reactive({
  keyword: '',
  paymentType: '' as PaymentType | '',
  status: '' as RecordStatus | '',
  parkId: '' as number | '',
})

const showParkModal = ref(false)
const showTicketModal = ref(false)
const editingParkId = ref<number | null>(null)
const editingTicketId = ref<number | null>(null)

const parkForm = reactive({
  code: '',
  name: '',
  paymentType: 'Prepaid' as PaymentType,
  searchCode: '',
  location: '',
  bankAccount: '',
  bankName: '',
  creditLimit: '',
  apiSiteId: '',
  apiProfileId: '',
  balanceTransformRule: 'None',
  apiNote: '',
  status: 'Active' as RecordStatus,
})

const ticketForm = reactive({
  parkId: '' as number | '',
  code: '',
  ticketTypeCode: '',
  name: '',
  ticketGroupName: '',
  costPrice: '',
  status: 'Active' as RecordStatus,
})

const isAdmin = computed(() => authState.user?.role === 'Admin')
const napParkOptions = computed(() => parkOptions.value.filter(p => p.paymentType === 'Prepaid'))
const cnParkOptions = computed(() => parkOptions.value.filter(p => p.paymentType === 'Debt'))
const currentParkOptions = computed(() => activeTab.value === 'nap' ? napParkOptions.value : cnParkOptions.value)
const currentPage = computed(() => activeTab.value === 'parks' ? parkPage.value : ticketPage.value)
const currentTotal = computed(() => activeTab.value === 'parks' ? parkTotal.value : ticketTotal.value)
const totalPages = computed(() => Math.max(1, Math.ceil(currentTotal.value / 100)))

function toNullable(value: string) {
  return value.trim() ? value.trim() : null
}

function toNullableNumber(value: string) {
  const trimmed = value.trim().replace(/[,. ]/g, '')
  return trimmed ? Number(trimmed) : null
}

function errorText(err: unknown, fallback: string) {
  return err instanceof ApiClientError ? err.message : fallback
}

async function loadParkOptions() {
  const result = await listParks({ page: 1 })
  parkOptions.value = result.items
}

async function loadParks() {
  const result = await listParks({
    page: parkPage.value,
    keyword: filters.keyword,
    paymentType: filters.paymentType,
    status: filters.status,
  })
  parks.value = result.items
  parkTotal.value = result.totalItems
  parkOptions.value = result.items
}

async function loadTicketTypes() {
  const tabPaymentType: PaymentType = activeTab.value === 'nap' ? 'Prepaid' : 'Debt'
  const result = await listParkTicketTypes({
    page: ticketPage.value,
    keyword: filters.keyword,
    paymentType: tabPaymentType,
    status: filters.status,
    parkId: filters.parkId,
  })
  ticketTypes.value = result.items
  ticketTotal.value = result.totalItems
  if (parkOptions.value.length === 0) {
    await loadParkOptions()
  }
}

async function load() {
  loading.value = true
  try {
    if (activeTab.value === 'parks') {
      await loadParks()
    } else {
      await loadTicketTypes()
    }
  } catch (err) {
    toast.error(errorText(err, 'Không tải được dữ liệu KVC.'))
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  filters.keyword = ''
  filters.paymentType = ''
  filters.status = ''
  filters.parkId = ''
  parkPage.value = 1
  ticketPage.value = 1
  void load()
}

function switchTab(tab: TabKey) {
  activeTab.value = tab
  resetFilters()
}

function openAddPark() {
  editingParkId.value = null
  Object.assign(parkForm, {
    code: '',
    name: '',
    paymentType: 'Prepaid',
    searchCode: '',
    location: '',
    bankAccount: '',
    bankName: '',
    creditLimit: '',
    apiSiteId: '',
    apiProfileId: '',
    balanceTransformRule: 'None',
    apiNote: '',
    status: 'Active',
  })
  showParkModal.value = true
}

function openEditPark(park: ParkDto) {
  editingParkId.value = park.id
  Object.assign(parkForm, {
    code: park.code,
    name: park.name,
    paymentType: park.paymentType,
    searchCode: park.searchCode ?? '',
    location: park.location ?? '',
    bankAccount: park.bankAccount ?? '',
    bankName: park.bankName ?? '',
    creditLimit: park.creditLimit?.toString() ?? '',
    apiSiteId: park.apiSiteId ?? '',
    apiProfileId: park.apiProfileId ?? '',
    balanceTransformRule: park.balanceTransformRule || 'None',
    apiNote: park.apiNote ?? '',
    status: park.status,
  })
  showParkModal.value = true
}

async function savePark() {
  if (saving.value) return
  saving.value = true
  try {
    const payload = {
      code: parkForm.code.trim(),
      name: parkForm.name.trim(),
      paymentType: parkForm.paymentType,
      searchCode: toNullable(parkForm.searchCode),
      location: toNullable(parkForm.location),
      bankAccount: toNullable(parkForm.bankAccount),
      bankName: toNullable(parkForm.bankName),
      creditLimit: toNullableNumber(parkForm.creditLimit),
      apiSiteId: toNullable(parkForm.apiSiteId),
      apiProfileId: toNullable(parkForm.apiProfileId),
      balanceTransformRule: toNullable(parkForm.balanceTransformRule),
      apiNote: toNullable(parkForm.apiNote),
      status: parkForm.status,
    }

    if (editingParkId.value) {
      await updatePark(editingParkId.value, payload)
      toast.success('Đã cập nhật khu vui chơi.')
    } else {
      await createPark(payload)
      toast.success('Đã tạo khu vui chơi.')
    }
    showParkModal.value = false
    await loadParks()
  } catch (err) {
    toast.error(errorText(err, 'Không lưu được khu vui chơi.'))
  } finally {
    saving.value = false
  }
}

function openAddTicket() {
  editingTicketId.value = null
  Object.assign(ticketForm, {
    parkId: filters.parkId || '',
    code: '',
    ticketTypeCode: '',
    name: '',
    ticketGroupName: '',
    costPrice: '',
    status: 'Active',
  })
  showTicketModal.value = true
}

function openEditTicket(ticket: ParkTicketTypeDto) {
  editingTicketId.value = ticket.id
  Object.assign(ticketForm, {
    parkId: ticket.parkId,
    code: ticket.code,
    ticketTypeCode: ticket.ticketTypeCode,
    name: ticket.name,
    ticketGroupName: ticket.ticketGroupName ?? '',
    costPrice: ticket.costPrice.toString(),
    status: ticket.status,
  })
  showTicketModal.value = true
}

async function saveTicket() {
  if (saving.value || !ticketForm.parkId) return
  saving.value = true
  try {
    const payload = {
      parkId: Number(ticketForm.parkId),
      code: ticketForm.code.trim(),
      ticketTypeCode: ticketForm.ticketTypeCode.trim(),
      name: ticketForm.name.trim(),
      ticketGroupName: toNullable(ticketForm.ticketGroupName),
      costPrice: Number(ticketForm.costPrice.trim().replace(/[,. ]/g, '')) || 0,
      status: ticketForm.status,
    }

    if (editingTicketId.value) {
      await updateParkTicketType(editingTicketId.value, payload)
      toast.success('Đã cập nhật loại vé.')
    } else {
      await createParkTicketType(payload)
      toast.success('Đã tạo loại vé.')
    }
    showTicketModal.value = false
    await loadTicketTypes()
  } catch (err) {
    toast.error(errorText(err, 'Không lưu được loại vé.'))
  } finally {
    saving.value = false
  }
}

async function inactivePark(id: number) {
  const ok = await confirm({
    title: 'Ngừng sử dụng khu vui chơi',
    message: 'Bạn có chắc muốn ngừng sử dụng khu vui chơi này?',
    confirmText: 'Ngừng sử dụng',
  })
  if (!ok) return
  try {
    await setParkInactive(id)
    toast.success('Đã ngừng sử dụng khu vui chơi.')
    await loadParks()
  } catch (err) {
    toast.error(errorText(err, 'Không thực hiện được thao tác.'))
  }
}

async function softDeletePark(id: number) {
  const ok = await confirm({
    title: 'Xóa khu vui chơi',
    message: 'Bạn có chắc muốn xóa mềm khu vui chơi này?',
    confirmText: 'Xóa',
  })
  if (!ok) return
  try {
    await deletePark(id)
    toast.success('Đã xóa mềm khu vui chơi.')
    await loadParks()
  } catch (err) {
    toast.error(errorText(err, 'Không xóa được khu vui chơi.'))
  }
}

async function inactiveTicket(id: number) {
  const ok = await confirm({
    title: 'Ngừng sử dụng loại vé',
    message: 'Bạn có chắc muốn ngừng sử dụng loại vé này?',
    confirmText: 'Ngừng sử dụng',
  })
  if (!ok) return
  try {
    await setParkTicketTypeInactive(id)
    toast.success('Đã ngừng sử dụng loại vé.')
    await loadTicketTypes()
  } catch (err) {
    toast.error(errorText(err, 'Không thực hiện được thao tác.'))
  }
}

async function softDeleteTicket(id: number) {
  const ok = await confirm({
    title: 'Xóa loại vé',
    message: 'Bạn có chắc muốn xóa mềm loại vé này?',
    confirmText: 'Xóa',
  })
  if (!ok) return
  try {
    await deleteParkTicketType(id)
    toast.success('Đã xóa mềm loại vé.')
    await loadTicketTypes()
  } catch (err) {
    toast.error(errorText(err, 'Không xóa được loại vé.'))
  }
}

function goPage(page: number) {
  if (activeTab.value === 'parks') {
    parkPage.value = page
  } else {
    ticketPage.value = page
  }
  void load()
}

onMounted(async () => {
  await load()
  await loadParkOptions().catch(() => undefined)
})
</script>

<template>
  <div class="park-codes-page">
  <PageHeader title="Mã khu vui chơi" subtitle="Quản lý KVC cha và loại vé/KVC con dùng cho đồng bộ, đối soát và báo cáo" />

  <div class="tabs-bar park-code-tabs">
    <button class="tab-btn" :class="{ active: activeTab === 'parks' }" type="button" @click="switchTab('parks')">
      Mã KVC
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'nap' }" type="button" @click="switchTab('nap')">
      Danh mục KVC con nạp trước
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'cn' }" type="button" @click="switchTab('cn')">
      Danh mục KVC con công nợ
    </button>
  </div>

  <div class="card">
    <div class="toolbar">
      <template v-if="activeTab === 'parks'">
        <input v-model="filters.keyword" class="tb-input" placeholder="🔍  Tìm mã hoặc tên KVC..." @keyup.enter="load" />
        <select v-model="filters.paymentType" class="tb-select" @change="load">
          <option value="">Tất cả loại</option>
          <option value="Prepaid">Nạp trước</option>
          <option value="Debt">Công nợ</option>
        </select>
        <select v-model="filters.status" class="tb-select" @change="load">
          <option value="">Tất cả trạng thái</option>
          <option value="Active">Đang hoạt động</option>
          <option value="Inactive">Ngừng hoạt động</option>
        </select>
        <button class="add-btn" type="button" @click="openAddPark()">
          <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
            <path stroke-linecap="round" d="M12 5v14M5 12h14" />
          </svg>
          Thêm KVC
        </button>
      </template>
      <template v-else>
        <input v-model="filters.keyword" class="tb-input" placeholder="🔍  Tìm mã hoặc tên KVC con..." @keyup.enter="load" />
        <select v-model="filters.parkId" class="tb-select" @change="load">
          <option value="">Tất cả KVC cha</option>
          <option v-for="park in currentParkOptions" :key="park.id" :value="park.id">
            {{ park.code }} - {{ park.name }}
          </option>
        </select>
        <button class="add-btn" type="button" @click="openAddTicket()">
          <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
            <path stroke-linecap="round" d="M12 5v14M5 12h14" />
          </svg>
          Thêm KVC con
        </button>
      </template>
    </div>


    <div v-if="activeTab === 'parks'" class="table-wrap park-code-table-wrap">
      <table class="park-code-table">
        <thead>
          <tr>
            <th>Mã KVC</th>
            <th>Tên khu vui chơi</th>
            <th>Loại</th>
            <th>Mã định danh tìm kiếm</th>
            <th>Mã ngân hàng / TK</th>
            <th>Trạng thái</th>
            <th style="width: 48px"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="7">Đang tải...</td>
          </tr>
          <tr v-for="park in parks" :key="park.id">
            <td class="cell-muted">{{ park.code }}</td>
            <td class="cell-strong">{{ park.name }}</td>
            <td><span class="badge" :class="park.paymentType === 'Prepaid' ? 'badge-teal' : 'badge-indigo'">{{ paymentTypeLabel(park.paymentType) }}</span></td>
            <td class="cell-muted">{{ park.searchCode || '-' }}</td>
            <td class="cell-muted">{{ park.bankAccount || '-' }}</td>
            <td><span class="badge" :class="badgeClassForStatus(park.status)">{{ recordStatusLabel(park.status) }}</span></td>
            <td>
              <button class="edit-btn" type="button" title="Sửa" @click="openEditPark(park)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
              </button>
              <button class="inactive-btn" type="button" title="Ngừng sử dụng" @click="inactivePark(park.id)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M10 5v14M14 5v14" />
                </svg>
              </button>
              <button v-if="isAdmin" class="delete-btn" type="button" title="Xóa" @click="softDeletePark(park.id)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </td>
          </tr>
          <tr v-if="!loading && parks.length === 0">
            <td colspan="7" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-else class="table-wrap park-code-table-wrap">
      <table class="park-code-table">
        <thead>
          <tr>
            <th>Mã KVC con</th>
            <th>Tên KVC con</th>
            <th>Mã KVC cha</th>
            <th>Tên KVC cha</th>
            <th>Mã loại vé</th>
            <th>Nhóm loại vé</th>
            <th>Đơn giá vốn</th>
            <th>Trạng thái</th>
            <th style="width: 48px"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="9">Đang tải...</td>
          </tr>
          <tr v-for="ticket in ticketTypes" :key="ticket.id">
            <td class="cell-muted">{{ ticket.code }}</td>
            <td>{{ ticket.name }}</td>
            <td class="cell-muted">{{ ticket.parkCode }}</td>
            <td class="cell-strong">{{ ticket.parkName }}</td>
            <td>{{ ticket.ticketTypeCode }}</td>
            <td>{{ ticket.ticketGroupName || '-' }}</td>
            <td class="amount">{{ formatMoney(ticket.costPrice) }}</td>
            <td><span class="badge" :class="badgeClassForStatus(ticket.status)">{{ recordStatusLabel(ticket.status) }}</span></td>
            <td>
              <button class="edit-btn" type="button" title="Sửa" @click="openEditTicket(ticket)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
              </button>
              <button class="inactive-btn" type="button" title="Ngừng sử dụng" @click="inactiveTicket(ticket.id)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M10 5v14M14 5v14" />
                </svg>
              </button>
              <button v-if="isAdmin" class="delete-btn" type="button" title="Xóa" @click="softDeleteTicket(ticket.id)">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </td>
          </tr>
          <tr v-if="!loading && ticketTypes.length === 0">
            <td colspan="9" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pg-info">Tổng {{ currentTotal }} dòng</span>
      <button class="pg-btn" type="button" :disabled="currentPage <= 1" @click="goPage(currentPage - 1)">‹</button>
      <button class="pg-btn active" type="button">{{ currentPage }}</button>
      <button class="pg-btn" type="button" :disabled="currentPage >= totalPages" @click="goPage(currentPage + 1)">›</button>
    </div>
  </div>

  <div v-if="showParkModal" class="modal-overlay park-code-modal-overlay">
    <div class="modal park-code-modal">
      <div class="modal-header">
        <span class="modal-title">{{ editingParkId ? 'Sửa khu vui chơi' : 'Thêm khu vui chơi' }}</span>
        <button class="modal-close" type="button" @click="showParkModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mã KVC</label>
            <input v-model="parkForm.code" class="form-input" :readonly="Boolean(editingParkId)" />
          </div>
          <div class="form-group">
            <label class="form-label">Tên khu vui chơi</label>
            <input v-model="parkForm.name" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Loại thanh toán</label>
            <select v-model="parkForm.paymentType" class="form-select">
              <option value="Prepaid">Nạp trước</option>
              <option value="Debt">Công nợ</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Trạng thái</label>
            <select v-model="parkForm.status" class="form-select">
              <option value="Active">Hoạt động</option>
              <option value="Inactive">Ngừng sử dụng</option>
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Địa điểm</label>
            <input v-model="parkForm.location" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Mã tìm kiếm/API alias</label>
            <input v-model="parkForm.searchCode" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Ngân hàng</label>
            <input v-model="parkForm.bankName" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Tài khoản</label>
            <input v-model="parkForm.bankAccount" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Site ID</label>
            <input v-model="parkForm.apiSiteId" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Profile ID</label>
            <input v-model="parkForm.apiProfileId" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Hạn mức công nợ</label>
            <input v-model="parkForm.creditLimit" class="form-input" inputmode="numeric" />
          </div>
          <div class="form-group">
            <label class="form-label">Quy tắc số dư</label>
            <select v-model="parkForm.balanceTransformRule" class="form-select">
              <option value="None">Giữ nguyên số dư</option>
              <option value="MultiplyMinusOne">Đảo dấu số dư (× -1)</option>
            </select>
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Ghi chú API</label>
          <textarea v-model="parkForm.apiNote" class="form-textarea"></textarea>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-cancel" type="button" @click="showParkModal = false">Hủy</button>
        <button class="btn-save" type="button" :disabled="saving" @click="savePark">Lưu thay đổi</button>
      </div>
    </div>
  </div>

  <div v-if="showTicketModal" class="modal-overlay park-code-modal-overlay">
    <div class="modal park-code-modal">
      <div class="modal-header">
        <span class="modal-title">{{ editingTicketId ? 'Sửa loại vé' : 'Thêm loại vé' }}</span>
        <button class="modal-close" type="button" @click="showTicketModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label class="form-label">KVC cha</label>
          <select v-model="ticketForm.parkId" class="form-select">
            <option value="">Chọn KVC</option>
            <option v-for="park in parkOptions" :key="park.id" :value="park.id">
              {{ park.code }} - {{ park.name }}
            </option>
          </select>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mã dòng/KVC con</label>
            <input v-model="ticketForm.code" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Mã loại vé</label>
            <input v-model="ticketForm.ticketTypeCode" class="form-input" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Tên loại vé</label>
          <input v-model="ticketForm.name" class="form-input" />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Nhóm vé</label>
            <input v-model="ticketForm.ticketGroupName" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Giá vốn</label>
            <input v-model="ticketForm.costPrice" class="form-input" inputmode="numeric" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Trạng thái</label>
          <select v-model="ticketForm.status" class="form-select">
            <option value="Active">Hoạt động</option>
            <option value="Inactive">Ngừng sử dụng</option>
          </select>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-cancel" type="button" @click="showTicketModal = false">Hủy</button>
        <button class="btn-save" type="button" :disabled="saving" @click="saveTicket">Lưu thay đổi</button>
      </div>
    </div>
  </div>
  </div>
</template>
