<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '../components/ui/PageHeader.vue'
import { ApiClientError } from '../services/apiClient'
import { authState, logout, setCurrentUser } from '../services/authStore'
import { useTheme, type ThemeMode } from '../composables/useTheme'
import {
  changePassword,
  getPreferences,
  getProfile,
  listSessions,
  revokeAllSessions,
  revokeSession,
  updatePreferences,
  updateProfile,
  uploadAvatar,
  type ProfileDto,
  type UserSessionDto,
} from '../services/meApi'
import {
  createUser,
  disableUser,
  enableUser,
  listUsers,
  resetUserPassword,
  unlockUser,
  updateUser,
} from '../services/usersApi'
import {
  createNotificationRecipient,
  deactivateNotificationRecipient,
  listNotificationRecipients,
  updateNotificationRecipient,
  type NotificationRecipientDto,
} from '../services/notificationsApi'
import {
  badgeClassForStatus,
  externalApiSourceLabel,
  formatDate,
  formatDateTime,
  formatDateTimeMs,
  notificationTypeLabel,
  userRoleLabel,
  userStatusLabel,
  type NotificationType,
  type UserRole,
  type UserStatus,
} from '../services/formatters'
import { resetAllData, RESET_CONFIRM_PHRASE } from '../services/systemApi'
import {
  listExternalApiLogs,
  getExternalApiLog,
  type ExternalApiLogDto,
  type ExternalApiLogDetailDto,
  type ExternalApiLogFilters,
} from '../services/externalApiLogsApi'

type TabKey = 'profile' | 'theme' | 'password' | 'sessions' | 'users' | 'recipients' | 'reset' | 'apilogs'

const router = useRouter()
const { mode, setMode } = useTheme()
const activeTab = ref<TabKey>('profile')
const loading = ref(false)
const error = ref('')
const message = ref('')
const avatarInput = ref<HTMLInputElement | null>(null)

const profile = reactive({
  fullName: '',
  phoneNumber: '',
  email: '',
  role: '' as UserRole | '',
})

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const sessions = ref<UserSessionDto[]>([])
const users = ref<ProfileDto[]>([])
const recipients = ref<NotificationRecipientDto[]>([])
const userKeyword = ref('')

const userModal = reactive({
  open: false,
  id: null as number | null,
  fullName: '',
  email: '',
  phoneNumber: '',
  role: 'Member' as UserRole,
  status: 'Active' as UserStatus,
  password: '',
})

const passwordModal = reactive({
  open: false,
  userId: 0,
  fullName: '',
  newPassword: '',
})

const recipientModal = reactive({
  open: false,
  id: null as number | null,
  notificationType: 'SyncErrorSummary' as NotificationType,
  email: '',
  displayName: '',
  isActive: true,
})

const isAdmin = computed(() => authState.user?.role === 'Admin')
const tabs = computed(() => {
  const base = [
    { id: 'profile', label: 'Thông tin tài khoản', icon: '👤' },
    { id: 'theme', label: 'Giao diện', icon: '🎨' },
    { id: 'password', label: 'Đổi mật khẩu', icon: '🔒' },
    { id: 'sessions', label: 'Quản lý thiết bị', icon: '💻' },
  ] as { id: TabKey; label: string; icon: string }[]

  if (isAdmin.value) {
    base.push({ id: 'users', label: 'Quản lý người dùng', icon: '👥' })
    base.push({ id: 'recipients', label: 'Email nhận lỗi', icon: '🔔' })
    base.push({ id: 'apilogs', label: 'Log gọi API', icon: '📡' })
    base.push({ id: 'reset', label: 'Xóa dữ liệu', icon: '🗑️' })
  }

  return base
})

const themeOptions: { id: ThemeMode; label: string; preview: string; icon: string }[] = [
  { id: 'dark', label: 'Tối', preview: 'theme-preview-dark', icon: '🌙' },
  { id: 'light', label: 'Sáng', preview: 'theme-preview-light', icon: '☀️' },
  { id: 'system', label: 'Hệ thống', preview: 'theme-preview-system', icon: '💻' },
]

function errorText(err: unknown, fallback: string) {
  return err instanceof ApiClientError ? err.message : fallback
}

function clearNotice() {
  error.value = ''
  message.value = ''
}

function initials(name: string) {
  return (name || 'U')
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
}

