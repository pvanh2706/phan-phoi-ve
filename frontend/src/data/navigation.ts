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
    icon: 'KV',
    children: [
      { label: 'Danh sách khu vui chơi', path: '/khu-vui-choi/danh-sach' },
      { label: 'Mã khu vui chơi', path: '/khu-vui-choi/ma-kvc' },
      { label: 'Số dư khu vui chơi hằng ngày', path: '/khu-vui-choi/so-du' },
      { label: 'Danh sách nạp tiền KVC theo ngày', path: '/khu-vui-choi/nap-tien' },
      { label: 'Chi tiết giá vốn vé bán', path: '/khu-vui-choi/gia-von-ve-ban' },
      { label: 'Đối soát Khu vui chơi', path: '/khu-vui-choi/doi-soat' },
      { label: 'KVC hoàn tiền', path: '/khu-vui-choi/kvc-hoan-tien' },
      { label: 'Nhật ký thay đổi', path: '/khu-vui-choi/nhat-ky' },
      { label: 'Lỗi đồng bộ cần xử lý', path: '/khu-vui-choi/loi-dong-bo' },
    ],
  },
  { key: 'agency', label: 'Đại lý', icon: 'DL', path: '/dang-phat-trien/dai-ly' },
  { key: 'retail', label: 'Khách lẻ', icon: 'KL', path: '/dang-phat-trien/khach-le' },
  { key: 'vin', label: 'Đối soát Vin', icon: 'VIN', path: '/dang-phat-trien/doi-soat-vin' },
  { key: 'ota', label: 'Các đại lý OTA', icon: 'OTA', path: '/dang-phat-trien/ota' },
  {
    key: 'refund',
    label: 'Hoàn tiền',
    icon: 'HT',
    children: [
      { label: 'Quy trình hoàn tiền', path: '/hoan-tien/quy-trinh' },
      { label: 'Trạng thái hoàn tiền cho khách hàng', path: '/hoan-tien/trang-thai-khach-hang' },
    ],
  },
  { key: 'process', label: 'Quy trình đối soát', icon: 'QT', path: '/dang-phat-trien/quy-trinh-doi-soat' },
]
