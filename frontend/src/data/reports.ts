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
  | 'agencyArTransactions'
  | 'agencyTaTransactions'
  | 'agencyBidvTransactions'
  | 'retailTaBookings'
  | 'retailBankInflows'
  | 'vinTicketCosts'
  | 'vinDailyBalances'
  | 'vinTopUps'
  | 'vinReconciliation'
  | 'otaTaBookings'
  | 'otaBankInflows'

const strong = (text: string): TableCell => ({ kind: 'strong', text })
const muted = (text: string): TableCell => ({ kind: 'muted', text })
const green = (text: string): TableCell => ({ kind: 'amount', text, tone: 'green' })
const red = (text: string): TableCell => ({ kind: 'amount', text, tone: 'red' })
const badge = (text: string, tone: BadgeTone): TableCell => ({ kind: 'badge', text, tone })
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

const agencyArColumns: TableColumn[] = [
  { key: 'booking', label: 'Mã booking' },
  { key: 'agency', label: 'Tên đại lý' },
  { key: 'time', label: 'Ngày giờ giao dịch' },
  { key: 'amount', label: 'Trừ tiền trên AR' },
]

const agencyTaColumns: TableColumn[] = [
  { key: 'booking', label: 'Mã booking' },
  { key: 'agency', label: 'Tên đại lý' },
  { key: 'time', label: 'Ngày tạo giờ' },
  { key: 'amount', label: 'Số tiền' },
]

const bankStatementColumns: TableColumn[] = [
  { key: 'fromMail', label: 'From mail' },
  { key: 'postingDate', label: 'Ngày ghi sổ' },
  { key: 'accountingDate', label: 'Ngày hạch toán' },
  { key: 'txCode', label: 'Mã giao dịch' },
  { key: 'debit', label: 'Phát sinh nợ', align: 'right' },
  { key: 'credit', label: 'Phát sinh có', align: 'right' },
  { key: 'balance', label: 'Số dư', align: 'right' },
  { key: 'docNo', label: 'Số chứng từ' },
  { key: 'description', label: 'Diễn giải' },
]

const retailTaColumns: TableColumn[] = [
  { key: 'booking', label: 'Mã booking' },
  { key: 'customer', label: 'Tên khách hàng' },
  { key: 'phone', label: 'SĐT' },
  { key: 'time', label: 'Ngày tạo giờ' },
  { key: 'amount', label: 'Số tiền' },
]

const vinDailyBalanceColumns: TableColumn[] = [
  { key: 'idx', label: 'STT' },
  { key: 'facility', label: 'Cơ sở' },
  { key: 'account', label: 'TK Công nợ' },
  { key: 'date', label: 'Ngày' },
  { key: 'balance', label: 'Số dư hiện tại (Tạm tính)', align: 'right' },
  { key: 'dueDays', label: 'Số ngày đáo hạn' },
]

const statusOptions = [
  { value: 'active', label: 'Hoạt động' },
  { value: 'warning', label: 'Cảnh báo' },
  { value: 'paused', label: 'Tạm dừng' },
]

const vinFacilities: { name: string; account: string; dueDays: string }[] = [
  { name: 'Phú Quốc', account: '20003041-PQ', dueDays: '7 ngày' },
  { name: 'Nam Hội An', account: '20003041-NHA', dueDays: '7 ngày' },
  { name: 'Hà Nội', account: '20003041-VVHN', dueDays: '7 ngày' },
  { name: 'Timescity', account: '20003041-TC', dueDays: '7 ngày' },
  { name: 'VINWONDERS TIMES CITY (V…)', account: '20003041-TC', dueDays: '7 ngày' },
  { name: 'Nha Trang', account: '20003041-NT', dueDays: '7 ngày' },
  { name: 'Nha Trang Hòn Tằm', account: '20003041-NTHT', dueDays: '7 ngày' },
  { name: 'VinWonders Cửa Hội', account: '20003041-VVCH', dueDays: '7 ngày' },
  { name: 'CÔNG VIÊN GRAND PARK', account: '20003041-VVGP', dueDays: '7 ngày' },
  { name: 'VinWonders Vũ Yên', account: '20003041-VVVY', dueDays: '7 ngày' },
  { name: 'Vinpearl HO', account: '20003041-VPHO', dueDays: '0 ngày' },
]

