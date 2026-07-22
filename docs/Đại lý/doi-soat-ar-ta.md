# Use Case: Đối soát AR - TA

**Màn hình:** `AgencyDiffView.vue` (direction: `ar-ta`)
**Đường dẫn truy cập:** Sidebar → Đại lý → Đối soát AR - TA (`/dai-ly/doi-soat-ar-ta`)
**Quyền truy cập:** Admin, Member (xem, tìm kiếm, lọc, đánh dấu xử lý, Build đối soát)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — tính toán trực tiếp trên 2 bộ dữ liệu tĩnh `reportPages.agencyArTransactions` và `reportPages.agencyTaTransactions` trong `data/reports.ts`, không gọi API. Bảng được dựng riêng (không dùng `ReportTableCard` dùng chung) để hỗ trợ ô tích xử lý + phân trang.

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

### UC-DSAT-04 – Đánh dấu đã xử lý chênh lệch

- **Điều kiện:** Có dòng booking lệch (badge "Không có trên TA") chưa được xử lý
- **Luồng chính:**
  1. Bấm vào ô tích ở cột **"Đã xử lý"** cuối dòng
  2. Popup **"Xử lý chênh lệch đối soát"** mở ra, hiển thị mã booking + tên đại lý
  3. Nhập **Cách thức xử lý \*** và **Lý do xử lý \*** (cả 2 bắt buộc, chặn lưu nếu thiếu — toast báo lỗi)
  4. Nhấn **Lưu xử lý**
- **Kết quả:** Dòng chuyển sang trạng thái đã xử lý — ô tích chuyển sang checked và **khoá lại** (không bấm lại được), dòng được tô nền riêng (`row-handled`) để dễ nhận biết; dữ liệu xử lý chỉ lưu tạm trong state của trang (mất khi tải lại)

### UC-DSAT-05 – Build đối soát

- **Điều kiện:** Người dùng muốn build/tính lại đối soát
- **Luồng chính:** Nhấn nút **"Build đối soát"** (góc phải toolbar) → chờ ~800ms → toast báo đã build kèm số dòng
- **Kết quả:** Demo/mock — minh hoạ hành vi UI, chưa gọi API thật

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh từ hệ thống AR |
| Tên đại lý | Tên đại lý |
| Ngày giờ giao dịch | Thời điểm ghi nhận trên AR |
| Số tiền | Số tiền bị trừ trên AR |
| Trạng thái | Badge amber "Không có trên TA" |
| Đã xử lý | Ô tích — checked + khoá khi đã xử lý xong qua popup |

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
- Popup xử lý chênh lệch dùng chung style modal với "Xử lý lệch đối soát" của Đối soát Khu vui chơi thật (`ReconciliationView.vue`), nhưng thay "Số điều chỉnh + Ghi chú xử lý" bằng 2 trường "Cách thức xử lý + Lý do xử lý" phù hợp ngữ cảnh booking thay vì số tiền
- Trạng thái "Đã xử lý" (`resolvedMap`) là state cục bộ trong component, key theo `id` dòng (id đã có tiền tố `ar-tx-`/`ta-tx-` riêng theo nguồn nên không trùng nhau giữa 2 chiều đối soát) — khi lên backend thật cần cột `Status`/`ResolvedByUserId`/`ResolvedAtUtc` tương tự `ParkReconciliation`
