# Use Case: Đối soát booking OTA với tiền về ngân hàng

**Màn hình:** `RetailDiffView.vue` (props: `taPageKey='otaTaBookings'`, `bankPageKey='otaBankInflows'`, `entityLabel='Tên đại lý OTA'`, `title='Đối soát booking OTA với tiền về ngân hàng'`)
**Đường dẫn truy cập:** Sidebar → Các đại lý OTA → Đối soát (`/dai-ly-ota/doi-soat`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — tính toán trực tiếp trên 2 bộ dữ liệu tĩnh `reportPages.otaTaBookings` và `reportPages.otaBankInflows`, không gọi API.

---

## Mô tả

Đối soát **booking của các kênh OTA trên TA** với **tiền thực nhận qua ngân hàng**, cùng cơ chế **VLOOKUP theo Mã booking** (parse từ Diễn giải sao kê ngân hàng bằng regex `BK(\d+)`) như màn "Đối soát" của Khách lẻ — chỉ khác nhãn thực thể là "Tên đại lý OTA" thay vì "Tên khách hàng".

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-DSOTA-01 – Xem kết quả đối soát OTA

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:**
  1. Hệ thống duyệt qua toàn bộ booking OTA trên TA
  2. Với mỗi booking, tìm khoản tiền về ngân hàng có Diễn giải chứa đúng mã booking
  3. So sánh số tiền, gắn trạng thái: **Đã khớp** (xanh) / **Lệch số tiền** (đỏ) / **Chưa về ngân hàng** (amber)
- **Kết quả:** Bảng đối soát; subtitle tổng hợp số booking chưa về tiền và số booking lệch số tiền

### UC-DSOTA-02 – Tìm kiếm trong kết quả đối soát

- **Điều kiện:** Người dùng đang xem kết quả
- **Luồng chính:** Nhập mã booking hoặc tên đại lý OTA vào ô tìm kiếm
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DSOTA-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày booking

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã booking trên TA |
| Tên đại lý OTA | Tên kênh OTA (Traveloka, Klook, Agoda…) |
| Số tiền trên TA | Số tiền booking gốc |
| Số tiền về ngân hàng | Số tiền tìm được từ sao kê (hoặc "—" nếu chưa có) |
| Chênh lệch | 0 đ nếu khớp, số tiền lệch (đỏ) nếu không khớp |
| Trạng thái | Badge: Đã khớp / Lệch số tiền / Chưa về ngân hàng |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Booking OTA trên TA | Nguồn danh sách booking (base) |
| Tiền về ngân hàng (OTA) | Nguồn sao kê để đối chiếu |
| Đối soát (Khách lẻ) | Cùng component `RetailDiffView`, chỉ khác cấu hình props đầu vào |

---

## Ghi chú thiết kế

- Do dùng chung component với "Đối soát Khách lẻ", mọi cải tiến logic đối soát (VD: cải thiện cách parse mã booking) sẽ áp dụng đồng thời cho cả 2 màn
- Các kênh OTA thường có tỷ lệ thanh toán trễ (chưa về ngân hàng) cao hơn khách lẻ trực tiếp do quy trình đối soát riêng của từng OTA — cần theo dõi ngưỡng thời gian hợp lý trước khi coi là bất thường
