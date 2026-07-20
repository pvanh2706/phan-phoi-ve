# Use Case: Lỗi đồng bộ cần xử lý

**Màn hình:** `JobErrorsView.vue`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Lỗi đồng bộ cần xử lý (`/khu-vui-choi/loi-dong-bo`)
**Quyền truy cập:** Admin, Accountant (nhập tay + gửi email), Member (chỉ xem)
**Nguồn dữ liệu:** API thật — `jobsApi` (chạy job, liệt kê lỗi), `summariesApi` (lưu dữ liệu nhập tay), `parksApi` (danh sách KVC cho dropdown)

---

## Mô tả

Màn hình trung tâm xử lý sự cố khi các job đồng bộ dữ liệu (Số dư KVC, Giá vốn vé bán, Giao dịch ngân hàng) gọi API bên ngoài bị lỗi. Kế toán/Accountant có thể:
1. **Chạy lại job** thủ công cho một ngày cụ thể (Số dư / Giá vốn / Ngân hàng)
2. **Nhập tay** dữ liệu thay thế khi API nguồn không khả dụng (áp dụng cho Giá vốn vé và Giao dịch ngân hàng — **không áp dụng cho Số dư KVC**)
3. **Gửi email tổng hợp lỗi** cho danh sách người nhận đã cấu hình ở Cài đặt

Mục tiêu là đảm bảo dữ liệu được ghi nhận đúng ngày phát sinh (business date), tránh lệch sang ngày hôm sau khi API nguồn gặp sự cố tạm thời.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc, chạy job, nhập tay, gửi email lỗi |
| Accountant | Xem, lọc, chạy job, nhập tay, gửi email lỗi |
| Member | Chỉ xem, lọc |

---

## Use Cases

### UC-LDB-01 – Xem danh sách lỗi cần xử lý (mặc định)

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Hệ thống tải danh sách với bộ lọc mặc định `status = Failed` (Cần xử lý)
- **Kết quả:** Danh sách hiển thị các lượt gọi API thất bại chưa được xử lý

### UC-LDB-02 – Lọc theo ngày, nguồn, trạng thái

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:**
  1. Chọn ngày dữ liệu (business date)
  2. Chọn nguồn: Số dư KVC / Giá vốn vé bán / Giao dịch ngân hàng
  3. Chọn trạng thái: Cần xử lý (`Failed`) / Đã nhập tay (`ManualResolved`)
  4. Nhấn **Lọc**
- **Kết quả:** Danh sách thu hẹp theo điều kiện

### UC-LDB-03 – Chạy lại job thủ công

- **Điều kiện:** Người dùng muốn thử gọi lại API cho một ngày cụ thể
- **Luồng chính:**
  1. Chọn ngày dữ liệu (nếu bỏ trống, hệ thống dùng ngày hôm nay)
  2. Nhấn một trong 3 nút: **Chạy số dư** / **Chạy giá vốn** / **Chạy ngân hàng**
  3. Hệ thống gọi API tương ứng (`runParkBalances` / `runTicketCosts` / `runBankTransactions`) cho toàn bộ KVC trong ngày đó
  4. Toast xác nhận đã chạy job, danh sách tự tải lại
- **Kết quả:** Job chạy lại; nếu API nguồn đã hoạt động trở lại, lỗi sẽ biến mất khỏi danh sách "Cần xử lý"

### UC-LDB-04 – Nhập tay dữ liệu Giá vốn vé bán

- **Điều kiện:** Dòng lỗi có `source = TicketCost` và `status = Failed`, người dùng có quyền Admin/Accountant
- **Luồng chính:**
  1. Nhấn **Nhập tay** ở dòng lỗi tương ứng
  2. Modal mở với ngày dữ liệu và KVC được điền sẵn theo dòng lỗi
  3. Nhập **Tổng giá vốn**, **Tổng tiền bán**, **Tổng số lượng vé**, và **Lý do nhập tay** (bắt buộc để truy vết)
  4. Nhấn **Lưu nhập tay**
- **Kết quả:** Bản ghi giá vốn được ghi nhận thủ công cho đúng ngày/KVC; dòng lỗi chuyển sang `ManualResolved`

### UC-LDB-05 – Nhập tay dữ liệu Giao dịch ngân hàng

- **Điều kiện:** Dòng lỗi có `source = BankTransaction` và `status = Failed`
- **Luồng chính:**
  1. Nhấn **Nhập tay**
  2. Chọn **Loại giao dịch**: Nạp tiền / Thanh toán công nợ / Hoàn tiền / Khác
  3. Nhập **Tổng tiền vào**, **Tổng tiền ra**, **Số giao dịch**, **Lý do nhập tay**
  4. Lưu
- **Kết quả:** Bản ghi giao dịch ngân hàng được ghi nhận thủ công; dòng lỗi chuyển sang `ManualResolved`

### UC-LDB-06 – Gửi email tổng hợp lỗi

- **Điều kiện:** Người dùng có quyền Admin/Accountant
- **Luồng chính:**
  1. Chọn ngày dữ liệu (mặc định hôm nay nếu bỏ trống)
  2. Nhấn **Gửi email lỗi**
  3. Hệ thống gửi email tổng hợp lỗi đồng bộ trong ngày đến các địa chỉ cấu hình tại Cài đặt → Email nhận lỗi
- **Kết quả:** Toast hiển thị thông báo kết quả gửi email

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Ngày | Business date của lượt chạy job |
| KVC | Mã + tên KVC |
| Nguồn | Số dư KVC / Giá vốn vé bán / Giao dịch ngân hàng |
| Trạng thái | Badge theo `jobRunItemStatusLabel` (Cần xử lý / Đã nhập tay / …) |
| Số lần gọi | `attemptCount` — số lần hệ thống đã retry |
| Thời điểm lỗi | `finishedAtUtc` |
| Lỗi | `errorMessage` hoặc `errorCode` |
| Hành động | Nút "Nhập tay" — chỉ hiện khi `status = Failed`, có quyền, và `source` khác `ParkBalance` |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Nhật ký thay đổi | Thao tác Chạy job / Nhập tay sinh bản ghi `RunJob` / `ManualEntry` |
| Cấu hình hệ thống → Email nhận lỗi | Danh sách người nhận email tổng hợp lỗi |
| Cấu hình hệ thống → Log gọi API | Chi tiết request/response của các lượt gọi API (kể cả lỗi) |
| Số dư khu vui chơi hằng ngày, Chi tiết giá vốn vé bán, Giao dịch ngân hàng | Nơi hiển thị dữ liệu sau khi job chạy thành công hoặc được nhập tay |

---

## Ghi chú thiết kế

- **Số dư KVC không hỗ trợ nhập tay** vì số dư phải lấy trực tiếp từ API đối tác để đảm bảo chính xác tuyệt đối
- Trường **Lý do nhập tay** nên được nhập rõ ràng vì sẽ hiển thị lại ở các màn tra cứu (badge nguồn "Nhập tay")
