# Use Case: Đối soát KVC

**Màn hình:** `doi-soat-kvc.html`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Đối soát KVC
**Quyền truy cập:** Admin, Member (xem)

---

## Mô tả

Màn hình tổng hợp đối soát số dư tài khoản KVC theo từng ngày bằng công thức cộng/trừ. Dữ liệu được kết hợp từ **3 nguồn** khác nhau: số dư KVC (2 thời điểm T-1 và T), số tiền đã nạp/thanh toán, và số tiền đã dùng (giá vốn vé bán). Hệ thống tự tính toán và so sánh số dư lý thuyết với số dư thực tế, phát hiện lệch nếu có.

Đây là màn hình kiểm soát cuối cùng trong quy trình KVC — nếu có lệch, kế toán cần điều tra nguyên nhân và đánh dấu "Đã xử lý" sau khi xác nhận.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc, đánh dấu "Đã xử lý" |
| Member | Xem, lọc |

---

## Nguồn dữ liệu (3 màn hình)

| Nguồn | Màn hình | Dữ liệu lấy |
|-------|---------|------------|
| Số dư T-1 và Số dư T | `so-du-kvc.html` | Cột (1) và cột (5) |
| Số nạp thêm | `nap-tien-kvc.html` Tab Nạp tiền / Tab Thanh toán | Cột (2) |
| Số đã dùng | `chi-tiet-gia-von-ve-ban.html` | Cột Tiền vốn → cột (3) |

---

## Cấu trúc 2 Tab

| Tab | Nhóm KVC | Ghi chú |
|-----|---------|---------|
| KVC nạp tiền | Các KVC thanh toán trước | (2) = số tiền đã nạp thêm vào tài khoản KVC |
| KVC công nợ | Các KVC hạn mức tín dụng | (2) = số tiền đã thanh toán công nợ cho KVC |

---

## Use Cases

### UC-DS-01 – Xem bảng đối soát KVC nạp tiền

- **Điều kiện:** Người dùng đang ở màn hình Đối soát KVC (tab mặc định)
- **Luồng chính:**
  1. Hệ thống hiển thị bảng đối soát cho tab KVC nạp tiền
  2. Mỗi dòng là một KVC trong một ngày cụ thể
  3. Cột (4) và cột Lệch được tính toán tự động từ công thức
  4. Dòng có lệch được tô màu đỏ nhạt, dòng khớp giữ màu bình thường
- **Kết quả:** Người dùng thấy được tình trạng đối soát từng ngày từng KVC

### UC-DS-02 – Xem bảng đối soát KVC công nợ

- **Điều kiện:** Người dùng nhấn tab "KVC công nợ"
- **Luồng chính:**
  1. Hệ thống hiển thị bảng đối soát cho nhóm KVC công nợ
  2. Cột (2) là số tiền đã thanh toán công nợ trong ngày (thay vì nạp thêm)
  3. Công thức và logic hiển thị giống hệt tab KVC nạp tiền
- **Kết quả:** Người dùng thấy tình trạng đối soát của nhóm KVC tín dụng

### UC-DS-03 – Lọc theo ngày

- **Điều kiện:** Người dùng muốn xem đối soát trong một khoảng thời gian
- **Luồng chính:**
  1. Người dùng chọn **Từ ngày** và/hoặc **Đến ngày**
  2. Bảng lọc chỉ hiển thị các dòng thuộc khoảng ngày đó
- **Kết quả:** Danh sách thu hẹp theo kỳ cần đối soát

### UC-DS-04 – Lọc theo trạng thái lệch

- **Điều kiện:** Người dùng muốn chỉ xem các KVC có lệch
- **Luồng chính:**
  1. Người dùng chọn dropdown lọc: **Tất cả / Có lệch / Khớp**
  2. Bảng cập nhật ngay theo lựa chọn
- **Kết quả:**
  - "Có lệch": chỉ hiện dòng có `(5)-(4) ≠ 0`
  - "Khớp": chỉ hiện dòng có `(5)-(4) = 0`

### UC-DS-05 – Đánh dấu "Đã xử lý"

- **Điều kiện:** Dòng có lệch (`(5)-(4) ≠ 0`) — Admin đã xác nhận nguyên nhân
- **Luồng chính:**
  1. Admin đánh dấu checkbox **"Đã xử lý"** ở dòng có lệch
  2. Dòng đó chuyển sang trạng thái mờ dần (`opacity: 0.45`)
  3. Cột cảnh báo vẫn hiển thị giá trị lệch nhưng có thêm dấu hiệu "đã xử lý"
