<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { navigation } from '../../data/navigation'
import { useTheme } from '../../composables/useTheme'
import { authState, logout } from '../../services/authStore'
import { userRoleLabel } from '../../services/formatters'
import AppIcon from '../ui/AppIcon.vue'

const route = useRoute()
const router = useRouter()
const collapsed = ref(false)
const openMenus = reactive<Record<string, boolean>>({
  park: true,
  refund: false,
})
const { toggleTheme } = useTheme()

const title = computed(() => String(route.meta.title ?? 'Đối soát phân phối vé'))
const user = computed(() => authState.user)
const initials = computed(() => {
  const name = user.value?.fullName?.trim() || user.value?.email || 'U'
  return name
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
})
const roleLabel = computed(() => userRoleLabel(user.value?.role))

function isChildActive(path: string) {
  return route.path === path
}

function isGroupActive(paths: string[]) {
  return paths.some((path) => route.path === path)
}

function toggleMenu(key: string) {
  openMenus[key] = !openMenus[key]
}

async function handleLogout() {
  await logout()
  await router.replace('/login')
}

watch(
  () => route.path,
  () => {
    for (const item of navigation) {
      if (item.children && isGroupActive(item.children.map((child) => child.path))) {
        openMenus[item.key] = true
      }
    }
  },
  { immediate: true },
)
</script>

<template>
  <div class="app-shell">
    <aside class="sidebar" :class="{ collapsed }" id="sidebar">
      <div class="sidebar-brand">
        <div class="brand-logo">ez</div>
        <div class="brand-text">
          <div class="brand-name">ezCloud PPV</div>
          <div class="brand-role">{{ roleLabel }}</div>
        </div>
      </div>

      <nav class="sidebar-nav">
        <template v-for="item in navigation" :key="item.key">
          <button
            v-if="item.children"
            class="nav-item has-children"
            :class="{ open: openMenus[item.key] }"
            type="button"
            @click="toggleMenu(item.key)"
          >
            <span class="nav-icon">{{ item.icon }}</span>
            <span class="nav-label">{{ item.label }}</span>
            <span class="nav-arrow">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="6 9 12 15 18 9" />
              </svg>
            </span>
          </button>
          <RouterLink
            v-else
            class="nav-item"
            :class="{ active: route.path === item.path }"
            :to="item.path ?? '/'"
          >
            <span class="nav-icon">{{ item.icon }}</span>
            <span class="nav-label">{{ item.label }}</span>
          </RouterLink>

          <div v-if="item.children" class="nav-sub" :class="{ open: openMenus[item.key] }">
            <RouterLink
              v-for="child in item.children"
              :key="child.path"
              class="nav-sub-item"
              :class="{ active: isChildActive(child.path) }"
              :to="child.path"
            >
              <span class="nav-sub-dot"></span>
              {{ child.label }}
            </RouterLink>
          </div>
        </template>
      </nav>

      <div class="sidebar-user">
        <div class="user-card">
          <div class="user-info">
            <div class="avatar">{{ initials }}<span class="avatar-dot"></span></div>
            <div style="min-width: 0">
              <div class="user-name">{{ user?.fullName }}</div>
              <div class="user-email">{{ user?.email }}</div>
            </div>
          </div>
          <div class="user-actions">
            <RouterLink class="user-btn" :class="{ active: route.path === '/system' }" to="/system">
              <AppIcon name="settings" :size="14" />
              Cài đặt
            </RouterLink>
            <button class="user-btn logout" type="button" @click="handleLogout">
              <AppIcon name="logout" :size="14" />
              Đăng xuất
            </button>
          </div>
        </div>
      </div>
    </aside>

    <div class="main-content">
      <header class="header">
        <div class="header-left">
          <button class="icon-btn" type="button" aria-label="Thu gọn menu" @click="collapsed = !collapsed">
            <AppIcon name="menu" :size="20" />
          </button>
          <span class="page-title">{{ title }}</span>
        </div>
        <div class="header-right">
          <input type="search" class="search-input" placeholder="Tìm kiếm..." />
          <button class="icon-btn notif-btn" type="button" aria-label="Thông báo">
            <AppIcon name="bell" :size="18" />
            <span class="notif-dot"></span>
          </button>
        </div>
      </header>

      <main class="page-content">
        <slot />
      </main>
    </div>
  </div>

  <button class="theme-toggle" type="button" @click="toggleTheme">
    <AppIcon name="palette" :size="14" />
    Đổi theme
  </button>
</template>
