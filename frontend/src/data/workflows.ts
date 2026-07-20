import type { ReportTabConfig, TableCell, TableColumn } from './reports'

export interface KanbanTask {
  id: string
  /** Id bản ghi trong DB (nếu card lấy từ API). */
  dbId?: number
  title: string
  park: string
  owner: string
  date: string
  amount: string
  status: string
  tone: 'green' | 'red' | 'gray' | 'amber' | 'blue' | 'indigo'
  details: Record<string, string>
}

export type KanbanTone = 'gray' | 'sky' | 'amber' | 'indigo' | 'green' | 'red'

export interface KanbanAvatar {
  initials: string
  name?: string
  tone?: 'indigo' | 'teal' | 'amber' | 'rose' | 'gray'
}

export interface KanbanColumnStats {
  done: number
  total: number
  overdue: number
  avgHours?: number
  workItems?: number
  /** Màu cho chỉ số "NV": green ở cột Hoàn thành, red ở cột Thất bại. */
  nvTone?: 'green' | 'red'
}

export interface KanbanColumn {
  id: string
  /** Id cột trong DB (nếu cột lấy từ API). */
  dbId?: number
  title: string
  icon: string
  tone?: KanbanTone
  avatars?: KanbanAvatar[]
  stats?: KanbanColumnStats
  /** Các id trường hiển thị trên card (cấu hình cột). */
  visibleFields?: string[]
  /** Các UserId được phép chuyển task khỏi cột. */
  permittedUserIds?: number[]
  tasks: KanbanTask[]
}

const strong = (text: string): TableCell => ({ kind: 'strong', text })
const muted = (text: string): TableCell => ({ kind: 'muted', text })
const green = (text: string): TableCell => ({ kind: 'amount', text, tone: 'green' })
const red = (text: string): TableCell => ({ kind: 'amount', text, tone: 'red' })
const badge = (text: string, tone: KanbanTask['tone']): TableCell => ({ kind: 'badge', text, tone })
const actions = (): TableCell => ({ kind: 'actions', actions: [{ icon: '👁️', label: 'Chi tiết' }, { icon: '✏️', label: 'Sửa' }] })

/**
 * Trang trí header cột (avatar người phụ trách + thống kê) theo columnKey.
 * Đây là phần hiển thị tĩnh, được ghép vào dữ liệu cột lấy từ API.
 */
export const topUpColumnDecor: Record<string, { avatars: KanbanAvatar[]; stats: KanbanColumnStats }> = {
  'lap-phieu': {
    avatars: [
      { initials: 'AT', name: 'Anh Thảo', tone: 'indigo' },
      { initials: 'NH', name: 'Ngọc Hà', tone: 'teal' },
      { initials: 'TL', name: 'Thu Lan', tone: 'gray' },
    ],
    stats: { done: 0, total: 0, overdue: 0, avgHours: 2, workItems: 1 },
  },
  'truong-bo-phan-duyet': {
    avatars: [
      { initials: 'TM', name: 'Trung Minh', tone: 'amber' },
      { initials: 'BH', name: 'Bảo Hân', tone: 'rose' },
    ],
    stats: { done: 0, total: 0, overdue: 0, avgHours: 2, workItems: 2 },
  },
  'kiem-tra-chuyen-khoan': {
    avatars: [
      { initials: 'AT', name: 'Anh Thảo', tone: 'indigo' },
      { initials: 'KT', name: 'Kế Toán', tone: 'teal' },
    ],
    stats: { done: 987, total: 1489, overdue: 898, avgHours: 72, workItems: 2 },
  },
  'hoan-thanh': {
    avatars: [
      { initials: 'NH', name: 'Ngọc Hà', tone: 'teal' },
      { initials: 'AT', name: 'Anh Thảo', tone: 'indigo' },
    ],
    stats: { done: 6, total: 520, overdue: 0, nvTone: 'green' },
  },
  'that-bai': {
    avatars: [
      { initials: 'BH', name: 'Bảo Hân', tone: 'rose' },
    ],
    stats: { done: 7, total: 18, overdue: 0, nvTone: 'red' },
  },
}

const topUpColumns: TableColumn[] = [
  { key: 'date', label: 'Ngày' },
  { key: 'content', label: 'Nội dung' },
  { key: 'park', label: 'KVC' },
  { key: 'account', label: 'Tài khoản' },
  { key: 'amount', label: 'Số tiền' },
  { key: 'status', label: 'Trạng thái' },
  { key: 'actions', label: 'Hành động' },
]

export const topUpReportTabs: ReportTabConfig[] = [
  {
    id: 'nap',
    label: 'Nạp tiền cho KVC nạp tiền',
    columns: topUpColumns,
    searchPlaceholder: '🔍  Tìm nội dung...',
    dateFilter: true,
    rows: [
      { id: 'th-1', search: 'Nạp tiền tháng 6 Vin Nha Trang', date: '2026-06-25', cells: ['25/06/2026', strong('Nạp tiền tháng 6'), 'Vin Nha Trang', muted('Techcombank · 19139932758899'), green('50,000,000 đ'), badge('Đang xử lý', 'amber'), actions()] },
      { id: 'th-2', search: 'Nạp tiền Sunworld', date: '2026-06-24', cells: ['24/06/2026', strong('Bổ sung hạn mức'), 'Sunworld', muted('BIDV · 19036688990011'), green('80,000,000 đ'), badge('Đang duyệt', 'indigo'), actions()] },
      { id: 'th-3', search: 'Nạp tiền Bản Mòng', date: '2026-06-23', cells: ['23/06/2026', strong('Nạp tiền vận hành'), 'Bản Mòng', muted('MB Bank · 088889991111'), green('35,000,000 đ'), badge('Chờ xác nhận', 'amber'), actions()] },
      { id: 'th-4', search: 'Nạp tiền Delight', date: '2026-06-22', cells: ['22/06/2026', strong('Nạp tiền tự động'), 'Delight', muted('ACB · 123456789'), green('20,000,000 đ'), badge('Hoàn tất', 'green'), actions()] },
    ],
  },
  {
    id: 'cn',
    label: 'Thanh toán KVC công nợ',
    columns: topUpColumns,
    searchPlaceholder: '🔍  Tìm nội dung...',
    dateFilter: true,
    rows: [
      { id: 'tc-1', search: 'Thanh toán công nợ TLTY', date: '2026-06-25', cells: ['25/06/2026', strong('Thanh toán công nợ'), 'TLTY', muted('Vietcombank · 0221009876543'), red('125,000,000 đ'), badge('Cần duyệt', 'amber'), actions()] },
      { id: 'tc-2', search: 'Thanh toán công nợ Sơn Tiên', date: '2026-06-24', cells: ['24/06/2026', strong('Thanh toán kỳ 06'), 'Sơn Tiên', muted('Sacombank · 070100556677'), red('89,500,000 đ'), badge('Đang chuyển', 'amber'), actions()] },
    ],
  },
]
