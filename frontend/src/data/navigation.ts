export interface NavChild {
  label: string
  path: string
}

export interface NavItem {
  key: string
  label: string
  icon: string
  path?: string
  children?: NavChild[]
}

export const navigation: NavItem[] = [
  {
    key: 'park',
    label: 'Khu vui chơi',
    icon: '🎢',
    children: [
      { label: 'Danh sách khu vui chơi', path: '/khu-vui-choi/danh-sach' },
      { label: 'Mã khu vui chơi', path: '/khu-vui-choi/ma-kvc' },
      { label: 'Số dư khu vui chơi hằng ngày', path: '/khu-vui-choi/so-du' },
      { label: 'Danh sách nạp tiền KVC theo ngày', path: '/khu-vui-choi/nap-tien' },
      { label: 'Chi tiết giá vốn vé bán', path: '/khu-vui-choi/gia-von-ve-ban' },
      { label: 'Đối soát Khu vui chơi', path: '/khu-vui-choi/doi-soat' },
      { label: 'Nhật ký thay đổi', path: '/khu-vui-choi/nhat-ky' },
      { label: 'Lỗi đồng bộ cần xử lý', path: '/khu-vui-choi/loi-dong-bo' },
    ],
  },
  {
    key: 'vin',
    label: 'Đối soát Vin',
    icon: '⚖️',
    children: [
      { label: 'Danh mục KVC con của Vin', path: '/doi-soat-vin/danh-muc-kvc-con' },
      { label: 'Số dư KVC Vin theo ngày', path: '/doi-soat-vin/so-du-theo-ngay' },
      { label: 'Danh sách nạp tiền cho Vin theo ngày', path: '/doi-soat-vin/nap-tien-theo-ngay' },
      { label: 'Chi tiết giá vốn vé bán', path: '/doi-soat-vin/gia-von-ve-ban' },
      { label: 'Đối soát KVC Vin', path: '/doi-soat-vin/doi-soat' },
    ],
  },
  {
    key: 'agency',
    label: 'Đại lý',
    icon: '🤝',
    children: [
      { label: 'Danh sách các đại lý', path: '/dai-ly/danh-sach' },
      { label: 'Giao dịch của các đại lý trên AR', path: '/dai-ly/giao-dich-ar' },
      { label: 'Giao dịch của các đại lý trên TA', path: '/dai-ly/giao-dich-ta' },
      { label: 'Giao dịch đại lý nạp tiền trên BIDV', path: '/dai-ly/giao-dich-bidv' },
      { label: 'Đối soát AR - TA', path: '/dai-ly/doi-soat-ar-ta' },
      { label: 'Đối soát TA - AR', path: '/dai-ly/doi-soat-ta-ar' },
    ],
  },
  {
    key: 'retail',
    label: 'Khách lẻ',
    icon: '👤',
    children: [
      { label: 'Booking khách lẻ trên TA', path: '/khach-le/booking-ta' },
      { label: 'Tiền về ngân hàng', path: '/khach-le/tien-ve-ngan-hang' },
      { label: 'Đối soát', path: '/khach-le/doi-soat' },
    ],
  },
  {
    key: 'ota',
    label: 'Các đại lý API',
    icon: '🌐',
    children: [
      { label: 'Booking API trên TA', path: '/dai-ly-ota/booking-ta' },
      { label: 'Tiền về ngân hàng', path: '/dai-ly-ota/tien-ve-ngan-hang' },
      { label: 'Đối soát', path: '/dai-ly-ota/doi-soat' },
    ],
  },
]
