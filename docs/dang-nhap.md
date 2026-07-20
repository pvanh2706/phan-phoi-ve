# Use Case: Đăng nhập

**Màn hình:** `LoginView.vue`
**Đường dẫn truy cập:** `/login` (route công khai, tự động chuyển hướng khi chưa đăng nhập)
**Quyền truy cập:** Tất cả người dùng nội bộ (Admin, Accountant, Member) sở hữu tài khoản hợp lệ
**Nguồn dữ liệu:** API thật — `services/authStore.ts` → `login(email, password)`

---

## Mô tả

Màn hình đăng nhập nội bộ duy nhất của hệ thống PPV (Đối soát phân phối vé). Không có chức năng "quên mật khẩu" hay tự đăng ký — tài khoản chỉ được Admin tạo sẵn tại Cấu hình hệ thống → Quản lý người dùng.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Người dùng nội bộ (Admin/Accountant/Member) | Đăng nhập bằng email + mật khẩu được cấp |

---

## Use Cases

### UC-DN-01 – Đăng nhập thành công

- **Điều kiện:** Người dùng có tài khoản hợp lệ, chưa đăng nhập
- **Luồng chính:**
  1. Truy cập bất kỳ URL nào trong hệ thống khi chưa đăng nhập → router tự động chuyển hướng sang `/login?redirect=<url gốc>`
  2. Nhập Email và Mật khẩu
  3. Nhấn **Đăng nhập** (nút disable cho đến khi cả 2 ô đều có giá trị)
  4. Hệ thống gọi API xác thực, lưu phiên đăng nhập
  5. Tự động chuyển hướng về URL gốc đã định lúc trước (`redirect` query param), hoặc `/` nếu không có
- **Kết quả:** Người dùng vào được hệ thống, thấy sidebar và màn hình tương ứng quyền của mình

### UC-DN-02 – Đăng nhập thất bại

- **Điều kiện:** Sai email/mật khẩu, tài khoản bị khoá, hoặc lỗi kết nối API
- **Luồng chính:**
  1. Người dùng nhấn Đăng nhập với thông tin không hợp lệ
  2. Hệ thống hiển thị thông báo lỗi màu đỏ ngay trong form (nội dung lỗi lấy từ API, hoặc thông báo mặc định "Không thể đăng nhập, vui lòng thử lại.")
- **Kết quả:** Người dùng ở lại màn hình đăng nhập, có thể thử lại

### UC-DN-03 – Đã đăng nhập truy cập lại `/login`

- **Điều kiện:** Người dùng đã có phiên đăng nhập hợp lệ
- **Luồng chính:** Truy cập `/login` → router phát hiện đã có `authState.user` → tự động chuyển hướng về `/`
- **Kết quả:** Không hiển thị lại form đăng nhập cho người đã đăng nhập

---

## Cấu trúc form

| Trường | Loại | Ghi chú |
|--------|------|---------|
| Email | text (type=email), autocomplete `username` | Bắt buộc |
| Mật khẩu | password, autocomplete `current-password` | Bắt buộc |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Cấu hình hệ thống → Quản lý người dùng | Nơi Admin tạo tài khoản, đặt mật khẩu, khoá/mở khoá |
| Cấu hình hệ thống → Quản lý thiết bị | Quản lý phiên đăng nhập (session) sau khi đăng nhập thành công |
| Nhật ký thay đổi (module "Đăng nhập") | Ghi nhận sự kiện đăng nhập/đăng xuất |

---

## Ghi chú thiết kế

- Route `/login` là route **public** duy nhất; mọi route khác đều yêu cầu `authState.user` tồn tại, nếu không sẽ redirect kèm query `redirect` để quay lại đúng trang sau khi đăng nhập
