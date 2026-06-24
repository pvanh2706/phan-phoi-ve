# Use Case: Quy trình nạp tiền KVC

**Màn hình:** `nap-tien-kvc.html` → Tab **Quy trình nạp tiền KVC**
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Danh sách nạp tiền KVC theo ngày → Tab "Quy trình nạp tiền KVC"
**Quyền truy cập:** Admin (toàn quyền), Member (theo phân quyền từng cột)

---

## Mô tả

Màn hình quản lý quy trình nạp tiền vào tài khoản khu vui chơi theo dạng **Kanban board**. Mỗi nhiệm vụ nạp tiền hoặc thanh toán công nợ được **tạo thủ công** bởi Kế toán / NVKD và **di chuyển thủ công** qua các bước xử lý cho đến khi hoàn thành hoặc thất bại.

Đây là công cụ phối hợp nội bộ giữa 3 vai trò: **Kế toán / NVKD**, **Trưởng bộ phận**, và **Kế toán kiểm tra**.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Tạo, xem, di chuyển nhiệm vụ ở mọi cột; cài đặt cột |
| Kế toán / NVKD (Member) | Tạo nhiệm vụ; xử lý ở cột 1 |
| Trưởng bộ phận (Member) | Duyệt nhiệm vụ ở cột 2 |
| Kế toán kiểm tra (Member) | Chuyển khoản, xác nhận ở cột 3 |

---

## Cấu trúc Kanban — 5 cột

| # | Tên cột | Màu | Người phụ trách | Mục đích |
|---|---------|-----|-----------------|---------|
| 1 | Kế toán / NVKD lập phiếu | Xám | AT, NH, TL | Khởi tạo phiếu nạp tiền |
| 2 | Trưởng bộ phận duyệt | Xanh dương | TM, BH | Kiểm tra và phê duyệt lệnh nạp |
| 3 | Kế toán kiểm tra & chuyển khoản | Tím | AT, KT | Xác minh thông tin, thực hiện CK |
| 4 | Hoàn thành | Xanh lá | NH, AT | Giao dịch đã được xác nhận thành công |
| 5 | Thất bại | Đỏ | BH | Giao dịch bị từ chối hoặc sai thông tin |

### Thống kê header mỗi cột

Mỗi cột hiển thị:
- **NV hoàn thành / tổng NV** (ví dụ: `987/1489 NV`)
- **Số nhiệm vụ quá hạn** (màu đỏ nếu > 0)
- **Thời gian xử lý trung bình** (`⏱ 2.00h`)
- **Số công việc đang có** (`📋 2 CV`)

---

## Use Cases

### UC-QT-01 – Tạo nhiệm vụ mới

- **Điều kiện:** Người dùng đang ở tab Quy trình nạp tiền KVC
- **Luồng chính:**
  1. Người dùng nhấn nút **+ Tạo nhiệm vụ** (góc trên phải kanban)
  2. Modal "Tạo nhiệm vụ mới" mở ra
  3. Người dùng điền form:
     - **Tên nhiệm vụ** *(bắt buộc)* — VD: "Nạp tiền tháng 6 - Vin Nha Trang"
     - **Loại nhiệm vụ**: Nạp tiền / Thanh toán Công nợ
     - **Tên KVC** *(bắt buộc)* — chọn từ dropdown có tìm kiếm, phân theo nhóm Nạp tiền / Công nợ
     - **Số tài khoản**, **Ngân hàng**
     - **Số tiền** *(bắt buộc)*, **Ngày thực hiện**
     - **Ghi chú** *(tuỳ chọn)*
  4. Nhấn **Tạo nhiệm vụ**
  5. Card mới xuất hiện ở cuối cột 1 (Kế toán / NVKD lập phiếu)
- **Kết quả:** Card mới có đầy đủ thông tin, hỗ trợ drag-drop và click xem chi tiết
- **Ghi chú:** Chọn KVC tự động đề xuất tên nhiệm vụ theo format `Loại - Tên KVC` nếu ô tên đang trống

### UC-QT-02 – Di chuyển nhiệm vụ giữa các bước

- **Điều kiện:** Có ít nhất 1 card trong kanban
- **Luồng chính:**
  1. Người dùng giữ và kéo card từ cột nguồn
  2. Card chuyển sang trạng thái bán trong suốt, hiển thị placeholder điểm thả
  3. Người dùng thả vào cột đích
  4. Card dịch chuyển, thống kê cột cập nhật lại
- **Kết quả:** Card nằm ở vị trí mới, phản ánh bước xử lý hiện tại của nhiệm vụ
- **Ghi chú:** Phân quyền ai được di chuyển ở từng cột cấu hình trong Cài đặt cột

### UC-QT-03 – Xem chi tiết nhiệm vụ

- **Điều kiện:** Người dùng click vào card (không đang kéo)
- **Luồng chính:**
  1. Màn hình chuyển toàn trang sang **màn hình chi tiết nhiệm vụ**
  2. Header hiển thị: tên nhiệm vụ, badge bước hiện tại (màu theo cột), ngày thực hiện
  3. Nội dung chia theo các section có thể thu gọn, tương ứng với từng bước đã qua:
     - Bước 1: KT/NVKD lập phiếu — các trường 01–06
     - Bước 2: Trưởng BP duyệt — hiển thị đầu vào từ bước 1 + các trường 07–16
     - Bước 3: KT kiểm tra & CK — hiển thị đầu vào từ bước 2 + các trường 17–19
     - Bước 4/5: Kết quả cuối (hoàn thành hoặc thất bại) — các trường 20–25
  4. Nhấn **← Quay lại** để trở về kanban
