# Use Case: Booking OTA trên TA

**Màn hình:** `AgencyReportView.vue` (pageKey: `otaTaBookings`)
**Đường dẫn truy cập:** Sidebar → Các đại lý OTA → Booking OTA trên TA (`/dai-ly-ota/booking-ta`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.otaTaBookings`), không gọi API.

---

## Mô tả

Danh sách booking của các **đại lý OTA** (Traveloka, Klook, Agoda, Booking.com, KKday…) đồng bộ từ hệ thống **TA**. Cùng cấu trúc cột với "Giao dịch của các đại lý trên TA" (nhóm Đại lý) nhưng phạm vi là các kênh OTA quốc tế/nội địa thay vì đại lý OneInventory nội bộ.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-BKOTA-01 – Xem danh sách booking OTA

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị booking từ các kênh OTA, 8 dòng/trang
- **Kết quả:** Danh sách booking OTA với tên kênh, thời điểm, số tiền

### UC-BKOTA-02 – Tìm kiếm theo mã booking / tên đại lý OTA

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm mã booking, tên đại lý OTA..."
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-BKOTA-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày tạo booking

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh booking trên TA |
| Tên đại lý | Tên kênh OTA (Traveloka, Klook, Agoda, Booking.com, KKday…) |
| Ngày tạo giờ | Thời điểm tạo booking |
| Số tiền | Số tiền booking, tô xanh (amount) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Tiền về ngân hàng (OTA) | Nguồn đối chiếu — sao kê ngân hàng ghi nhận tiền OTA chuyển vào |
| Đối soát (OTA) | So khớp mã booking giữa màn này và sao kê ngân hàng OTA |

---

## Ghi chú thiết kế

- Cấu trúc dữ liệu và cột giống hệt "Giao dịch của các đại lý trên TA" — dùng chung `agencyTaColumns` trong `data/reports.ts`, chỉ khác tên page/route và tập dữ liệu mẫu
