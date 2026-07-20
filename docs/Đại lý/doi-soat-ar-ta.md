# Use Case: Đối soát AR - TA

**Màn hình:** `AgencyDiffView.vue` (direction: `ar-ta`)
**Đường dẫn truy cập:** Sidebar → Đại lý → Đối soát AR - TA (`/dai-ly/doi-soat-ar-ta`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — tính toán trực tiếp trên 2 bộ dữ liệu tĩnh `reportPages.agencyArTransactions` và `reportPages.agencyTaTransactions` trong `data/reports.ts`, không gọi API.

---

## Mô tả

Đối soát chéo giữa hai hệ thống **AR** và **TA** theo cơ chế **VLOOKUP theo Mã booking**: liệt kê những booking **có trên hệ thống AR nhưng không tìm thấy trên TA**. Đây là chiều đối soát ngược lại của "Đối soát TA - AR".

Mục tiêu: phát hiện các giao dịch bị trừ tiền đại lý (AR) nhưng không có booking tương ứng ghi nhận trên hệ thống bán vé (TA) — dấu hiệu của lỗi đồng bộ hoặc gian lận.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-DSAT-01 – Xem danh sách booking lệch AR → TA

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:**
  1. Hệ thống lấy toàn bộ mã booking trên tab đầu tiên của "Giao dịch AR" và "Giao dịch TA"
  2. So sánh tập mã booking AR với tập mã booking TA
  3. Lọc ra các dòng AR có mã booking **không tồn tại** trong tập TA
  4. Mỗi dòng kết quả được gắn badge **"Không có trên TA"** (màu amber)
- **Kết quả:** Danh sách các booking cần kiểm tra vì bị trừ tiền AR nhưng thiếu trên TA; subtitle hiển thị tổng số dòng lệch

### UC-DSAT-02 – Tìm kiếm trong kết quả đối soát

- **Điều kiện:** Người dùng đang xem kết quả
- **Luồng chính:** Nhập mã booking hoặc tên đại lý vào ô tìm kiếm → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DSAT-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày giao dịch AR

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh từ hệ thống AR |
| Tên đại lý | Tên đại lý |
| Ngày giờ giao dịch | Thời điểm ghi nhận trên AR |
| Số tiền | Số tiền bị trừ trên AR |
| Trạng thái | Badge amber "Không có trên TA" |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên AR | Nguồn dữ liệu gốc (base) của phép so khớp |
| Giao dịch của các đại lý trên TA | Nguồn dữ liệu đối chiếu (tập mã booking dùng để loại trừ) |
| Đối soát TA - AR | Chiều ngược lại — liệt kê booking có trên TA nhưng thiếu trên AR |

---

## Ghi chú thiết kế

- Thuật toán so khớp hiện tại chỉ dựa vào **cột đầu tiên (mã booking)** của mỗi dòng dữ liệu, dùng `Set` để loại trừ — độ phức tạp O(n)
- Vì dữ liệu 2 nguồn là mock tĩnh, số lượng "lệch" hiển thị chỉ minh hoạ hành vi UI, không phản ánh chênh lệch thật giữa AR và TA
- Khi có API thật, cần thực hiện so khớp này ở backend (theo lịch định kỳ) thay vì tính lại mỗi lần load trang trên frontend
