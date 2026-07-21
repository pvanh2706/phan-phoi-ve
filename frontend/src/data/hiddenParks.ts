// Các mã KVC cần ẩn tạm thời khỏi giao diện (Danh sách khu vui chơi, Đối soát Khu vui chơi)
// theo yêu cầu nghiệp vụ, không xóa dữ liệu ở backend.
export const HIDDEN_PARK_CODES = new Set([
  '11810', // Timescity
  '11813', // VinWonders Vũ Yên
  '10288', // Phú Quốc
  '10064', // Nam Hội An
  '11732', // Công viên Grand Park
  '11591', // Hà Nội
  '11573', // VinWonders Times City
  '10072', // Nam Trang
  '11714', // VinWonders Cửa Hội
])

export function isHiddenParkCode(code?: string | null) {
  return Boolean(code && HIDDEN_PARK_CODES.has(code))
}
