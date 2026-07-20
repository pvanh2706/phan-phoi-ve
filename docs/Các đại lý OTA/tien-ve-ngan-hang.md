# Use Case: Tiền về ngân hàng (OTA)

**Màn hình:** `AgencyReportView.vue` (pageKey: `otaBankInflows`)
**Đường dẫn truy cập:** Sidebar → Các đại lý OTA → Tiền về ngân hàng (`/dai-ly-ota/tien-ve-ngan-hang`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.otaBankInflows`), mô phỏng sao kê lấy từ email báo có.

---

## Mô tả

Sao kê ngân hàng ghi nhận **tiền các đại lý OTA chuyển vào** tài khoản ezCloud, cùng định dạng sao kê đầy đủ như các màn "Tiền về ngân hàng" khác trong hệ thống. Diễn giải chứa mã booking dạng `BK<số>` để đối chiếu tự động với booking trên TA.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-TVNHOTA-01 – Xem sao kê tiền về từ OTA

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị các dòng sao kê ngân hàng liên quan đến OTA
- **Kết quả:** Danh sách giao dịch tiền về từ các kênh OTA

### UC-TVNHOTA-02 – Tìm kiếm theo số chứng từ / nội dung

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm số chứng từ, nội dung..."
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-TVNHOTA-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày ghi sổ

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

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Booking OTA trên TA | Đối chiếu theo mã booking parse từ cột Diễn giải |
| Đối soát (OTA) | Dùng dữ liệu màn này làm nguồn "số tiền về ngân hàng" trong phép đối soát |

---

## Ghi chú thiết kế

- Cùng cấu trúc cột (`bankStatementColumns`) với các màn sao kê ngân hàng khác trong hệ thống — nên dùng chung 1 component sao kê khi lên code thật thay vì lặp lại cấu hình cho từng nhóm (Đại lý/Khách lẻ/OTA)
