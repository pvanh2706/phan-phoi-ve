# Use Case: Cấu hình kết nối

**Màn hình:** `SystemSettingsView.vue` → Tab **🔌 Cấu hình kết nối** (`connections`)
**Đường dẫn truy cập:** Sidebar → Cấu hình hệ thống (`/system`) → Tab "Cấu hình kết nối"
**Quyền truy cập:** Chỉ Admin
**Nguồn dữ liệu:** API thật — `services/connectionSettingsApi.ts`

---

## Mô tả

Màn hình cấu hình toàn bộ **kết nối lấy dữ liệu từ bên ngoài** mà các job đồng bộ của hệ thống sử dụng: hộp mail IMAP nhận sao kê BIDV, API số dư KVC (ParkBalance), API OneInventory (giá vốn vé), và khung giờ chạy job tự động. Thay đổi được **áp dụng ngay** cho lần chạy job kế tiếp, không cần khởi động lại hệ thống.

Đây là màn hình nhạy cảm nhất trong Cấu hình hệ thống vì chứa thông tin đăng nhập (mật khẩu mail, mật khẩu API).

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, sửa, kiểm tra kết nối |
| Member / Accountant | Không thấy tab này |

---

## Use Cases

### UC-CHKN-01 – Xem và tải cấu hình hiện tại

- **Điều kiện:** Admin mở tab Cấu hình kết nối
- **Luồng chính:** Hệ thống gọi `getConnectionSettings`, hiển thị 4 nhóm: Mail BIDV, API Số dư KVC, API OneInventory, Khung giờ chạy job
- **Kết quả:** Form hiển thị đầy đủ cấu hình đang áp dụng; mật khẩu mặc định ẩn (dạng `•••`)

### UC-CHKN-02 – Cấu hình mail sao kê BIDV (IMAP)

- **Điều kiện:** Admin cần đổi thông tin hộp mail nhận sao kê
- **Luồng chính:**
  1. Nhập **Host**, **Port**, **Username**, **Password**
  2. Nhập **Mailbox** (VD: `INBOX`), **Lọc người gửi** (VD: `insaoke@bidv.com.vn`)
  3. Nhập **Mật khẩu mở file PDF sao kê**, bật/tắt **Dùng SSL**
  4. Có thể bấm biểu tượng 👁️ để hiện/ẩn mật khẩu khi nhập
- **Kết quả:** Thông tin sẵn sàng để lưu hoặc kiểm tra kết nối

### UC-CHKN-03 – Kiểm tra kết nối mail

- **Điều kiện:** Đã nhập đủ thông tin mail
- **Luồng chính:** Nhấn **🔌 Kiểm tra kết nối mail** → hệ thống thử kết nối IMAP ngay với giá trị đang nhập (chưa cần lưu)
- **Kết quả:** Toast báo thành công kèm thời gian phản hồi (ms), hoặc báo lỗi cụ thể

### UC-CHKN-04 – Cấu hình API Số dư KVC / OneInventory

- **Điều kiện:** Admin cần đổi endpoint hoặc thông tin xác thực API nguồn
- **Luồng chính:**
  1. Nhập **Endpoint**/**Base URL**, **Timeout (giây)**
  2. Với OneInventory: nhập thêm **Username**, **Password**
  3. Nhấn **🔌 Kiểm tra kết nối API số dư** / **🔌 Kiểm tra kết nối OneInventory** để test trực tiếp
- **Kết quả:** Xác nhận API nguồn phản hồi đúng trước khi lưu

### UC-CHKN-05 – Cấu hình khung giờ chạy job

- **Điều kiện:** Admin muốn đổi lịch chạy tự động
- **Luồng chính:**
  1. Đặt **Giờ chạy Số dư KVC**, **Giờ chạy Giá vốn vé**
  2. Đặt khung **Quét mail BIDV** (từ giờ – đến giờ) và **Chu kỳ quét (phút)**
- **Kết quả:** Job scheduler áp dụng khung giờ mới từ lần chạy kế tiếp

### UC-CHKN-06 – Lưu cấu hình kết nối

- **Điều kiện:** Đã chỉnh sửa ít nhất 1 trường
- **Luồng chính:** Nhấn **💾 Lưu cấu hình** → hệ thống gọi `saveConnectionSettings`
- **Kết quả:** Toast xác nhận đã lưu, áp dụng ngay cho lần lấy dữ liệu/chạy job kế tiếp

### UC-CHKN-07 – Tải lại cấu hình

- **Điều kiện:** Admin vừa sửa nhưng muốn huỷ bỏ thay đổi chưa lưu
- **Luồng chính:** Nhấn **Tải lại** → form nạp lại giá trị đã lưu gần nhất từ server
- **Kết quả:** Mọi thay đổi chưa lưu bị huỷ

---

## Các nhóm cấu hình

| Nhóm | Trường chính |
|------|-------------|
| 📧 Mail sao kê BIDV (IMAP) | Host, Port, Username, Password, Mailbox, FromFilter, PDF Password, SSL |
| 🏦 API Số dư KVC (ParkBalance) | Endpoint, Timeout |
| 🎟️ API OneInventory (giá vốn vé) | Base URL, Timeout, Username, Password |
| ⏰ Khung giờ chạy job | Giờ chạy số dư, giờ chạy giá vốn, khung quét mail BIDV (từ–đến), chu kỳ quét (phút) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Lỗi đồng bộ cần xử lý | Job chạy theo cấu hình endpoint/giờ tại đây; lỗi kết nối hiển thị tại đó |
| Cấu hình hệ thống → Log gọi API | Ghi lại từng lượt gọi thực tế đến các endpoint cấu hình ở đây |

---

## Ghi chú thiết kế

- **Vùng nhạy cảm:** chứa mật khẩu mail và mật khẩu API — nên giới hạn chỉ Admin truy cập (đã áp dụng đúng trong code)
- Nút kiểm tra kết nối dùng giá trị **đang nhập trên form**, không cần lưu trước mới test được
