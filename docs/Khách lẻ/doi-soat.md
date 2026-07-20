# Use Case: Đối soát (Khách lẻ)

**Màn hình:** `RetailDiffView.vue` (props mặc định: `taPageKey='retailTaBookings'`, `bankPageKey='retailBankInflows'`, `entityLabel='Tên khách hàng'`)
**Đường dẫn truy cập:** Sidebar → Khách lẻ → Đối soát (`/khach-le/doi-soat`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — tính toán trực tiếp trên 2 bộ dữ liệu tĩnh `reportPages.retailTaBookings` và `reportPages.retailBankInflows`, không gọi API.

---

## Mô tả

Đối soát **booking khách lẻ trên TA** với **tiền thực nhận qua ngân hàng**, theo cơ chế **VLOOKUP theo Mã booking** (mã được parse từ cột "Diễn giải" của sao kê ngân hàng bằng regex `BK(\d+)`). Với mỗi booking trên TA, hệ thống tìm khoản tiền về ngân hàng tương ứng và so sánh số tiền.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-DSKL-01 – Xem kết quả đối soát

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:**
  1. Hệ thống duyệt qua toàn bộ booking trên TA (`retailTaBookings`)
  2. Với mỗi booking, tìm dòng sao kê ngân hàng có Diễn giải chứa đúng mã booking đó (regex `BK(\d+)`)
  3. So sánh **Số tiền trên TA** với **Số tiền về ngân hàng**:
     - Nếu không tìm thấy khoản tiền về tương ứng → trạng thái **"Chưa về ngân hàng"** (badge amber)
     - Nếu tìm thấy và số tiền khớp (chênh lệch = 0) → **"Đã khớp"** (badge xanh)
     - Nếu tìm thấy nhưng số tiền lệch → **"Lệch số tiền"** (badge đỏ), cột Chênh lệch hiển thị số tiền lệch
- **Kết quả:** Bảng đối soát đầy đủ; subtitle tổng hợp số booking chưa về tiền và số booking lệch số tiền

### UC-DSKL-02 – Tìm kiếm trong kết quả đối soát

- **Điều kiện:** Người dùng đang xem kết quả
- **Luồng chính:** Nhập mã booking hoặc tên khách hàng vào ô tìm kiếm
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DSKL-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày booking

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã booking trên TA |
| Tên khách hàng | Tên khách lẻ |
| Số tiền trên TA | Số tiền booking gốc |
| Số tiền về ngân hàng | Số tiền tìm được từ sao kê (hoặc "—" nếu chưa có) |
| Chênh lệch | 0 đ nếu khớp, số tiền lệch (đỏ) nếu không khớp, "—" nếu chưa về |
| Trạng thái | Badge: Đã khớp (xanh) / Lệch số tiền (đỏ) / Chưa về ngân hàng (amber) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Booking khách lẻ trên TA | Nguồn danh sách booking (base) |
| Tiền về ngân hàng (Khách lẻ) | Nguồn sao kê để đối chiếu |

---

## Ghi chú thiết kế

- Component `RetailDiffView` được **tái sử dụng** cho cả màn "Đối soát Khách lẻ" và "Đối soát OTA" (khác nhau qua props `taPageKey`/`bankPageKey`/`entityLabel`/`title`) — xem thêm `docs/Các đại lý OTA/doi-soat.md`
- Việc đối soát phụ thuộc hoàn toàn vào chất lượng parse mã booking từ Diễn giải sao kê; cần xử lý chuẩn hoá dữ liệu đầu vào khi nối API/mail thật
