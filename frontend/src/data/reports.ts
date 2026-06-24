export type BadgeTone = 'green' | 'red' | 'gray' | 'amber' | 'blue' | 'indigo'

export interface TableColumn {
  key: string
  label: string
  align?: 'left' | 'center' | 'right'
}

export type TableCell =
  | string
  | { kind: 'strong'; text: string }
  | { kind: 'muted'; text: string }
  | { kind: 'amount'; text: string; tone: 'green' | 'red' }
  | { kind: 'badge'; text: string; tone: BadgeTone }
  | { kind: 'actions'; actions: { icon: string; label: string }[] }

export interface TableRow {
  id: string
  search: string
  date?: string
  filters?: Record<string, string>
  cells: TableCell[]
}

export interface ReportFilter {
  key: string
  label: string
  options: { value: string; label: string }[]
}

export interface ReportStat {
  label: string
  value: string
  tone?: 'green' | 'red'
}

export interface ReportTabConfig {
  id: string
  label: string
  columns: TableColumn[]
  rows: TableRow[]
  searchPlaceholder?: string
  filters?: ReportFilter[]
  dateFilter?: boolean
  pageSize?: number
  addLabel?: string
}

export interface ReportPageConfig {
  title: string
  subtitle: string
  stats?: ReportStat[]
  tabs: ReportTabConfig[]
}

export type ReportPageKey =
  | 'parkList'
  | 'dailyBalances'
  | 'ticketCosts'
  | 'reconciliation'
  | 'parkRefunds'
  | 'customerRefundStatus'

const strong = (text: string): TableCell => ({ kind: 'strong', text })
const muted = (text: string): TableCell => ({ kind: 'muted', text })
const green = (text: string): TableCell => ({ kind: 'amount', text, tone: 'green' })
const red = (text: string): TableCell => ({ kind: 'amount', text, tone: 'red' })
const badge = (text: string, tone: BadgeTone): TableCell => ({ kind: 'badge', text, tone })
const actions = (...items: string[]): TableCell => ({
  kind: 'actions',
  actions: items.map((item) => ({ icon: item, label: item === '✏️' ? 'Sửa' : item === '🗑️' ? 'Xoá' : 'Chi tiết' })),
})

const parkColumns: TableColumn[] = [
  { key: 'idx', label: '#' },
  { key: 'name', label: 'Tên khu vui chơi' },
  { key: 'code', label: 'Mã KVC' },
  { key: 'location', label: 'Địa điểm' },
  { key: 'balance', label: 'Số dư hiện tại' },
  { key: 'total', label: 'Tổng đã nạp' },
  { key: 'latest', label: 'Nạp gần nhất' },
  { key: 'status', label: 'Trạng thái' },
]

const debtColumns: TableColumn[] = [
  { key: 'idx', label: '#' },
  { key: 'name', label: 'Tên khu vui chơi' },
  { key: 'code', label: 'Mã KVC' },
  { key: 'location', label: 'Địa điểm' },
  { key: 'debt', label: 'Công nợ hiện tại' },
  { key: 'limit', label: 'Hạn mức công nợ' },
  { key: 'due', label: 'Ngày đến hạn' },
  { key: 'status', label: 'Trạng thái' },
]

const dateColumns: TableColumn[] = [
  { key: 'date', label: 'Ngày' },
  { key: 'park', label: 'Khu vui chơi' },
  { key: 'opening', label: 'Đầu ngày' },
  { key: 'in', label: 'Phát sinh tăng' },
  { key: 'out', label: 'Phát sinh giảm' },
  { key: 'closing', label: 'Cuối ngày' },
  { key: 'status', label: 'Trạng thái' },
]

const costColumns: TableColumn[] = [
  { key: 'date', label: 'Ngày bán' },
  { key: 'park', label: 'KVC' },
  { key: 'agent', label: 'Đại lý' },
  { key: 'ticket', label: 'Loại vé' },
  { key: 'qty', label: 'SL', align: 'right' },
  { key: 'cost', label: 'Giá vốn' },
  { key: 'total', label: 'Tổng giá vốn' },
  { key: 'status', label: 'Trạng thái' },
]