- **Điều kiện đặc biệt:** Checkbox chỉ **bật được** khi dòng đó có lệch; dòng khớp (`lệch = 0`) checkbox bị `disabled`
- **Kết quả:** Admin theo dõi được những lệch đã điều tra xong và chưa điều tra

### UC-DS-06 – Tìm kiếm theo tên KVC

- **Điều kiện:** Người dùng cần lọc nhanh theo tên KVC cụ thể
- **Luồng chính:**
  1. Người dùng nhập tên KVC vào ô tìm kiếm
  2. Bảng lọc real-time theo tên và mã KVC
- **Kết quả:** Chỉ hiện dòng thuộc KVC được tìm

---

## Cấu trúc bảng — 13 cột

| # | Tên cột | Nguồn dữ liệu | Ghi chú |
|---|---------|--------------|---------|
| 1 | Ngày T-1 | `so-du-kvc.html` | Ngày trước đó |
| 2 | Ngày T | `so-du-kvc.html` | Ngày hiện tại đang đối soát |
| 3 | TK Ngân hàng | `ma-kvc.html` | Số tài khoản KVC thụ hưởng |
| 4 | Mã KVC | `ma-kvc.html` | Mã định danh KVC |
| 5 | Tên KVC | `ma-kvc.html` | Tên đầy đủ KVC |
| 6 | **(1) Số dư T-1** | `so-du-kvc.html` | Số dư cuối ngày T-1, màu xanh dương |
| 7 | **(2) Số nạp thêm** | `nap-tien-kvc.html` | Tiền nạp thêm (hoặc TT công nợ) trong ngày T, màu xanh lá |
| 8 | **(3) Số đã dùng** | `chi-tiet-gia-von-ve-ban.html` | Tổng Tiền vốn vé đã bán trong ngày T, màu đỏ |
| 9 | **(4) = (1)+(2)-(3)** | Tính toán | Số dư lý thuyết cuối ngày T — tự động |
| 10 | **(5) Số dư T** | `so-du-kvc.html` | Số dư thực tế cuối ngày T, màu xanh dương |
| 11 | **(5)-(4) Lệch** | Tính toán | Chênh lệch thực tế vs lý thuyết |
| 12 | Cảnh báo | Hệ thống tạo | Hiển thị mô tả lệch |
| 13 | Đã xử lý | Người dùng | Checkbox — chỉ bật được khi có lệch |

---

## Công thức tính toán

```
(4) Số dư lý thuyết  = (1) Số dư T-1  +  (2) Số nạp thêm  −  (3) Số đã dùng
(5)−(4) Lệch         = (5) Số dư T  −  (4) Số dư lý thuyết
```

| Kết quả Lệch | Ý nghĩa | Xử lý |
|-------------|---------|-------|
| = 0 | Khớp — số dư thực tế đúng theo lý thuyết | Không cần làm gì |
| > 0 (Dư) | Số dư thực tế cao hơn lý thuyết | Cần điều tra — có thể tiền nạp chưa được ghi nhận |
| < 0 (Thiếu) | Số dư thực tế thấp hơn lý thuyết | Cần điều tra — có thể có vé bán chưa được tính |

---

## Màu sắc hiển thị

| Trường | Màu | Ý nghĩa |
|--------|-----|---------|
| (1) Số dư T-1 | Xanh dương | Số dư đầu kỳ |
| (2) Số nạp thêm | Xanh lá | Tiền vào |
| (3) Số đã dùng | Đỏ | Tiền ra |
| (4) Lý thuyết | Xám/Mono | Kết quả tính toán |
| (5) Số dư T | Xanh dương | Số dư thực tế cuối kỳ |
| Lệch = 0 | Xám | Khớp |
| Lệch > 0 | Xanh lá | Dư |
| Lệch < 0 | Đỏ | Thiếu |
| Dòng "Đã xử lý" | opacity: 0.45 | Đã được xác nhận |

---

## Cột Cảnh báo (Hệ thống tự tạo)

| Trường hợp | Hiển thị |
|-----------|---------|
| Lệch = 0 | `—` (xám) |
| Lệch > 0 | `⚠ Dư X ₫` (amber/vàng) |
| Lệch < 0 | `⚠ Thiếu X ₫` (đỏ) |

---

## Dữ liệu mẫu — Tab KVC nạp tiền

