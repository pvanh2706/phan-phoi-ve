# Use Case: Quản lý thiết bị

**Màn hình:** `system.html` → Tab **Quản lý thiết bị**
**Đường dẫn truy cập:** Sidebar → Cài đặt → Tab "📱 Quản lý thiết bị"

---

## Mô tả

Hiển thị danh sách các thiết bị đang đăng nhập vào tài khoản hiện tại. Người dùng có thể đăng xuất khỏi một thiết bị cụ thể hoặc đăng xuất toàn bộ thiết bị khác (trừ thiết bị hiện tại).

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem và quản lý thiết bị đăng nhập của tài khoản mình |
| Member | Xem và quản lý thiết bị đăng nhập của tài khoản mình |

---

## Use Cases

### UC-05.1 – Xem danh sách thiết bị đang đăng nhập
- **Điều kiện:** Người dùng đang ở tab Quản lý thiết bị
- **Luồng chính:**
  1. Hệ thống hiển thị danh sách tất cả thiết bị đang có phiên đăng nhập hoạt động
  2. Mỗi thiết bị hiển thị: tên thiết bị, loại trình duyệt/OS, địa chỉ IP, thời gian đăng nhập lần cuối
  3. Thiết bị hiện tại được đánh dấu badge "Thiết bị này"
- **Kết quả:** Người dùng biết được bao nhiêu thiết bị đang dùng tài khoản của mình

### UC-05.2 – Đăng xuất một thiết bị cụ thể
- **Điều kiện:** Người dùng thấy một thiết bị không nhận ra hoặc muốn đăng xuất từ xa
- **Luồng chính:**
  1. Người dùng tìm thiết bị cần đăng xuất trong danh sách
  2. Nhấn nút **Đăng xuất** ở dòng thiết bị đó
  3. Hệ thống huỷ phiên đăng nhập của thiết bị đó
  4. Thiết bị bị xoá khỏi danh sách
- **Kết quả:** Thiết bị đó bị đăng xuất và phải đăng nhập lại để tiếp tục dùng hệ thống
- **Ghi chú:** Không thể đăng xuất thiết bị hiện tại từ màn hình này

### UC-05.3 – Đăng xuất tất cả thiết bị khác
- **Điều kiện:** Người dùng muốn bảo mật tài khoản (vd: sau khi đổi mật khẩu)
- **Luồng chính:**
  1. Người dùng nhấn nút **Đăng xuất tất cả thiết bị khác**
  2. Hệ thống hiển thị xác nhận
  3. Người dùng xác nhận
  4. Hệ thống huỷ toàn bộ phiên đăng nhập trừ thiết bị hiện tại
- **Kết quả:** Chỉ còn một phiên đăng nhập là thiết bị đang dùng

---

## Thông tin hiển thị mỗi thiết bị

| Trường | Mô tả |
|--------|-------|
| Icon thiết bị | 🖥️ Desktop / 📱 Mobile / 💻 Laptop |
| Tên thiết bị | VD: "Chrome trên Windows", "Safari trên iPhone" |
| Địa chỉ IP | VD: `192.168.1.105` |
| Thời gian hoạt động cuối | VD: "2 giờ trước", "Đang hoạt động" |
| Badge | "Thiết bị này" (chỉ thiết bị hiện tại) |

---

## Danh sách thiết bị mẫu (dữ liệu tĩnh trong UI)

| # | Thiết bị | IP | Trạng thái |
|---|---------|-----|-----------|
| 1 | Chrome trên Windows | 192.168.1.100 | Thiết bị này (đang hoạt động) |
| 2 | Firefox trên MacBook | 192.168.1.105 | 2 giờ trước |
| 3 | Safari trên iPhone | 103.45.67.89 | 1 ngày trước |

---

## Kết nối với màn hình khác

- **Truy cập từ:** Tất cả màn hình → nút "Cài đặt" ở user card cuối sidebar
- **Liên quan đến:** Tab Đổi mật khẩu — sau khi đổi mật khẩu nên đăng xuất thiết bị khác
- **Sau đăng xuất toàn bộ:** Các thiết bị bị đăng xuất sẽ redirect về trang đăng nhập
