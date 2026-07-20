# Use Case: Danh mục KVC con của Vin

**Màn hình:** `AgencyReportView.vue` (pageKey: `vinChildParks`)
**Đường dẫn truy cập:** Sidebar → Đối soát Vin → Danh mục KVC con của Vin (`/doi-soat-vin/danh-muc-kvc-con`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.vinChildParks`, 10 dòng mẫu), không gọi API.

---

## Mô tả

Danh mục các **mã KVC con** thuộc hệ sinh thái **Vinpearl/VinWonders**, mỗi KVC con gắn với 1 tài khoản ngân hàng riêng và dùng chung loại công nợ **"Nạp trước"**. Đây là danh mục nền tảng để các màn khác trong nhóm "Đối soát Vin" (giá vốn, số dư, đối soát) tham chiếu tới.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm |
| Member | Xem, tìm kiếm |

---

## Use Cases

### UC-DMKVC-01 – Xem danh mục KVC con Vin

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị toàn bộ KVC con (không phân trang theo ngày, không có bộ lọc ngày vì đây là danh mục tĩnh)
- **Kết quả:** Danh sách mã KVC, tài khoản ngân hàng, tên KVC, loại công nợ

### UC-DMKVC-02 – Tìm kiếm theo mã hoặc tên KVC

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm mã KVC, tên KVC..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã KVC | Mã định danh nội bộ (VD: `11810`) |
| TK Ngân hàng | Số tài khoản ngân hàng gắn với KVC con |
| Tên KVC | Tên cơ sở (VD: Timescity, Phú Quốc, Nha Trang…) |
| Loại Công nợ | Hiện tại toàn bộ là badge xanh dương "Nạp trước" |

## Danh mục mẫu (10 KVC con)

Timescity, VinWonders Vũ Yên, Phú Quốc, Nam Hội An, CÔNG VIÊN GRAND PARK, Hà Nội, VINWONDERS TIMES CITY, Nha Trang, VinWonders Cửa Hội, Nam Hội An (TK thứ 2)

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Chi tiết giá vốn vé bán (Vin) | Dùng tên KVC con làm khoá tra cứu giá vốn |
| Số dư KVC Vin theo ngày | Danh sách 11 "cơ sở" ở màn số dư (`vinFacilities`) tương ứng với các KVC con trong danh mục này, kèm thêm cột "Số ngày đáo hạn" |
| Đối soát KVC Vin | Tên KVC dùng để so khớp số liệu hệ thống với số liệu Vin cung cấp |

---

## Ghi chú thiết kế

- Danh mục này hiện là **danh sách tĩnh chỉ đọc** (không có nút thêm/sửa/xoá) — khi lên backend thật nên bổ sung CRUD tương tự "Danh sách các đại lý" nếu cần thêm/bớt KVC con thường xuyên