// Giá trị lấy đúng theo ảnh sao kê; ô "-290,40…" bị cắt do độ rộng cột trong ảnh gốc, chưa xác nhận được đủ số.
const vinDailyBalanceData: { date: string; display: string; values: string[]; highlightIdx?: number }[] = [
  {
    date: '2026-04-28',
    display: '28/04/2026',
    values: ['-99,315,500', '-45,157,200', '-202,893,700', '-30,623,500', '-30,623,500', '-26,795,740', '0.00', '-62,263,050', '-290,40…', '-207,966,200', '0.00'],
  },
  {
    date: '2026-04-29',
    display: '29/04/2026',
    values: ['-88,931,000', '-34,591,200', '-198,567,200', '-25,183,500', '-25,183,500', '-20,691,540', '0.00', '-39,389,550', '-290,40…', '-264,254,200', '0.00'],
    highlightIdx: 4,
  },
  {
    date: '2026-04-30',
    display: '30/04/2026',
    values: ['-28,742,600', '-7,870,200', '-110,430,700', '-6,058,500', '-6,058,500', '13,330,060', '0.00', '-82,771,050', '-290,40…', '-21,742,200', '0.00'],
    highlightIdx: 4,
  },
  {
    date: '2026-05-01',
    display: '01/05/2026',
    values: ['-95,520,600', '-39,348,200', '-104,940,200', '-41,000,000'],
    highlightIdx: 3,
  },
]

function vinBalanceCell(text: string, highlighted: boolean): TableCell {
  if (highlighted) return badge(`${text} đ`, 'amber')
  if (text.startsWith('-')) return red(`${text} đ`)
  if (text === '0.00') return muted('0 đ')
  return green(`${text} đ`)
}