const reconcileColumns: TableColumn[] = [
  { key: 'date', label: 'Ngày' },
  { key: 'park', label: 'KVC' },
  { key: 'system', label: 'Hệ thống' },
  { key: 'partner', label: 'Đối tác' },
  { key: 'diff', label: 'Chênh lệch' },
  { key: 'note', label: 'Ghi chú' },
  { key: 'status', label: 'Trạng thái' },
]

const refundColumns: TableColumn[] = [
  { key: 'booking', label: 'Mã đặt vé' },
  { key: 'park', label: 'KVC' },
  { key: 'customer', label: 'Khách hàng' },
  { key: 'amount', label: 'Số tiền' },
  { key: 'date', label: 'Ngày yêu cầu' },
  { key: 'method', label: 'Phương thức' },
  { key: 'status', label: 'Trạng thái' },
  { key: 'actions', label: 'Hành động' },
]

const statusOptions = [
  { value: 'active', label: 'Hoạt động' },
  { value: 'warning', label: 'Cảnh báo' },
  { value: 'paused', label: 'Tạm dừng' },
]

const refundStatusOptions = [
  { value: 'new', label: 'Mới tạo' },
  { value: 'processing', label: 'Đang xử lý' },
  { value: 'done', label: 'Hoàn tất' },
  { value: 'rejected', label: 'Từ chối' },
]

