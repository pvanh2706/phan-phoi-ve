# Use Case: Thông tin tài khoản

**Màn hình:** `system.html` → Tab **Thông tin tài khoản**
**Đường dẫn truy cập:** Sidebar → Ảnh đại diện / tên tài khoản → Cài đặt → Tab "👤 Thông tin tài khoản"

---

## Mô tả

Cho phép người dùng xem và chỉnh sửa thông tin cá nhân của tài khoản đang đăng nhập, bao gồm họ tên, số điện thoại và ảnh đại diện. Email và chức vụ được hiển thị nhưng không thể chỉnh sửa (readonly).

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem và chỉnh sửa thông tin cá nhân của mình |
| Member | Xem và chỉnh sửa thông tin cá nhân của mình |

---

## Use Cases

### UC-01.1 – Xem thông tin cá nhân
- **Điều kiện:** Người dùng đã đăng nhập
- **Luồng chính:**
  1. Người dùng mở màn hình Cài đặt
  2. Tab "Thông tin tài khoản" hiển thị mặc định
  3. Hệ thống hiển thị: Họ và tên, Số điện thoại, Email (readonly), Chức vụ (readonly)
- **Kết quả:** Người dùng thấy đầy đủ thông tin tài khoản hiện tại

### UC-01.2 – Cập nhật họ tên và số điện thoại
- **Điều kiện:** Người dùng đang ở tab Thông tin tài khoản
- **Luồng chính:**
  1. Người dùng chỉnh sửa ô "Họ và tên" và/hoặc "Số điện thoại"
  2. Nhấn nút **Lưu thay đổi**
  3. Hệ thống cập nhật thông tin
- **Trường chỉnh sửa được:** Họ và tên, Số điện thoại
- **Trường readonly:** Email (`admin@ezcloud.vn`), Chức vụ (`Quản trị viên`)

### UC-01.3 – Tải lên ảnh đại diện
- **Điều kiện:** Người dùng đang ở tab Thông tin tài khoản
- **Luồng chính:**
  1. Người dùng nhấn vào vùng ảnh đại diện (icon 📷)
  2. Hệ thống mở dialog chọn file
  3. Người dùng chọn file ảnh (JPG hoặc PNG, tối đa 2MB)
  4. Ảnh xem trước hiển thị ngay lập tức
- **Ràng buộc:** Định dạng JPG, PNG; dung lượng tối đa 2MB

---

## Dữ liệu hiển thị

| Trường | Loại | Ghi chú |
|--------|------|---------|
| Họ và tên | Input text | Có thể chỉnh sửa |
| Số điện thoại | Input text | Có thể chỉnh sửa |
| Email | Input text readonly | Không thể sửa |
| Chức vụ | Input text readonly | Không thể sửa |
| Ảnh đại diện | File upload | JPG/PNG, tối đa 2MB |

---

## Kết nối với màn hình khác

- **Truy cập từ:** Tất cả màn hình → nút "Cài đặt" ở khu vực user card cuối sidebar
- **Sidebar hiển thị:** Toàn bộ menu điều hướng chính (Khu vui chơi, Hoàn tiền, v.v.)
- **Theme áp dụng:** Theo cài đặt tại tab Giao diện (`system.html` → Tab S2)

---

## Ghi chú thiết kế

- Email và Chức vụ hiển thị dạng `readonly` (opacity 0.5, cursor not-allowed) — không được Admin/Member tự thay đổi
- Ảnh đại diện hiển thị chữ viết tắt tên (initials) khi chưa có ảnh
- Thông tin hiển thị công khai với các thành viên khác trong hệ thống (notice màu xanh)
