# Use Case: Giao dịch của các đại lý trên AR

**Màn hình:** `AgencyReportView.vue` (pageKey: `agencyArTransactions`)
**Đường dẫn truy cập:** Sidebar → Đại lý → Giao dịch của các đại lý trên AR (`/dai-ly/giao-dich-ar`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh khai báo trong `data/reports.ts` (`reportPages.agencyArTransactions`). Có nút **"Gọi API test"** mô phỏng gọi API thật (chỉ đợi 800ms rồi báo giả lập số dòng lấy được), chưa nối API thật.

---

## Mô tả

Danh sách giao dịch **trừ tiền của các đại lý** ghi nhận trên hệ thống AR (ar.ezcloud.vn) — hệ thống nội bộ ezCloud dùng để trừ tiền đại lý khi có booking phát sinh. Mỗi dòng gồm mã booking, tên đại lý, thời điểm giao dịch, và số tiền bị trừ.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, gọi API test |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-GDAR-01 – Xem danh sách giao dịch AR

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị 8 dòng/trang (mặc định), mới nhất theo dữ liệu mẫu
- **Kết quả:** Danh sách giao dịch trừ tiền AR của các đại lý

### UC-GDAR-02 – Tìm kiếm theo mã booking / tên đại lý

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm mã booking, tên đại lý..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-GDAR-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng muốn xem giao dịch trong khoảng thời gian cụ thể
- **Luồng chính:** Chọn "Từ ngày" và/hoặc "Đến ngày" → danh sách lọc theo `date` của từng dòng
- **Kết quả:** Chỉ hiển thị giao dịch trong khoảng ngày đã chọn

### UC-GDAR-04 – Gọi API test

- **Điều kiện:** Người dùng muốn mô phỏng việc lấy dữ liệu mới từ API AR
- **Luồng chính:** Nhấn **"Gọi API test"** → nút chuyển "Đang gọi..." trong ~800ms → toast báo số dòng lấy được (= số dòng hiện có trong tab)
- **Kết quả:** Chỉ là hành vi giả lập minh hoạ UI, **không thực sự gọi API và không thay đổi dữ liệu**

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh giao dịch trên AR |
| Tên đại lý | Tên đại lý bị trừ tiền |
| Ngày giờ giao dịch | Định dạng `DD/MM/YYYY HH:mm:ss` |
| Trừ tiền trên AR | Số tiền, tô đỏ (amount) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên TA | Cùng khái niệm mã booking — dùng để đối soát chéo AR vs TA |
| Đối soát AR - TA / Đối soát TA - AR | So khớp (VLOOKUP theo mã booking) giữa dữ liệu màn này và màn TA để tìm booking thiếu 1 bên |

---

## Ghi chú thiết kế

- Khi triển khai backend thật, màn này cần API đồng bộ giao dịch AR theo lịch (tương tự cách `Số dư khu vui chơi hằng ngày` đã tích hợp API `parksApi`/`summariesApi`)
- Nút "Gọi API test" hiện tại chỉ nhằm minh hoạ giao diện, cần thay bằng lời gọi job đồng bộ thật khi có API AR