function apiThemeToUi(value: 'Dark' | 'Light' | 'System'): ThemeMode {
  if (value === 'Dark') return 'dark'
  if (value === 'Light') return 'light'
  return 'system'
}

function uiThemeToApi(value: ThemeMode): 'Dark' | 'Light' | 'System' {
  if (value === 'dark') return 'Dark'
  if (value === 'light') return 'Light'
  return 'System'
}

async function loadProfile() {
  const data = await getProfile()
  profile.fullName = data.fullName
  profile.phoneNumber = data.phoneNumber ?? ''
  profile.email = data.email
  profile.role = data.role
}

async function saveProfile() {
  loading.value = true
  clearNotice()
  try {
    const data = await updateProfile({
      fullName: profile.fullName,
      phoneNumber: profile.phoneNumber || null,
    })
    if (authState.user) {
      setCurrentUser({
        ...authState.user,
        fullName: data.fullName,
        phoneNumber: data.phoneNumber,
      })
    }
    message.value = 'Đã cập nhật thông tin tài khoản.'
  } catch (err) {
    error.value = errorText(err, 'Không lưu được thông tin tài khoản.')
  } finally {
    loading.value = false
  }
}

async function handleAvatarChange(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) return
  loading.value = true
  clearNotice()
  try {
    const result = await uploadAvatar(file)
    if (authState.user) {
      setCurrentUser({ ...authState.user, avatarPath: result.avatarPath })
    }
    message.value = 'Đã cập nhật avatar.'
  } catch (err) {
    error.value = errorText(err, 'Không upload được avatar.')
  } finally {
    loading.value = false
  }
}

async function loadPreferences() {
  const data = await getPreferences()
  setMode(apiThemeToUi(data.themeMode))
}

async function saveTheme(value: ThemeMode) {
  setMode(value)
  clearNotice()
  try {
    await updatePreferences({ themeMode: uiThemeToApi(value), language: 'vi-VN' })
    message.value = 'Đã lưu giao diện.'
  } catch (err) {
    error.value = errorText(err, 'Không lưu được giao diện.')
  }
}

async function savePassword() {
  clearNotice()
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    error.value = 'Mật khẩu mới nhập lại không khớp.'
    return
  }

  loading.value = true
  try {
    await changePassword({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    })
    passwordForm.currentPassword = ''
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
    message.value = 'Đã đổi mật khẩu.'
  } catch (err) {
    error.value = errorText(err, 'Không đổi được mật khẩu.')
  } finally {
    loading.value = false
  }
}

async function loadSessions() {
  sessions.value = await listSessions()
}

async function revokeDevice(session: UserSessionDto) {
  await revokeSession(session.id)
  if (session.isCurrent) {
    await logout()
    await router.replace('/login')
    return
  }
  await loadSessions()
  message.value = 'Đã đăng xuất thiết bị.'
}

async function revokeAllDevices() {
  await revokeAllSessions()
  await logout()
  await router.replace('/login')
}

async function loadUsers() {
  if (!isAdmin.value) return
  const result = await listUsers({ page: 1, keyword: userKeyword.value })
  users.value = result.items
}

function openUserModal(user?: ProfileDto) {
  userModal.open = true
  userModal.id = user?.id ?? null
  userModal.fullName = user?.fullName ?? ''
  userModal.email = user?.email ?? ''
  userModal.phoneNumber = user?.phoneNumber ?? ''
  userModal.role = user?.role ?? 'Member'
  userModal.status = user?.status ?? 'Active'
  userModal.password = ''
}

async function saveUser() {
  loading.value = true
  clearNotice()
  try {
    if (userModal.id) {
      await updateUser(userModal.id, {
        fullName: userModal.fullName,
        phoneNumber: userModal.phoneNumber || null,
        role: userModal.role,
        status: userModal.status,
      })
      message.value = 'Đã cập nhật người dùng.'
    } else {
      await createUser({
        fullName: userModal.fullName,
        email: userModal.email,
        phoneNumber: userModal.phoneNumber || null,
        role: userModal.role,
        password: userModal.password,
      })
      message.value = 'Đã tạo người dùng.'
    }
    userModal.open = false
    await loadUsers()
  } catch (err) {
    error.value = errorText(err, 'Không lưu được người dùng.')
  } finally {
    loading.value = false
  }
}

