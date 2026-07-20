# Use Case: Giao dịch của các đại lý trên TA

**Màn hình:** `AgencyReportView.vue` (pageKey: `agencyTaTransactions`)
**Đường dẫn truy cập:** Sidebar → Đại lý → Giao dịch của các đại lý trên TA (`/dai-ly/giao-dich-ta`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.agencyTaTransactions`). Có nút **"Gọi API test"** giả lập (không nối API thật).

---

## Mô tả

Danh sách giao dịch của các đại lý ghi nhận trên hệ thống **TA** (hệ thống booking/vé). Mỗi dòng gồm mã booking, tên đại lý, ngày tạo giao dịch, và số tiền. Dùng để đối chiếu chéo với dữ liệu trên AR nhằm phát hiện booking bị thiếu ở một trong hai hệ thống.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, gọi API test |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-GDTA-01 – Xem danh sách giao dịch TA

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị 8 dòng/trang, dữ liệu mẫu trải từ 09/2025 đến 04/2026
- **Kết quả:** Danh sách giao dịch đại lý trên TA

### UC-GDTA-02 – Tìm kiếm theo mã booking / tên đại lý

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô tìm kiếm → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-GDTA-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng muốn giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày giao dịch

### UC-GDTA-04 – Gọi API test

- **Điều kiện:** Người dùng muốn mô phỏng lấy dữ liệu mới
- **Luồng chính:** Nhấn **"Gọi API test"** → chờ ~800ms → toast báo số dòng "lấy được"
- **Kết quả:** Chỉ minh hoạ UI, không có hiệu ứng thực tế lên dữ liệu

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh giao dịch trên TA |
| Tên đại lý | Tên đại lý phát sinh giao dịch |
| Ngày tạo giờ | Ngày tạo giao dịch |
| Số tiền | Số tiền giao dịch, tô đỏ (amount) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên AR | Nguồn đối chiếu chéo theo mã booking |
| Đối soát AR - TA / Đối soát TA - AR | Dùng chính dữ liệu tab đầu tiên của màn này (`reportPages.agencyTaTransactions.tabs[0].rows`) làm nguồn so khớp |

---

## Ghi chú thiết kế

- Cùng nhóm dữ liệu demo với "Giao dịch của các đại lý trên AR" — khi có API TA thật, nên đồng bộ theo cùng cơ chế polling/webhook để đảm bảo mã booking khớp giữa 2 hệ thống theo thời gian thực
