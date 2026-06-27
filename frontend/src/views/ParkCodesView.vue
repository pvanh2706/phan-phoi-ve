<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
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

type TabKey = 'parks' | 'tickets'

const activeTab = ref<TabKey>('parks')
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const message = ref('')

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
  balanceTransformRule: '',
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
  const result = await listParkTicketTypes({
    page: ticketPage.value,
    keyword: filters.keyword,
    paymentType: filters.paymentType,
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
  error.value = ''
  try {
    if (activeTab.value === 'parks') {
      await loadParks()
    } else {
      await loadTicketTypes()
    }
  } catch (err) {
    error.value = errorText(err, 'Không tải được dữ liệu KVC.')
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
    balanceTransformRule: '',
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
    balanceTransformRule: park.balanceTransformRule ?? '',
    apiNote: park.apiNote ?? '',
    status: park.status,
  })
  showParkModal.value = true
}

async function savePark() {
  if (saving.value) return
  saving.value = true
  error.value = ''
  message.value = ''
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
      message.value = 'Đã cập nhật khu vui chơi.'
    } else {
      await createPark(payload)
      message.value = 'Đã tạo khu vui chơi.'
    }
    showParkModal.value = false
    await loadParks()
  } catch (err) {
    error.value = errorText(err, 'Không lưu được khu vui chơi.')
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
  error.value = ''
  message.value = ''
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
      message.value = 'Đã cập nhật loại vé.'
    } else {
      await createParkTicketType(payload)
      message.value = 'Đã tạo loại vé.'
    }
    showTicketModal.value = false
    await loadTicketTypes()
  } catch (err) {
    error.value = errorText(err, 'Không lưu được loại vé.')
  } finally {
    saving.value = false
  }
}

async function inactivePark(id: number) {
  if (!confirm('Ngừng sử dụng khu vui chơi này?')) return
  await setParkInactive(id)
  message.value = 'Đã ngừng sử dụng khu vui chơi.'
  await loadParks()
}

async function softDeletePark(id: number) {
  if (!confirm('Xóa mềm khu vui chơi này?')) return
  await deletePark(id)
  message.value = 'Đã xóa mềm khu vui chơi.'
  await loadParks()
}

async function inactiveTicket(id: number) {
  if (!confirm('Ngừng sử dụng loại vé này?')) return
  await setParkTicketTypeInactive(id)
  message.value = 'Đã ngừng sử dụng loại vé.'
  await loadTicketTypes()
}

async function softDeleteTicket(id: number) {
  if (!confirm('Xóa mềm loại vé này?')) return
  await deleteParkTicketType(id)
  message.value = 'Đã xóa mềm loại vé.'
  await loadTicketTypes()
}

function goPage(page: number) {
  if (activeTab.value === 'parks') {
    parkPage.value = page
  } else {
    ticketPage.value = page
  }
  void load()
}

watch(activeTab, () => {
  message.value = ''
  error.value = ''
})

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
      KVC cha
    </button>
    <button class="tab-btn" :class="{ active: activeTab === 'tickets' }" type="button" @click="switchTab('tickets')">
      Loại vé / KVC con
    </button>
  </div>

  <div class="card">
    <div class="toolbar">
      <input
        v-model="filters.keyword"
        class="tb-input"
        :placeholder="activeTab === 'parks' ? '🔍  Tìm mã hoặc tên KVC...' : '🔍  Tìm mã hoặc tên KVC con...'"
        @keyup.enter="load"
      />
      <select v-model="filters.paymentType" class="tb-select" @change="load">
        <option value="">Tất cả loại thanh toán</option>
        <option value="Prepaid">Nạp trước</option>
        <option value="Debt">Công nợ</option>
      </select>
      <select v-model="filters.status" class="tb-select" @change="load">
        <option value="">Tất cả trạng thái</option>
        <option value="Active">Hoạt động</option>
        <option value="Inactive">Ngừng sử dụng</option>
      </select>
      <select v-if="activeTab === 'tickets'" v-model="filters.parkId" class="tb-select" @change="load">
        <option value="">Tất cả KVC cha</option>
        <option v-for="park in parkOptions" :key="park.id" :value="park.id">
          {{ park.code }} - {{ park.name }}
        </option>
      </select>
      <button class="btn-secondary" type="button" @click="resetFilters">Xóa lọc</button>
      <button class="add-btn" type="button" @click="activeTab === 'parks' ? openAddPark() : openAddTicket()">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" d="M12 5v14M5 12h14" />
        </svg>
        {{ activeTab === 'parks' ? 'Thêm KVC' : 'Thêm KVC con' }}
      </button>
    </div>

    <div v-if="message" class="notice notice-indigo" style="margin-bottom: 14px">{{ message }}</div>
    <div v-if="error" class="notice notice-blue" style="margin-bottom: 14px">{{ error }}</div>

    <div v-if="activeTab === 'parks'" class="table-wrap park-code-table-wrap">
      <table class="park-code-table">
        <thead>
          <tr>
            <th>Mã KVC</th>
            <th>Tên khu vui chơi</th>
            <th>Loại</th>
            <th>Địa điểm</th>
            <th>Tài khoản</th>
            <th>API</th>
            <th>Trạng thái</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="8">Đang tải...</td>
          </tr>
          <tr v-for="park in parks" :key="park.id">
            <td class="cell-muted">{{ park.code }}</td>
            <td class="cell-strong">{{ park.name }}</td>
            <td><span class="badge badge-indigo">{{ paymentTypeLabel(park.paymentType) }}</span></td>
            <td>{{ park.location || '-' }}</td>
            <td class="cell-muted">{{ park.bankName || '-' }} {{ park.bankAccount ? `- ${park.bankAccount}` : '' }}</td>
            <td class="cell-muted">{{ park.apiSiteId || '-' }} {{ park.apiProfileId ? `/ ${park.apiProfileId}` : '' }}</td>
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
            <td colspan="8" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-else class="table-wrap park-code-table-wrap">
      <table class="park-code-table">
        <thead>
          <tr>
            <th>KVC cha</th>
            <th>Mã dòng</th>
            <th>Mã loại vé</th>
            <th>Tên loại vé</th>
            <th>Nhóm vé</th>
            <th>Giá vốn</th>
            <th>Trạng thái</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="8">Đang tải...</td>
          </tr>
          <tr v-for="ticket in ticketTypes" :key="ticket.id">
            <td class="cell-strong">{{ ticket.parkCode }} - {{ ticket.parkName }}</td>
            <td class="cell-muted">{{ ticket.code }}</td>
            <td>{{ ticket.ticketTypeCode }}</td>
            <td>{{ ticket.name }}</td>
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
            <td colspan="8" class="cell-muted" style="text-align: center">Không có dữ liệu phù hợp</td>
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

  <div v-if="showParkModal" class="modal-overlay park-code-modal-overlay" @click.self="showParkModal = false">
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
            <input v-model="parkForm.balanceTransformRule" class="form-input" placeholder="None, MultiplyMinusOne..." />
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

  <div v-if="showTicketModal" class="modal-overlay park-code-modal-overlay" @click.self="showTicketModal = false">
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
