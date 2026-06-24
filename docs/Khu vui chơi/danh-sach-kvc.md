# Use Case: Danh sách khu vui chơi

**Màn hình:** `danh-sach-kvc.html`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Danh sách khu vui chơi
**Quyền truy cập:** Admin, Member (xem)

---

## Mô tả

Màn hình tổng hợp danh sách tất cả khu vui chơi (KVC) đang hợp tác với ezCloud, phân theo 2 nhóm: **Nạp tiền** (trả trước) và **Công nợ** (tín dụng). Dữ liệu được lấy từ màn hình **Số dư KVC hàng ngày** — vì ngày nào cũng chỉ có đúng từng đó KVC, mỗi KVC chỉ xuất hiện **một lần duy nhất** trong danh sách (không lặp theo ngày).

Màn hình phục vụ mục đích theo dõi tổng quan số dư và công nợ hiện tại của từng KVC.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc, tìm kiếm |
| Member | Xem, lọc, tìm kiếm |

---

## Nguồn dữ liệu

- **Lấy từ:** Màn hình `so-du-kvc.html` (Số dư KVC hàng ngày)
- **Cơ chế:** Deduplicate theo Mã KVC — mỗi KVC chỉ lấy bản ghi mới nhất, không hiển thị nhiều dòng theo ngày
- **Cập nhật:** Tự động khi dữ liệu số dư được đồng bộ từ API hàng ngày lúc 00:00

---

## Cấu trúc 2 Tab

| Tab | Nhóm KVC | Số lượng mẫu |
|-----|---------|-------------|
| Số dư KVC nạp tiền | KVC thanh toán trước (nạp tiền vào tài khoản hệ thống) | 5 KVC |
| Số dư KVC công nợ | KVC hạn mức tín dụng (bán trước, thanh toán định kỳ) | 11 KVC |

---

## Use Cases

### UC-DS-01 – Xem danh sách KVC nạp tiền

- **Điều kiện:** Người dùng đang ở màn hình Danh sách khu vui chơi
- **Luồng chính:**
  1. Tab "Số dư KVC nạp tiền" hiển thị mặc định
  2. Mỗi dòng là một KVC thuộc nhóm nạp trước, với số dư hiện tại và lịch sử nạp
  3. Trạng thái được phân biệt bằng badge màu
- **Kết quả:** Người dùng thấy tổng quan số dư tất cả KVC nạp tiền

### UC-DS-02 – Xem danh sách KVC công nợ

- **Điều kiện:** Người dùng đang ở màn hình Danh sách khu vui chơi
- **Luồng chính:**
  1. Người dùng nhấn tab **"Số dư KVC công nợ"**
  2. Danh sách hiển thị công nợ hiện tại, hạn mức và ngày đến hạn của từng KVC
  3. KVC gần đến hạn hoặc vượt hạn mức hiển thị badge cảnh báo "Gần hạn"
- **Kết quả:** Người dùng thấy tổng quan tình trạng công nợ tất cả KVC tín dụng

### UC-DS-03 – Tìm kiếm KVC

- **Điều kiện:** Người dùng đang ở tab bất kỳ
- **Luồng chính:**
  1. Người dùng nhập tên KVC vào ô **"Tìm khu vui chơi..."**
  2. Danh sách lọc real-time theo tên KVC (không phân biệt hoa/thường)
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-DS-04 – Lọc theo trạng thái

- **Điều kiện:** Người dùng muốn xem KVC theo trạng thái cụ thể
- **Luồng chính:**
  1. Người dùng chọn dropdown: **Tất cả trạng thái / Hoạt động / Tạm dừng**
  2. Danh sách lọc ngay lập tức
- **Kết quả:** Danh sách chỉ hiển thị KVC theo trạng thái đã chọn

---

## Cấu trúc bảng — Tab Số dư KVC nạp tiền

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| # | Số thứ tự | |
| Tên khu vui chơi | Tên đầy đủ | In đậm |
| Mã KVC | Mã định danh nội bộ | VD: `KVC-001` |
| Địa điểm | Tỉnh/thành phố | |
| Số dư hiện tại | Số dư tài khoản KVC tại thời điểm mới nhất | Màu xanh lá nếu dương, đỏ nếu âm |
| Tổng đã nạp | Tổng tiền ezCloud đã nạp vào tài khoản KVC | |
| Nạp gần nhất | Ngày thực hiện nạp tiền gần nhất | Định dạng `DD/MM/YYYY` |
| Trạng thái | Hoạt động / Tạm dừng / Sắp hết | Badge màu |

## Cấu trúc bảng — Tab Số dư KVC công nợ

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| # | Số thứ tự | |
| Tên khu vui chơi | Tên đầy đủ | |
| Mã KVC | Mã định danh nội bộ | |
| Địa điểm | Tỉnh/thành phố | |
| Công nợ hiện tại | Số tiền đã dùng chưa thanh toán | |
| Hạn mức công nợ | Hạn mức tín dụng tối đa | |
| Ngày đến hạn | Ngày thanh toán công nợ kỳ này | |
| Trạng thái | Hoạt động / Gần hạn / Quá hạn | Badge màu cảnh báo |

