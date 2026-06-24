# Use Case: Thông báo

**Màn hình:** `system.html` → Tab **Thông báo**
**Đường dẫn truy cập:** Sidebar → Cài đặt → Tab "🔔 Thông báo"

---

## Mô tả

Cho phép người dùng bật/tắt nhận thông báo theo từng loại sự kiện và từng kênh gửi thông báo (phần mềm hoặc email). Mỗi loại sự kiện có thể được cấu hình độc lập cho hai kênh.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Tuỳ chỉnh cài đặt thông báo cho tài khoản của mình |
| Member | Tuỳ chỉnh cài đặt thông báo cho tài khoản của mình |

---

## Use Cases

### UC-03.1 – Bật/tắt thông báo trên phần mềm
- **Điều kiện:** Người dùng đang ở tab Thông báo
- **Luồng chính:**
  1. Người dùng tìm đến dòng loại thông báo mong muốn (vd: Đặt phòng mới)
  2. Nhấn toggle ở cột "Phần mềm"
  3. Toggle chuyển sang trạng thái ON (xanh) hoặc OFF (xám)
- **Kết quả:** Hệ thống ghi nhận người dùng muốn/không muốn nhận thông báo in-app cho loại sự kiện đó

### UC-03.2 – Bật/tắt thông báo qua Email
- **Điều kiện:** Người dùng đang ở tab Thông báo
- **Luồng chính:**
  1. Người dùng tìm đến dòng loại thông báo mong muốn
  2. Nhấn toggle ở cột "Email"
  3. Toggle chuyển trạng thái ON/OFF
- **Kết quả:** Hệ thống ghi nhận người dùng muốn/không muốn nhận email thông báo cho loại sự kiện đó

### UC-03.3 – Lưu cài đặt thông báo
- **Điều kiện:** Người dùng đã thay đổi ít nhất một toggle
- **Luồng chính:**
  1. Người dùng nhấn nút **Lưu cài đặt**
  2. Hệ thống lưu toàn bộ ma trận cài đặt thông báo
- **Kết quả:** Cài đặt được áp dụng cho phiên làm việc tiếp theo

---

## Ma trận cài đặt thông báo

| Loại thông báo | Phần mềm | Email |
|----------------|----------|-------|
| Đặt phòng mới | Toggle | Toggle |
| Yêu cầu đối soát | Toggle | Toggle |
| Báo cáo hàng ngày | Toggle | Toggle |
| Thanh toán thành công | Toggle | Toggle |

- **Mặc định:** Tất cả toggle đều ON (bật)
- **Trạng thái ON:** Màu xanh lá, hiển thị dấu ✓
- **Trạng thái OFF:** Màu xám

---

## Kết nối với màn hình khác

- **Truy cập từ:** Tất cả màn hình → nút "Cài đặt" ở user card cuối sidebar
- **Liên quan đến:** Các thông báo hiển thị trong header (bell icon) của toàn hệ thống
