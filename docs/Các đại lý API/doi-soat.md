# Use Case: Đối soát booking API với tiền về ngân hàng

**Màn hình:** `RetailDiffView.vue` (props: `taPageKey='otaTaBookings'`, `bankPageKey='otaBankInflows'`, `entityLabel='Tên đại lý API'`, `title='Đối soát booking API với tiền về ngân hàng'`)
**Đường dẫn truy cập:** Sidebar → Các đại lý API → Đối soát (`/dai-ly-ota/doi-soat`)
**Quyền truy cập:** Admin, Member (xem, tìm kiếm, lọc theo ngày, Build đối soát)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — tính toán trực tiếp trên 2 bộ dữ liệu tĩnh `reportPages.otaTaBookings` (14 dòng) và `reportPages.otaBankInflows` (11 dòng), không gọi API. Có nút **"Build đối soát"** giả lập.

---

## ⚠️ Lỗi đã phát hiện và sửa (quan trọng)

Trước khi có các thay đổi trong tài liệu này, **màn hình này bị crash** (`Cannot read properties of undefined (reading 'kind')`) khi mở, vì `RetailDiffView.vue` lấy dữ liệu theo **vị trí cột cố định** (`row.cells[4]` cho số tiền TA, `row.cells[1]` cho tên đại lý) — vị trí này chỉ đúng với bảng "Booking khách lẻ trên TA" (5 cột, có thêm cột SĐT ở giữa). Bảng "Booking API trên TA" (`agencyTaColumns`) chỉ có **4 cột** (không có SĐT), nên `cells[4]` là `undefined` và gây lỗi khi render.

**Đã sửa:** `RetailDiffView.vue` giờ lấy vị trí cột bằng cách tra `key` trong mảng `columns` (`booking`/`customer` hoặc `agency`/`amount`) thay vì số thứ tự cố định — hoạt động đúng cho cả 2 dạng bảng (4 cột và 5 cột) mà không cần biết trước cấu trúc bảng nguồn.

---

## Mô tả

Đối soát **booking của các kênh API trên TA** với **tiền thực nhận qua ngân hàng**, cùng cơ chế **VLOOKUP theo Mã booking** (parse từ Diễn giải sao kê ngân hàng bằng regex `BK(\d+)`) như màn "Đối soát" của Khách lẻ — chỉ khác nhãn thực thể là "Tên đại lý API" thay vì "Tên khách hàng".

> **Đổi tên:** "Các đại lý OTA" → **"Các đại lý API"**; tiêu đề trang và `entityLabel` đã đổi từ "OTA" sang "API" tương ứng.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, Build đối soát |
| Member | Xem, tìm kiếm, lọc theo ngày, Build đối soát |

---

## Use Cases

### UC-DSAPI-01 – Xem kết quả đối soát API

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:**
  1. Hệ thống duyệt qua toàn bộ 14 booking API trên TA
  2. Với mỗi booking, tìm khoản tiền về ngân hàng có Diễn giải chứa đúng mã booking
  3. So sánh số tiền, gắn trạng thái: **Đã khớp** (xanh) / **Lệch số tiền** (đỏ) / **Chưa về ngân hàng** (amber)
- **Kết quả:** Với bộ dữ liệu mẫu hiện tại: **9 booking khớp**, **2 booking lệch số tiền** (Klook, Traveloka), **3 booking chưa về ngân hàng** (KKday, Klook, Expedia); subtitle tổng hợp số booking chưa về tiền và số booking lệch số tiền

### UC-DSAPI-02 – Tìm kiếm trong kết quả đối soát

- **Điều kiện:** Người dùng đang xem kết quả
- **Luồng chính:** Nhập mã booking hoặc tên đại lý API vào ô tìm kiếm
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DSAPI-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày booking

### UC-DSAPI-04 – Build đối soát

- **Điều kiện:** Người dùng muốn build/tính lại đối soát
- **Luồng chính:** Nhấn nút **"Build đối soát"** (góc phải toolbar) → chờ ~800ms → toast báo đã build kèm số dòng
- **Kết quả:** Demo/mock — minh hoạ hành vi UI, chưa gọi API thật

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã booking trên TA |
| Tên đại lý API | Tên kênh API (Traveloka, Klook, Agoda, Booking.com, KKday, Expedia, Trip.com…) |
| Số tiền trên TA | Số tiền booking gốc |
| Số tiền về ngân hàng | Số tiền tìm được từ sao kê (hoặc "—" nếu chưa có) |
| Chênh lệch | 0 đ nếu khớp, số tiền lệch (đỏ) nếu không khớp |
| Trạng thái | Badge: Đã khớp / Lệch số tiền / Chưa về ngân hàng |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Booking API trên TA | Nguồn danh sách booking (base) |
| Tiền về ngân hàng (Các đại lý API) | Nguồn sao kê để đối chiếu |
| Đối soát (Khách lẻ) | Cùng component `RetailDiffView`, chỉ khác cấu hình props đầu vào — xem `docs/Khách lẻ/doi-soat.md` |

---

## Ghi chú thiết kế

- Do dùng chung component với "Đối soát Khách lẻ", mọi cải tiến logic đối soát (VD: cải thiện cách parse mã booking, hoặc fix lỗi lấy cột theo `key`) sẽ áp dụng đồng thời cho cả 2 màn
- Các kênh API/OTA thường có tỷ lệ thanh toán trễ (chưa về ngân hàng) cao hơn khách lẻ trực tiếp do quy trình đối soát riêng của từng đối tác — cần theo dõi ngưỡng thời gian hợp lý trước khi coi là bất thường
- **Bài học thiết kế:** khi tái sử dụng 1 component cho nhiều nguồn dữ liệu có cấu trúc cột khác nhau (khác số cột), luôn tra cứu vị trí theo `key`/tên cột thay vì hard-code index — xem phần "Lỗi đã phát hiện và sửa" ở đầu tài liệu
