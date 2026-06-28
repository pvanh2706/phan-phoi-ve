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
    id: 'lap-phieu',
    title: 'Kế toán / NVKD lập phiếu',
    icon: 'clipboard',
    tasks: [
      {
        id: 'tu-1',
        title: 'Nạp tiền - Vin Nha Trang',
        park: 'Nạp tiền',
        owner: 'Techcombank',
        date: '24/06/2026',
        amount: '50.000.000 ₫',
        status: 'Lập phiếu',
        tone: 'gray',
        details: {
          'Số tài khoản': '19139932758899',
          'Ngân hàng': 'Techcombank',
          'Ghi chú': 'Cần ezCloud Key tạy tiền trên hệ thống',
        },
      },
    ],
  },
  {
    id: 'truong-bo-phan-duyet',
    title: 'Trưởng bộ phận duyệt',
    icon: 'check',
    tasks: [
      {
        id: 'tu-2',
        title: 'Nạp tiền - Vin Phú Quốc',
        park: 'Nạp tiền',
        owner: 'Vietcombank',
        date: '28/04/2026',
        amount: '100.000.000 ₫',
        status: 'Chờ duyệt',
        tone: 'blue',
        details: {
          'Số tài khoản': '0091000593278',
          'Ngân hàng': 'Vietcombank',
          'Ghi chú': 'Cần ezCloud Key tạy tiền trên hệ thống',
        },
      },
      {
        id: 'tu-3',
        title: 'Nạp tiền - Vin Nam Hội An',
        park: 'Nạp tiền',
        owner: 'Vietcombank',
        date: '28/04/2026',
        amount: '50.000.000 ₫',
        status: 'Chờ duyệt',
        tone: 'blue',
        details: {
          'Số tài khoản': '1029876329',
          'Ngân hàng': 'Vietcombank',
          'Ghi chú': 'Cần ezCloud Key tạy tiền trên hệ thống',
        },
      },
    ],
  },
  {
    id: 'kiem-tra-chuyen-khoan',
    title: 'Kế toán kiểm tra & chuyển khoản',
    icon: 'bank',
    tasks: [
      {
        id: 'tu-4',
        title: 'Nạp tiền - Thủy Cung Lotte (Lần 1)',
        park: 'Nạp tiền',
        owner: 'Shinhan Bank',
        date: '29/04/2026',
        amount: '365.625.000 ₫',
        status: 'Chuyển khoản',
        tone: 'indigo',
        details: {
          'Số tài khoản': '700029610000',
          'Ngân hàng': 'Shinhan Bank',
          'Ghi chú': 'Bộ phận công tác: Phân Phối Vé · Kế Toán',
        },
      },
      {
        id: 'tu-5',
        title: 'Nạp tiền - Thủy Cung Lotte (Lần 2)',
        park: 'Nạp tiền',
        owner: 'Shinhan Bank',
        date: '29/04/2026',
        amount: '237.375.000 ₫',
        status: 'Chuyển khoản',
        tone: 'indigo',
        details: {
          'Số tài khoản': '700029610000',
          'Ngân hàng': 'Shinhan Bank',
          'Ghi chú': 'Bộ phận công tác: Phân Phối Vé · Kế Toán',
        },
      },
    ],
  },
  {
    id: 'hoan-thanh',
    title: 'Hoàn thành',
    icon: 'target',
    tasks: [
      {
        id: 'tu-6',
        title: 'Nạp tiền - Bản Mòng',
        park: 'Nạp tiền',
        owner: 'NCB',
        date: '14/11/2025',
        amount: '490.000.000 ₫',
        status: 'Hoàn thành',
        tone: 'green',
        details: {
          'Số tài khoản': '1213776969',
          'Ngân hàng': 'NCB',
          'Ghi chú': 'Bộ phận công tác: Phân Phối Vé · Trưởng bộ',
        },
      },
      {
        id: 'tu-7',
        title: 'Nạp tiền - Sunworld',
        park: 'Nạp tiền',
        owner: 'NCB',
        date: '28/04/2026',
        amount: '490.000.000 ₫',
        status: 'Hoàn thành',
        tone: 'green',
        details: {
          'Số tài khoản': '1SB2B24',
          'Ngân hàng': 'NCB',
          'Ghi chú': 'Bộ phận công tác: Phân Phối Vé · Kế Toán',
        },
      },
      {
        id: 'tu-8',
        title: 'Nạp tiền - Vin Cửa Hội',
        park: 'Nạp tiền',
        owner: 'Techcombank',
        date: '28/04/2026',
        amount: '50.000.000 ₫',
        status: 'Hoàn thành',
        tone: 'green',
        details: {
          'Số tài khoản': '19139932758899',
          'Ngân hàng': 'Techcombank',
          'Ghi chú': 'Bộ phận công tác: Phân Phối Vé · Kế Toán',
        },
      },
    ],
  },
  {
    id: 'that-bai',
    title: 'Thất bại',
    icon: 'alert',
    tasks: [
      {
        id: 'tu-9',
        title: 'Nạp tiền - Vin Vũ Yên (Lần 2)',
        park: 'Nạp tiền',
        owner: '—',
        date: '15/06/2026',
        amount: '50.000.000 ₫',
        status: 'Thất bại',
        tone: 'red',
        details: {
          'Số tài khoản': '—',
          'Ngân hàng': '—',
          'Ghi chú': 'Sai số tài khoản, cần kiểm tra lại',
        },
      },
      {
        id: 'tu-10',
        title: 'Thanh toán Công nợ - Sealinks',
        park: 'Công nợ',
        owner: 'Vietcombank',
        date: '16/09/2025',
        amount: '8.110.000 ₫',
        status: 'Thất bại',
        tone: 'red',
        details: {
          'Số tài khoản': '1100030038237',
          'Ngân hàng': 'Vietcombank',
          'Ghi chú': 'Sai thông tin tài khoản thụ hưởng',
        },
      },
    ],
  },
]

export const refundWorkflow: KanbanColumn[] = [
  {
    id: 'new',
    title: 'Tiếp nhận',
    icon: 'inbox',
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