const vinDailyBalanceRows: TableRow[] = vinDailyBalanceData.flatMap((day) =>
  day.values.map((value, i) => {
    const facility = vinFacilities[i]
    return {
      id: `vin-${day.date}-${i}`,
      search: `${facility.name} ${facility.account}`,
      date: day.date,
      cells: [
        muted(String(i + 1)),
        strong(facility.name),
        muted(facility.account),
        day.display,
        vinBalanceCell(value, day.highlightIdx === i),
        facility.dueDays,
      ],
    }
  }),
)

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
  agencyArTransactions: {
    title: 'Giao dịch của các đại lý trên AR',
    subtitle: 'Danh sách giao dịch trừ tiền của các đại lý trên hệ thống AR (ar.ezcloud.vn)',
    tabs: [
      {
        id: 'ar',
        label: 'Giao dịch AR',
        columns: agencyArColumns,
        searchPlaceholder: '🔍  Tìm mã booking, tên đại lý...',
        dateFilter: true,
        rows: [
          { id: 'ar-tx-1', search: '22148483962 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('22148483962'), 'Oneinventory_Hanh BANA', '28/04/2026 03:34:08', red('1,202,200 đ')] },
          { id: 'ar-tx-2', search: '27838613502 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('27838613502'), 'Oneinventory_Hanh BANA', '28/04/2026 05:08:59', red('462,000 đ')] },
          { id: 'ar-tx-3', search: '29294515902 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('29294515902'), 'Oneinventory_Hanh BANA', '28/04/2026 05:33:14', red('461,100 đ')] },
          { id: 'ar-tx-4', search: '27979518101 Oneinventory_Anh Thư', date: '2026-04-28', cells: [strong('27979518101'), 'Oneinventory_Anh Thư', '28/04/2026 05:41:37', red('1,803,300 đ')] },
          { id: 'ar-tx-5', search: '30712268284 Oneinventory_Anh Thư', date: '2026-04-28', cells: [strong('30712268284'), 'Oneinventory_Anh Thư', '28/04/2026 05:56:52', red('4,686,600 đ')] },
          { id: 'ar-tx-6', search: '30931514955 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('30931514955'), 'Oneinventory_Hanh BANA', '28/04/2026 06:00:31', red('1,562,200 đ')] },
          { id: 'ar-tx-7', search: '31639787574 Oneinventory_Anh Thư', date: '2026-04-28', cells: [strong('31639787574'), 'Oneinventory_Anh Thư', '28/04/2026 06:12:20', red('5,548,800 đ')] },
          { id: 'ar-tx-8', search: '32822213849 Oneinventory_NhatKimYenTicket', date: '2026-04-28', cells: [strong('32822213849'), 'Oneinventory_NhatKimYenTicket', '28/04/2026 06:32:01', red('652,000 đ')] },
          { id: 'ar-tx-9', search: '33123808770 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('33123808770'), 'Oneinventory_Hanh BANA', '28/04/2026 06:37:04', red('5,287,700 đ')] },
          { id: 'ar-tx-10', search: '33326936162 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('33326936162'), 'Oneinventory_Hanh BANA', '28/04/2026 06:40:27', red('1,383,300 đ')] },
          { id: 'ar-tx-11', search: '33708865388 Oneinventory_AGSAPA', date: '2026-04-28', cells: [strong('33708865388'), 'Oneinventory_AGSAPA', '28/04/2026 06:46:48', red('720,000 đ')] },
          { id: 'ar-tx-12', search: '34484009830 Oneinventory_Sao Mai Sa Pa', date: '2026-04-28', cells: [strong('34484009830'), 'Oneinventory_Sao Mai Sa Pa', '28/04/2026 06:59:44', red('3,166,600 đ')] },
          { id: 'ar-tx-13', search: '34649332148 Oneinventory_Phương Lan', date: '2026-04-28', cells: [strong('34649332148'), 'Oneinventory_Phương Lan', '28/04/2026 07:02:29', red('3,005,500 đ')] },
          { id: 'ar-tx-14', search: '34875907369 Oneinventory_AGSAPA', date: '2026-04-28', cells: [strong('34875907369'), 'Oneinventory_AGSAPA', '28/04/2026 07:06:16', red('1,568,400 đ')] },
          { id: 'ar-tx-15', search: '35640282480 Oneinventory_Sao Mai Sa Pa', date: '2026-04-28', cells: [strong('35640282480'), 'Oneinventory_Sao Mai Sa Pa', '28/04/2026 07:19:00', red('23,526,000 đ')] },
          { id: 'ar-tx-16', search: '35718398635 Oneinventory_Sao Mai Sa Pa', date: '2026-04-28', cells: [strong('35718398635'), 'Oneinventory_Sao Mai Sa Pa', '28/04/2026 07:20:22', red('15,684,000 đ')] },
          { id: 'ar-tx-17', search: '35818491208 Oneinventory_Sao Mai Sa Pa', date: '2026-04-28', cells: [strong('35818491208'), 'Oneinventory_Sao Mai Sa Pa', '28/04/2026 07:21:58', red('21,957,600 đ')] },
          { id: 'ar-tx-18', search: '36275692551 Oneinventory_AGSAPA', date: '2026-04-28', cells: [strong('36275692551'), 'Oneinventory_AGSAPA', '28/04/2026 07:29:36', red('1,280,000 đ')] },
          { id: 'ar-tx-19', search: '36290702590 Oneinventory_Cát Bà Vi Vu', date: '2026-04-28', cells: [strong('36290702590'), 'Oneinventory_Cát Bà Vi Vu', '28/04/2026 07:29:50', red('301,000 đ')] },
          { id: 'ar-tx-20', search: '36670253146 Oneinventory_Sao Mai Sa Pa', date: '2026-04-28', cells: [strong('36670253146'), 'Oneinventory_Sao Mai Sa Pa', '28/04/2026 07:36:10', red('9,019,800 đ')] },
          { id: 'ar-tx-21', search: '36787521807 Oneinventory_Cát Bà Vi Vu', date: '2026-04-28', cells: [strong('36787521807'), 'Oneinventory_Cát Bà Vi Vu', '28/04/2026 07:38:08', red('2,474,800 đ')] },
          { id: 'ar-tx-22', search: '36968911046 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('36968911046'), 'Oneinventory_Hanh BANA', '28/04/2026 07:41:09', red('301,000 đ')] },
          { id: 'ar-tx-23', search: '37110410446 Oneinventory_Phương Lan', date: '2026-04-28', cells: [strong('37110410446'), 'Oneinventory_Phương Lan', '28/04/2026 07:43:30', red('1,452,100 đ')] },
        ],
      },
    ],
  },
  agencyTaTransactions: {
    title: 'Giao dịch của các đại lý trên TA',
    subtitle: 'Danh sách giao dịch của các đại lý trên hệ thống TA',
    tabs: [
      {
        id: 'ta',
        label: 'Giao dịch TA',
        columns: agencyTaColumns,
        searchPlaceholder: '🔍  Tìm mã booking, tên đại lý...',
        dateFilter: true,
        rows: [
          { id: 'ta-tx-1', search: '60023151211 Oneinventory_SJC Thủy cung Lotte', date: '2025-09-01', cells: [strong('60023151211'), 'Oneinventory_SJC Thủy cung Lotte', '01/09/2025', red('886,000 đ')] },
          { id: 'ta-tx-2', search: '22148483962 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('22148483962'), 'Oneinventory_Hanh BANA', '28/04/2026', red('1,202,200 đ')] },
          { id: 'ta-tx-3', search: '27838613502 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('27838613502'), 'Oneinventory_Hanh BANA', '28/04/2026', red('462,200 đ')] },
          { id: 'ta-tx-4', search: '29294515996 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('29294515996'), 'Oneinventory_Hanh BANA', '28/04/2026', red('461,100 đ')] },
          { id: 'ta-tx-5', search: '29797518101 Oneinventory_Anh Thư', date: '2026-04-28', cells: [strong('29797518101'), 'Oneinventory_Anh Thư', '28/04/2026', red('1,803,300 đ')] },
          { id: 'ta-tx-6', search: '30712268284 Oneinventory_Anh Thư', date: '2026-04-28', cells: [strong('30712268284'), 'Oneinventory_Anh Thư', '28/04/2026', red('4,686,600 đ')] },
          { id: 'ta-tx-7', search: '30931514955 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('30931514955'), 'Oneinventory_Hanh BANA', '28/04/2026', red('1,562,200 đ')] },
          { id: 'ta-tx-8', search: '31639787574 Oneinventory_Anh Thư', date: '2026-04-28', cells: [strong('31639787574'), 'Oneinventory_Anh Thư', '28/04/2026', red('5,548,800 đ')] },
          { id: 'ta-tx-9', search: '32822113849 Oneinventory_NhatKimYenTicket', date: '2026-04-28', cells: [strong('32822113849'), 'Oneinventory_NhatKimYenTicket', '28/04/2026', red('652,000 đ')] },
          { id: 'ta-tx-10', search: '33123808770 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('33123808770'), 'Oneinventory_Hanh BANA', '28/04/2026', red('5,287,700 đ')] },
          { id: 'ta-tx-11', search: '33326936162 Oneinventory_Hanh BANA', date: '2026-04-28', cells: [strong('33326936162'), 'Oneinventory_Hanh BANA', '28/04/2026', red('1,383,300 đ')] },
          { id: 'ta-tx-12', search: '33708865388 Oneinventory_AGSAPA', date: '2026-04-28', cells: [strong('33708865388'), 'Oneinventory_AGSAPA', '28/04/2026', red('720,000 đ')] },
          { id: 'ta-tx-13', search: '34484009830 Oneinventory_Sao Mai Sa Pa', date: '2026-04-28', cells: [strong('34484009830'), 'Oneinventory_Sao Mai Sa Pa', '28/04/2026', red('3,166,600 đ')] },
        ],
      },
    ],
  },
  agencyBidvTransactions: {
    title: 'Giao dịch đại lý nạp tiền trên BIDV',
    subtitle: 'Sao kê giao dịch nạp tiền của các đại lý trên tài khoản BIDV, lấy từ email báo có',
    tabs: [
      {
        id: 'bidv',
        label: 'Giao dịch BIDV',
        columns: bankStatementColumns,
        searchPlaceholder: '🔍  Tìm số chứng từ, nội dung...',
        dateFilter: true,
        rows: [
          {
            id: 'bidv-tx-1',
            search: 'insaoke@bidv.com 51768 990CTL NH4 990000 TKThe 7939666999 Techcombank',
            date: '2025-11-05',
            cells: [
              muted('insaoke@bidv.com'),
              '05/11/2025',
              '05/11/2025',
              'DD',
              '0',
              green('3,734,500 đ'),
              '1,850,983,986 đ',
              muted('51768'),
              'TKThe :7939666999, tai Techcombank: @V...',
            ],
          },
        ],
      },
    ],
  },
  retailTaBookings: {
    title: 'Booking khách lẻ trên TA',
    subtitle: 'Danh sách booking của khách lẻ đồng bộ từ hệ thống TA',
    tabs: [
      {
        id: 'ta',
        label: 'Booking TA',
        columns: retailTaColumns,
        searchPlaceholder: '🔍  Tìm mã booking, tên khách, SĐT...',
        dateFilter: true,
        rows: [
          { id: 'rt-1', search: '41203551201 Nguyễn Văn An 0912345678', date: '2026-06-25', cells: [strong('41203551201'), 'Nguyễn Văn An', muted('0912 345 678'), '25/06/2026 08:12:03', green('850,000 đ')] },
          { id: 'rt-2', search: '41203551845 Trần Thị Bích 0923456789', date: '2026-06-25', cells: [strong('41203551845'), 'Trần Thị Bích', muted('0923 456 789'), '25/06/2026 09:47:21', green('1,250,000 đ')] },
          { id: 'rt-3', search: '41203552390 Lê Hoàng Cường 0934567890', date: '2026-06-24', cells: [strong('41203552390'), 'Lê Hoàng Cường', muted('0934 567 890'), '24/06/2026 14:30:56', green('420,000 đ')] },
          { id: 'rt-4', search: '41203553012 Phạm Thị Dung 0945678901', date: '2026-06-24', cells: [strong('41203553012'), 'Phạm Thị Dung', muted('0945 678 901'), '24/06/2026 16:05:12', green('2,150,000 đ')] },
          { id: 'rt-5', search: '41203553688 Hoàng Văn Em 0956789012', date: '2026-06-23', cells: [strong('41203553688'), 'Hoàng Văn Em', muted('0956 789 012'), '23/06/2026 10:22:47', green('680,000 đ')] },
          { id: 'rt-6', search: '41203554201 Vũ Thị Giang 0967890123', date: '2026-06-23', cells: [strong('41203554201'), 'Vũ Thị Giang', muted('0967 890 123'), '23/06/2026 19:14:03', green('1,020,000 đ')] },
        ],
      },
    ],
  },
  retailBankInflows: {
    title: 'Tiền về ngân hàng',
    subtitle: 'Sao kê ngân hàng ghi nhận tiền khách lẻ chuyển vào, lấy từ email báo có',
    tabs: [
      {
        id: 'bank',
        label: 'Sao kê ngân hàng',
        columns: bankStatementColumns,
        searchPlaceholder: '🔍  Tìm số chứng từ, nội dung...',
        dateFilter: true,
        rows: [
          {
            id: 'rb-1',
            search: 'insaoke@bidv.com 51820 Nguyen Van An',
            date: '2026-06-25',
            cells: [
              muted('insaoke@bidv.com'),
              '25/06/2026',
              '25/06/2026',
              'DD',
              '0',
              green('850,000 đ'),
              '1,851,833,986 đ',
              muted('51820'),
              'TT ve ve BK41203551201, Nguyen Van An chuyen khoan',
            ],
          },
          {
            id: 'rb-2',
            search: 'insaoke@bidv.com 51834 Tran Thi Bich',
            date: '2026-06-25',
            cells: [
              muted('insaoke@bidv.com'),
              '25/06/2026',
              '25/06/2026',
              'DD',
              '0',
              green('1,250,000 đ'),
              '1,853,083,986 đ',
              muted('51834'),
              'TT ve ve BK41203551845, Tran Thi Bich chuyen khoan',
            ],
          },
        ],
      },
    ],
  },
  vinTicketCosts: {
    title: 'Chi tiết giá vốn vé bán',
    subtitle: 'Đối chiếu giá vốn từng loại vé theo KVC con Vin và đại lý mua',
    tabs: [
      {
        id: 'vin-ticket-costs',
        label: 'Giá vốn vé bán',
        columns: costColumns,
        searchPlaceholder: '🔍  Tìm tên KVC, đại lý, loại vé...',
        dateFilter: true,
        rows: [
          { id: 'vtc-1', search: 'Timescity Đại Lý ezCloud Mua_PL vé người lớn', date: '2026-04-28', cells: ['28/04/2026', strong('Timescity'), 'Đại Lý ezCloud Mua_PL', 'Vé người lớn', '38', red('160,000 đ'), red('6,080,000 đ'), badge('Đã ghi nhận', 'green')] },
          { id: 'vtc-2', search: 'VinWonders Vũ Yên Đại Lý ezCloud Mua_PL combo', date: '2026-04-28', cells: ['28/04/2026', strong('VinWonders Vũ Yên'), 'Đại Lý ezCloud Mua_PL', 'Combo gia đình', '15', red('590,000 đ'), red('8,850,000 đ'), badge('Đã ghi nhận', 'green')] },
          { id: 'vtc-3', search: 'Phú Quốc Đại Lý ezCloud Mua_PL vé trẻ em', date: '2026-04-29', cells: ['29/04/2026', strong('Phú Quốc'), 'Đại Lý ezCloud Mua_PL', 'Vé trẻ em', '27', red('85,000 đ'), red('2,295,000 đ'), badge('Đã ghi nhận', 'green')] },
          { id: 'vtc-4', search: 'CÔNG VIÊN GRAND PARK Đại Lý ezCloud Mua_PL vé ngày', date: '2026-04-29', cells: ['29/04/2026', strong('CÔNG VIÊN GRAND PARK'), 'Đại Lý ezCloud Mua_PL', 'Vé ngày', '52', red('120,000 đ'), red('6,240,000 đ'), badge('Cần kiểm tra', 'amber')] },
          { id: 'vtc-5', search: 'Nha Trang Đại Lý ezCloud Mua_PL vé người lớn', date: '2026-04-30', cells: ['30/04/2026', strong('Nha Trang'), 'Đại Lý ezCloud Mua_PL', 'Vé người lớn', '19', red('170,000 đ'), red('3,230,000 đ'), badge('Đã ghi nhận', 'green')] },
          { id: 'vtc-6', search: 'VinWonders Cửa Hội Đại Lý ezCloud Mua_PL onsen', date: '2026-05-01', cells: ['01/05/2026', strong('VinWonders Cửa Hội'), 'Đại Lý ezCloud Mua_PL', 'Onsen package', '9', red('450,000 đ'), red('4,050,000 đ'), badge('Đã ghi nhận', 'green')] },
        ],
      },
    ],
  },
  vinDailyBalances: {
    title: 'Số dư KVC Vin theo ngày',
    subtitle: 'Số dư công nợ tạm tính từng cơ sở Vinpearl/VinWonders theo ngày, cùng số ngày đáo hạn',
    tabs: [
      {
        id: 'vin',
        label: 'Số dư theo ngày',
        columns: vinDailyBalanceColumns,
        searchPlaceholder: '🔍  Tìm tên cơ sở, TK công nợ...',
        dateFilter: true,
        rows: vinDailyBalanceRows,
      },
    ],
  },
  vinTopUps: {
    title: 'Danh sách nạp tiền cho Vin theo ngày',
    subtitle: 'Lịch sử các giao dịch nạp tiền vào tài khoản KVC con của Vin, lấy từ email báo có',
    tabs: [
      {
        id: 'vin-top-up',
        label: 'Giao dịch nạp tiền',
        columns: bankStatementColumns,
        searchPlaceholder: '🔍  Tìm số chứng từ, nội dung...',
        dateFilter: true,
        rows: [
          { id: 'vtu-1', search: 'insaoke@bidv.com 61205 Timescity TKThe M368010106273448', date: '2026-04-28', cells: ['insaoke@bidv.com', '28/04/2026', '28/04/2026', '990CTL', red('50,000,000 đ'), '0 đ', muted('-99,315,500 đ'), '061205', 'TKThe M368010106273448 Timescity nap tien'] },
          { id: 'vtu-2', search: 'insaoke@bidv.com 61340 VinWonders Vu Yen TKThe 19010000888334', date: '2026-04-29', cells: ['insaoke@bidv.com', '29/04/2026', '29/04/2026', '990CTL', red('30,000,000 đ'), '0 đ', muted('-88,931,000 đ'), '061340', 'TKThe 19010000888334 VinWonders Vu Yen nap tien'] },
          { id: 'vtu-3', search: 'insaoke@bidv.com 61478 Phu Quoc TKThe 0091000593278', date: '2026-04-30', cells: ['insaoke@bidv.com', '30/04/2026', '30/04/2026', '990CTL', red('20,000,000 đ'), '0 đ', muted('-28,742,600 đ'), '061478', 'TKThe 0091000593278 Phu Quoc nap tien'] },
        ],
      },
    ],
  },
  vinReconciliation: {
    title: 'Đối soát KVC Vin',
    subtitle: 'So sánh số dư/doanh thu hệ thống tự tính với số liệu đối tác Vin cung cấp',
    tabs: [
      {
        id: 'vin-reconcile',
        label: 'Đối soát theo ngày',
        columns: reconcileColumns,
        searchPlaceholder: '🔍  Tìm tên KVC...',
        dateFilter: true,
        filters: [{ key: 'variance', label: 'Tất cả chênh lệch', options: [{ value: 'none', label: 'Không lệch' }, { value: 'has', label: 'Có lệch' }] }],
        rows: [
          { id: 'vtr-1', search: 'Timescity', date: '2026-04-28', filters: { variance: 'none' }, cells: ['28/04/2026', strong('Timescity'), green('6,080,000 đ'), green('6,080,000 đ'), '0 đ', muted('Khớp'), badge('Đã đối soát', 'green')] },
          { id: 'vtr-2', search: 'VinWonders Vũ Yên', date: '2026-04-28', filters: { variance: 'has' }, cells: ['28/04/2026', strong('VinWonders Vũ Yên'), green('8,850,000 đ'), green('8,700,000 đ'), red('150,000 đ'), 'Lệch 1 giao dịch', badge('Cần xử lý', 'amber')] },
          { id: 'vtr-3', search: 'Phú Quốc', date: '2026-04-29', filters: { variance: 'none' }, cells: ['29/04/2026', strong('Phú Quốc'), green('2,295,000 đ'), green('2,295,000 đ'), '0 đ', muted('Khớp'), badge('Đã đối soát', 'green')] },
          { id: 'vtr-4', search: 'CÔNG VIÊN GRAND PARK', date: '2026-04-29', filters: { variance: 'has' }, cells: ['29/04/2026', strong('CÔNG VIÊN GRAND PARK'), green('6,240,000 đ'), green('6,000,000 đ'), red('240,000 đ'), 'Chờ Vin xác nhận', badge('Cần xử lý', 'amber')] },
          { id: 'vtr-5', search: 'Nha Trang', date: '2026-04-30', filters: { variance: 'none' }, cells: ['30/04/2026', strong('Nha Trang'), green('3,230,000 đ'), green('3,230,000 đ'), '0 đ', muted('Khớp'), badge('Đã đối soát', 'green')] },
        ],
      },
    ],
  },
  otaTaBookings: {
    title: 'Booking API trên TA',
    subtitle: 'Danh sách booking của các đại lý API đồng bộ từ hệ thống TA',
    tabs: [
      {
        id: 'ta',
        label: 'Booking TA',
        columns: agencyTaColumns,
        searchPlaceholder: '🔍  Tìm mã booking, tên đại lý API...',
        dateFilter: true,
        rows: [
          { id: 'ot-1', search: '52301884201 Traveloka', date: '2026-06-25', cells: [strong('52301884201'), 'Traveloka', '25/06/2026 08:40:12', green('1,650,000 đ')] },
          { id: 'ot-2', search: '52301884733 Klook', date: '2026-06-25', cells: [strong('52301884733'), 'Klook', '25/06/2026 10:15:47', green('2,340,000 đ')] },
          { id: 'ot-3', search: '52301885290 Agoda', date: '2026-06-24', cells: [strong('52301885290'), 'Agoda', '24/06/2026 13:52:31', green('980,000 đ')] },
          { id: 'ot-4', search: '52301885811 Booking.com', date: '2026-06-24', cells: [strong('52301885811'), 'Booking.com', '24/06/2026 15:08:19', green('3,120,000 đ')] },
          { id: 'ot-5', search: '52301886402 KKday', date: '2026-06-23', cells: [strong('52301886402'), 'KKday', '23/06/2026 09:33:05', green('760,000 đ')] },
          { id: 'ot-6', search: '52301886955 Traveloka', date: '2026-06-23', cells: [strong('52301886955'), 'Traveloka', '23/06/2026 20:11:44', green('1,410,000 đ')] },
          { id: 'ot-7', search: '52301887502 Expedia', date: '2026-06-22', cells: [strong('52301887502'), 'Expedia', '22/06/2026 11:20:33', green('2,890,000 đ')] },
          { id: 'ot-8', search: '52301888014 Trip.com', date: '2026-06-22', cells: [strong('52301888014'), 'Trip.com', '22/06/2026 14:45:10', green('1,980,000 đ')] },
          { id: 'ot-9', search: '52301888633 Klook', date: '2026-06-21', cells: [strong('52301888633'), 'Klook', '21/06/2026 09:12:55', green('3,450,000 đ')] },
          { id: 'ot-10', search: '52301889120 Agoda', date: '2026-06-21', cells: [strong('52301889120'), 'Agoda', '21/06/2026 16:33:21', green('1,120,000 đ')] },
          { id: 'ot-11', search: '52301889745 Booking.com', date: '2026-06-20', cells: [strong('52301889745'), 'Booking.com', '20/06/2026 10:05:44', green('4,260,000 đ')] },
          { id: 'ot-12', search: '52301890288 Traveloka', date: '2026-06-20', cells: [strong('52301890288'), 'Traveloka', '20/06/2026 18:22:09', green('890,000 đ')] },
          { id: 'ot-13', search: '52301890903 KKday', date: '2026-06-19', cells: [strong('52301890903'), 'KKday', '19/06/2026 07:50:16', green('1,560,000 đ')] },
          { id: 'ot-14', search: '52301891477 Expedia', date: '2026-06-19', cells: [strong('52301891477'), 'Expedia', '19/06/2026 21:15:38', green('2,230,000 đ')] },
        ],
      },
    ],
  },
  otaBankInflows: {
    title: 'Tiền về ngân hàng',
    subtitle: 'Sao kê ngân hàng ghi nhận tiền các đại lý API chuyển vào, lấy từ email báo có',
    tabs: [
      {
        id: 'bank',
        label: 'Sao kê ngân hàng',
        columns: bankStatementColumns,
        searchPlaceholder: '🔍  Tìm số chứng từ, nội dung...',
        dateFilter: true,
        rows: [
          {
            id: 'ob-1',
            search: 'insaoke@bidv.com 61920 Traveloka',
            date: '2026-06-25',
            cells: [
              muted('insaoke@bidv.com'),
              '25/06/2026',
              '25/06/2026',
              'DD',
              '0',
              green('1,650,000 đ'),
              '1,854,733,986 đ',
              muted('61920'),
              'TT ve ve BK52301884201, Traveloka chuyen khoan',
            ],
          },
          {
            id: 'ob-2',
            search: 'insaoke@bidv.com 61921 Klook',
            date: '2026-06-25',
            cells: [
              muted('insaoke@bidv.com'),
              '25/06/2026',
              '25/06/2026',
              'DD',
              '0',
              green('2,300,000 đ'),
              '1,857,033,986 đ',
              muted('61921'),
              'TT ve ve BK52301884733, Klook chuyen khoan',
            ],
          },
          {
            id: 'ob-3',
            search: 'insaoke@bidv.com 61934 Agoda',
            date: '2026-06-24',
            cells: [
              muted('insaoke@bidv.com'),
              '24/06/2026',
              '24/06/2026',
              'DD',
              '0',
              green('980,000 đ'),
              '1,855,713,986 đ',
              muted('61934'),
              'TT ve ve BK52301885290, Agoda chuyen khoan',
            ],
          },
          {
            id: 'ob-4',
            search: 'insaoke@bidv.com 61940 Booking.com',
            date: '2026-06-24',
            cells: [
              muted('insaoke@bidv.com'),
              '24/06/2026',
              '24/06/2026',
              'DD',
              '0',
              green('3,120,000 đ'),
              '1,858,833,986 đ',
              muted('61940'),
              'TT ve ve BK52301885811, Booking.com chuyen khoan',
            ],
          },
          {
            id: 'ob-5',
            search: 'insaoke@bidv.com 61955 Traveloka',
            date: '2026-06-23',
            cells: [
              muted('insaoke@bidv.com'),
              '23/06/2026',
              '23/06/2026',
              'DD',
              '0',
              green('1,410,000 đ'),
              '1,860,243,986 đ',
              muted('61955'),
              'TT ve ve BK52301886955, Traveloka chuyen khoan',
            ],
          },
          {
            id: 'ob-6',
            search: 'insaoke@bidv.com 61970 Expedia',
            date: '2026-06-22',
            cells: [
              muted('insaoke@bidv.com'),
              '22/06/2026',
              '22/06/2026',
              'DD',
              '0',
              green('2,890,000 đ'),
              '1,863,133,986 đ',
              muted('61970'),
              'TT ve ve BK52301887502, Expedia chuyen khoan',
            ],
          },
          {
            id: 'ob-7',
            search: 'insaoke@bidv.com 61982 Trip.com',
            date: '2026-06-22',
            cells: [
              muted('insaoke@bidv.com'),
              '22/06/2026',
              '22/06/2026',
              'DD',
              '0',
              green('1,980,000 đ'),
              '1,865,113,986 đ',
              muted('61982'),
              'TT ve ve BK52301888014, Trip.com chuyen khoan',
            ],
          },
          {
            id: 'ob-8',
            search: 'insaoke@bidv.com 61998 Agoda',
            date: '2026-06-21',
            cells: [
              muted('insaoke@bidv.com'),
              '21/06/2026',
              '21/06/2026',
              'DD',
              '0',
              green('1,120,000 đ'),
              '1,866,233,986 đ',
              muted('61998'),
              'TT ve ve BK52301889120, Agoda chuyen khoan',
            ],
          },
          {
            id: 'ob-9',
            search: 'insaoke@bidv.com 62010 Booking.com',
            date: '2026-06-20',
            cells: [
              muted('insaoke@bidv.com'),
              '20/06/2026',
              '20/06/2026',
              'DD',
              '0',
              green('4,260,000 đ'),
              '1,870,493,986 đ',
              muted('62010'),
              'TT ve ve BK52301889745, Booking.com chuyen khoan',
            ],
          },
          {
            id: 'ob-10',
            search: 'insaoke@bidv.com 62025 Traveloka',
            date: '2026-06-20',
            cells: [
              muted('insaoke@bidv.com'),
              '20/06/2026',
              '20/06/2026',
              'DD',
              '0',
              green('850,000 đ'),
              '1,871,343,986 đ',
              muted('62025'),
              'TT ve ve BK52301890288, Traveloka chuyen khoan',
            ],
          },
          {
            id: 'ob-11',
            search: 'insaoke@bidv.com 62041 KKday',
            date: '2026-06-19',
            cells: [
              muted('insaoke@bidv.com'),
              '19/06/2026',
              '19/06/2026',
              'DD',
              '0',
              green('1,560,000 đ'),
              '1,872,903,986 đ',
              muted('62041'),
              'TT ve ve BK52301890903, KKday chuyen khoan',
            ],
          },
        ],
      },
    ],
  },
}
