# Use Case: Số dư KVC Vin theo ngày

**Màn hình:** `AgencyReportView.vue` (pageKey: `vinDailyBalances`)
**Đường dẫn truy cập:** Sidebar → Đối soát Vin → Số dư KVC Vin theo ngày (`/doi-soat-vin/so-du-theo-ngay`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.vinDailyBalances`), nguồn gốc lấy từ ảnh chụp sao kê thực tế (ghi chú trong code: *"Giá trị lấy đúng theo ảnh sao kê; ô '-290,40…' bị cắt do độ rộng cột trong ảnh gốc, chưa xác nhận được đủ số"*). Có nút **"Gọi API test"** giả lập.

---

## Mô tả

Bảng **số dư công nợ tạm tính** của từng cơ sở Vinpearl/VinWonders theo ngày, cùng **số ngày đáo hạn** thanh toán. Vì đây là mô hình công nợ (không phải nạp trước như tên các KVC con gợi ý ở màn Danh mục), số dư âm là bình thường — thể hiện số tiền ezCloud đang nợ/còn phải thanh toán cho Vin.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, gọi API test |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-SDVIN-01 – Xem số dư theo ngày

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị số dư của 11 cơ sở Vin qua các ngày 28/04–01/05/2026 (dữ liệu mẫu)
- **Kết quả:** Danh sách số dư công nợ tạm tính từng cơ sở theo từng ngày

### UC-SDVIN-02 – Tìm kiếm theo tên cơ sở / TK công nợ

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm tên cơ sở, TK công nợ..."
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-SDVIN-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày

### UC-SDVIN-04 – Gọi API test

- **Điều kiện:** Người dùng muốn mô phỏng lấy dữ liệu mới
- **Luồng chính:** Nhấn **"Gọi API test"** → chờ ~800ms → toast báo số dòng "lấy được"
- **Kết quả:** Chỉ minh hoạ UI

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| STT | Số thứ tự cố định theo danh sách 11 cơ sở |
| Cơ sở | Tên cơ sở Vin (Phú Quốc, Nam Hội An, Hà Nội, Timescity, Nha Trang, Nha Trang Hòn Tằm, VinWonders Cửa Hội, CÔNG VIÊN GRAND PARK, VinWonders Vũ Yên, Vinpearl HO…) |
| TK Công nợ | Mã tài khoản công nợ (VD: `20003041-PQ`) |
| Ngày | Ngày snapshot số dư |
| Số dư hiện tại (Tạm tính) | Số âm = đang nợ Vin (tô đỏ); một số ô được đánh dấu **highlight amber** vì dữ liệu gốc bị cắt/chưa xác nhận đủ số |
| Số ngày đáo hạn | Thường "7 ngày"; riêng Vinpearl HO là "0 ngày" |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Danh mục KVC con của Vin | Nguồn danh mục cơ sở/tài khoản công nợ |
| Đối soát KVC Vin | Số dư tạm tính tại đây là 1 vế trong phép so sánh với số liệu Vin cung cấp |

---

## Ghi chú thiết kế

- Một số giá trị mẫu (VD: `-290,40…`) **bị cắt do giới hạn độ rộng cột trong ảnh sao kê gốc dùng làm nguồn dữ liệu** — cần xác nhận lại số liệu đầy đủ khi có API/số liệu chính thức từ Vin, không nên coi các ô được đánh dấu vàng (amber) là số liệu đã xác thực
- "Số ngày đáo hạn" là thông tin quan trọng để Kế toán ưu tiên chuẩn bị thanh toán đúng hạn cho từng cơ sở
