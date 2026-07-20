# Use Case: Đối soát KVC Vin

**Màn hình:** `AgencyReportView.vue` (pageKey: `vinReconciliation`)
**Đường dẫn truy cập:** Sidebar → Đối soát Vin → Đối soát KVC Vin (`/doi-soat-vin/doi-soat`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.vinReconciliation`), không gọi API.

---

## Mô tả

So sánh **số dư/doanh thu do hệ thống tự tính** với **số liệu đối tác Vin cung cấp** cho từng KVC con theo ngày, nhằm phát hiện chênh lệch cần xử lý trước khi thanh toán/quyết toán công nợ với Vin.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày và theo chênh lệch |
| Member | Xem, tìm kiếm, lọc theo ngày và theo chênh lệch |

---

## Use Cases

### UC-DSVIN-01 – Xem kết quả đối soát theo ngày

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị từng KVC con theo ngày với số liệu Hệ thống, Đối tác, Chênh lệch, Ghi chú, Trạng thái
- **Kết quả:** Danh sách đối soát; các dòng khớp có trạng thái "Đã đối soát", dòng lệch có trạng thái "Cần xử lý"

### UC-DSVIN-02 – Tìm kiếm theo tên KVC

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập tên KVC vào ô "🔍 Tìm tên KVC..."
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DSVIN-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày đối soát

### UC-DSVIN-04 – Lọc theo trạng thái chênh lệch

- **Điều kiện:** Người dùng muốn xem riêng các dòng có/không có lệch
- **Luồng chính:** Chọn dropdown **"Tất cả chênh lệch"**: Không lệch / Có lệch
- **Kết quả:** Danh sách lọc theo đúng nhóm đã chọn

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Ngày | Ngày đối soát |
| KVC | Tên KVC con Vin |
| Hệ thống | Số liệu hệ thống tự tính, tô xanh (amount) |
| Đối tác | Số liệu Vin cung cấp, tô xanh (amount) |
| Chênh lệch | 0 đ nếu khớp; số tiền lệch (đỏ) nếu không khớp |
| Ghi chú | VD: "Lệch 1 giao dịch", "Chờ Vin xác nhận" |
| Trạng thái | Badge xanh "Đã đối soát" / amber "Cần xử lý" |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Chi tiết giá vốn vé bán (Vin) | Nguồn số liệu "Hệ thống" (tổng giá vốn tự tính) |
| Số dư KVC Vin theo ngày | Nguồn số liệu số dư/công nợ tạm tính liên quan |
| Danh mục KVC con của Vin | Danh mục tên KVC dùng để đối chiếu |

---

## Ghi chú thiết kế

- Cấu trúc và hành vi giống hệt "Đối soát Khu vui chơi" của nhóm Khu vui chơi thường (cùng dùng `reconcileColumns`) — chỉ khác phạm vi dữ liệu (KVC con Vin thay vì toàn bộ KVC)
- Khi có backend thật, luồng lấy "số liệu Đối tác" cần cơ chế nhập tay hoặc API riêng vì Vin là bên thứ 3 không có API công khai cho ezCloud
