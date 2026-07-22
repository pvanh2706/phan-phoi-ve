# Use Case: Tiền về ngân hàng (Các đại lý API)

**Màn hình:** `AgencyReportView.vue` (pageKey: `otaBankInflows`)
**Đường dẫn truy cập:** Sidebar → Các đại lý API → Tiền về ngân hàng (`/dai-ly-ota/tien-ve-ngan-hang`)
**Quyền truy cập:** Admin, Member (xem, Get API, Upload tay sao kê)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.otaBankInflows`, 11 dòng mẫu), mô phỏng sao kê lấy từ email báo có. Có nút **"⤓ Get API"** và **"📤 Upload tay sao kê"** giả lập (thuộc `pagesWithBankUpload` trong `AgencyReportView.vue`).

---

## Mô tả

Sao kê ngân hàng ghi nhận **tiền các đại lý API chuyển vào** tài khoản ezCloud, cùng định dạng sao kê đầy đủ như các màn "Tiền về ngân hàng" khác trong hệ thống. Diễn giải chứa mã booking dạng `BK<số>` để đối chiếu tự động với booking trên TA.

> **Đổi tên:** menu "Các đại lý OTA" → **"Các đại lý API"**; nhãn, subtitle trong `reports.ts` đã đổi "đại lý OTA" → "đại lý API"; đường dẫn và `pageKey` (`otaBankInflows`) giữ nguyên.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, Get API, Upload tay sao kê |
| Member | Xem, tìm kiếm, lọc theo ngày, Get API, Upload tay sao kê |

---

## Use Cases

### UC-TVNHAPI-01 – Xem sao kê tiền về từ API

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị 11 dòng sao kê ngân hàng mẫu liên quan đến các kênh API
- **Kết quả:** Danh sách giao dịch tiền về từ các kênh API

### UC-TVNHAPI-02 – Tìm kiếm theo số chứng từ / nội dung

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm số chứng từ, nội dung..."
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-TVNHAPI-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày ghi sổ

### UC-TVNHAPI-04 – Gọi API test (Get API)

- **Điều kiện:** Người dùng muốn mô phỏng lấy sao kê mới
- **Luồng chính:** Nhấn nút **"⤓ Get API"** (góc phải toolbar) → chờ ~800ms → toast báo số dòng "lấy được"
- **Kết quả:** Chỉ minh hoạ UI, không gọi API thật

### UC-TVNHAPI-05 – Upload tay sao kê

- **Điều kiện:** Ngân hàng gặp sự cố, không tự gửi được sao kê qua email
- **Luồng chính:**
  1. Nhấn nút **"📤 Upload tay sao kê"** (cạnh nút Get API)
  2. Chọn file PDF từ máy tính
  3. Hệ thống giả lập xử lý (~800ms) rồi báo toast đã tải lên (demo)
- **Kết quả:** Minh hoạ UI; chú thích nhỏ bên dưới nút giải thích trường hợp sử dụng, chiều rộng dòng chữ chú thích khớp đúng chiều rộng nút Upload

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| From mail | Email nguồn sao kê |
| Ngày ghi sổ | Ngày ngân hàng ghi nhận |
| Ngày hạch toán | Ngày hạch toán kế toán |
| Mã giao dịch | Loại bút toán |
| Phát sinh nợ | Ghi nợ (căn phải) |
| Phát sinh có | Ghi có, tô xanh (căn phải) |
| Số dư | Số dư sau giao dịch (căn phải) |
| Số chứng từ | Mã chứng từ |
| Diễn giải | Nội dung chuyển khoản — chứa mã booking dạng `BK<số>` |

## Dữ liệu mẫu (11 dòng, khớp với hầu hết booking ở "Booking API trên TA")

11/14 booking có tiền về (9 khớp đúng số tiền + 2 lệch số tiền); 3 booking còn lại (KKday, Klook, Expedia) chưa có sao kê tương ứng — dùng để minh hoạ trạng thái "Chưa về ngân hàng" ở trang Đối soát

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Booking API trên TA | Đối chiếu theo mã booking parse từ cột Diễn giải |
| Đối soát (Các đại lý API) | Dùng dữ liệu màn này làm nguồn "số tiền về ngân hàng" trong phép đối soát |

---

## Ghi chú thiết kế

- Cùng cấu trúc cột (`bankStatementColumns`) với các màn sao kê ngân hàng khác trong hệ thống — nên dùng chung 1 component sao kê khi lên code thật thay vì lặp lại cấu hình cho từng nhóm (Đại lý/Khách lẻ/Các đại lý API)
- Dữ liệu mẫu đã được **mở rộng từ 2 lên 11 dòng**, có chủ đích tạo 2 trường hợp lệch số tiền (Klook, Traveloka) và để trống 3 booking không có sao kê, nhằm minh hoạ đầy đủ 3 trạng thái đối soát (khớp/lệch/chưa về) — xem `doi-soat.md`
