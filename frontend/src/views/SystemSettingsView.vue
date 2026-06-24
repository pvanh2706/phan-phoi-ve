<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import { useTheme, type ThemeMode } from '../composables/useTheme'

interface UserRecord {
  id: string
  name: string
  phone: string
  email: string
  dept: string
  role: 'Admin' | 'Member'
  status: 'active' | 'inactive'
}

const tabs = [
  { id: 's1', icon: '👤', label: 'Thông tin tài khoản' },
  { id: 's2', icon: '🎨', label: 'Giao diện' },
  { id: 's3', icon: '🔔', label: 'Thông báo' },
  { id: 's4', icon: '🔒', label: 'Đổi mật khẩu' },
  { id: 's5', icon: '💻', label: 'Quản lý thiết bị' },
  { id: 's6', icon: '👥', label: 'Quản lý người dùng' },
  { id: 's7', icon: '🏨', label: 'Tài khoản khách hàng' },
]

const { mode, setMode } = useTheme()
const activeTab = ref('s1')
const avatarUrl = ref('')
const avatarInput = ref<HTMLInputElement | null>(null)
const newPassword = ref('')
const showPassword = ref(false)
const userSearch = ref('')
const roleFilter = ref('')
const statusFilter = ref('')
const showUserModal = ref(false)
const showDeleteModal = ref(false)
const editingUserId = ref<string | null>(null)
const deletingUserId = ref<string | null>(null)
const users = ref<UserRecord[]>([
  { id: 'u1', name: 'Nguyễn Anh Thảo', phone: '0912 345 678', email: 'admin@ezcloud.vn', dept: 'Ban Giám đốc', role: 'Admin', status: 'active' },
  { id: 'u2', name: 'Trần Minh Tuấn', phone: '0913 456 789', email: 'tuan.tm@ezcloud.vn', dept: 'Kinh doanh', role: 'Member', status: 'active' },
  { id: 'u3', name: 'Lê Thị Lan Anh', phone: '0914 567 890', email: 'lananh@ezcloud.vn', dept: 'Kinh doanh', role: 'Member', status: 'active' },
  { id: 'u4', name: 'Phạm Hồng Nhung', phone: '0915 678 901', email: 'nhung.ph@ezcloud.vn', dept: 'Kế toán', role: 'Member', status: 'active' },
  { id: 'u5', name: 'Đặng Thị Mai', phone: '0917 890 123', email: 'mai.dt@ezcloud.vn', dept: 'Kỹ thuật', role: 'Member', status: 'inactive' },
])
const userForm = reactive<UserRecord & { password: string }>({
  id: '',
  name: '',
  phone: '',
  email: '',
  dept: '',
  role: 'Member',
  status: 'active',
  password: '',
})
const themeOptions: { id: ThemeMode; label: string; icon: string; preview: string }[] = [
  { id: 'dark', label: 'Tối', icon: '🌙', preview: 'theme-preview-dark' },
  { id: 'light', label: 'Sáng', icon: '☀️', preview: 'theme-preview-light' },
  { id: 'system', label: 'Hệ thống', icon: '💻', preview: 'theme-preview-system' },
]

const criteria = computed(() => [
  { label: 'Tối thiểu 8 ký tự', ok: newPassword.value.length >= 8 },
  { label: 'Có chữ hoa', ok: /[A-Z]/.test(newPassword.value) },
  { label: 'Có chữ số', ok: /[0-9]/.test(newPassword.value) },
  { label: 'Có ký tự đặc biệt', ok: /[^A-Za-z0-9]/.test(newPassword.value) },
])

const passwordScore = computed(() => {
  const base = criteria.value.filter((item) => item.ok).length
  return Math.min(5, base + (newPassword.value.length >= 12 ? 1 : 0))
})

const filteredUsers = computed(() => {
  const keyword = userSearch.value.trim().toLowerCase()
  return users.value.filter((user) => {
    const text = `${user.name} ${user.phone} ${user.email} ${user.dept}`.toLowerCase()
    return (
      (!keyword || text.includes(keyword)) &&
      (!roleFilter.value || user.role === roleFilter.value) &&
      (!statusFilter.value || user.status === statusFilter.value)
    )
  })
})

function selectTheme(value: ThemeMode) {
  setMode(value)
}

function openAvatarPicker() {
  avatarInput.value?.click()
}

function onAvatarChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    avatarUrl.value = URL.createObjectURL(file)
  }
}

