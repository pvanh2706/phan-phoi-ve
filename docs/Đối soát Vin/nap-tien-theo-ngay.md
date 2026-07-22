# Use Case: Danh sách nạp tiền cho Vin theo ngày

**Màn hình:** `AgencyReportView.vue` (pageKey: `vinTopUps`)
**Đường dẫn truy cập:** Sidebar → Đối soát Vin → Danh sách nạp tiền cho Vin theo ngày (`/doi-soat-vin/nap-tien-theo-ngay`) — nằm ngay dưới "Số dư KVC Vin theo ngày", phía trên "Chi tiết giá vốn vé bán"
**Quyền truy cập:** Admin, Member (xem, tìm kiếm, lọc theo ngày, Get API, Upload tay sao kê)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.vinTopUps`), mô phỏng sao kê lấy từ email báo có, chưa nối API/mail thật. Có nút **"⤓ Get API"** và **"📤 Upload tay sao kê"** giả lập (thuộc `pagesWithBankUpload` trong `AgencyReportView.vue`).

---

## Mô tả

Sao kê ngân hàng ghi nhận các khoản **tiền nạp vào tài khoản của các KVC con thuộc Vin** (Timescity, VinWonders Vũ Yên, Phú Quốc…), cùng định dạng sao kê đầy đủ như các màn "Tiền về ngân hàng"/"Giao dịch nạp tiền" khác trong hệ thống. Diễn giải chứa cú pháp `TKThe <số TK>` để đối chiếu với danh mục KVC con Vin.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, Get API, Upload tay sao kê |
| Member | Xem, tìm kiếm, lọc theo ngày, Get API, Upload tay sao kê |

---

## Use Cases

### UC-NTV-01 – Xem danh sách giao dịch nạp tiền

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị các dòng sao kê ngân hàng liên quan đến nạp tiền cho KVC con Vin
- **Kết quả:** Danh sách giao dịch nạp tiền theo từng KVC con

### UC-NTV-02 – Tìm kiếm theo số chứng từ / nội dung

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm số chứng từ, nội dung..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-NTV-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày ghi sổ

### UC-NTV-04 – Gọi API test (Get API)

- **Điều kiện:** Người dùng muốn mô phỏng lấy sao kê mới
- **Luồng chính:** Nhấn nút **"⤓ Get API"** (góc phải toolbar) → chờ ~800ms → toast báo số dòng "lấy được"
- **Kết quả:** Chỉ minh hoạ UI, không gọi API thật

### UC-NTV-05 – Upload tay sao kê

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
| Diễn giải | Nội dung chuyển khoản — chứa `TKThe <số tài khoản>` + tên KVC con |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Danh mục KVC con của Vin | Đối chiếu số tài khoản ngân hàng trong Diễn giải với danh mục KVC con |
| Số dư KVC Vin theo ngày | Số nạp tiền tại đây về nguyên tắc ảnh hưởng tới số dư công nợ tạm tính |

---

## Ghi chú thiết kế

- Cùng cấu trúc cột (`bankStatementColumns`) với các màn sao kê ngân hàng khác trong hệ thống (Đại lý, Khách lẻ, Các đại lý API) — nên dùng chung 1 component sao kê khi lên code thật
- Đây là trang **mới thêm** trong phiên làm việc này, khác với "Danh sách nạp tiền KVC theo ngày" (Khu vui chơi) vốn đã có backend thật — trang này vẫn ở dạng mock, nhất quán với toàn bộ khu vực Đối soát Vin
