<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiClientError } from '../services/apiClient'
import { login } from '../services/authStore'

const route = useRoute()
const router = useRouter()
const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

const canSubmit = computed(() => email.value.trim() && password.value)

async function submit() {
  if (!canSubmit.value || loading.value) return

  loading.value = true
  errorMessage.value = ''

  try {
    await login(email.value.trim(), password.value)
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    await router.replace(redirect)
  } catch (error) {
    errorMessage.value = error instanceof ApiClientError
      ? error.message
      : 'Không thể đăng nhập, vui lòng thử lại.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="login-page">
    <section class="login-panel">
      <div class="login-brand">
        <div class="brand-logo login-logo">PPV</div>
        <div>
          <h1>Đối soát phân phối vé</h1>
          <p>Đăng nhập nội bộ</p>
        </div>
      </div>

      <form class="login-form" @submit.prevent="submit">
        <label class="form-group">
          <span class="form-label">Email</span>
          <input v-model="email" class="form-input" type="email" autocomplete="username" />
        </label>

        <label class="form-group">
          <span class="form-label">Mật khẩu</span>
          <input v-model="password" class="form-input" type="password" autocomplete="current-password" />
        </label>

        <p v-if="errorMessage" class="login-error">{{ errorMessage }}</p>

        <button class="btn-primary login-submit" type="submit" :disabled="!canSubmit || loading">
          {{ loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
        </button>
      </form>
    </section>
  </main>
</template>

<style scoped>
.login-page {
  display: grid;
  min-height: 100vh;
  place-items: center;
  padding: 24px;
}

.login-panel {
  width: min(420px, 100%);
  border: 1px solid var(--border);
  border-radius: 14px;
  background: var(--surface-strong);
  backdrop-filter: blur(16px);
  padding: 28px;
}

.login-brand {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 24px;
}

.login-logo {
  width: 42px;
  height: 42px;
}

.login-brand h1 {
  color: var(--text-primary);
  font-size: 20px;
  line-height: 1.25;
}

.login-brand p {
  margin-top: 4px;
  color: var(--text-muted);
  font-size: 13px;
}

.login-form {
  display: grid;
  gap: 14px;
}

.login-submit {
  width: 100%;
  margin-top: 4px;
}

.login-submit:disabled {
  cursor: not-allowed;
  opacity: 0.65;
}

.login-error {
  border: 1px solid rgba(239, 68, 68, 0.25);
  border-radius: 8px;
  background: rgba(239, 68, 68, 0.12);
  color: #fca5a5;
  padding: 10px 12px;
  font-size: 13px;
}
</style>
