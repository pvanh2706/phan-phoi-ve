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
  'agencyTaTransactions',
  'vinDailyBalances',
  'vinTicketCosts',
]

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
    <template v-if="pagesWithApiTest.includes(props.pageKey)" #actions>
      <button class="add-btn" type="button" :disabled="apiTesting" @click="runApiTest">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 3l14 9-14 9V3z" />
        </svg>
        {{ apiTesting ? 'Đang gọi...' : 'Gọi API test' }}
      </button>
    </template>
  </ReportTableCard>
</template>