export const reportPages: Record<ReportPageKey, ReportPageConfig> = {
  parkList: {
    title: 'Danh sách khu vui chơi',
    subtitle: 'Quản lý thông tin và trạng thái các khu vui chơi',
    tabs: [
      {
        id: 'nap-tien',
        label: 'Số dư KVC nạp tiền',
        columns: parkColumns,
        searchPlaceholder: '🔍  Tìm khu vui chơi...',
        filters: [{ key: 'status', label: 'Tất cả trạng thái', options: statusOptions }],
        rows: [
          { id: 'pl-1', search: 'Bản Mòng KVC-001 Sơn La', filters: { status: 'active' }, cells: [muted('01'), strong('Bản Mòng'), muted('KVC-001'), 'Sơn La', green('75,000,000 đ'), '500,000,000 đ', '20/06/2026', badge('● Hoạt động', 'green')] },
          { id: 'pl-2', search: 'Sun Group KVC-002 Đà Nẵng', filters: { status: 'active' }, cells: [muted('02'), strong('Sun Group'), muted('KVC-002'), 'Đà Nẵng', green('120,500,000 đ'), '800,000,000 đ', '18/06/2026', badge('● Hoạt động', 'green')] },
          { id: 'pl-3', search: 'Đồi Rồng KVC-003 Hải Phòng', filters: { status: 'warning' }, cells: [muted('03'), strong('Đồi Rồng'), muted('KVC-003'), 'Hải Phòng', red('8,200,000 đ'), '350,000,000 đ', '10/06/2026', badge('⚠ Sắp hết', 'amber')] },
          { id: 'pl-4', search: 'Delight KVC-004 Hà Nội', filters: { status: 'active' }, cells: [muted('04'), strong('Delight'), muted('KVC-004'), 'Hà Nội', green('54,000,000 đ'), '200,000,000 đ', '25/06/2026', badge('● Hoạt động', 'green')] },
          { id: 'pl-5', search: 'Samten Hills Dalat KVC-005 Đà Lạt', filters: { status: 'active' }, cells: [muted('05'), strong('Samten Hills Dalat'), muted('KVC-005'), 'Đà Lạt, Lâm Đồng', green('44,800,000 đ'), '420,000,000 đ', '22/06/2026', badge('● Hoạt động', 'green')] },
        ],
      },
      {
        id: 'cong-no',
        label: 'Số dư KVC công nợ',
        columns: debtColumns,
        searchPlaceholder: '🔍  Tìm khu vui chơi...',
        filters: [{ key: 'status', label: 'Tất cả trạng thái', options: statusOptions }],
        rows: [
          { id: 'pd-1', search: 'TLTY KVC-001', filters: { status: 'active' }, cells: [muted('01'), strong('TLTY'), muted('KVC-001'), '—', red('125,000,000 đ'), '200,000,000 đ', '30/06/2026', badge('● Hoạt động', 'green')] },
          { id: 'pd-2', search: 'Sơn Tiên KVC-002 Đồng Nai', filters: { status: 'active' }, cells: [muted('02'), strong('Sơn Tiên'), muted('KVC-002'), 'Đồng Nai', red('89,500,000 đ'), '150,000,000 đ', '15/07/2026', badge('● Hoạt động', 'green')] },
          { id: 'pd-3', search: 'Lumiere KVC-003', filters: { status: 'warning' }, cells: [muted('03'), strong('Lumiere'), muted('KVC-003'), '—', red('210,000,000 đ'), '250,000,000 đ', '05/07/2026', badge('⚠ Gần hạn', 'amber')] },
          { id: 'pd-4', search: 'Mikazuki KVC-004 Đà Nẵng', filters: { status: 'active' }, cells: [muted('04'), strong('Mikazuki'), muted('KVC-004'), 'Đà Nẵng', red('67,000,000 đ'), '100,000,000 đ', '20/07/2026', badge('● Hoạt động', 'green')] },
          { id: 'pd-5', search: 'Hồ Tràm KVC-007', filters: { status: 'warning' }, cells: [muted('05'), strong('Hồ Tràm'), muted('KVC-007'), 'Bà Rịa - Vũng Tàu', red('98,000,000 đ'), '150,000,000 đ', '01/07/2026', badge('⚠ Gần hạn', 'amber')] },
        ],
      },
    ],
  },
  dailyBalances: {
    title: 'Số dư khu vui chơi hàng ngày',
    subtitle: 'Theo dõi biến động số dư nạp tiền và công nợ theo từng ngày',
    stats: [
      { label: 'Tổng số dư nạp tiền', value: '302.5M', tone: 'green' },
      { label: 'Tổng công nợ', value: '534.2M', tone: 'red' },
      { label: 'KVC cần chú ý', value: '3' },
    ],
    tabs: [
      {
        id: 'nap',
        label: 'Số dư KVC nạp tiền',
        columns: dateColumns,
        searchPlaceholder: '🔍  Tìm tên KVC...',
        dateFilter: true,
        rows: [
          { id: 'db-1', search: 'Bản Mòng', date: '2026-06-25', cells: ['25/06/2026', strong('Bản Mòng'), green('80,000,000 đ'), green('20,000,000 đ'), red('25,000,000 đ'), green('75,000,000 đ'), badge('Đã chốt', 'green')] },
          { id: 'db-2', search: 'Sun Group', date: '2026-06-24', cells: ['24/06/2026', strong('Sun Group'), green('110,000,000 đ'), green('35,000,000 đ'), red('24,500,000 đ'), green('120,500,000 đ'), badge('Đã chốt', 'green')] },
          { id: 'db-3', search: 'Đồi Rồng', date: '2026-06-23', cells: ['23/06/2026', strong('Đồi Rồng'), green('21,000,000 đ'), '0 đ', red('12,800,000 đ'), red('8,200,000 đ'), badge('Cảnh báo', 'amber')] },
        ],
      },
      {
        id: 'cn',
        label: 'Số dư KVC công nợ',
        columns: dateColumns,
        searchPlaceholder: '🔍  Tìm tên KVC...',
        dateFilter: true,
        rows: [
          { id: 'dc-1', search: 'TLTY', date: '2026-06-25', cells: ['25/06/2026', strong('TLTY'), red('116,000,000 đ'), red('12,500,000 đ'), green('3,500,000 đ'), red('125,000,000 đ'), badge('Gần hạn', 'amber')] },
          { id: 'dc-2', search: 'Sơn Tiên', date: '2026-06-24', cells: ['24/06/2026', strong('Sơn Tiên'), red('92,000,000 đ'), red('8,000,000 đ'), green('10,500,000 đ'), red('89,500,000 đ'), badge('Đã chốt', 'green')] },
          { id: 'dc-3', search: 'Lumiere', date: '2026-06-23', cells: ['23/06/2026', strong('Lumiere'), red('198,000,000 đ'), red('12,000,000 đ'), '0 đ', red('210,000,000 đ'), badge('Cảnh báo', 'amber')] },
        ],
      },
    ],
  },
  ticketCosts: {
    title: 'Chi tiết giá vốn vé bán',
    subtitle: 'Đối chiếu giá vốn từng loại vé theo KVC và đại lý',
    tabs: [
      {
        id: 'nap',
        label: 'KVC nạp tiền',
        columns: costColumns,
        searchPlaceholder: '🔍  Tìm tên KVC, đại lý, loại vé...',
        dateFilter: true,
        rows: [
          { id: 'tc-1', search: 'Bản Mòng ezCloud vé người lớn', date: '2026-06-25', cells: ['25/06/2026', strong('Bản Mòng'), 'ezCloud OTA', 'Vé người lớn', '46', red('180,000 đ'), red('8,280,000 đ'), badge('Đã ghi nhận', 'green')] },
          { id: 'tc-2', search: 'Sun Group Traveloka combo', date: '2026-06-24', cells: ['24/06/2026', strong('Sun Group'), 'Traveloka', 'Combo gia đình', '18', red('620,000 đ'), red('11,160,000 đ'), badge('Đã ghi nhận', 'green')] },
          { id: 'tc-3', search: 'Đồi Rồng Klook vé trẻ em', date: '2026-06-24', cells: ['24/06/2026', strong('Đồi Rồng'), 'Klook', 'Vé trẻ em', '32', red('90,000 đ'), red('2,880,000 đ'), badge('Cần kiểm tra', 'amber')] },
        ],
      },
      {
        id: 'cn',
        label: 'KVC công nợ',
        columns: costColumns,
        searchPlaceholder: '🔍  Tìm tên KVC, đại lý, loại vé...',
        dateFilter: true,
        rows: [
          { id: 'tcc-1', search: 'TLTY VinWonders vé ngày', date: '2026-06-25', cells: ['25/06/2026', strong('TLTY'), 'VinWonders', 'Vé ngày', '25', red('450,000 đ'), red('11,250,000 đ'), badge('Đã ghi nhận', 'green')] },
          { id: 'tcc-2', search: 'Mikazuki đại lý A onsen', date: '2026-06-23', cells: ['23/06/2026', strong('Mikazuki'), 'Đại lý A', 'Onsen package', '12', red('520,000 đ'), red('6,240,000 đ'), badge('Đã ghi nhận', 'green')] },
        ],
      },
    ],
  },
  reconciliation: {
    title: 'Đối soát Khu vui chơi',
    subtitle: 'So sánh doanh thu hệ thống với số liệu đối tác',
    tabs: [
      {
        id: 'nap',
        label: 'KVC nạp tiền',
        columns: reconcileColumns,
        searchPlaceholder: '🔍  Tìm mã KVC, tên KVC...',
        dateFilter: true,
        filters: [{ key: 'variance', label: 'Tất cả chênh lệch', options: [{ value: 'none', label: 'Không lệch' }, { value: 'has', label: 'Có lệch' }] }],
        rows: [
          { id: 'rc-1', search: 'Bản Mòng KVC-001', date: '2026-06-25', filters: { variance: 'none' }, cells: ['25/06/2026', strong('Bản Mòng'), green('84,250,000 đ'), green('84,250,000 đ'), '0 đ', muted('Khớp'), badge('Đã đối soát', 'green')] },
          { id: 'rc-2', search: 'Sun Group KVC-002', date: '2026-06-24', filters: { variance: 'has' }, cells: ['24/06/2026', strong('Sun Group'), green('122,450,000 đ'), green('121,900,000 đ'), red('550,000 đ'), 'Lệch 1 booking', badge('Cần xử lý', 'amber')] },
          { id: 'rc-3', search: 'Đồi Rồng KVC-003', date: '2026-06-23', filters: { variance: 'none' }, cells: ['23/06/2026', strong('Đồi Rồng'), green('42,300,000 đ'), green('42,300,000 đ'), '0 đ', muted('Khớp'), badge('Đã đối soát', 'green')] },
        ],
      },
      {
        id: 'cn',
        label: 'KVC công nợ',
        columns: reconcileColumns,
        searchPlaceholder: '🔍  Tìm mã KVC, tên KVC...',
        dateFilter: true,
        filters: [{ key: 'variance', label: 'Tất cả chênh lệch', options: [{ value: 'none', label: 'Không lệch' }, { value: 'has', label: 'Có lệch' }] }],
        rows: [
          { id: 'rcc-1', search: 'TLTY KVC-001', date: '2026-06-25', filters: { variance: 'has' }, cells: ['25/06/2026', strong('TLTY'), green('68,400,000 đ'), green('69,000,000 đ'), red('-600,000 đ'), 'Chờ file bổ sung', badge('Cần xử lý', 'amber')] },
          { id: 'rcc-2', search: 'Sơn Tiên KVC-002', date: '2026-06-24', filters: { variance: 'none' }, cells: ['24/06/2026', strong('Sơn Tiên'), green('39,200,000 đ'), green('39,200,000 đ'), '0 đ', muted('Khớp'), badge('Đã đối soát', 'green')] },
        ],
      },
    ],
  },
  parkRefunds: {
    title: 'KVC hoàn tiền',
    subtitle: 'Quản lý yêu cầu hoàn tiền theo nhóm KVC nạp tiền và công nợ',
    stats: [
      { label: 'Tổng yêu cầu', value: '18' },
      { label: 'Đã hoàn tất', value: '12', tone: 'green' },
      { label: 'Đang xử lý', value: '6', tone: 'red' },
    ],
    tabs: [
      {
        id: 'nap',
        label: 'KVC nạp tiền',
        columns: refundColumns,
        searchPlaceholder: '🔍  Tìm mã đặt vé, mã KVC, tên KVC...',
        dateFilter: true,
        addLabel: '+ Thêm hoàn tiền',
        filters: [{ key: 'status', label: 'Tất cả trạng thái', options: refundStatusOptions }],
        rows: [
          { id: 'rf-1', search: 'BK-240625-001 Bản Mòng Nguyễn Văn A', date: '2026-06-25', filters: { status: 'processing' }, cells: [strong('BK-240625-001'), 'Bản Mòng', 'Nguyễn Văn A', red('1,250,000 đ'), '25/06/2026', 'Chuyển khoản', badge('Đang xử lý', 'amber'), actions('👁️', '✏️')] },
          { id: 'rf-2', search: 'BK-240624-014 Sun Group Trần Thị B', date: '2026-06-24', filters: { status: 'done' }, cells: [strong('BK-240624-014'), 'Sun Group', 'Trần Thị B', red('820,000 đ'), '24/06/2026', 'Ví điện tử', badge('Hoàn tất', 'green'), actions('👁️')] },
          { id: 'rf-3', search: 'BK-240623-102 Đồi Rồng Lê Minh C', date: '2026-06-23', filters: { status: 'new' }, cells: [strong('BK-240623-102'), 'Đồi Rồng', 'Lê Minh C', red('450,000 đ'), '23/06/2026', 'Chuyển khoản', badge('Mới tạo', 'blue'), actions('👁️', '✏️')] },
        ],
      },
      {
        id: 'cn',
        label: 'KVC công nợ',
        columns: refundColumns,
        searchPlaceholder: '🔍  Tìm mã đặt vé, mã KVC, tên KVC...',
        dateFilter: true,
        addLabel: '+ Thêm hoàn tiền',
        filters: [{ key: 'status', label: 'Tất cả trạng thái', options: refundStatusOptions }],
        rows: [
          { id: 'rfc-1', search: 'BK-240625-071 TLTY Phạm Đức D', date: '2026-06-25', filters: { status: 'processing' }, cells: [strong('BK-240625-071'), 'TLTY', 'Phạm Đức D', red('2,100,000 đ'), '25/06/2026', 'Bù trừ công nợ', badge('Đang xử lý', 'amber'), actions('👁️', '✏️')] },
          { id: 'rfc-2', search: 'BK-240624-033 Sơn Tiên Hoàng Lan E', date: '2026-06-24', filters: { status: 'done' }, cells: [strong('BK-240624-033'), 'Sơn Tiên', 'Hoàng Lan E', red('1,600,000 đ'), '24/06/2026', 'Bù trừ công nợ', badge('Hoàn tất', 'green'), actions('👁️')] },
        ],
      },
    ],
  },
  customerRefundStatus: {
    title: 'Trạng thái hoàn tiền cho khách hàng',
    subtitle: 'Theo dõi tiến độ hoàn tiền và phương thức chi trả cho khách hàng',
    stats: [
      { label: 'Chờ xử lý', value: '4', tone: 'red' },
      { label: 'Đang xử lý', value: '7' },
      { label: 'Đã hoàn tiền', value: '23', tone: 'green' },
    ],
    tabs: [
      {
        id: 'status',
        label: 'Danh sách trạng thái',
        columns: [
          { key: 'booking', label: 'Mã đặt vé' },
          { key: 'customer', label: 'Khách hàng' },
          { key: 'phone', label: 'SĐT' },
          { key: 'amount', label: 'Số tiền' },
          { key: 'method', label: 'Phương thức' },
          { key: 'date', label: 'Ngày cập nhật' },
          { key: 'status', label: 'Trạng thái' },
        ],
        searchPlaceholder: '🔍  Tìm mã đặt vé, tên khách, SĐT...',
        dateFilter: true,
        filters: [
          { key: 'status', label: 'Tất cả trạng thái', options: refundStatusOptions },
          { key: 'method', label: 'Tất cả phương thức', options: [{ value: 'bank', label: 'Chuyển khoản' }, { value: 'wallet', label: 'Ví điện tử' }, { value: 'offset', label: 'Bù trừ' }] },
        ],
        rows: [
          { id: 'cr-1', search: 'BK-240625-001 Nguyễn Văn A 0901234567', date: '2026-06-25', filters: { status: 'processing', method: 'bank' }, cells: [strong('BK-240625-001'), 'Nguyễn Văn A', muted('0901 234 567'), red('1,250,000 đ'), 'Chuyển khoản', '25/06/2026 09:20', badge('Đang xử lý', 'amber')] },
          { id: 'cr-2', search: 'BK-240624-014 Trần Thị B 0902345678', date: '2026-06-24', filters: { status: 'done', method: 'wallet' }, cells: [strong('BK-240624-014'), 'Trần Thị B', muted('0902 345 678'), red('820,000 đ'), 'Ví điện tử', '24/06/2026 16:40', badge('Đã hoàn tiền', 'green')] },
          { id: 'cr-3', search: 'BK-240623-102 Lê Minh C 0903456789', date: '2026-06-23', filters: { status: 'new', method: 'bank' }, cells: [strong('BK-240623-102'), 'Lê Minh C', muted('0903 456 789'), red('450,000 đ'), 'Chuyển khoản', '23/06/2026 11:05', badge('Mới tạo', 'blue')] },
          { id: 'cr-4', search: 'BK-240622-087 Phạm Đức D 0904567890', date: '2026-06-22', filters: { status: 'rejected', method: 'offset' }, cells: [strong('BK-240622-087'), 'Phạm Đức D', muted('0904 567 890'), red('600,000 đ'), 'Bù trừ', '22/06/2026 14:10', badge('Từ chối', 'red')] },
        ],
      },
    ],
  },
}
