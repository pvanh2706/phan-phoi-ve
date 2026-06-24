# Use Case: Đổi mật khẩu

**Màn hình:** `system.html` → Tab **Đổi mật khẩu**
**Đường dẫn truy cập:** Sidebar → Cài đặt → Tab "🔒 Đổi mật khẩu"

---

## Mô tả

Cho phép người dùng đổi mật khẩu tài khoản. Form yêu cầu nhập mật khẩu hiện tại để xác thực, sau đó nhập mật khẩu mới và xác nhận lại. Thanh độ mạnh mật khẩu và danh sách tiêu chí hiển thị real-time khi người dùng gõ.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Đổi mật khẩu tài khoản của mình |
| Member | Đổi mật khẩu tài khoản của mình |

---

## Use Cases

### UC-04.1 – Đổi mật khẩu thành công
- **Điều kiện:** Người dùng đang ở tab Đổi mật khẩu
- **Luồng chính:**
  1. Người dùng nhập mật khẩu hiện tại vào ô "Mật khẩu hiện tại"
  2. Nhập mật khẩu mới vào ô "Mật khẩu mới"
  3. Thanh độ mạnh và các tiêu chí cập nhật real-time
  4. Nhập lại mật khẩu mới vào ô "Xác nhận mật khẩu mới"
  5. Nhấn nút **Đổi mật khẩu**
  6. Hệ thống xác thực và cập nhật mật khẩu
- **Kết quả:** Mật khẩu được đổi thành công, hiển thị thông báo xác nhận

### UC-04.2 – Xem/ẩn mật khẩu
- **Điều kiện:** Người dùng đang nhập vào bất kỳ ô mật khẩu nào
- **Luồng chính:**
  1. Người dùng nhấn icon 👁 ở cuối ô input
  2. Mật khẩu chuyển từ dạng `•••••` sang hiển thị rõ ký tự
  3. Nhấn lại để ẩn
- **Kết quả:** Người dùng kiểm tra được mật khẩu đã nhập

### UC-04.3 – Kiểm tra độ mạnh mật khẩu real-time
- **Điều kiện:** Người dùng đang gõ vào ô "Mật khẩu mới"
- **Luồng chính:**
  1. Hệ thống phân tích mật khẩu theo 5 tiêu chí
  2. Thanh độ mạnh (5 đoạn) cập nhật màu sắc theo số tiêu chí đạt
  3. Danh sách tiêu chí đánh dấu ✓ (xanh) hoặc ✗ (xám) cho từng tiêu chí
- **Kết quả:** Người dùng biết mật khẩu đủ mạnh hay chưa trước khi submit

---

## Thanh độ mạnh mật khẩu

| Số tiêu chí đạt | Màu thanh | Nhãn |
|-----------------|-----------|------|
| 1 | Đỏ | Rất yếu |
| 2 | Cam | Yếu |
| 3 | Vàng | Trung bình |
| 4 | Xanh lá nhạt | Mạnh |
| 5 | Xanh lá đậm | Rất mạnh |

---

## Tiêu chí mật khẩu

| # | Tiêu chí |
|---|---------|
| 1 | Ít nhất 8 ký tự |
| 2 | Có chữ hoa (A-Z) |
| 3 | Có chữ thường (a-z) |
| 4 | Có chữ số (0-9) |
| 5 | Có ký tự đặc biệt (!@#$%^&*) |

---

## Trường dữ liệu

| Trường | Loại | Bắt buộc |
|--------|------|---------|
| Mật khẩu hiện tại | Password input | Có |
| Mật khẩu mới | Password input | Có |
| Xác nhận mật khẩu mới | Password input | Có |

---

## Luồng thay thế / ngoại lệ

| Tình huống | Xử lý |
|-----------|-------|
| Mật khẩu hiện tại sai | Hiển thị lỗi "Mật khẩu hiện tại không đúng" |
| Mật khẩu mới không khớp xác nhận | Hiển thị lỗi "Mật khẩu xác nhận không khớp" |
| Mật khẩu mới giống mật khẩu cũ | Hiển thị cảnh báo |
| Mật khẩu quá yếu (< 3 tiêu chí) | Nút "Đổi mật khẩu" disabled hoặc hiển thị cảnh báo |

---

## Kết nối với màn hình khác

- **Truy cập từ:** Tất cả màn hình → nút "Cài đặt" ở user card cuối sidebar
- **Sau khi đổi thành công:** Người dùng có thể tiếp tục ở màn hình Cài đặt hoặc đăng xuất
