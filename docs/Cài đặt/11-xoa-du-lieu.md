# Use Case: Xóa dữ liệu

**Màn hình:** `SystemSettingsView.vue` → Tab **🗑️ Xóa dữ liệu** (`reset`)
**Đường dẫn truy cập:** Sidebar → Cấu hình hệ thống (`/system`) → Tab "Xóa dữ liệu"
**Quyền truy cập:** Chỉ Admin
**Nguồn dữ liệu:** API thật — `services/systemApi.ts` → `resetAllData`

---

## Mô tả

**Vùng nguy hiểm** của hệ thống — cho phép Admin xoá **vĩnh viễn toàn bộ dữ liệu nghiệp vụ** (khu vui chơi, loại vé, giao dịch ngân hàng, đối soát, giá vốn, job, nhật ký hệ thống, và các tài khoản người dùng khác), chỉ giữ lại tài khoản Admin đang thao tác và cấu hình hệ thống. Dùng khi cần đưa hệ thống về trạng thái sạch (VD: sau giai đoạn thử nghiệm/demo, trước khi go-live thật).

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Thực hiện xoá toàn bộ dữ liệu |
| Member / Accountant | Không thấy tab này |

---

## Use Cases

### UC-XDL-01 – Xoá toàn bộ dữ liệu nghiệp vụ

- **Điều kiện:** Admin xác nhận muốn đưa hệ thống về trạng thái sạch
- **Luồng chính:**
  1. Đọc cảnh báo: hành động xoá **vĩnh viễn**, không thể khôi phục
  2. Gõ chính xác cụm từ xác nhận hiển thị sẵn (`RESET_CONFIRM_PHRASE`) vào ô nhập
  3. Nút **🗑️ Xóa toàn bộ dữ liệu** chỉ bật khi gõ đúng cụm xác nhận
  4. Nhấn nút → modal xác nhận lần 2 xuất hiện ("Xóa vĩnh viễn?")
  5. Xác nhận lần 2 → hệ thống gọi API xoá
- **Kết quả:** Toast báo số bản ghi đã xoá và số tài khoản Admin còn được giữ lại; toàn bộ dữ liệu nghiệp vụ về trạng thái trống

---

## Cơ chế an toàn (2 lớp xác nhận)

| Lớp | Cơ chế |
|-----|--------|
| 1 | Phải gõ đúng cụm từ xác nhận (case-sensitive, so khớp chính xác) mới bật được nút xoá |
| 2 | Modal confirm thứ hai nhắc lại hậu quả trước khi thực sự gọi API |

---

## Dữ liệu bị xoá / được giữ lại

| Loại dữ liệu | Kết quả |
|-------------|---------|
| Khu vui chơi, loại vé, giao dịch ngân hàng, đối soát, giá vốn | Xoá |
| Job, nhật ký hệ thống (audit log) | Xoá |
| Tài khoản người dùng khác | Xoá |
| Tài khoản Admin đang đăng nhập | **Giữ lại** |
| Cấu hình hệ thống (Cấu hình kết nối) | **Giữ lại** |

---

## Kết nối với màn hình khác

- Ảnh hưởng đến **toàn bộ** các màn hình có dữ liệu nghiệp vụ trong hệ thống (Khu vui chơi, Đại lý, Khách lẻ, Đối soát Vin, OTA) — tất cả sẽ trống sau khi xoá
- **Cấu hình hệ thống → Quản lý người dùng**: nơi tạo lại các tài khoản đã bị xoá nếu cần

---

## Ghi chú thiết kế

- Đây là thao tác **không thể hoàn tác** — cần cân nhắc kỹ trước khi dùng trên môi trường có dữ liệu thật
- Phù hợp cho môi trường demo/UAT, **không nên** để Admin thao tác nhầm trên môi trường production đang vận hành
