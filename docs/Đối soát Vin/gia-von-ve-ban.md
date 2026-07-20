# Use Case: Chi tiết giá vốn vé bán (Đối soát Vin)

**Màn hình:** `AgencyReportView.vue` (pageKey: `vinTicketCosts`)
**Đường dẫn truy cập:** Sidebar → Đối soát Vin → Chi tiết giá vốn vé bán (`/doi-soat-vin/gia-von-ve-ban`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.vinTicketCosts`). Có nút **"Gọi API test"** giả lập (nằm trong danh sách `pagesWithApiTest` của `AgencyReportView.vue`).

---

## Mô tả

Đối chiếu **giá vốn từng loại vé** bán qua các KVC con thuộc Vin, theo đại lý mua (chủ yếu là "Đại Lý ezCloud Mua_PL"). Cùng cấu trúc cột với "Chi tiết giá vốn vé bán" của nhóm Khu vui chơi thường, nhưng phạm vi dữ liệu giới hạn trong các KVC con Vin.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, gọi API test |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-GVV-01 – Xem chi tiết giá vốn vé bán Vin

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị các dòng giá vốn theo ngày bán, KVC con, đại lý, loại vé
- **Kết quả:** Danh sách chi tiết giá vốn với trạng thái ghi nhận

### UC-GVV-02 – Tìm kiếm

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá (tên KVC, đại lý, loại vé) vào ô tìm kiếm
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-GVV-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày bán

### UC-GVV-04 – Gọi API test

- **Điều kiện:** Người dùng muốn mô phỏng lấy dữ liệu mới
- **Luồng chính:** Nhấn **"Gọi API test"** → chờ ~800ms → toast báo số dòng "lấy được"
- **Kết quả:** Chỉ minh hoạ UI, không ảnh hưởng dữ liệu thật

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Ngày bán | Ngày phát sinh giao dịch |
| KVC | Tên KVC con Vin |
| Đại lý | Tên đại lý mua vé (thường: Đại Lý ezCloud Mua_PL) |
| Loại vé | VD: Vé người lớn, Combo gia đình, Vé trẻ em, Onsen package |
| SL | Số lượng vé (căn phải) |
| Giá vốn | Đơn giá vốn/vé |
| Tổng giá vốn | SL × Giá vốn |
| Trạng thái | Badge xanh "Đã ghi nhận" / amber "Cần kiểm tra" |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Danh mục KVC con của Vin | Nguồn danh mục tên KVC hiển thị ở cột "KVC" |
| Đối soát KVC Vin | Tổng giá vốn theo ngày là 1 trong 2 vế so sánh (hệ thống tự tính) khi đối soát với số liệu Vin cung cấp |

---

## Ghi chú thiết kế

- Trạng thái "Cần kiểm tra" cần được Kế toán rà soát trước khi đưa vào đối soát chính thức, tương tự cơ chế ở màn giá vốn Khu vui chơi thường
