# Use Case: Log gọi API

**Màn hình:** `SystemSettingsView.vue` → Tab **📡 Log gọi API** (`apilogs`)
**Đường dẫn truy cập:** Sidebar → Cấu hình hệ thống (`/system`) → Tab "Log gọi API"
**Quyền truy cập:** Chỉ Admin
**Nguồn dữ liệu:** API thật — `services/externalApiLogsApi.ts`

---

## Mô tả

Nhật ký chi tiết **từng lượt gọi API bên ngoài** (số dư KVC, giá vốn vé, giao dịch ngân hàng) mà hệ thống thực hiện, bao gồm cả request payload và response body đầy đủ. Dùng để debug khi job lỗi hoặc khi dữ liệu trả về không như mong đợi — chi tiết hơn nhiều so với màn "Lỗi đồng bộ cần xử lý" (chỉ hiển thị lỗi tóm tắt).

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc, xem chi tiết request/response |
| Member / Accountant | Không thấy tab này |

---

## Use Cases

### UC-LGA-01 – Xem danh sách log

- **Điều kiện:** Admin mở tab Log gọi API
- **Luồng chính:** Hệ thống tải trang đầu tiên (100 dòng), mới nhất trước
- **Kết quả:** Danh sách hiển thị các lượt gọi API gần đây

### UC-LGA-02 – Lọc log

- **Điều kiện:** Admin đang ở tab Log gọi API
- **Luồng chính:**
  1. Chọn khoảng ngày (Từ ngày – Đến ngày)
  2. Chọn **Nguồn**: Số dư KVC / Giá vốn vé / Giao dịch ngân hàng
  3. Chọn **Trạng thái**: Thành công / Lỗi
  4. Nhập từ khoá tìm theo URL hoặc nội dung lỗi
  5. Nhấn **Lọc**
- **Kết quả:** Danh sách thu hẹp theo điều kiện

### UC-LGA-03 – Xoá bộ lọc

- **Điều kiện:** Đang áp dụng bộ lọc
- **Luồng chính:** Nhấn **Xóa lọc** → tất cả điều kiện reset về mặc định
- **Kết quả:** Danh sách trở về trạng thái mặc định

### UC-LGA-04 – Xem chi tiết một lượt gọi

- **Điều kiện:** Có ít nhất 1 dòng log
- **Luồng chính:**
  1. Nhấn **🔍 Xem** ở dòng log muốn xem chi tiết
  2. Modal hiển thị: nguồn, trạng thái, KVC, ngày dữ liệu, HTTP status, thời gian gọi, thời điểm, mã Job Run, Request URL, thông báo lỗi (nếu có), Request payload (JSON format đẹp), Response body (JSON format đẹp)
- **Kết quả:** Admin xem được toàn bộ dữ liệu trao đổi với API bên ngoài để chẩn đoán lỗi

### UC-LGA-05 – Phân trang

- **Điều kiện:** Tổng số log > 100
- **Luồng chính:** Dùng nút `‹`/`›` để chuyển trang
- **Kết quả:** Duyệt hết log theo trang

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Thời điểm | `receivedAtUtc`, có mili-giây |
| Mã chạy | `#jobRunId` nếu log gắn với 1 lượt chạy job |
| Nguồn | Số dư KVC / Giá vốn vé / Giao dịch ngân hàng |
| Ngày DL | Business date liên quan |
| KVC | Tên KVC liên quan (nếu có) |
| HTTP | Mã trạng thái HTTP trả về |
| Trạng thái | Badge xanh "Thành công" / đỏ "Lỗi" |
| Thời gian | Thời gian phản hồi (ms) |
| Chi tiết | Nút 🔍 Xem mở modal |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Lỗi đồng bộ cần xử lý | Log ở đây là chi tiết kỹ thuật đầy đủ của các lượt gọi hiển thị (tóm tắt) tại màn đó |
| Cấu hình kết nối | Endpoint/timeout cấu hình tại đó quyết định các request được ghi log ở đây |

---

## Ghi chú thiết kế

- Request/Response được hiển thị dạng JSON đã format đẹp (pretty-print); nếu không parse được JSON thì hiển thị nguyên văn
- Đây là công cụ **debug kỹ thuật**, không phải màn nghiệp vụ — chỉ Admin truy cập