function openUserModal(user?: UserRecord) {
  editingUserId.value = user?.id ?? null
  Object.assign(userForm, {
    id: user?.id ?? '',
    name: user?.name ?? '',
    phone: user?.phone ?? '',
    email: user?.email ?? '',
    dept: user?.dept ?? '',
    role: user?.role ?? 'Member',
    status: user?.status ?? 'active',
    password: '',
  })
  showUserModal.value = true
}

function saveUser() {
  if (!userForm.name || !userForm.email) {
    return
  }

  const payload: UserRecord = {
    id: editingUserId.value ?? `u-${Date.now()}`,
    name: userForm.name,
    phone: userForm.phone,
    email: userForm.email,
    dept: userForm.dept,
    role: 'Member',
    status: userForm.status,
  }
  const index = users.value.findIndex((user) => user.id === payload.id)
  if (index >= 0) {
    users.value[index] = payload
  } else {
    users.value.push(payload)
  }
  showUserModal.value = false
}

function askDelete(id: string) {
  deletingUserId.value = id
  showDeleteModal.value = true
}

function confirmDelete() {
  users.value = users.value.filter((user) => user.id !== deletingUserId.value)
  showDeleteModal.value = false
}
</script>

<template>
  <PageHeader title="Cấu hình hệ thống" subtitle="Quản lý tài khoản, giao diện, thông báo và phân quyền truy cập" />

  <div class="sys-tabs">
    <button
      v-for="tab in tabs"
      :key="tab.id"
      class="sys-tab"
      :class="{ active: activeTab === tab.id }"
      type="button"
      @click="activeTab = tab.id"
    >
      <span>{{ tab.icon }}</span>
      {{ tab.label }}
    </button>
  </div>

  <section v-if="activeTab === 's1'" class="card">
    <div class="form-row">
      <div class="form-group">
        <label class="form-label">Ảnh đại diện</label>
        <div class="avatar-lg" @click="openAvatarPicker">
          <img v-if="avatarUrl" :src="avatarUrl" alt="" style="position: absolute; inset: 0; width: 100%; height: 100%; border-radius: 50%; object-fit: cover" />
          <span v-else>AT</span>
        </div>
        <input ref="avatarInput" type="file" accept="image/png,image/jpeg" style="display: none" @change="onAvatarChange" />
      </div>
      <div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Họ và tên</label>
            <input class="form-input" value="Nguyễn Anh Thảo" />
          </div>
          <div class="form-group">
            <label class="form-label">Số điện thoại</label>
            <input class="form-input" value="0912 345 678" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Email</label>
            <input class="form-input" value="admin@ezcloud.vn" />
          </div>
          <div class="form-group">
            <label class="form-label">Vai trò</label>
            <input class="form-input" value="Admin" readonly />
          </div>
        </div>
        <button class="btn-primary" type="button">Lưu thay đổi</button>
      </div>
    </div>
  </section>

  <section v-if="activeTab === 's2'" class="card">
    <div class="theme-btns">
      <button
        v-for="theme in themeOptions"
        :key="theme.id"
        class="theme-opt"
        :class="{ selected: mode === theme.id }"
        type="button"
        @click="selectTheme(theme.id)"
      >
        <div class="theme-preview" :class="theme.preview">
          <div class="tp-sidebar"></div>
          <div class="tp-content">
            <div class="tp-bar"></div>
            <div class="tp-bar tp-bar-short"></div>
          </div>
        </div>
        <div class="theme-opt-label">
          <span>{{ theme.icon }}</span>
          {{ theme.label }}
          <span v-if="mode === theme.id" class="theme-check">✓</span>
        </div>
      </button>
    </div>
  </section>

  <section v-if="activeTab === 's3'" class="card">
    <table>
      <thead>
        <tr><th>Loại thông báo</th><th>Email</th><th>Trong ứng dụng</th><th>Âm thanh</th></tr>
      </thead>
      <tbody>
        <tr v-for="item in ['Nạp tiền KVC', 'Hoàn tiền', 'Đối soát lệch', 'Tài khoản đăng nhập']" :key="item">
          <td class="cell-strong">{{ item }}</td>
          <td><label class="toggle"><input type="checkbox" checked /><span class="toggle-slider"></span></label></td>
          <td><label class="toggle"><input type="checkbox" checked /><span class="toggle-slider"></span></label></td>
          <td><label class="toggle"><input type="checkbox" /><span class="toggle-slider"></span></label></td>
        </tr>
      </tbody>
    </table>
  </section>

  <section v-if="activeTab === 's4'" class="card">
    <div class="form-row">
      <div class="form-group">
        <label class="form-label">Mật khẩu hiện tại</label>
        <div class="pw-input-wrap">
          <input class="form-input" :type="showPassword ? 'text' : 'password'" placeholder="Nhập mật khẩu hiện tại" />
          <button class="pw-toggle" type="button" @click="showPassword = !showPassword">👁️</button>
        </div>
      </div>
      <div class="form-group">
        <label class="form-label">Mật khẩu mới</label>
        <input v-model="newPassword" class="form-input" type="password" placeholder="Nhập mật khẩu mới" />
        <div class="strength-bar">
          <span
            v-for="index in 5"
            :key="index"
            class="strength-seg"
            :style="{ background: index <= passwordScore ? ['#ef4444', '#f97316', '#eab308', '#22c55e', '#10b981'][passwordScore - 1] : '' }"
          ></span>
        </div>
        <div class="pw-criteria">
          <div v-for="item in criteria" :key="item.label" class="pw-criterion" :class="{ met: item.ok }">
            <span class="pw-criterion-dot"></span>{{ item.label }}
          </div>
        </div>
      </div>
    </div>
    <div class="form-group">
      <label class="form-label">Nhập lại mật khẩu mới</label>
      <input class="form-input" type="password" placeholder="Nhập lại mật khẩu mới" />
    </div>
    <button class="btn-primary" type="button">Cập nhật mật khẩu</button>
  </section>

  <section v-if="activeTab === 's5'" class="device-list">
    <div v-for="device in ['Chrome trên Windows · hiện tại', 'Safari trên iPhone · 22/06/2026', 'Edge trên Windows · 18/06/2026']" :key="device" class="device-item">
      <div class="device-icon">💻</div>
      <div>
        <div class="device-name">{{ device.split(' · ')[0] }}</div>
        <div class="device-meta">{{ device.split(' · ')[1] }}</div>
      </div>
      <button class="btn-secondary" type="button" style="margin-left: auto">Đăng xuất</button>
    </div>
  </section>

  <section v-if="activeTab === 's6'">
    <div class="role-cards">
      <div class="role-card">
        <div class="role-card-title">Admin</div>
        <div class="role-card-desc">Toàn quyền cấu hình hệ thống và quản lý người dùng.</div>
      </div>
      <div class="role-card">
        <div class="role-card-title">Member</div>
        <div class="role-card-desc">Truy cập nghiệp vụ, không được sửa phân quyền người dùng.</div>
      </div>
    </div>
    <div class="card">
      <div class="toolbar">
        <input v-model="userSearch" class="tb-input" placeholder="🔍  Tìm tên, email, SĐT..." />
        <select v-model="roleFilter" class="tb-select">
          <option value="">Tất cả vai trò</option>
          <option value="Admin">Admin</option>
          <option value="Member">Member</option>
        </select>
        <select v-model="statusFilter" class="tb-select">
          <option value="">Tất cả trạng thái</option>
          <option value="active">Hoạt động</option>
          <option value="inactive">Vô hiệu hoá</option>
        </select>
        <button class="tb-btn" type="button" @click="openUserModal()">+ Thêm Member</button>
      </div>
      <div class="table-wrap">
        <table>
          <thead><tr><th>Họ tên</th><th>SĐT</th><th>Email</th><th>Bộ phận</th><th>Vai trò</th><th>Trạng thái</th><th>Hành động</th></tr></thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.id">
              <td class="cell-strong">{{ user.name }}</td>
              <td>{{ user.phone }}</td>
              <td class="cell-muted">{{ user.email }}</td>
              <td>{{ user.dept }}</td>
              <td><span class="badge" :class="user.role === 'Admin' ? 'badge-blue' : 'badge-indigo'">{{ user.role === 'Admin' ? '👑' : '👤' }} {{ user.role }}</span></td>
              <td><span class="badge" :class="user.status === 'active' ? 'badge-green' : 'badge-gray'">● {{ user.status === 'active' ? 'Hoạt động' : 'Vô hiệu hoá' }}</span></td>
              <td>
                <span v-if="user.role === 'Admin'" class="cell-muted" style="font-size: 11px; font-style: italic">Không thể sửa/xoá</span>
                <template v-else>
                  <button class="act-btn" type="button" @click="openUserModal(user)">✏️</button>
                  <button class="act-btn" type="button" @click="askDelete(user.id)">🗑️</button>
                </template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>

  <section v-if="activeTab === 's7'">
    <div class="notice notice-indigo">🏨 Tài khoản khách hàng cho phép khách sạn truy cập cổng thông tin để xem báo cáo và theo dõi thanh toán.</div>
    <div class="stat-grid" style="margin-top: 16px">
      <div class="stat-card"><div class="stat-label">Tổng tài khoản</div><div class="stat-value">6</div></div>
      <div class="stat-card"><div class="stat-label">Đang online</div><div class="stat-value green">2</div></div>
      <div class="stat-card"><div class="stat-label">Bị khoá</div><div class="stat-value red">1</div></div>
    </div>
    <div class="card">
      <div class="toolbar">
        <input class="tb-input" placeholder="🔍  Tìm tài khoản..." />
        <button class="tb-btn" type="button">+ Thêm tài khoản</button>
      </div>
      <div class="table-wrap">
        <table>
          <thead><tr><th>Khách hàng</th><th>Khách sạn</th><th>Tài khoản</th><th>SĐT</th><th>Đăng nhập gần nhất</th><th>Trạng thái</th><th>Hành động</th></tr></thead>
          <tbody>
            <tr v-for="account in ['Nguyễn Văn A|Bình Minh|binhminh@portal.vn|0901 234 567|22/06/2026 08:30|Online', 'Trần Thị B|Resort Sao Biển|saobien@portal.vn|0902 345 678|22/06/2026 07:15|Online', 'Hoàng Lan E|Phố Cổ Boutique|phoco@portal.vn|0905 678 901|15/06/2026 16:45|Bị khoá']" :key="account">
              <td class="cell-strong">{{ account.split('|')[0] }}</td>
              <td>{{ account.split('|')[1] }}</td>
              <td class="cell-muted">{{ account.split('|')[2] }}</td>
              <td>{{ account.split('|')[3] }}</td>
              <td>{{ account.split('|')[4] }}</td>
              <td><span class="badge" :class="account.includes('Bị khoá') ? 'badge-red' : 'badge-green'">● {{ account.split('|')[5] }}</span></td>
              <td><button class="act-btn" type="button">🔑</button><button class="act-btn" type="button">🔒</button></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>

  <div v-if="showUserModal" class="modal-overlay" @click.self="showUserModal = false">
    <div class="modal">
      <div class="modal-header">
        <span class="modal-title">{{ editingUserId ? 'Sửa tài khoản Member' : 'Thêm tài khoản Member' }}</span>
        <button class="modal-close" type="button" @click="showUserModal = false">✕</button>
      </div>
      <div class="modal-body">
        <div class="notice notice-indigo" style="margin-bottom: 14px">👤 Tài khoản được tạo sẽ có role Member.</div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Họ và tên</label>
            <input v-model="userForm.name" class="form-input" placeholder="Nguyễn Văn A" />
          </div>
          <div class="form-group">
            <label class="form-label">Số điện thoại</label>
            <input v-model="userForm.phone" class="form-input" placeholder="09xx xxx xxx" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Email</label>
            <input v-model="userForm.email" class="form-input" type="email" placeholder="ten@ezcloud.vn" />
          </div>
          <div class="form-group">
            <label class="form-label">Bộ phận</label>
            <input v-model="userForm.dept" class="form-input" placeholder="Kinh doanh, Kế toán..." />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mật khẩu</label>
            <input v-model="userForm.password" class="form-input" type="password" placeholder="Tối thiểu 8 ký tự" />
          </div>
          <div class="form-group">
            <label class="form-label">Trạng thái</label>
            <select v-model="userForm.status" class="form-select">
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Vô hiệu hoá</option>
            </select>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showUserModal = false">Hủy</button>
        <button class="btn-primary" type="button" @click="saveUser">Lưu</button>
      </div>
    </div>
  </div>

  <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
    <div class="modal" style="width: 380px">
      <div class="modal-header">
        <span class="modal-title">Xác nhận xoá tài khoản</span>
        <button class="modal-close" type="button" @click="showDeleteModal = false">✕</button>
      </div>
      <div class="modal-body">Bạn có chắc muốn xoá tài khoản này?</div>
      <div class="modal-footer">
        <button class="btn-secondary" type="button" @click="showDeleteModal = false">Hủy</button>
        <button class="btn-danger" type="button" @click="confirmDelete">Xoá</button>
      </div>
    </div>
  </div>
</template>
