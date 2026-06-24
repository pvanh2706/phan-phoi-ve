import { createRouter, createWebHistory } from 'vue-router'
import ComingSoonView from '../views/ComingSoonView.vue'
import ParkCodesView from '../views/ParkCodesView.vue'
import RefundProcessView from '../views/RefundProcessView.vue'
import ReportView from '../views/ReportView.vue'
import SystemSettingsView from '../views/SystemSettingsView.vue'
import TopUpWorkflowView from '../views/TopUpWorkflowView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
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
      meta: { title: 'Khu vui chơi / Số dư khu vui chơi hàng ngày' },
    },
    {
      path: '/khu-vui-choi/nap-tien',
      component: TopUpWorkflowView,
      meta: { title: 'Khu vui chơi / Danh sách nạp tiền KVC theo ngày' },
    },
    {
      path: '/khu-vui-choi/gia-von-ve-ban',
      component: ReportView,
      props: { pageKey: 'ticketCosts' },
      meta: { title: 'Khu vui chơi / Chi tiết giá vốn vé bán' },
    },
    {
      path: '/khu-vui-choi/doi-soat',
      component: ReportView,
      props: { pageKey: 'reconciliation' },
      meta: { title: 'Khu vui chơi / Đối soát Khu vui chơi' },
    },
    {
      path: '/khu-vui-choi/kvc-hoan-tien',
      component: ReportView,
      props: { pageKey: 'parkRefunds' },
      meta: { title: 'Khu vui chơi / KVC hoàn tiền' },
    },
    {
      path: '/hoan-tien/quy-trinh',
      component: RefundProcessView,
      meta: { title: 'Hoàn tiền / Quy trình hoàn tiền' },
    },
    {
      path: '/hoan-tien/trang-thai-khach-hang',
      component: ReportView,
      props: { pageKey: 'customerRefundStatus' },
      meta: { title: 'Hoàn tiền / Trạng thái hoàn tiền cho khách hàng' },
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

export default router
