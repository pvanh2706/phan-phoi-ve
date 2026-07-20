# Use Case: Số dư theo ngày của các đại lý (tự tính)

**Màn hình:** `AgencyMonthlyBalanceView.vue`
**Đường dẫn truy cập:** Sidebar → Đại lý → Số dư theo ngày của các đại lý (tự tính) (`/dai-ly/doi-soat`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo hoàn toàn — dữ liệu sinh ngẫu nhiên có seed (deterministic) ngay trên trình duyệt**, không gọi API, không có backend. Số liệu sinh ra dựa trên hash của tên đại lý + ngày nên **luôn giống nhau mỗi lần tải lại cùng 1 tháng**, nhưng không phản ánh dữ liệu thật.

---

## Mô tả

Mô phỏng bảng **số dư luỹ kế theo ngày** của 7 đại lý mẫu trong một tháng, theo công thức:

> **Số dư mới tự tính (ngày N) = Số dư đầu tháng + Tổng nạp thêm luỹ kế đến ngày N − Tổng đã dùng luỹ kế đến ngày N**

Mục đích minh hoạ cách hệ thống sẽ tính số dư công nợ/nạp trước của đại lý theo từng ngày trong tháng, phục vụ đối soát công nợ đại lý.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc theo tháng/đại lý, tìm kiếm |
| Member | Xem, lọc theo tháng/đại lý, tìm kiếm |

---

## Use Cases

### UC-SDDL-01 – Xem số dư theo ngày trong tháng hiện tại

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Mặc định chọn tháng hiện tại, hiển thị toàn bộ ngày trong tháng × 7 đại lý mẫu, phân trang 15 dòng/trang
- **Kết quả:** Bảng số dư từng ngày của tất cả đại lý mẫu trong tháng hiện tại

### UC-SDDL-02 – Đổi tháng xem

- **Điều kiện:** Người dùng muốn xem tháng khác
- **Luồng chính:** Chọn ô **Tháng** (input type=month) → dữ liệu được tính lại toàn bộ cho tháng mới
- **Kết quả:** Bảng cập nhật theo tháng đã chọn

### UC-SDDL-03 – Lọc theo đại lý

- **Điều kiện:** Người dùng muốn xem riêng 1 đại lý
- **Luồng chính:** Chọn tên đại lý trong dropdown **Tất cả đại lý**
- **Kết quả:** Bảng chỉ hiển thị các ngày của đại lý đã chọn

### UC-SDDL-04 – Tìm kiếm theo tên đại lý

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô tìm kiếm → lọc real-time theo tên đại lý
- **Kết quả:** Danh sách thu hẹp theo từ khoá

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Ngày | Ngày trong tháng đã chọn |
| Đại lý | Tên đại lý (trong 7 đại lý mẫu cố định) |
| Số dư ngày 01 đầu tháng | Số dư mở đầu tháng (`opening`, cố định cho cả tháng) |
| Số nạp thêm | Số tiền nạp thêm trong ngày đó (nếu có, tô xanh); ~18% số ngày có phát sinh nạp |
| Số đã dùng | Số tiền đã sử dụng trong ngày đó (tô đỏ nếu > 0) |
| Số dư mới tự tính | Số dư luỹ kế sau khi cộng nạp, trừ dùng đến hết ngày đó (tô xanh) |
| Số dư ngày cuối tháng | Số dư cuối cùng của cả tháng (`closing`, giống nhau ở mọi dòng cùng đại lý) |

## 7 đại lý mẫu cố định

`Oneinventory_Hanh BANA`, `Oneinventory_Anh Thư`, `Oneinventory_AGSAPA`, `Oneinventory_Sao Mai Sa Pa`, `Oneinventory_Phương Lan`, `Oneinventory_Cát Bà Vi Vu`, `Oneinventory_NhatKimYenTicket`

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Danh sách các đại lý | Nguồn danh mục đại lý đầy đủ (114 đại lý) — màn này chỉ minh hoạ với 7 đại lý mẫu |
| Giao dịch của các đại lý trên AR/TA | Về mặt nghiệp vụ, "Số đã dùng" nên lấy từ tổng giao dịch AR/TA thật khi có backend |

---

## Ghi chú thiết kế

- **Đây là dữ liệu giả lập thuần tuý để demo UI/UX** — số liệu sinh bằng hàm hash giả-ngẫu-nhiên (FNV-1a) theo `tên đại lý + ngày`, không lấy từ nguồn thật nào
- Khi triển khai thật, công thức số dư luỹ kế cần thay bằng dữ liệu thật: số dư đầu kỳ + tổng nạp tiền (từ sao kê BIDV) − tổng đã dùng (từ giao dịch AR/TA)
- Tên route hiện tại là `/dai-ly/doi-soat` nhưng nội dung là "số dư tự tính", không phải đối soát theo nghĩa so khớp 2 nguồn — có thể cân nhắc đổi tên route cho nhất quán khi refactor
