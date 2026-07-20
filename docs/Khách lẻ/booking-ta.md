# Use Case: Booking khách lẻ trên TA

**Màn hình:** `AgencyReportView.vue` (pageKey: `retailTaBookings`)
**Đường dẫn truy cập:** Sidebar → Khách lẻ → Booking khách lẻ trên TA (`/khach-le/booking-ta`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.retailTaBookings`), không gọi API.

---

## Mô tả

Danh sách booking của **khách lẻ** (khách hàng cá nhân, không qua đại lý) đồng bộ từ hệ thống **TA**. Khác với đại lý, mỗi dòng gồm thêm thông tin liên hệ khách hàng (tên, số điện thoại) để phục vụ chăm sóc/tra soát khi cần.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-BKKL-01 – Xem danh sách booking khách lẻ

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị các booking khách lẻ, 8 dòng/trang
- **Kết quả:** Danh sách booking với thông tin khách hàng và số tiền

### UC-BKKL-02 – Tìm kiếm theo mã booking / tên khách / SĐT

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm mã booking, tên khách, SĐT..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-BKKL-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày tạo booking

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh booking trên TA |
| Tên khách hàng | Tên khách lẻ đặt vé |
| SĐT | Số điện thoại liên hệ, định dạng nhóm 4 số |
| Ngày tạo giờ | Thời điểm tạo booking |
| Số tiền | Số tiền booking, tô xanh (amount) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Tiền về ngân hàng (Khách lẻ) | Nguồn đối chiếu — sao kê ngân hàng ghi nhận tiền khách lẻ chuyển vào |
| Đối soát (Khách lẻ) | So khớp mã booking giữa màn này và sao kê ngân hàng để phát hiện booking chưa nhận được tiền hoặc lệch số tiền |

---

## Ghi chú thiết kế

- Đây là dữ liệu demo minh hoạ hành vi UI; khi có API TA thật cho khách lẻ, cần đảm bảo số điện thoại được ẩn/che một phần nếu có yêu cầu bảo mật dữ liệu cá nhân (PII)
