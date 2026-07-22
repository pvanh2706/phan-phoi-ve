# Use Case: Danh mục KVC con của Vin

**Màn hình:** `VinChildParksView.vue` (trang riêng, **không** còn dùng chung `AgencyReportView.vue` như trước)
**Đường dẫn truy cập:** Sidebar → Đối soát Vin → Danh mục KVC con của Vin (`/doi-soat-vin/danh-muc-kvc-con`)
**Quyền truy cập:** Admin, Member (xem, tìm kiếm, thêm, sửa, xoá)
**Nguồn dữ liệu:** ⚠️ **Demo/mock — state cục bộ trong component (không có backend riêng cho KVC con Vin).** Danh sách khởi tạo với 5 KVC con mẫu; mọi thêm/sửa/xoá chỉ lưu tạm trong bộ nhớ trình duyệt và **mất khi tải lại trang**.

---

## Mô tả

Danh mục các **mã KVC con** thuộc hệ sinh thái **Vinpearl/VinWonders**, mỗi KVC con gắn với 1 tài khoản ngân hàng riêng và loại công nợ (Nạp trước / Công nợ). Đây là danh mục nền tảng để các màn khác trong nhóm "Đối soát Vin" (giá vốn, số dư, nạp tiền, đối soát) tham chiếu tới.

Khác với trước đây (bảng tĩnh chỉ đọc), màn hình này giờ có đầy đủ **Thêm/Sửa/Xoá** với popup, theo đúng mẫu giao diện đã dùng ở "Mã khu vui chơi" (`ParkCodesView.vue`).

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, thêm, sửa, xoá |
| Member | Xem, tìm kiếm, thêm, sửa, xoá |

---

## Use Cases

### UC-DMKVC-01 – Xem danh mục KVC con Vin

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị toàn bộ KVC con hiện có (không phân trang, không lọc ngày vì đây là danh mục)
- **Kết quả:** Danh sách mã KVC, tài khoản ngân hàng, tên KVC, loại công nợ (badge)

### UC-DMKVC-02 – Tìm kiếm theo mã hoặc tên KVC

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm mã KVC, tên KVC..." → lọc real-time trên mã, tên, và số tài khoản ngân hàng
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DMKVC-03 – Thêm KVC con mới

- **Điều kiện:** Người dùng cần bổ sung KVC con mới vào danh mục
- **Luồng chính:**
  1. Nhấn **"+ Thêm KVC con"**
  2. Popup **"Thêm KVC con của Vin"** mở ra (rỗng)
  3. Nhập **Mã KVC \*** và **Tên KVC \*** (bắt buộc — chặn lưu và báo lỗi nếu thiếu), **TK Ngân hàng** (tuỳ chọn), chọn **Loại Công nợ**: Nạp trước / Công nợ
  4. Nhấn **Lưu thay đổi**
- **Kết quả:** KVC con mới xuất hiện cuối danh sách, toast báo "Đã thêm KVC con của Vin (demo, chưa lưu backend)"

### UC-DMKVC-04 – Sửa KVC con

- **Điều kiện:** Đã có KVC con trong danh sách
- **Luồng chính:** Nhấn biểu tượng ✏️ trên dòng → popup "Sửa KVC con của Vin" điền sẵn dữ liệu → chỉnh sửa → **Lưu thay đổi**
- **Kết quả:** Thông tin KVC con được cập nhật trong danh sách hiện tại

### UC-DMKVC-05 – Xoá KVC con (có popup cảnh báo)

- **Điều kiện:** Người dùng muốn loại bỏ KVC con khỏi danh mục
- **Luồng chính:**
  1. Nhấn biểu tượng 🗑️ trên dòng
  2. **Popup cảnh báo xác nhận** mở ra (dùng component `ConfirmDialog` dùng chung, tông màu đỏ "danger"): *"Bạn có chắc muốn xóa '{tên KVC}' (Mã {mã KVC}) khỏi danh mục? Thao tác này không thể hoàn tác."*
  3. Nhấn **Xóa** để xác nhận, hoặc **Hủy** để huỷ bỏ
- **Kết quả:** Nếu xác nhận, KVC con bị xoá khỏi danh sách hiển thị ngay; toast báo "Đã xóa KVC con của Vin (demo, chưa lưu backend)"

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã KVC | Mã định danh nội bộ (VD: `11810`) |
| TK Ngân hàng | Số tài khoản ngân hàng gắn với KVC con (hiện "-" nếu để trống) |
| Tên KVC | Tên cơ sở (VD: Timescity, Phú Quốc, Nha Trang…) |
| Loại Công nợ | Badge: xanh dương "Nạp trước" / chàm "Công nợ" |
| (hành động) | ✏️ Sửa, 🗑️ Xoá |

## Dữ liệu mẫu khởi tạo (5 KVC con)

Timescity, VinWonders Vũ Yên, Phú Quốc, Nam Hội An, CÔNG VIÊN GRAND PARK (đều mặc định loại "Nạp trước")

---

## Cấu trúc popup Thêm/Sửa

| Trường | Loại | Ghi chú |
|--------|------|---------|
| Mã KVC * | text | Bắt buộc |
| Tên KVC * | text | Bắt buộc |
| TK Ngân hàng | text | Tuỳ chọn |
| Loại Công nợ | select | Nạp trước / Công nợ, mặc định "Nạp trước" khi thêm mới |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Chi tiết giá vốn vé bán (Vin) | Dùng tên KVC con làm khoá tra cứu giá vốn |
| Số dư KVC Vin theo ngày | Danh sách 11 "cơ sở" ở màn số dư (`vinFacilities`) tương ứng với các KVC con trong danh mục này |
| Danh sách nạp tiền cho Vin theo ngày | Dùng tên/tài khoản KVC con để đối chiếu giao dịch nạp tiền |
| Đối soát KVC Vin | Tên KVC dùng để so khớp số liệu hệ thống với số liệu Vin cung cấp |

---

## Ghi chú thiết kế

- Trước đây màn này dùng chung `AgencyReportView.vue` + dữ liệu tĩnh `reportPages.vinChildParks` (chỉ đọc); đã tách thành component riêng `VinChildParksView.vue` để hỗ trợ CRUD, đồng thời **xoá bỏ** entry `vinChildParks`/`vinChildParkColumns`/`vinChildParkRows` khỏi `data/reports.ts` (không còn dùng)
- Popup xác nhận xoá dùng component dùng chung `ConfirmDialog.vue` + composable `useConfirm()` — cùng cơ chế đã dùng ở "Mã khu vui chơi" (`ParkCodesView.vue`) cho Xoá/Ngừng sử dụng KVC, KVC con, đại lý
- Đây là dữ liệu **mock hoàn toàn phía frontend** — khi lên backend thật cần entity riêng (tạm gọi `VinChildPark`) + API CRUD (`GET/POST/PUT/DELETE /api/vin-child-parks`), tương tự `ParkTicketType` của Khu vui chơi thường nhưng thêm trường `BankAccount`/`DebtType`
