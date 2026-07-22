# Use Case: Booking API trên TA

**Màn hình:** `AgencyReportView.vue` (pageKey: `otaTaBookings`)
**Đường dẫn truy cập:** Sidebar → Các đại lý API → Booking API trên TA (`/dai-ly-ota/booking-ta`)
**Quyền truy cập:** Admin, Member (xem, Get API)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.otaTaBookings`, 14 dòng mẫu). Có nút **"⤓ Get API"** giả lập (thuộc `pagesWithGetApiOnly` trong `AgencyReportView.vue`).

---

## Mô tả

Danh sách booking của các **đại lý API** (Traveloka, Klook, Agoda, Booking.com, KKday, Expedia, Trip.com…) đồng bộ từ hệ thống **TA**. Cùng cấu trúc cột với "Giao dịch của các đại lý trên TA" (nhóm Đại lý) nhưng phạm vi là các kênh đối tác API (OTA) quốc tế/nội địa thay vì đại lý OneInventory nội bộ.

> **Đổi tên:** menu và nhãn màn hình đã đổi từ "Các đại lý OTA"/"Booking OTA trên TA" thành **"Các đại lý API"**/**"Booking API trên TA"**; đường dẫn (`/dai-ly-ota/...`) và `pageKey` (`otaTaBookings`) giữ nguyên không đổi.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, Get API |
| Member | Xem, tìm kiếm, lọc theo ngày, Get API |

---

## Use Cases

### UC-BKAPI-01 – Xem danh sách booking API

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị 14 booking mẫu từ các kênh API, 8 dòng/trang
- **Kết quả:** Danh sách booking với tên kênh, thời điểm, số tiền

### UC-BKAPI-02 – Tìm kiếm theo mã booking / tên đại lý API

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm mã booking, tên đại lý API..."
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-BKAPI-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày tạo booking

### UC-BKAPI-04 – Gọi API test (Get API)

- **Điều kiện:** Người dùng muốn mô phỏng lấy booking mới
- **Luồng chính:** Nhấn nút **"⤓ Get API"** (góc phải toolbar) → chờ ~800ms → toast báo số dòng "lấy được"
- **Kết quả:** Chỉ minh hoạ UI, không gọi API thật

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh booking trên TA |
| Tên đại lý | Tên kênh API (Traveloka, Klook, Agoda, Booking.com, KKday, Expedia, Trip.com…) |
| Ngày tạo giờ | Thời điểm tạo booking |
| Số tiền | Số tiền booking, tô xanh (amount) |

## Dữ liệu mẫu (14 booking, 22–25/06/2026)

Traveloka ×3, Klook ×2, Agoda ×2, Booking.com ×2, KKday ×2, Expedia ×2, Trip.com ×1

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Tiền về ngân hàng (Các đại lý API) | Nguồn đối chiếu — sao kê ngân hàng ghi nhận tiền API chuyển vào |
| Đối soát (Các đại lý API) | So khớp mã booking giữa màn này và sao kê ngân hàng API |

---

## Ghi chú thiết kế

- Cấu trúc dữ liệu và cột giống hệt "Giao dịch của các đại lý trên TA" — dùng chung `agencyTaColumns` trong `data/reports.ts`, chỉ khác tên page/route và tập dữ liệu mẫu
- Dữ liệu mẫu đã được **mở rộng từ 6 lên 14 dòng** và bổ sung 2 kênh mới (Expedia, Trip.com) để trang "Đối soát" có đủ case khớp/lệch/chưa về ngân hàng thực tế hơn — xem `doi-soat.md`
