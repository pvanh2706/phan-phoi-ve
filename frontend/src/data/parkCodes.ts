export type ParkTab = 'ma' | 'nap' | 'cn'

export interface ParkCodeRecord {
  id: string
  code: string
  name: string
  type: 'Nạp tiền' | 'Công nợ'
  parent?: string
  location: string
  account: string
  bank: string
  status: 'Hoạt động' | 'Tạm dừng'
}

export const parentParkCodes: ParkCodeRecord[] = [
  { id: 'p-1', code: 'KVC-001', name: 'Bản Mòng', type: 'Nạp tiền', location: 'Sơn La', account: '19139932758899', bank: 'Techcombank', status: 'Hoạt động' },
  { id: 'p-2', code: 'KVC-002', name: 'Sun Group', type: 'Nạp tiền', location: 'Đà Nẵng', account: '19036688990011', bank: 'BIDV', status: 'Hoạt động' },
  { id: 'p-3', code: 'KVC-003', name: 'Đồi Rồng', type: 'Nạp tiền', location: 'Hải Phòng', account: '088889991111', bank: 'MB Bank', status: 'Hoạt động' },
  { id: 'p-4', code: 'KVC-101', name: 'TLTY', type: 'Công nợ', location: '—', account: '0221009876543', bank: 'Vietcombank', status: 'Hoạt động' },
  { id: 'p-5', code: 'KVC-102', name: 'Sơn Tiên', type: 'Công nợ', location: 'Đồng Nai', account: '070100556677', bank: 'Sacombank', status: 'Hoạt động' },
]

export const childTopupCodes: ParkCodeRecord[] = [
  { id: 'n-1', code: 'KVC-NAP-001', name: 'Vin Nha Trang', parent: 'Sun Group', type: 'Nạp tiền', location: 'Khánh Hòa', account: '19139932758899', bank: 'Techcombank', status: 'Hoạt động' },
  { id: 'n-2', code: 'KVC-NAP-002', name: 'Vin Phú Quốc', parent: 'Sun Group', type: 'Nạp tiền', location: 'Phú Quốc', account: '19036688990011', bank: 'BIDV', status: 'Hoạt động' },
  { id: 'n-3', code: 'KVC-NAP-003', name: 'Delight', parent: 'Delight', type: 'Nạp tiền', location: 'Hà Nội', account: '123456789', bank: 'ACB', status: 'Hoạt động' },
]

export const childDebtCodes: ParkCodeRecord[] = [
  { id: 'c-1', code: 'KVC-CN-001', name: 'Sealinks', parent: 'Sealinks', type: 'Công nợ', location: 'Phan Thiết', account: '889900112233', bank: 'VPBank', status: 'Hoạt động' },
  { id: 'c-2', code: 'KVC-CN-002', name: 'Mikazuki', parent: 'Mikazuki', type: 'Công nợ', location: 'Đà Nẵng', account: '445566778899', bank: 'VietinBank', status: 'Hoạt động' },
  { id: 'c-3', code: 'KVC-CN-003', name: 'Hồ Tràm', parent: 'Hồ Tràm', type: 'Công nợ', location: 'Bà Rịa - Vũng Tàu', account: '334455667788', bank: 'OCB', status: 'Tạm dừng' },
]