function openPasswordModal(user: ProfileDto) {
  passwordModal.open = true
  passwordModal.userId = user.id
  passwordModal.fullName = user.fullName
  passwordModal.newPassword = ''
}

async function saveResetPassword() {
  loading.value = true
  clearNotice()
  try {
    await resetUserPassword(passwordModal.userId, passwordModal.newPassword)
    passwordModal.open = false
    message.value = 'Đã đặt lại mật khẩu.'
    await loadUsers()
  } catch (err) {
    error.value = errorText(err, 'Không đặt lại được mật khẩu.')
  } finally {
    loading.value = false
  }
}

async function toggleUserStatus(user: ProfileDto) {
  clearNotice()
  if (user.status === 'Inactive') {
    await enableUser(user.id)
    message.value = 'Đã kích hoạt người dùng.'
  } else {
    await disableUser(user.id)
    message.value = 'Đã vô hiệu hóa người dùng.'
  }
  await loadUsers()
}

async function unlock(user: ProfileDto) {
  clearNotice()
  await unlockUser(user.id)
  message.value = 'Đã mở khóa người dùng.'
  await loadUsers()
}

async function loadRecipients() {
  if (!isAdmin.value) return
  const result = await listNotificationRecipients({ page: 1 })
  recipients.value = result.items
}

function openRecipientModal(recipient?: NotificationRecipientDto) {
  recipientModal.open = true
  recipientModal.id = recipient?.id ?? null
  recipientModal.notificationType = recipient?.notificationType ?? 'SyncErrorSummary'
  recipientModal.email = recipient?.email ?? ''
  recipientModal.displayName = recipient?.displayName ?? ''
  recipientModal.isActive = recipient?.isActive ?? true
}

async function saveRecipient() {
  loading.value = true
  clearNotice()
  try {
    const payload = {
      notificationType: recipientModal.notificationType,
      email: recipientModal.email,
      displayName: recipientModal.displayName || null,
      isActive: recipientModal.isActive,
    }
    if (recipientModal.id) {
      await updateNotificationRecipient(recipientModal.id, payload)
      message.value = 'Đã cập nhật email nhận lỗi.'
    } else {
      await createNotificationRecipient(payload)
      message.value = 'Đã thêm email nhận lỗi.'
    }
    recipientModal.open = false
    await loadRecipients()
  } catch (err) {
    error.value = errorText(err, 'Không lưu được email nhận lỗi.')
  } finally {
    loading.value = false
  }
}

async function deactivateRecipient(id: number) {
  clearNotice()
  await deactivateNotificationRecipient(id)
  message.value = 'Đã ngừng gửi email cho dòng này.'
  await loadRecipients()
}

// ── Log gọi API ngoài (chỉ Admin) ──
const apiLogs = ref<ExternalApiLogDto[]>([])
const apiLogTotal = ref(0)
const apiLogPage = ref(1)
const apiLogDetail = ref<ExternalApiLogDetailDto | null>(null)
const apiLogFilters = reactive<ExternalApiLogFilters>({
  source: '',
  isSuccess: '',
  keyword: '',
  fromDate: '',
  toDate: '',
})

async function loadApiLogs() {
  if (!isAdmin.value) return
  loading.value = true
  clearNotice()
  try {
    const result = await listExternalApiLogs({ ...apiLogFilters, page: apiLogPage.value })
    apiLogs.value = result.items
    apiLogTotal.value = result.totalItems
  } catch (err) {
    error.value = errorText(err, 'Không tải được log gọi API.')
  } finally {
    loading.value = false
  }
}

function applyApiLogFilters() {
  apiLogPage.value = 1
  void loadApiLogs()
}

function resetApiLogFilters() {
  apiLogFilters.source = ''
  apiLogFilters.isSuccess = ''
  apiLogFilters.keyword = ''
  apiLogFilters.fromDate = ''
  apiLogFilters.toDate = ''
  apiLogPage.value = 1
  void loadApiLogs()
}

function goApiLogPage(next: number) {
  if (next < 1 || (next - 1) * 100 >= apiLogTotal.value) return
  apiLogPage.value = next
  void loadApiLogs()
}

async function openApiLogDetail(id: number) {
  clearNotice()
  try {
    apiLogDetail.value = await getExternalApiLog(id)
  } catch (err) {
    error.value = errorText(err, 'Không tải được chi tiết log.')
  }
}