- **Kết quả:** Người dùng thấy toàn bộ thông tin và lịch sử xử lý của nhiệm vụ

### UC-QT-04 – Cài đặt cột

- **Điều kiện:** Người dùng có quyền Admin
- **Luồng chính:**
  1. Người dùng nhấn icon **⚙** ở góc trên phải của header cột
  2. Popover cài đặt mở ra ngay bên dưới icon, gồm 2 tab:
     - **Trường thông tin**: bật/tắt từng trường hiển thị trên card (7 trường)
     - **Phân quyền**: chọn ai có thể di chuyển nhiệm vụ ở bước này (6 người dùng)
  3. Thay đổi được áp dụng ngay
- **Kết quả:** Cột hiển thị đúng trường và chỉ cho phép đúng người chuyển card

---

## Cấu trúc Card

| Thành phần | Mô tả |
|-----------|-------|
| Tag loại | Badge teal "Nạp tiền" hoặc indigo "Thanh toán Công nợ" |
| Tên nhiệm vụ | Tiêu đề ngắn gọn, VD: "Nạp tiền - Vin Nha Trang" |
| Mô tả | Nội dung/ghi chú + `TK: ACCOUNT · Ngân hàng` |
| Số tiền | Hiển thị màu đỏ nếu thất bại |
| Ngày | 📅 nếu đang xử lý, ✅ nếu hoàn thành, ❌ nếu thất bại |

---

## Trường thông tin chi tiết nhiệm vụ

| Số | Câu hỏi / Trường | Bước |
|----|-----------------|------|
| 01 | Có ezCloud Key tạy tiền trên hệ thống? | 1 |
| 05 | Các file, bằng chứng | 1 |
| 07 | Đơn vị / người thu hưởng | 2 |
| 08 | Số tài khoản thụ hưởng | 2 |
| 09 | Ngân hàng thụ hưởng | 2 |
| 13 | Tổng số tiền (bằng số) | 2 |
| 14 | Tổng số tiền (bằng chữ) | 2 |
| 16 | Số phải trả | 2 |
| 17–19 | Kiểm tra & xác nhận chuyển khoản | 3 |
| 20–22 | Thông tin hoàn thành | 4 |
| 23–25 | Thông tin thất bại / lý do | 5 |

---

## Dữ liệu mẫu (hiển thị tĩnh trong UI)

| Card | Cột | Loại | Số tiền | Ngày |
|------|-----|------|---------|------|
| Nạp tiền - Vin Nha Trang | 1 | Nạp tiền | 50.000.000 ₫ | 24/06/2026 |
| Nạp tiền - Vin Phú Quốc | 2 | Nạp tiền | 100.000.000 ₫ | 28/04/2026 |
| Nạp tiền - Vin Nam Hội An | 2 | Nạp tiền | 50.000.000 ₫ | 28/04/2026 |
| Nạp tiền - Thủy Cung Lotte (Lần 1) | 3 | Nạp tiền | 365.625.000 ₫ | 29/04/2026 |
| Nạp tiền - Thủy Cung Lotte (Lần 2) | 3 | Nạp tiền | 237.375.000 ₫ | 29/04/2026 |
| Nạp tiền - Bản Mòng | 4 | Nạp tiền | 490.000.000 ₫ | ✅ 14/11/2025 |
| Nạp tiền - Sunworld | 4 | Nạp tiền | 490.000.000 ₫ | ✅ 28/04/2026 |
| Nạp tiền - Vin Cửa Hội | 4 | Nạp tiền | 50.000.000 ₫ | ✅ 28/04/2026 |
| Nạp tiền - Vin Vũ Yên (Lần 2) | 5 | Nạp tiền | 50.000.000 ₫ | ❌ 15/06/2026 |
| Thanh toán Công nợ - Sealinks | 5 | Công nợ | 8.110.000 ₫ | ❌ 16/09/2025 |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `nap-tien-kvc.html` Tab 2 | Lịch sử giao dịch nạp tiền thực tế từ API |
| `nap-tien-kvc.html` Tab 3 | Lịch sử thanh toán công nợ thực tế từ API |
| `so-du-kvc.html` | Số dư hiện tại của KVC — kiểm tra trước khi tạo lệnh nạp |
| `doi-soat-kvc.html` | Đối soát số liệu sau khi nạp tiền hoàn thành |

---

## Ghi chú thiết kế

- Mọi nhiệm vụ đều do con người tạo và chuyển bước — **không có tự động hoá**
- Cột 3 (Kế toán kiểm tra) thường có nhiều nhiệm vụ nhất và thời gian xử lý lâu nhất (⏱ 72h)
- Một KVC có thể có nhiều lần nạp trong tháng (VD: Thủy Cung Lotte Lần 1, Lần 2)
- Cột Thất bại giữ lại bản ghi để tra cứu nguyên nhân, không xoá khỏi hệ thống