---

## Trạng thái và màu sắc

| Trạng thái | Màu badge | Điều kiện |
|-----------|-----------|----------|
| Hoạt động | Xanh lá | Bình thường |
| Sắp hết | Vàng/Amber | Số dư thấp (KVC nạp tiền) |
| Gần hạn | Vàng/Amber | Sắp đến ngày đến hạn (KVC công nợ) |
| Tạm dừng | Xám | KVC không còn hợp tác |
| Quá hạn | Đỏ | Đã qua ngày đến hạn mà chưa thanh toán |

---

## Dữ liệu mẫu — Tab KVC nạp tiền

| # | Tên KVC | Mã KVC | Địa điểm | Số dư | Tổng đã nạp | Nạp gần nhất | Trạng thái |
|---|---------|--------|---------|-------|------------|-------------|-----------|
| 1 | Bản Mòng | KVC-001 | Sơn La | 75.000.000 ₫ | 500.000.000 ₫ | 20/06/2026 | Hoạt động |
| 2 | Sun Group | KVC-002 | Đà Nẵng | 120.500.000 ₫ | 800.000.000 ₫ | 18/06/2026 | Hoạt động |
| 3 | Đồi Rồng | KVC-003 | Hải Phòng | 8.200.000 ₫ | 350.000.000 ₫ | 10/06/2026 | Sắp hết |
| 4 | Delight | KVC-004 | Hà Nội | 54.000.000 ₫ | 200.000.000 ₫ | 25/06/2026 | Hoạt động |
| 5 | Samten Hills Dalat | KVC-005 | Đà Lạt, Lâm Đồng | 44.800.000 ₫ | 420.000.000 ₫ | 22/06/2026 | Hoạt động |

## Dữ liệu mẫu — Tab KVC công nợ

| # | Tên KVC | Mã KVC | Công nợ | Hạn mức | Ngày đến hạn | Trạng thái |
|---|---------|--------|---------|---------|-------------|-----------|
| 1 | TLTY | KVC-001 | 125.000.000 ₫ | 200.000.000 ₫ | 30/06/2026 | Hoạt động |
| 2 | Sơn Tiên | KVC-002 | 89.500.000 ₫ | 150.000.000 ₫ | 15/07/2026 | Hoạt động |
| 3 | Lumiere | KVC-003 | 210.000.000 ₫ | 250.000.000 ₫ | 05/07/2026 | Gần hạn |
| 4 | Mikazuki | KVC-004 | 67.000.000 ₫ | 100.000.000 ₫ | 20/07/2026 | Hoạt động |
| 5 | Mekong | KVC-005 | 45.000.000 ₫ | 120.000.000 ₫ | 10/07/2026 | Hoạt động |
| 6 | Tà Cú | KVC-006 | 32.500.000 ₫ | 80.000.000 ₫ | 25/07/2026 | Hoạt động |
| 7 | Hồ Tràm | KVC-007 | 98.000.000 ₫ | 150.000.000 ₫ | 01/07/2026 | Gần hạn |
| 8 | Nova Phan Thiết | KVC-008 | 55.000.000 ₫ | 100.000.000 ₫ | 18/07/2026 | Hoạt động |
| 9 | Công viên nước Hồ Tây | KVC-009 | 41.000.000 ₫ | 90.000.000 ₫ | 12/07/2026 | Hoạt động |
| 10 | Sealinks | KVC-010 | 73.000.000 ₫ | 130.000.000 ₫ | 08/07/2026 | Hoạt động |
| 11 | Sightseeing HN | KVC-011 | 28.000.000 ₫ | 60.000.000 ₫ | 22/07/2026 | Hoạt động |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `so-du-kvc.html` | Nguồn dữ liệu — lịch sử số dư hàng ngày theo từng KVC |
| `ma-kvc.html` | Quản lý mã định danh, loại, và tài khoản ngân hàng của từng KVC |
| `nap-tien-kvc.html` | Lịch sử và quy trình nạp tiền — tác động đến Số dư hiện tại |
| `doi-soat-kvc.html` | Đối soát số dư T-1 và T theo từng KVC |

---

## Ghi chú thiết kế

- Danh sách KVC **không tăng theo thời gian** — chỉ thêm mới khi có KVC hợp tác mới, nên hiển thị 1 dòng/KVC là đủ
- KVC "Sắp hết" (tab nạp tiền) cần được ưu tiên nạp tiền sớm để tránh gián đoạn bán vé
- KVC "Gần hạn" (tab công nợ) cần kế toán chuẩn bị thanh toán trước ngày đến hạn