function prettyJson(value?: string | null) {
  if (!value) return '—'
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

// ── Xóa toàn bộ dữ liệu (chỉ Admin) ──
const resetConfirmText = ref('')
const resetting = ref(false)
const canReset = computed(() => resetConfirmText.value.trim() === RESET_CONFIRM_PHRASE)

async function runReset() {
  if (!canReset.value || resetting.value) return
  if (!window.confirm(
    'Hành động này sẽ xóa VĨNH VIỄN toàn bộ dữ liệu nghiệp vụ (chỉ giữ lại tài khoản Admin '
    + 'và cấu hình hệ thống) và KHÔNG THỂ hoàn tác. Bạn chắc chắn muốn tiếp tục?'
  )) return

  resetting.value = true
  clearNotice()
  try {
    const result = await resetAllData(resetConfirmText.value.trim())
    resetConfirmText.value = ''
    message.value = `Đã xóa ${result.totalDeleted.toLocaleString('vi-VN')} bản ghi, `
      + `giữ lại ${result.keptAdminCount} tài khoản Admin và cấu hình hệ thống.`
  } catch (err) {
    error.value = errorText(err, 'Không xóa được dữ liệu.')
  } finally {
    resetting.value = false
  }
}

watch(activeTab, async () => {
  clearNotice()
  if (activeTab.value === 'sessions') await loadSessions()
  if (activeTab.value === 'users') await loadUsers()
  if (activeTab.value === 'recipients') await loadRecipients()
  if (activeTab.value === 'apilogs') await loadApiLogs()
})

watch(isAdmin, (value) => {
  if (!value && ['users', 'recipients', 'apilogs', 'reset'].includes(activeTab.value)) {
    activeTab.value = 'profile'
  }
})

onMounted(async () => {
  loading.value = true
  try {
    await loadProfile()
    await loadPreferences().catch(() => undefined)
  } catch (err) {
    error.value = errorText(err, 'Không tải được cài đặt tài khoản.')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <PageHeader title="Cấu hình hệ thống" subtitle="Quản lý tài khoản, giao diện, thiết bị, người dùng và email nhận lỗi đồng bộ" />

  <div class="sys-tabs">
    <button
      v-for="tab in tabs"
      :key="tab.id"
      class="sys-tab"
      :class="{ active: activeTab === tab.id }"
      type="button"
      @click="activeTab = tab.id"
    >
      <span class="sys-tab-icon">{{ tab.icon }}</span>
      {{ tab.label }}
    </button>
  </div>

  <div v-if="message" class="notice notice-indigo" style="margin-bottom: 16px">{{ message }}</div>
  <div v-if="error" class="notice notice-blue" style="margin-bottom: 16px">{{ error }}</div>

  <section v-if="activeTab === 'profile'" class="card">
    <div class="form-row">
      <div class="form-group">
        <label class="form-label">Ảnh đại diện</label>
        <div class="avatar-lg" @click="avatarInput?.click()">
          <img
            v-if="authState.user?.avatarPath"
            :src="authState.user.avatarPath"
            alt=""
            style="position: absolute; inset: 0; width: 100%; height: 100%; border-radius: 50%; object-fit: cover"
          />
          <span v-else>{{ initials(profile.fullName) }}</span>
          <span class="avatar-camera">📷</span>
        </div>
        <input ref="avatarInput" type="file" accept="image/png,image/jpeg,image/webp" style="display: none" @change="handleAvatarChange" />
      </div>
      <div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Họ và tên</label>
            <input v-model="profile.fullName" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Số điện thoại</label>
            <input v-model="profile.phoneNumber" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Email</label>
            <input v-model="profile.email" class="form-input" readonly />
          </div>
          <div class="form-group">
            <label class="form-label">Vai trò</label>
            <input class="form-input" :value="userRoleLabel(profile.role)" readonly />
          </div>
        </div>
        <button class="btn-primary" type="button" :disabled="loading" @click="saveProfile">Lưu thay đổi</button>
      </div>
    </div>
  </section>

  <section v-if="activeTab === 'theme'" class="card">
    <div class="theme-btns">
      <button
        v-for="theme in themeOptions"
        :key="theme.id"
        class="theme-opt"
        :class="{ selected: mode === theme.id }"
        type="button"
        @click="saveTheme(theme.id)"
      >
        <div class="theme-preview" :class="theme.preview">
          <div class="tp-sidebar"></div>
          <div class="tp-content">
            <div class="tp-bar"></div>
            <div class="tp-bar tp-bar-short"></div>
          </div>
        </div>
        <div class="theme-opt-label">
          <span class="theme-opt-icon">{{ theme.icon }}</span>
          {{ theme.label }}
          <span v-if="mode === theme.id" class="theme-check">✓</span>
        </div>
      </button>
    </div>
  </section>

  <section v-if="activeTab === 'password'" class="card">
    <div class="form-row">
      <div class="form-group">
        <label class="form-label">Mật khẩu hiện tại</label>
        <input v-model="passwordForm.currentPassword" class="form-input" type="password" autocomplete="current-password" />
      </div>
      <div class="form-group">
        <label class="form-label">Mật khẩu mới</label>
        <input v-model="passwordForm.newPassword" class="form-input" type="password" autocomplete="new-password" />
      </div>
    </div>
    <div class="form-group">
      <label class="form-label">Nhập lại mật khẩu mới</label>
      <input v-model="passwordForm.confirmPassword" class="form-input" type="password" autocomplete="new-password" />
    </div>
    <button class="btn-primary" type="button" :disabled="loading" @click="savePassword">Cập nhật mật khẩu</button>
  </section>

  <section v-if="activeTab === 'sessions'" class="device-list">
    <div class="toolbar">
      <button class="btn-secondary" type="button" @click="loadSessions">Tải lại</button>
      <button class="btn-danger" type="button" @click="revokeAllDevices">Đăng xuất tất cả thiết bị</button>
    </div>
    <div v-for="session in sessions" :key="session.id" class="device-item">
      <div class="device-icon">🖥️</div>
      <div>
        <div class="device-name">
          {{ session.deviceName || 'Thiết bị đăng nhập' }}
          <span v-if="session.isCurrent" class="badge badge-green" style="margin-left: 6px">Hiện tại</span>
        </div>
        <div class="device-meta">
          IP {{ session.lastUsedIp || session.createdByIp || '-' }} - tạo {{ formatDateTime(session.createdAtUtc) }} - hết hạn {{ formatDateTime(session.expiresAtUtc) }}
        </div>
      </div>
      <button v-if="session.isActive" class="btn-secondary" type="button" style="margin-left: auto" @click="revokeDevice(session)">
        Đăng xuất
      </button>
    </div>
    <div v-if="sessions.length === 0" class="notice notice-indigo">Chưa có phiên đăng nhập.</div>
  </section>

  <section v-if="activeTab === 'users' && isAdmin" class="card">
    <div class="toolbar">
      <input v-model="userKeyword" class="tb-input" placeholder="Tìm tên, email, số điện thoại..." @keyup.enter="loadUsers" />
      <button class="btn-secondary" type="button" @click="loadUsers">Tìm</button>
      <button class="add-btn" type="button" @click="openUserModal()">Thêm người dùng</button>
    </div>
    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Họ tên</th>
            <th>Email</th>
            <th>SĐT</th>
            <th>Vai trò</th>
            <th>Trạng thái</th>
            <th>Sai mật khẩu</th>
            <th>Đăng nhập gần nhất</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id">
            <td class="cell-strong">{{ user.fullName }}</td>
            <td class="cell-muted">{{ user.email }}</td>
            <td>{{ user.phoneNumber || '-' }}</td>
            <td>{{ userRoleLabel(user.role) }}</td>
            <td><span class="badge" :class="badgeClassForStatus(user.status)">{{ userStatusLabel(user.status) }}</span></td>
            <td>{{ user.failedLoginCount }}</td>
            <td>{{ formatDateTime(user.lastLoginAtUtc) }}</td>
            <td>
              <button class="act-btn" type="button" @click="openUserModal(user)">✏️ Sửa</button>
              <button class="act-btn" type="button" @click="openPasswordModal(user)">🔑 Reset pass</button>
              <button v-if="user.status === 'Locked'" class="act-btn" type="button" @click="unlock(user)">🔓 Mở khóa</button>
              <button class="act-btn" type="button" @click="toggleUserStatus(user)">
                {{ user.status === 'Inactive' ? '▶️' : '⏸️' }}
                {{ user.status === 'Inactive' ? 'Kích hoạt' : 'Vô hiệu' }}
              </button>
            </td>
          </tr>
          <tr v-if="users.length === 0">
            <td colspan="8" class="cell-muted" style="text-align: center">Chưa có người dùng phù hợp</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <section v-if="activeTab === 'recipients' && isAdmin" class="card">
    <div class="toolbar">
      <button class="btn-secondary" type="button" @click="loadRecipients">Tải lại</button>
      <button class="add-btn" type="button" @click="openRecipientModal()">Thêm email</button>
    </div>
    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Loại thông báo</th>
            <th>Email</th>
            <th>Tên hiển thị</th>
            <th>Trạng thái</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="recipient in recipients" :key="recipient.id">
            <td>{{ notificationTypeLabel(recipient.notificationType) }}</td>
            <td class="cell-strong">{{ recipient.email }}</td>
            <td>{{ recipient.displayName || '-' }}</td>
            <td><span class="badge" :class="recipient.isActive ? 'badge-green' : 'badge-gray'">{{ recipient.isActive ? 'Đang nhận' : 'Ngừng nhận' }}</span></td>
            <td>
              <button class="act-btn" type="button" @click="openRecipientModal(recipient)">✏️ Sửa</button>
              <button v-if="recipient.isActive" class="act-btn" type="button" @click="deactivateRecipient(recipient.id)">⏸️ Ngừng</button>
            </td>
          </tr>
          <tr v-if="recipients.length === 0">
            <td colspan="5" class="cell-muted" style="text-align: center">Chưa có email nhận lỗi</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <section v-if="activeTab === 'apilogs' && isAdmin" class="card">
    <div class="toolbar">
      <input v-model="apiLogFilters.fromDate" class="tb-date" type="date" />
      <input v-model="apiLogFilters.toDate" class="tb-date" type="date" />
      <select v-model="apiLogFilters.source" class="tb-select">
        <option value="">Tất cả nguồn</option>
        <option value="ParkBalance">Số dư KVC</option>
        <option value="TicketCost">Giá vốn vé</option>
        <option value="BankTransaction">Giao dịch ngân hàng</option>
      </select>
      <select v-model="apiLogFilters.isSuccess" class="tb-select">
        <option :value="''">Tất cả trạng thái</option>
        <option :value="true">Thành công</option>
        <option :value="false">Lỗi</option>
      </select>
      <input v-model="apiLogFilters.keyword" class="tb-input" placeholder="Tìm URL / lỗi..." @keyup.enter="applyApiLogFilters" />
      <button class="btn-secondary" type="button" @click="applyApiLogFilters">Lọc</button>
      <button class="btn-secondary" type="button" @click="resetApiLogFilters">Xóa lọc</button>
    </div>

    <div class="table-wrap report-table-wrap">
      <table>
        <thead>
          <tr>
            <th>Thời điểm</th>
            <th>Mã chạy</th>
            <th>Nguồn</th>
            <th>Ngày DL</th>
            <th>KVC</th>
            <th>HTTP</th>
            <th>Trạng thái</th>
            <th class="td-num">Thời gian</th>
            <th>Chi tiết</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="9">Đang tải...</td></tr>
          <tr v-for="log in apiLogs" :key="log.id">
            <td class="cell-muted">{{ formatDateTimeMs(log.receivedAtUtc) }}</td>
            <td>{{ log.jobRunId != null ? '#' + log.jobRunId : '—' }}</td>
            <td>{{ externalApiSourceLabel(log.source) }}</td>
            <td>{{ log.businessDate ? formatDate(log.businessDate) : '—' }}</td>
            <td>{{ log.parkName ?? '—' }}</td>
            <td>{{ log.responseStatusCode ?? '—' }}</td>
            <td>
              <span class="badge" :class="log.isSuccess ? 'badge-green' : 'badge-red'">
                {{ log.isSuccess ? 'Thành công' : 'Lỗi' }}
              </span>
            </td>
            <td class="td-num cell-muted">{{ log.durationMs != null ? log.durationMs + ' ms' : '—' }}</td>
            <td><button class="act-btn" type="button" @click="openApiLogDetail(log.id)">🔍 Xem</button></td>
          </tr>
          <tr v-if="!loading && apiLogs.length === 0">
            <td colspan="9" class="cell-muted" style="text-align: center">Chưa có log gọi API phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pg-info">Tổng {{ apiLogTotal }} dòng</span>
      <button class="pg-btn" type="button" :disabled="apiLogPage <= 1" @click="goApiLogPage(apiLogPage - 1)">‹</button>
      <button class="pg-btn active" type="button">{{ apiLogPage }}</button>
      <button class="pg-btn" type="button" :disabled="apiLogPage * 100 >= apiLogTotal" @click="goApiLogPage(apiLogPage + 1)">›</button>
    </div>
  </section>

  <section v-if="activeTab === 'reset' && isAdmin" class="card">
    <div class="notice notice-blue" style="margin-bottom: 16px">
      <strong>⚠️ Vùng nguy hiểm.</strong> Thao tác này xóa <strong>toàn bộ dữ liệu</strong>
      (khu vui chơi, loại vé, giao dịch ngân hàng, đối soát, giá vốn, công việc, nhật ký hệ thống,
      và các tài khoản người dùng khác). Hệ thống chỉ giữ lại <strong>tài khoản Admin</strong> và
      <strong>cấu hình hệ thống</strong>. Dữ liệu đã xóa <strong>không thể khôi phục</strong>.
    </div>
    <div class="form-group">
      <label class="form-label">
        Để xác nhận, hãy gõ chính xác: <code class="reset-phrase">{{ RESET_CONFIRM_PHRASE }}</code>
      </label>
      <input
        v-model="resetConfirmText"
        class="form-input"
        :placeholder="RESET_CONFIRM_PHRASE"
        autocomplete="off"
        spellcheck="false"
      />
    </div>
    <button class="btn-danger" type="button" :disabled="!canReset || resetting" @click="runReset">
      {{ resetting ? 'Đang xóa...' : '🗑️ Xóa toàn bộ dữ liệu' }}
    </button>
  </section>

  <div v-if="userModal.open" class="modal-overlay" @click.self="userModal.open = false">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">{{ userModal.id ? 'Sửa người dùng' : 'Thêm người dùng' }}</span>
        <button class="modal-close" type="button" @click="userModal.open = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Họ tên</label>
            <input v-model="userModal.fullName" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Số điện thoại</label>
            <input v-model="userModal.phoneNumber" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Email</label>
            <input v-model="userModal.email" class="form-input" type="email" :readonly="Boolean(userModal.id)" />
          </div>
          <div class="form-group">
            <label class="form-label">Vai trò</label>
            <select v-model="userModal.role" class="form-select">
              <option value="Admin">Quản trị</option>
              <option value="Member">Thành viên</option>
              <option value="Accountant">Kế toán</option>
            </select>
          </div>
        </div>
        <div class="form-row">
          <div v-if="!userModal.id" class="form-group">
            <label class="form-label">Mật khẩu</label>
            <input v-model="userModal.password" class="form-input" type="password" />
          </div>
          <div class="form-group">
            <label class="form-label">Trạng thái</label>
            <select v-model="userModal.status" class="form-select">
              <option value="Active">Hoạt động</option>
              <option value="Inactive">Vô hiệu hóa</option>
              <option value="Locked">Bị khóa</option>
            </select>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="userModal.open = false">Hủy</button>
        <button class="btn-primary" type="button" :disabled="loading" @click="saveUser">Lưu</button>
      </div>
    </div>
  </div>

  <div v-if="passwordModal.open" class="modal-overlay" @click.self="passwordModal.open = false">
    <div class="modal" style="width: 420px">
      <div class="modal-header">
        <span class="modal-title">Reset mật khẩu</span>
        <button class="modal-close" type="button" @click="passwordModal.open = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="notice notice-indigo" style="margin-bottom: 14px">{{ passwordModal.fullName }}</div>
        <div class="form-group">
          <label class="form-label">Mật khẩu mới</label>
          <input v-model="passwordModal.newPassword" class="form-input" type="password" />
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="passwordModal.open = false">Hủy</button>
        <button class="btn-primary" type="button" :disabled="loading" @click="saveResetPassword">Lưu</button>
      </div>
    </div>
  </div>

  <div v-if="recipientModal.open" class="modal-overlay" @click.self="recipientModal.open = false">
    <div class="modal" style="width: 460px">
      <div class="modal-header">
        <span class="modal-title">{{ recipientModal.id ? 'Sửa email nhận lỗi' : 'Thêm email nhận lỗi' }}</span>
        <button class="modal-close" type="button" @click="recipientModal.open = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label class="form-label">Loại thông báo</label>
          <select v-model="recipientModal.notificationType" class="form-select">
            <option value="SyncErrorSummary">Tổng hợp lỗi đồng bộ</option>
            <option value="DailyReport">Báo cáo ngày</option>
            <option value="ReconciliationVarianceAlert">Cảnh báo lệch đối soát</option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Email</label>
          <input v-model="recipientModal.email" class="form-input" type="email" />
        </div>
        <div class="form-group">
          <label class="form-label">Tên hiển thị</label>
          <input v-model="recipientModal.displayName" class="form-input" />
        </div>
        <label class="toggle">
          <input v-model="recipientModal.isActive" type="checkbox" />
          <span class="toggle-slider"></span>
        </label>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="recipientModal.open = false">Hủy</button>
        <button class="btn-primary" type="button" :disabled="loading" @click="saveRecipient">Lưu</button>
      </div>
    </div>
  </div>

  <div v-if="apiLogDetail" class="modal-overlay" @click.self="apiLogDetail = null">
    <div class="modal" style="width: 720px; max-width: 92vw">
      <div class="modal-header">
        <span class="modal-title">Chi tiết log gọi API #{{ apiLogDetail.id }}</span>
        <button class="modal-close" type="button" @click="apiLogDetail = null">✕</button>
      </div>
      <div class="modal-body">
        <div class="api-log-grid">
          <div><span class="detail-label">Nguồn</span>{{ externalApiSourceLabel(apiLogDetail.source) }}</div>
          <div><span class="detail-label">Trạng thái</span>
            <span class="badge" :class="apiLogDetail.isSuccess ? 'badge-green' : 'badge-red'">
              {{ apiLogDetail.isSuccess ? 'Thành công' : 'Lỗi' }}
            </span>
          </div>
          <div><span class="detail-label">KVC</span>{{ apiLogDetail.parkName ?? '—' }}</div>
          <div><span class="detail-label">Ngày dữ liệu</span>{{ apiLogDetail.businessDate ? formatDate(apiLogDetail.businessDate) : '—' }}</div>
          <div><span class="detail-label">HTTP status</span>{{ apiLogDetail.responseStatusCode ?? '—' }}</div>
          <div><span class="detail-label">Thời gian gọi</span>{{ apiLogDetail.durationMs != null ? apiLogDetail.durationMs + ' ms' : '—' }}</div>
          <div><span class="detail-label">Thời điểm</span>{{ formatDateTimeMs(apiLogDetail.receivedAtUtc) }}</div>
          <div><span class="detail-label">Job Run</span>{{ apiLogDetail.jobRunId ?? '—' }}</div>
        </div>

        <div class="form-group" style="margin-top: 14px">
          <label class="form-label">Request URL</label>
          <div class="api-log-url">{{ apiLogDetail.requestUrl ?? '—' }}</div>
        </div>

        <div v-if="apiLogDetail.errorMessage" class="form-group">
          <label class="form-label">Lỗi</label>
          <div class="notice notice-blue" style="margin: 0">{{ apiLogDetail.errorMessage }}</div>
        </div>

        <div class="form-group">
          <label class="form-label">Request payload</label>
          <pre class="api-log-json">{{ prettyJson(apiLogDetail.requestPayloadJson) }}</pre>
        </div>

        <div class="form-group" style="margin-bottom: 0">
          <label class="form-label">Response body</label>
          <pre class="api-log-json">{{ prettyJson(apiLogDetail.responseBodyJson) }}</pre>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="apiLogDetail = null">Đóng</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.reset-phrase {
  padding: 2px 8px;
  border-radius: 6px;
  background: rgba(239, 68, 68, 0.14);
  color: #f87171;
  font-weight: 700;
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  user-select: all;
}

.btn-danger:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.api-log-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px 24px;
  font-size: 13px;
}

.api-log-grid .detail-label {
  display: block;
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  opacity: 0.6;
  margin-bottom: 2px;
}

.api-log-url {
  word-break: break-all;
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 12px;
  padding: 8px 10px;
  border-radius: 8px;
  background: rgba(127, 127, 127, 0.12);
}

.api-log-json {
  margin: 0;
  max-height: 240px;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-word;
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 12px;
  line-height: 1.5;
  padding: 10px 12px;
  border-radius: 8px;
  background: rgba(127, 127, 127, 0.12);
}
</style>
