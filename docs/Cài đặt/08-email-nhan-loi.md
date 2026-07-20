# Use Case: Email nhận lỗi

**Màn hình:** `SystemSettingsView.vue` → Tab **🔔 Email nhận lỗi** (`recipients`)
**Đường dẫn truy cập:** Sidebar → Cấu hình hệ thống (`/system`) → Tab "Email nhận lỗi"
**Quyền truy cập:** Chỉ Admin (tab chỉ hiển thị khi `authState.user.role === 'Admin'`)
**Nguồn dữ liệu:** API thật — `services/notificationsApi.ts`

---

## Mô tả

Quản lý danh sách **địa chỉ email nội bộ** nhận các loại thông báo tự động của hệ thống: tổng hợp lỗi đồng bộ hằng ngày, báo cáo ngày, và cảnh báo lệch đối soát. Khác với tab "Thông báo" (tuỳ chỉnh nhận thông báo cá nhân trong phần mềm/email của từng người dùng), tab này quản lý **danh sách email dùng chung** để hệ thống tự động gửi báo cáo/cảnh báo.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, thêm, sửa, ngừng nhận |
| Member / Accountant | Không thấy tab này |

---

## Use Cases

### UC-ENL-01 – Xem danh sách email nhận lỗi

- **Điều kiện:** Admin mở tab Email nhận lỗi
- **Luồng chính:** Hệ thống tải danh sách qua `listNotificationRecipients`
- **Kết quả:** Bảng hiển thị loại thông báo, email, tên hiển thị, trạng thái (Đang nhận/Ngừng nhận)

### UC-ENL-02 – Thêm email nhận thông báo

- **Điều kiện:** Admin đang ở tab Email nhận lỗi
- **Luồng chính:**
  1. Nhấn **Thêm email**
  2. Chọn **Loại thông báo**: Tổng hợp lỗi đồng bộ / Báo cáo ngày / Cảnh báo lệch đối soát
  3. Nhập **Email**, **Tên hiển thị** (tuỳ chọn), bật/tắt **Đang nhận**
  4. Nhấn **Lưu**
- **Kết quả:** Email mới xuất hiện trong danh sách, bắt đầu nhận loại thông báo đã chọn từ lần gửi kế tiếp

### UC-ENL-03 – Sửa email nhận thông báo

- **Điều kiện:** Đã có bản ghi trong danh sách
- **Luồng chính:** Nhấn **✏️ Sửa** → modal điền sẵn dữ liệu → chỉnh sửa → **Lưu**
- **Kết quả:** Thông tin được cập nhật

### UC-ENL-04 – Ngừng nhận thông báo

- **Điều kiện:** Bản ghi đang ở trạng thái "Đang nhận"
- **Luồng chính:** Nhấn **⏸️ Ngừng** ở dòng tương ứng
- **Kết quả:** Trạng thái chuyển sang "Ngừng nhận" (badge xám); email này không còn nhận loại thông báo đó nữa

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Loại thông báo | Tổng hợp lỗi đồng bộ / Báo cáo ngày / Cảnh báo lệch đối soát |
| Email | Địa chỉ nhận |
| Tên hiển thị | Tên gợi nhớ (tuỳ chọn) |
| Trạng thái | Badge xanh "Đang nhận" / xám "Ngừng nhận" |
| Hành động | ✏️ Sửa, ⏸️ Ngừng (chỉ hiện khi đang nhận) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Lỗi đồng bộ cần xử lý | Nút "Gửi email lỗi" gửi đến danh sách email loại "Tổng hợp lỗi đồng bộ" đang active tại đây |
| Cài đặt → Thông báo (03) | Khác biệt: đó là tuỳ chọn *cá nhân* trong app/email của từng user, còn tab này là *danh sách chung* nhận báo cáo hệ thống |
