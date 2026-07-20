# Use Case: Đối soát TA - AR

**Màn hình:** `AgencyDiffView.vue` (direction: `ta-ar`)
**Đường dẫn truy cập:** Sidebar → Đại lý → Đối soát TA - AR (`/dai-ly/doi-soat-ta-ar`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — tính toán trực tiếp trên 2 bộ dữ liệu tĩnh `reportPages.agencyTaTransactions` và `reportPages.agencyArTransactions`, không gọi API.

---

## Mô tả

Chiều ngược lại của "Đối soát AR - TA": liệt kê những booking **có trên hệ thống TA nhưng không tìm thấy trên AR** — dấu hiệu của booking đã tạo trên hệ thống bán vé nhưng chưa (hoặc không) bị trừ tiền đại lý tương ứng.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-DSTA-01 – Xem danh sách booking lệch TA → AR

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:**
  1. Hệ thống lấy toàn bộ mã booking từ tab đầu tiên của "Giao dịch TA" và "Giao dịch AR"
  2. Lọc ra các dòng TA có mã booking **không tồn tại** trong tập AR
  3. Mỗi dòng gắn badge **"Không có trên AR"** (màu amber)
- **Kết quả:** Danh sách booking cần kiểm tra vì có trên TA nhưng chưa bị trừ tiền AR; subtitle hiển thị tổng số dòng lệch

### UC-DSTA-02 – Tìm kiếm trong kết quả đối soát

- **Điều kiện:** Người dùng đang xem kết quả
- **Luồng chính:** Nhập mã booking hoặc tên đại lý vào ô tìm kiếm
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DSTA-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày giao dịch TA

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Mã booking | Mã định danh từ hệ thống TA |
| Tên đại lý | Tên đại lý |
| Ngày giờ giao dịch | Thời điểm tạo trên TA |
| Số tiền | Số tiền giao dịch trên TA |
| Trạng thái | Badge amber "Không có trên AR" |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên TA | Nguồn dữ liệu gốc (base) của phép so khớp |
| Giao dịch của các đại lý trên AR | Nguồn dữ liệu đối chiếu |
| Đối soát AR - TA | Chiều ngược lại — liệt kê booking có trên AR nhưng thiếu trên TA |

---

## Ghi chú thiết kế

- Cùng cơ chế và hạn chế như "Đối soát AR - TA": so khớp chỉ theo mã booking, chạy trên frontend với dữ liệu mock tĩnh
- Về nghiệp vụ, "TA có mà AR không có" thường ít nghiêm trọng hơn chiều ngược lại (có thể do độ trễ đồng bộ), nhưng vẫn cần theo dõi nếu tồn tại lâu ngày không tự khớp
