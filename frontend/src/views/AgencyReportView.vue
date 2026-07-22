<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import ReportTableCard from '../components/ui/ReportTableCard.vue'
import { useToast } from '../composables/useToast'
import { reportPages, type ReportPageKey } from '../data/reports'

const props = defineProps<{
  pageKey: ReportPageKey
}>()

const toast = useToast()

const page = computed(() => reportPages[props.pageKey])
const activeTab = ref(page.value.tabs[0]?.id)
const currentTab = computed(() => page.value.tabs.find((tab) => tab.id === activeTab.value) ?? page.value.tabs[0])

const apiTesting = ref(false)
const pagesWithApiTest: ReportPageKey[] = [
  'agencyArTransactions',
  'vinDailyBalances',
  'vinTicketCosts',
]

const uploading = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)
const pagesWithBankUpload: ReportPageKey[] = [
  'agencyBidvTransactions',
  'retailBankInflows',
  'otaBankInflows',
  'vinTopUps',
]

const building = ref(false)
const pagesWithBuildReconciliation: ReportPageKey[] = ['vinReconciliation']

const pagesWithGetApiOnly: ReportPageKey[] = ['retailTaBookings', 'otaTaBookings']

watch(
  () => props.pageKey,
  () => {
    activeTab.value = page.value.tabs[0]?.id
  },
)

async function runApiTest() {
  if (apiTesting.value) return
  apiTesting.value = true
  try {
    await new Promise((resolve) => setTimeout(resolve, 800))
    const count = currentTab.value?.rows.length ?? 0
    toast.success(`Đã gọi API test — lấy được ${count} dòng dữ liệu mới.`, { duration: 6000 })
  } finally {
    apiTesting.value = false
  }
}

function triggerUploadPicker() {
  fileInputRef.value?.click()
}

async function onStatementFileSelected(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return

  uploading.value = true
  try {
    await new Promise((resolve) => setTimeout(resolve, 800))
    toast.success(`Đã tải lên file "${file.name}" (demo, chưa nối API thật).`, { duration: 6000 })
  } finally {
    uploading.value = false
    input.value = ''
  }
}

async function runBuildReconciliation() {
  if (building.value) return
  building.value = true
  try {
    await new Promise((resolve) => setTimeout(resolve, 800))
    const count = currentTab.value?.rows.length ?? 0
    toast.success(`Đã build đối soát (demo, chưa nối API thật) — ${count} dòng.`, { duration: 6000 })
  } finally {
    building.value = false
  }
}
</script>

<template>
  <PageHeader :title="page.title" :subtitle="page.subtitle" />

  <div v-if="page.tabs.length > 1" class="tabs-bar">
    <button
      v-for="tab in page.tabs"
      :key="tab.id"
      class="tab-btn"
      :class="{ active: activeTab === tab.id }"
      type="button"
      @click="activeTab = tab.id"
    >
      {{ tab.label }}
    </button>
  </div>

  <ReportTableCard v-if="currentTab" :tab="currentTab">
    <template #actions>
      <button
        v-if="pagesWithApiTest.includes(props.pageKey)"
        class="add-btn"
        type="button"
        :disabled="apiTesting"
        @click="runApiTest"
      >
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 3l14 9-14 9V3z" />
        </svg>
        {{ apiTesting ? 'Đang gọi...' : 'Gọi API test' }}
      </button>
      <div v-if="pagesWithBankUpload.includes(props.pageKey)" class="tb-actions-right">
        <button class="btn-primary" type="button" :disabled="apiTesting" @click="runApiTest">
          {{ apiTesting ? 'Đang lấy...' : '⤓ Get API' }}
        </button>
        <div class="tb-upload-group">
          <input
            ref="fileInputRef"
            type="file"
            accept="application/pdf"
            style="display: none"
            @change="onStatementFileSelected"
          />
          <button
            class="btn-secondary"
            type="button"
            title="Dùng khi ngân hàng bị lỗi, không gửi được sao kê qua email: tải file PDF sao kê lên đây để nhập tay."
            :disabled="uploading"
            @click="triggerUploadPicker"
          >
            {{ uploading ? 'Đang xử lý...' : '📤 Upload tay sao kê' }}
          </button>
          <p class="tb-hint">
            * Chỉ dùng khi ngân hàng gặp sự cố và không gửi được sao kê qua email.
          </p>
        </div>
      </div>
      <div v-if="pagesWithBuildReconciliation.includes(props.pageKey)" class="tb-actions-right">
        <button class="add-btn" type="button" :disabled="building" @click="runBuildReconciliation">
          <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
            <path stroke-linecap="round" d="M12 5v14M5 12h14" />
          </svg>
          {{ building ? 'Đang build...' : 'Build đối soát' }}
        </button>
      </div>
      <div v-if="pagesWithGetApiOnly.includes(props.pageKey)" class="tb-actions-right">
        <button class="btn-primary" type="button" :disabled="apiTesting" @click="runApiTest">
          {{ apiTesting ? 'Đang lấy...' : '⤓ Get API' }}
        </button>
      </div>
    </template>
  </ReportTableCard>
</template>
