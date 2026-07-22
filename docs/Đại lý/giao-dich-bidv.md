# Use Case: Giao dịch đại lý nạp tiền trên BIDV

**Màn hình:** `AgencyReportView.vue` (pageKey: `agencyBidvTransactions`)
**Đường dẫn truy cập:** Sidebar → Đại lý → Giao dịch đại lý nạp tiền trên BIDV (`/dai-ly/giao-dich-bidv`) — **mục cuối cùng** trong menu Đại lý (đã chuyển xuống dưới "Đối soát TA - AR", phía trên là mục mới "Tổng tiền các đại lý đã dùng theo tháng" xem `tong-tien-thang.md`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.agencyBidvTransactions`).

---

## Mô tả

Sao kê giao dịch **nạp tiền của đại lý** vào tài khoản BIDV của ezCloud, mô phỏng dữ liệu lấy tự động từ **email báo có** (theo mô hình đã áp dụng thật cho luồng "Danh sách nạp tiền KVC theo ngày"). Mỗi dòng là 1 giao dịch ngân hàng với đầy đủ thông tin sao kê: ngày ghi sổ, ngày hạch toán, mã giao dịch, số tiền, số chứng từ, diễn giải.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, Get API, Upload tay sao kê |
| Member | Xem, tìm kiếm, lọc theo ngày, Get API, Upload tay sao kê |

---

## Use Cases

### UC-GDBIDV-01 – Xem sao kê giao dịch BIDV

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị các dòng sao kê ngân hàng dạng đầy đủ (giống format sao kê gốc)
- **Kết quả:** Danh sách giao dịch nạp tiền của đại lý qua BIDV

### UC-GDBIDV-02 – Tìm kiếm theo số chứng từ / nội dung

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm số chứng từ, nội dung..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-GDBIDV-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày giao dịch

### UC-GDBIDV-04 – Gọi API test (Get API)

- **Điều kiện:** Người dùng muốn mô phỏng lấy sao kê mới
- **Luồng chính:** Nhấn nút **"⤓ Get API"** (góc phải toolbar) → chờ ~800ms → toast báo số dòng "lấy được"
- **Kết quả:** Chỉ minh hoạ UI, không gọi API thật, không thay đổi dữ liệu

### UC-GDBIDV-05 – Upload tay sao kê

- **Điều kiện:** Ngân hàng gặp sự cố, không tự gửi được sao kê qua email cho "Get API"
- **Luồng chính:**
  1. Nhấn nút **"📤 Upload tay sao kê"** (cạnh nút Get API, cùng ở góc phải toolbar)
  2. Chọn file PDF từ máy tính
  3. Hệ thống giả lập xử lý (~800ms) rồi báo toast đã tải lên (demo, chưa nối API thật)
- **Kết quả:** Minh hoạ hành vi UI cho luồng nạp sao kê thủ công; chú thích nhỏ bên dưới nút giải thích rõ trường hợp sử dụng, chiều rộng dòng chữ chú thích luôn khớp đúng chiều rộng nút Upload

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| From mail | Địa chỉ email nguồn sao kê (VD: `insaoke@bidv.com`) |
| Ngày ghi sổ | Ngày ngân hàng ghi nhận |
| Ngày hạch toán | Ngày hạch toán kế toán |
| Mã giao dịch | Loại bút toán (VD: `DD`) |
| Phát sinh nợ | Số tiền ghi nợ (căn phải) |
| Phát sinh có | Số tiền ghi có — tô xanh (căn phải) |
| Số dư | Số dư tài khoản sau giao dịch (căn phải) |
| Số chứng từ | Mã chứng từ ngân hàng |
| Diễn giải | Nội dung chuyển khoản gốc |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Danh sách nạp tiền KVC theo ngày | Cùng cơ chế lấy sao kê từ email — đã có API thật (`syncBankTransactions`); màn BIDV đại lý này nên tái sử dụng cùng pipeline khi lên backend |
| Danh sách các đại lý | Tên đại lý trong nội dung sao kê dùng để đối chiếu với danh mục đại lý |

---

## Ghi chú thiết kế

- Cấu trúc cột giống hệt sao kê ngân hàng dùng ở "Tiền về ngân hàng" của Khách lẻ/Các đại lý API — nên cân nhắc dùng chung 1 component sao kê ngân hàng khi code hoá thật
- Nút Get API/Upload tay sao kê hiện đều là **demo/mock** (state cục bộ, dùng `setTimeout` giả lập độ trễ) — khi có backend thật, tái sử dụng đúng pipeline `syncBankTransactions`/upload PDF đã làm thật cho "Danh sách nạp tiền KVC theo ngày" (`POST /api/bank-transaction-details/sync` và `/upload`)
