# Use Case: Tài khoản khách hàng

**Màn hình:** `system.html` → Tab **Tài khoản khách hàng**
**Đường dẫn truy cập:** Sidebar → Cài đặt → Tab "🏨 Tài khoản khách hàng"
**Quyền truy cập:** Admin (toàn quyền), Member (xem)

---

## Mô tả

Quản lý tài khoản cổng thông tin dành cho khách hàng lưu trú tại khách sạn. Khách hàng sử dụng tài khoản này để tự theo dõi tình trạng hoàn tiền khu vui chơi và xem lịch sử giao dịch. Admin có thể reset mật khẩu và khoá/mở khoá tài khoản khách.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, reset mật khẩu, khoá/mở khoá tài khoản khách |
| Member | Chỉ xem danh sách (không có hành động) |
| Khách hàng | Tự đăng nhập vào cổng khách hàng (màn hình khác) |

---

## Use Cases

### UC-07.1 – Xem tổng quan tài khoản khách hàng
- **Điều kiện:** Người dùng đang ở tab Tài khoản khách hàng
- **Luồng chính:**
  1. Hệ thống hiển thị 3 card thống kê tổng quan
  2. Bên dưới là bảng danh sách tài khoản khách hàng
- **Kết quả:** Người dùng thấy tổng quan số lượng và trạng thái tài khoản khách

### UC-07.2 – Lọc tài khoản khách hàng
- **Điều kiện:** Người dùng đang ở tab Tài khoản khách hàng
- **Luồng chính:**
  1. Nhập từ khoá vào ô tìm kiếm (tên, email, mã phòng)
  2. Chọn trạng thái từ dropdown (Tất cả / Đang hoạt động / Đã khoá)
  3. Danh sách lọc theo thời gian thực
- **Kết quả:** Danh sách thu hẹp theo điều kiện lọc

### UC-07.3 – Reset mật khẩu tài khoản khách
- **Điều kiện:** Admin đang xem danh sách, khách hàng quên mật khẩu hoặc cần hỗ trợ
- **Luồng chính:**
  1. Admin nhấn icon **🔑** ở dòng khách hàng muốn reset
  2. Modal xác nhận hiển thị: "Reset mật khẩu cho [tên khách]?"
  3. Admin xác nhận
  4. Hệ thống tạo mật khẩu tạm thời và gửi cho khách (qua email hoặc hiển thị trực tiếp)
- **Kết quả:** Khách hàng nhận mật khẩu mới và có thể đăng nhập lại vào cổng khách hàng

### UC-07.4 – Khoá tài khoản khách hàng
- **Điều kiện:** Admin phát hiện tài khoản khách bị lạm dụng hoặc khách đã trả phòng
- **Luồng chính:**
  1. Admin nhấn icon **🔒** ở dòng tài khoản khách muốn khoá
  2. Modal xác nhận hiển thị: "Khoá tài khoản [tên khách]?"
  3. Admin xác nhận
  4. Tài khoản chuyển sang trạng thái "Đã khoá" (badge đỏ)
- **Kết quả:** Khách hàng không thể đăng nhập vào cổng khách hàng cho đến khi được mở khoá

### UC-07.5 – Mở khoá tài khoản khách hàng
- **Điều kiện:** Tài khoản đang ở trạng thái bị khoá
- **Luồng chính:**
  1. Admin nhấn icon **🔓** ở dòng tài khoản đang bị khoá
  2. Xác nhận mở khoá
  3. Tài khoản chuyển về trạng thái "Đang hoạt động"
- **Kết quả:** Khách hàng có thể đăng nhập trở lại

---

## Card thống kê tổng quan

| Card | Giá trị mẫu | Mô tả |
|------|-------------|-------|
| 📊 Tổng tài khoản | 6 | Tổng số tài khoản khách đã tạo |
| 🟢 Đang online | 2 | Số khách đang đăng nhập trong 15 phút qua |
| 🔒 Đã khoá | 1 | Số tài khoản bị khoá |

---

## Dữ liệu bảng tài khoản khách

| Cột | Mô tả |
|-----|-------|
| Khách hàng | Tên và email |
| Mã phòng | Phòng khách sạn đang lưu trú |
| Ngày tạo | Ngày tạo tài khoản (thường = ngày check-in) |
| Lần đăng nhập cuối | Thời gian hoạt động gần nhất |
| Trạng thái | Đang hoạt động (xanh) / Đã khoá (đỏ) |
| Hành động | 🔑 Reset mật khẩu + 🔒/🔓 Khoá/Mở khoá |

---

## Dữ liệu mẫu

| # | Khách hàng | Phòng | Trạng thái |
|---|-----------|-------|-----------|
| 1 | Nguyễn Minh Tuấn | 301 | Đang hoạt động |
| 2 | Trần Thị Hoa | 215 | Đang hoạt động |
| 3 | Lê Văn Mạnh | 408 | Đã khoá |
| 4 | Phạm Thị Lan | 112 | Đang hoạt động |
| 5 | Hoàng Đức Việt | 305 | Đang hoạt động |
| 6 | Vũ Thị Mai | 220 | Đang hoạt động |

---

## Phân biệt với Tab Quản lý người dùng (S6)

| Tiêu chí | S6 – Quản lý người dùng | S7 – Tài khoản khách hàng |
|---------|------------------------|--------------------------|
| Đối tượng | Nhân viên nội bộ (Admin/Member) | Khách lưu trú tại khách sạn |
| Mục đích dùng | Vận hành hệ thống PPV | Xem tình trạng hoàn tiền KVC |
| Tạo tài khoản | Admin tạo thủ công | Tự động khi khách check-in (hoặc Admin tạo) |
| Quyền hạn | Theo vai trò (Admin/Member) | Chỉ xem dữ liệu cá nhân |

---

## Kết nối với màn hình khác

- **Truy cập từ:** Tất cả màn hình → nút "Cài đặt" ở user card cuối sidebar
- **Liên quan đến:** `trang-thai-hoan-tien-kh.html` — trang khách hàng xem trạng thái hoàn tiền KVC của mình
- **Liên quan đến:** Tab S6 Quản lý người dùng — quản lý nhân viên nội bộ
