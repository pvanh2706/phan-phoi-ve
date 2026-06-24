# Use Case: Quản lý người dùng

**Màn hình:** `system.html` → Tab **Quản lý người dùng**
**Đường dẫn truy cập:** Sidebar → Cài đặt → Tab "👥 Quản lý người dùng"
**Quyền truy cập:** Chỉ Admin

---

## Mô tả

Cho phép Admin quản lý toàn bộ tài khoản người dùng nội bộ của hệ thống ezCloud PPV. Gồm 2 phân quyền: Admin và Member. Admin có thể thêm, chỉnh sửa, xoá tài khoản Member và xem danh sách tất cả người dùng kèm bộ lọc.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Toàn quyền: xem, thêm, sửa, xoá Member |
| Member | Không truy cập được tab này (hidden hoặc disabled) |

---

## Use Cases

### UC-06.1 – Xem danh sách người dùng
- **Điều kiện:** Admin đang ở tab Quản lý người dùng
- **Luồng chính:**
  1. Hệ thống hiển thị 2 card vai trò (Admin / Member) với số lượng thành viên
  2. Bên dưới là bảng danh sách tất cả người dùng
  3. Mỗi dòng hiển thị: Avatar, Họ tên, Email, Vai trò, Trạng thái, Ngày tạo, Hành động
- **Kết quả:** Admin thấy toàn bộ người dùng trong hệ thống

### UC-06.2 – Lọc người dùng
- **Điều kiện:** Admin đang xem danh sách người dùng
- **Luồng chính:**
  1. Admin nhập từ khoá vào ô tìm kiếm (lọc theo tên hoặc email)
  2. Và/hoặc chọn vai trò từ dropdown (Tất cả / Admin / Member)
  3. Và/hoặc chọn trạng thái từ dropdown (Tất cả / Hoạt động / Không hoạt động)
  4. Danh sách lọc kết quả theo thời gian thực
- **Kết quả:** Danh sách thu hẹp theo điều kiện lọc

### UC-06.3 – Thêm người dùng mới
- **Điều kiện:** Admin đang ở tab Quản lý người dùng
- **Luồng chính:**
  1. Admin nhấn nút **+ Thêm người dùng**
  2. Modal "Thêm người dùng mới" mở ra
  3. Admin nhập: Họ và tên, Email, Vai trò (Admin/Member), Mật khẩu tạm thời
  4. Nhấn **Thêm người dùng** để xác nhận
- **Kết quả:** Tài khoản mới được tạo, xuất hiện trong bảng danh sách
- **Validation:** Email không được trùng với tài khoản đã có

### UC-06.4 – Chỉnh sửa người dùng
- **Điều kiện:** Admin đang xem danh sách người dùng
- **Luồng chính:**
  1. Admin nhấn nút **✏️ Sửa** ở dòng người dùng muốn chỉnh
  2. Modal "Chỉnh sửa người dùng" mở ra, điền sẵn thông tin hiện tại
  3. Admin sửa: Họ và tên, Vai trò, Trạng thái hoạt động
  4. Nhấn **Lưu thay đổi**
- **Kết quả:** Thông tin người dùng được cập nhật trong bảng
- **Ghi chú:** Không thể đổi email sau khi tạo (readonly)

### UC-06.5 – Xoá người dùng
- **Điều kiện:** Admin đang xem danh sách người dùng
- **Luồng chính:**
  1. Admin nhấn nút **🗑️ Xoá** ở dòng người dùng
  2. Modal xác nhận xoá hiển thị, nêu rõ tên người dùng sẽ bị xoá
  3. Admin nhấn **Xác nhận xoá**
- **Kết quả:** Tài khoản bị xoá khỏi hệ thống
- **Ràng buộc:** Không thể xoá tài khoản Admin cuối cùng; không thể tự xoá tài khoản đang đăng nhập

---

## Card vai trò

| Vai trò | Mô tả | Số lượng (mẫu) |
|---------|-------|----------------|
| 👑 Admin | Toàn quyền hệ thống | 1 |
| 👤 Member | Quyền hạn chế theo phân công | 5 |

---

## Dữ liệu bảng người dùng

| Cột | Mô tả |
|-----|-------|
| Avatar | Chữ viết tắt tên hoặc ảnh đại diện |
| Họ tên | Tên đầy đủ |
| Email | Email đăng nhập (unique) |
| Vai trò | Admin / Member (badge màu) |
| Trạng thái | Hoạt động (xanh) / Không hoạt động (xám) |
| Ngày tạo | Ngày tạo tài khoản |
| Hành động | Nút Sửa + Xoá |

---

## Dữ liệu mẫu

| # | Họ tên | Email | Vai trò | Trạng thái |
|---|--------|-------|---------|-----------|
| 1 | Nguyễn Văn Admin | admin@ezcloud.vn | Admin | Hoạt động |
| 2 | Trần Thị Bình | binh.tran@ezcloud.vn | Member | Hoạt động |
| 3 | Lê Văn Cường | cuong.le@ezcloud.vn | Member | Hoạt động |
| 4 | Phạm Thị Dung | dung.pham@ezcloud.vn | Member | Hoạt động |
| 5 | Hoàng Văn Em | em.hoang@ezcloud.vn | Member | Không hoạt động |
| 6 | Ngô Thị Phương | phuong.ngo@ezcloud.vn | Member | Hoạt động |

---

## Kết nối với màn hình khác

- **Truy cập từ:** Tất cả màn hình → nút "Cài đặt" ở user card cuối sidebar (chỉ Admin thấy tab này)
- **Liên quan đến:** Tab Thông tin tài khoản — Admin có thể xem profile các Member
- **Phân biệt với:** Tab S7 Tài khoản khách hàng — S6 quản lý nhân viên nội bộ, S7 quản lý khách hàng khách sạn
