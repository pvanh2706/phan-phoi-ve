# Use Case: Danh sách các đại lý

**Màn hình:** `AgencyListView.vue`
**Đường dẫn truy cập:** Sidebar → Đại lý → Danh sách các đại lý (`/dai-ly/danh-sach`)
**Quyền truy cập:** Admin (thêm/sửa/xoá), Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock — dữ liệu client-side, không có API backend.** Danh sách khởi tạo từ `data/agencies.ts` (114 đại lý, trích từ file Excel "Danh sách đại lý đang sử dụng 01-20.07.2026"); mọi thêm/sửa/xoá chỉ lưu tạm trong bộ nhớ trình duyệt và **mất khi tải lại trang**.

---

## Mô tả

Danh mục các đại lý thuộc nhóm **mua cấp trên mã 5129 (Đại Lý ezCloud Mua_PL)** — mô hình đại lý nạp trước một khoản tiền và lấy vé dần cho đến khi hết số dư. Màn hình cho phép quản lý danh mục (mã đại lý, tên, đại lý mua cấp trên, nguồn dữ liệu đồng bộ).

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, thêm, sửa, xoá |
| Member | Xem, tìm kiếm |

---

## Use Cases

### UC-DSDL-01 – Xem danh sách đại lý

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị 20 đại lý/trang, sắp xếp theo thứ tự khởi tạo
- **Kết quả:** Người dùng thấy toàn bộ danh mục đại lý và đại lý mua cấp trên tương ứng

### UC-DSDL-02 – Tìm kiếm đại lý

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập mã hoặc tên đại lý vào ô tìm kiếm → danh sách lọc real-time (không phân biệt hoa/thường), tự về trang 1
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DSDL-03 – Thêm đại lý mới

- **Điều kiện:** Admin cần bổ sung đại lý mới vào danh mục
- **Luồng chính:**
  1. Nhấn **Thêm đại lý**
  2. Modal mở với **Mã đại lý mua cấp trên** và **Tên đại lý mua cấp trên** điền sẵn mặc định (`5129` / `Đại Lý ezCloud Mua_PL`)
  3. Nhập **Mã đại lý \*** và **Tên đại lý \*** (bắt buộc, đánh dấu `*` đỏ), chọn **Nguồn dữ liệu**: OneInventory / AR / AR & OneInventory
  4. Nhấn **Lưu**
- **Kết quả:** Đại lý mới xuất hiện đầu danh sách (chỉ tồn tại trong phiên làm việc hiện tại)

### UC-DSDL-04 – Sửa thông tin đại lý

- **Điều kiện:** Đã có đại lý trong danh sách
- **Luồng chính:** Nhấn biểu tượng ✏️ → modal điền sẵn dữ liệu → chỉnh sửa → **Lưu**
- **Kết quả:** Thông tin đại lý được cập nhật trong danh sách hiện tại

### UC-DSDL-05 – Xoá đại lý

- **Điều kiện:** Admin muốn loại bỏ đại lý khỏi danh mục
- **Luồng chính:** Nhấn biểu tượng 🗑️ → modal xác nhận "Bạn có chắc muốn xóa đại lý…?" → xác nhận
- **Kết quả:** Đại lý bị loại khỏi danh sách hiển thị

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| # | Số thứ tự |
| Mã đại lý * | Mã định danh (VD: `7391`) — bắt buộc |
| Tên đại lý * | Tên đầy đủ, in đậm — bắt buộc |
| Mã đại lý mua cấp trên | Mặc định `5129` |
| Tên đại lý mua cấp trên | Mặc định `Đại Lý ezCloud Mua_PL` |
| Nguồn dữ liệu | Badge xanh dương — OneInventory / AR / AR & OneInventory |
| Hành động | ✏️ Sửa, 🗑️ Xoá |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên AR/TA/BIDV | Dùng chung khái niệm "tên đại lý" để tra cứu giao dịch |
| Số dư theo ngày của các đại lý (tự tính) | Danh sách 7 đại lý mẫu ở màn số dư là tập con minh hoạ, không đồng bộ trực tiếp với danh sách đầy đủ ở đây |

---

## Ghi chú thiết kế

- **Quan trọng:** đây là màn hình **demo dữ liệu tĩnh**, chưa có API backend — mọi thao tác Thêm/Sửa/Xoá không được lưu trữ lâu dài, sẽ mất khi tải lại trang trình duyệt
- Khi lên backend thật, cần bổ sung API CRUD đại lý (`GET/POST/PUT/DELETE /api/agencies`) và cân nhắc validate trùng mã đại lý
