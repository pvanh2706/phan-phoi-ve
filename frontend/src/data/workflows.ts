import type { ReportTabConfig, TableCell, TableColumn } from './reports'

export interface KanbanTask {
  id: string
  title: string
  park: string
  owner: string
  date: string
  amount: string
  status: string
  tone: 'green' | 'red' | 'gray' | 'amber' | 'blue' | 'indigo'
  details: Record<string, string>
}

export interface KanbanColumn {
  id: string
  title: string
  icon: string
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

export const topUpWorkflow: KanbanColumn[] = [
  {
    id: 'request',
    title: 'Tạo yêu cầu',
    icon: '📝',
    tasks: [
      {
        id: 'tu-1',
        title: 'Nạp tiền tháng 6 - Vin Nha Trang',
        park: 'Vin Nha Trang',
        owner: 'Anh Thảo',
        date: '25/06/2026',
        amount: '50,000,000 đ',
        status: 'Mới tạo',
        tone: 'blue',
        details: {
          'Số tài khoản': '19139932758899',
          'Ngân hàng': 'Techcombank',
          'Ghi chú': 'Bổ sung hạn mức cuối tháng',
        },
      },
      {
        id: 'tu-2',
        title: 'Thanh toán công nợ - TLTY',
        park: 'TLTY',
        owner: 'Minh Tuấn',
        date: '25/06/2026',
        amount: '125,000,000 đ',
        status: 'Cần duyệt',
        tone: 'amber',
        details: {
          'Số tài khoản': '0221009876543',
          'Ngân hàng': 'Vietcombank',
          'Ghi chú': 'Đến hạn 30/06/2026',
        },
      },
    ],
  },
  {
    id: 'approval',
    title: 'Kế toán duyệt',
    icon: '✅',
    tasks: [
      {
        id: 'tu-3',
        title: 'Nạp tiền - Sunworld',
        park: 'Sunworld',
        owner: 'Lan Anh',
        date: '24/06/2026',
        amount: '80,000,000 đ',
        status: 'Đang duyệt',
        tone: 'indigo',
        details: {
          'Số tài khoản': '19036688990011',
          'Ngân hàng': 'BIDV',
          'Ghi chú': 'Đã đính kèm đề nghị thanh toán',
        },
      },
    ],
  },
  {
    id: 'transfer',
    title: 'Chuyển khoản',
    icon: '🏦',
    tasks: [
      {
        id: 'tu-4',
        title: 'Thanh toán công nợ - Sơn Tiên',
        park: 'Sơn Tiên',
        owner: 'Nguyễn Hùng',
        date: '24/06/2026',
        amount: '89,500,000 đ',
        status: 'Đang chuyển',
        tone: 'amber',
        details: {
          'Số tài khoản': '070100556677',
          'Ngân hàng': 'Sacombank',
          'Ghi chú': 'Chờ chứng từ ngân hàng',
        },
      },
    ],
  },
  {
    id: 'confirm',
    title: 'Xác nhận KVC',
    icon: '📩',
    tasks: [
      {
        id: 'tu-5',
        title: 'Nạp tiền - Bản Mòng',
        park: 'Bản Mòng',
        owner: 'Hồng Nhung',
        date: '23/06/2026',
        amount: '35,000,000 đ',
        status: 'Chờ xác nhận',
        tone: 'amber',
        details: {
          'Số tài khoản': '088889991111',
          'Ngân hàng': 'MB Bank',
          'Ghi chú': 'KVC xác nhận qua email',
        },
      },
    ],
  },
  {
    id: 'done',
    title: 'Hoàn tất',
    icon: '🎯',
    tasks: [
      {
        id: 'tu-6',
        title: 'Nạp tiền - Delight',
        park: 'Delight',
        owner: 'Anh Thảo',
        date: '22/06/2026',
        amount: '20,000,000 đ',
        status: 'Hoàn tất',
        tone: 'green',
        details: {
          'Số tài khoản': '123456789',
          'Ngân hàng': 'ACB',
          'Ghi chú': 'Đã cập nhật số dư',
        },
      },
    ],
  },
]

export const refundWorkflow: KanbanColumn[] = [
  {
    id: 'new',
    title: 'Tiếp nhận',
    icon: '📥',
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
    icon: '🔎',
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
    icon: '✅',
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
    icon: '💳',
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
    icon: '🎯',
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
