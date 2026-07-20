# Use Case: Tiền về ngân hàng (Khách lẻ)

**Màn hình:** `AgencyReportView.vue` (pageKey: `retailBankInflows`)
**Đường dẫn truy cập:** Sidebar → Khách lẻ → Tiền về ngân hàng (`/khach-le/tien-ve-ngan-hang`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.retailBankInflows`), mô phỏng sao kê lấy từ email báo có, chưa nối API/mail thật cho luồng khách lẻ.

---

## Mô tả

Sao kê ngân hàng ghi nhận các khoản **tiền khách lẻ chuyển khoản vào** tài khoản ezCloud, theo định dạng sao kê đầy đủ (ngày ghi sổ, ngày hạch toán, số chứng từ, diễn giải…). Diễn giải thường chứa mã booking dạng `BK<số>` để phục vụ đối chiếu tự động ở màn "Đối soát".

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-TVNH-01 – Xem sao kê tiền về ngân hàng

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị các dòng sao kê ngân hàng liên quan đến khách lẻ
- **Kết quả:** Danh sách giao dịch tiền về từ khách lẻ

### UC-TVNH-02 – Tìm kiếm theo số chứng từ / nội dung

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm số chứng từ, nội dung..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-TVNH-03 – Lọc theo khoảng ngày

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
| Diễn giải | Nội dung chuyển khoản — chứa mã booking dạng `BK<số>` để đối chiếu tự động |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Booking khách lẻ trên TA | Đối chiếu theo mã booking parse từ cột Diễn giải |
| Đối soát (Khách lẻ) | Dùng chính dữ liệu màn này làm nguồn "số tiền về ngân hàng" trong phép VLOOKUP đối soát |

---

## Ghi chú thiết kế

- Diễn giải cần luôn chứa đúng định dạng `BK<mã booking>` để cơ chế đối soát tự động (regex `BK(\d+)`) hoạt động chính xác — nếu email ngân hàng thật không tuân thủ định dạng này, cần chuẩn hoá ở bước xử lý mail trước khi ghi vào bảng
