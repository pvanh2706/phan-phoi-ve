<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { navigation } from '../../data/navigation'
import { useTheme } from '../../composables/useTheme'
import { authState, logout } from '../../services/authStore'
import { userRoleLabel } from '../../services/formatters'

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
        <div class="brand-logo">PPV</div>
        <div class="brand-text">
          <div class="brand-name">Đối soát vé</div>
          <div class="brand-role">{{ roleLabel }}</div>
        </div>
      </div>

      <nav class="sidebar-nav">
        <template v-for="item in navigation" :key="item.key">
          <button
            v-if="item.children"
            class="nav-item has-children"
            :class="{ open: openMenus[item.key], active: isGroupActive(item.children.map((child) => child.path)) }"
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
              <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"
                />
                <circle cx="12" cy="12" r="3" />
              </svg>
              Cài đặt
            </RouterLink>
            <button class="user-btn logout" type="button" @click="handleLogout">
              <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                <path stroke-linecap="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
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
            <svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <path stroke-linecap="round" d="M4 6h16M4 12h10M4 18h16" />
            </svg>
          </button>
          <span class="page-title">{{ title }}</span>
        </div>
        <div class="header-right">
          <input type="search" class="search-input" placeholder="Tìm kiếm..." />
          <button class="icon-btn notif-btn" type="button" aria-label="Thông báo">
            <svg width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <path stroke-linecap="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
            <span class="notif-dot"></span>
          </button>
        </div>
      </header>

      <main class="page-content">
        <slot />
      </main>
    </div>
  </div>

  <button class="theme-toggle" type="button" @click="toggleTheme">Đổi theme</button>
</template>
