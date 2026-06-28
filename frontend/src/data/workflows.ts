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

export function cloneColumns(columns: KanbanColumn[]) {
  return columns.map((column) => ({
    ...column,
    tasks: column.tasks.map((task) => ({ ...task, details: { ...task.details } })),
  }))
}

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

export const refundWorkflow: KanbanColumn[] = [
  {
    id: 'new',
    title: 'Tiếp nhận',
    icon: 'inbox',
    tone: 'gray',
    tasks: [
      {
        id: 'rw-1',
        title: 'Hoàn tiền BK-240625-001',
        park: 'Bản Mòng',
        owner: 'CSKH',
        date: '25/06/2026',
        amount: '1,250,000 đ',
        status: 'Mới tạo',
        tone: 'blue',
        details: { 'Khách hàng': 'Nguyễn Văn A', 'Lý do': 'Khách huỷ vé trước hạn', 'Phương thức': 'Chuyển khoản' },
      },
    ],
  },
  {
    id: 'check',
    title: 'Kiểm tra điều kiện',
    icon: 'search',
    tone: 'sky',
    tasks: [
      {
        id: 'rw-2',
        title: 'Hoàn tiền BK-240624-014',
        park: 'Sun Group',
        owner: 'Vận hành',
        date: '24/06/2026',
        amount: '820,000 đ',
        status: 'Đang kiểm tra',
        tone: 'amber',
        details: { 'Khách hàng': 'Trần Thị B', 'Lý do': 'Lỗi thanh toán', 'Phương thức': 'Ví điện tử' },
      },
    ],
  },
  {
    id: 'approve',
    title: 'Duyệt hoàn',
    icon: 'check',
    tone: 'amber',
    tasks: [
      {
        id: 'rw-3',
        title: 'Hoàn tiền BK-240623-102',
        park: 'Đồi Rồng',
        owner: 'Kế toán',
        date: '23/06/2026',
        amount: '450,000 đ',
        status: 'Chờ duyệt',
        tone: 'indigo',
        details: { 'Khách hàng': 'Lê Minh C', 'Lý do': 'Đổi lịch không thành công', 'Phương thức': 'Chuyển khoản' },
      },
    ],
  },
  {
    id: 'payment',
    title: 'Chi hoàn tiền',
    icon: 'card',
    tone: 'indigo',
    tasks: [
      {
        id: 'rw-4',
        title: 'Hoàn tiền BK-240622-087',
        park: 'TLTY',
        owner: 'Kế toán',
        date: '22/06/2026',
        amount: '600,000 đ',
        status: 'Đang chi',
        tone: 'amber',
        details: { 'Khách hàng': 'Phạm Đức D', 'Lý do': 'Trùng booking', 'Phương thức': 'Bù trừ công nợ' },
      },
    ],
  },
  {
    id: 'complete',
    title: 'Hoàn tất',
    icon: 'target',
    tone: 'green',
    tasks: [
      {
        id: 'rw-5',
        title: 'Hoàn tiền BK-240621-044',
        park: 'Sơn Tiên',
        owner: 'CSKH',
        date: '21/06/2026',
        amount: '1,600,000 đ',
        status: 'Hoàn tất',
        tone: 'green',
        details: { 'Khách hàng': 'Hoàng Lan E', 'Lý do': 'Huỷ vé hợp lệ', 'Phương thức': 'Bù trừ công nợ' },
      },
    ],
  },
]

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
