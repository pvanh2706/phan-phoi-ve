import { createRouter, createWebHistory } from 'vue-router'
import AgencyDiffView from '../views/AgencyDiffView.vue'
import AgencyListView from '../views/AgencyListView.vue'
import AgencyMonthlyBalanceView from '../views/AgencyMonthlyBalanceView.vue'
import AgencyMonthlyUsageView from '../views/AgencyMonthlyUsageView.vue'
import AgencyReportView from '../views/AgencyReportView.vue'
import AuditLogView from '../views/AuditLogView.vue'
import ComingSoonView from '../views/ComingSoonView.vue'
import JobErrorsView from '../views/JobErrorsView.vue'
import LoginView from '../views/LoginView.vue'
import ParkCodesView from '../views/ParkCodesView.vue'
import ReconciliationView from '../views/ReconciliationView.vue'
import ReportView from '../views/ReportView.vue'
import RetailDiffView from '../views/RetailDiffView.vue'
import SystemSettingsView from '../views/SystemSettingsView.vue'
import TicketCostDetailView from '../views/TicketCostDetailView.vue'
import TopUpWorkflowView from '../views/TopUpWorkflowView.vue'
import VinChildParksView from '../views/VinChildParksView.vue'
import { authState, initializeAuth } from '../services/authStore'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      component: LoginView,
      meta: { title: 'Đăng nhập', public: true },
    },
    { path: '/', redirect: '/khu-vui-choi/danh-sach' },
    {
      path: '/khu-vui-choi/danh-sach',
      component: ReportView,
      props: { pageKey: 'parkList' },
      meta: { title: 'Khu vui chơi / Danh sách khu vui chơi' },
    },
    {
      path: '/khu-vui-choi/ma-kvc',
      component: ParkCodesView,
      meta: { title: 'Khu vui chơi / Mã khu vui chơi' },
    },
    {
      path: '/khu-vui-choi/so-du',
      component: ReportView,
      props: { pageKey: 'dailyBalances' },
      meta: { title: 'Khu vui chơi / Số dư khu vui chơi hằng ngày' },
    },
    {
      path: '/khu-vui-choi/nap-tien',
      component: TopUpWorkflowView,
      meta: { title: 'Khu vui chơi / Danh sách nạp tiền KVC theo ngày' },
    },
    {
      path: '/khu-vui-choi/gia-von-ve-ban',
      component: TicketCostDetailView,
      meta: { title: 'Khu vui chơi / Chi tiết giá vốn vé bán' },
    },
    {
      path: '/khu-vui-choi/doi-soat',
      component: ReconciliationView,
      meta: { title: 'Khu vui chơi / Đối soát Khu vui chơi' },
    },
    {
      path: '/khu-vui-choi/nhat-ky',
      component: AuditLogView,
      meta: { title: 'Khu vui chơi / Nhật ký thay đổi' },
    },
    {
      path: '/khu-vui-choi/loi-dong-bo',
      component: JobErrorsView,
      meta: { title: 'Khu vui chơi / Lỗi đồng bộ cần xử lý' },
    },
    {
      path: '/dai-ly/danh-sach',
      component: AgencyListView,
      meta: { title: 'Đại lý / Danh sách các đại lý' },
    },
    {
      path: '/dai-ly/giao-dich-ar',
      component: AgencyReportView,
      props: { pageKey: 'agencyArTransactions' },
      meta: { title: 'Đại lý / Giao dịch của các đại lý trên AR' },
    },
    {
      path: '/dai-ly/giao-dich-ta',
      component: AgencyReportView,
      props: { pageKey: 'agencyTaTransactions' },
      meta: { title: 'Đại lý / Giao dịch của các đại lý trên TA' },
    },
    {
      path: '/dai-ly/giao-dich-bidv',
      component: AgencyReportView,
      props: { pageKey: 'agencyBidvTransactions' },
      meta: { title: 'Đại lý / Giao dịch đại lý nạp tiền trên BIDV' },
    },
    {
      path: '/dai-ly/tong-tien-thang',
      component: AgencyMonthlyUsageView,
      meta: { title: 'Đại lý / Tổng tiền các đại lý đã dùng theo tháng' },
    },
    {
      path: '/dai-ly/doi-soat',
      component: AgencyMonthlyBalanceView,
      meta: { title: 'Đại lý / Số dư theo ngày của các đại lý (tự tính)' },
    },
    {
      path: '/dai-ly/doi-soat-ar-ta',
      component: AgencyDiffView,
      props: { direction: 'ar-ta' },
      meta: { title: 'Đại lý / Đối soát AR - TA' },
    },
    {
      path: '/dai-ly/doi-soat-ta-ar',
      component: AgencyDiffView,
      props: { direction: 'ta-ar' },
      meta: { title: 'Đại lý / Đối soát TA - AR' },
    },
    {
      path: '/khach-le/booking-ta',
      component: AgencyReportView,
      props: { pageKey: 'retailTaBookings' },
      meta: { title: 'Khách lẻ / Booking khách lẻ trên TA' },
    },
    {
      path: '/khach-le/tien-ve-ngan-hang',
      component: AgencyReportView,
      props: { pageKey: 'retailBankInflows' },
      meta: { title: 'Khách lẻ / Tiền về ngân hàng' },
    },
    {
      path: '/khach-le/doi-soat',
      component: RetailDiffView,
      meta: { title: 'Khách lẻ / Đối soát' },
    },
    {
      path: '/doi-soat-vin/danh-muc-kvc-con',
      component: VinChildParksView,
      meta: { title: 'Đối soát Vin / Danh mục KVC con của Vin' },
    },
    {
      path: '/doi-soat-vin/gia-von-ve-ban',
      component: AgencyReportView,
      props: { pageKey: 'vinTicketCosts' },
      meta: { title: 'Đối soát Vin / Chi tiết giá vốn vé bán' },
    },
    {
      path: '/doi-soat-vin/so-du-theo-ngay',
      component: AgencyReportView,
      props: { pageKey: 'vinDailyBalances' },
      meta: { title: 'Đối soát Vin / Số dư KVC Vin theo ngày' },
    },
    {
      path: '/doi-soat-vin/nap-tien-theo-ngay',
      component: AgencyReportView,
      props: { pageKey: 'vinTopUps' },
      meta: { title: 'Đối soát Vin / Danh sách nạp tiền cho Vin theo ngày' },
    },
    {
      path: '/doi-soat-vin/doi-soat',
      component: AgencyReportView,
      props: { pageKey: 'vinReconciliation' },
      meta: { title: 'Đối soát Vin / Đối soát KVC Vin' },
    },
    {
      path: '/dai-ly-ota/booking-ta',
      component: AgencyReportView,
      props: { pageKey: 'otaTaBookings' },
      meta: { title: 'Các đại lý API / Booking API trên TA' },
    },
    {
      path: '/dai-ly-ota/tien-ve-ngan-hang',
      component: AgencyReportView,
      props: { pageKey: 'otaBankInflows' },
      meta: { title: 'Các đại lý API / Tiền về ngân hàng' },
    },
    {
      path: '/dai-ly-ota/doi-soat',
      component: RetailDiffView,
      props: {
        taPageKey: 'otaTaBookings',
        bankPageKey: 'otaBankInflows',
        entityLabel: 'Tên đại lý API',
        title: 'Đối soát booking API với tiền về ngân hàng',
      },
      meta: { title: 'Các đại lý API / Đối soát' },
    },
    {
      path: '/system',
      component: SystemSettingsView,
      meta: { title: 'Cấu hình hệ thống' },
    },
    {
      path: '/dang-phat-trien/:feature',
      component: ComingSoonView,
      meta: { title: 'Tính năng đang phát triển' },
    },
  ],
})

router.beforeEach(async (to) => {
  if (!authState.ready) {
    await initializeAuth()
  }

  if (to.meta.public) {
    return authState.user && to.path === '/login' ? '/' : true
  }

  if (!authState.user) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  return true
})

export default router