| Ngày T-1 | Ngày T | KVC | (1) Số dư T-1 | (2) Nạp thêm | (3) Đã dùng | (4) Lý thuyết | (5) Số dư T | Lệch | Cảnh báo |
|---------|--------|-----|--------------|------------|------------|--------------|------------|------|---------|
| 23/06/2026 | 24/06/2026 | Bản Mòng | 75.000.000 | 0 | 3.150.000 | 71.850.000 | 71.850.000 | 0 | — |
| 23/06/2026 | 24/06/2026 | Sun Group | 120.500.000 | 490.000.000 | 25.440.000 | 585.060.000 | 585.060.000 | 0 | — |
| 23/06/2026 | 24/06/2026 | Đồi Rồng | 8.200.000 | 0 | 9.450.000 | −1.250.000 | 0 | +1.250.000 | ⚠ Dư 1.250.000 ₫ |
| 23/06/2026 | 24/06/2026 | Delight | 54.000.000 | 0 | 4.725.000 | 49.275.000 | 48.900.000 | −375.000 | ⚠ Thiếu 375.000 ₫ |
| 23/06/2026 | 24/06/2026 | Samten Hills | 44.800.000 | 0 | 2.900.000 | 41.900.000 | 41.900.000 | 0 | — |

## Dữ liệu mẫu — Tab KVC công nợ

| Ngày T-1 | Ngày T | KVC | (1) Số dư T-1 | (2) Thanh toán | (3) Đã dùng | (4) Lý thuyết | (5) Số dư T | Lệch | Cảnh báo |
|---------|--------|-----|--------------|--------------|------------|--------------|------------|------|---------|
| 23/06/2026 | 24/06/2026 | Sơn Tiên | 89.500.000 | 42.495.000 | 8.300.000 | 123.695.000 | 123.695.000 | 0 | — |
| 23/06/2026 | 24/06/2026 | Mikazuki | 67.000.000 | 35.953.000 | 11.700.000 | 91.253.000 | 91.253.000 | 0 | — |
| 23/06/2026 | 24/06/2026 | Mekong | 45.000.000 | 49.833.500 | 9.300.000 | 85.533.500 | 84.000.000 | −1.533.500 | ⚠ Thiếu 1.533.500 ₫ |
| 23/06/2026 | 24/06/2026 | Hồ Tràm | 98.000.000 | 22.850.000 | 14.600.000 | 106.250.000 | 106.250.000 | 0 | — |
| 23/06/2026 | 24/06/2026 | Nova Phan Thiết | 55.000.000 | 119.995.000 | 17.600.000 | 157.395.000 | 157.395.000 | 0 | — |

---

## Cơ chế cập nhật dữ liệu

```
Hàng ngày:
  00:00  ─── so-du-kvc API ────── Cột (1) T-1 và cột (5) T
  23:59  ─── gia-von-ve-ban API ─ Cột (3) Tiền vốn đã dùng
  Liên tục ─ nap-tien-kvc API ── Cột (2) Tiền nạp thêm / TT công nợ

  → Hệ thống tự tổng hợp 3 nguồn và tính (4), (5)-(4)
```

- Đối soát có thể đầy đủ sớm nhất sau **23h59** khi dữ liệu giá vốn được cập nhật
- Trước 23h59, cột (3) có thể chưa có dữ liệu → cột (4) và Lệch chưa chính xác

---

## Kết nối với màn hình khác

| Màn hình | Vai trò trong đối soát |
|---------|----------------------|
| `so-du-kvc.html` | Cung cấp (1) Số dư T-1 và (5) Số dư T |
| `nap-tien-kvc.html` | Cung cấp (2) Số tiền đã nạp/thanh toán công nợ |
| `chi-tiet-gia-von-ve-ban.html` | Cung cấp (3) Tổng tiền vốn vé đã bán = số đã dùng |
| `ma-kvc.html` | Tra cứu Mã KVC, Tên KVC, TK ngân hàng |
| `danh-sach-kvc.html` | Tổng quan — số dư và hạn mức hiện tại mỗi KVC |

---

## Ghi chú thiết kế

- **Lệch dương (Dư)** thường xảy ra khi tiền nạp chưa cập nhật vào hệ thống KVC, hoặc vé bán bị huỷ sau khi đã tính giá vốn
- **Lệch âm (Thiếu)** có thể do vé bán được ghi nhận trễ trong `chi-tiet-gia-von-ve-ban` hoặc có giao dịch ngoài hệ thống
- Checkbox **"Đã xử lý"** chỉ là đánh dấu theo dõi nội bộ — không tự động sửa dữ liệu nguồn
- Đối soát theo ngày nên được thực hiện vào **sáng hôm sau** khi tất cả API đã chạy đủ
- **Đồi Rồng lệch dương thường xuyên**: số dư KVC có thể xuống âm khi vé bán > số dư còn lại — hệ thống KVC cho phép âm nhưng phải nạp tiền sớm
