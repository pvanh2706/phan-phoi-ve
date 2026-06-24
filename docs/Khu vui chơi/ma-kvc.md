# Use Case: Mã khu vui chơi

**Màn hình:** `ma-kvc.html`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Mã khu vui chơi
**Quyền truy cập:** Admin (toàn quyền), Member (xem)

---

## Mô tả

Màn hình quản lý danh mục mã định danh của các khu vui chơi trong hệ thống ezCloud. Gồm 3 tab: **Mã KVC** (danh sách KVC gốc), **Danh mục KVC con nạp trước** và **Danh mục KVC con công nợ**. Dữ liệu mặc định được khai báo tĩnh trong hệ thống — Admin có quyền **Thêm / Sửa / Xoá** các mục.

Mã KVC là mã định danh dùng để gọi API lấy số dư, giá vốn và đối soát, nên việc duy trì danh sách chính xác là rất quan trọng.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, Thêm, Sửa, Xoá |
| Member | Chỉ xem |

---

## Cấu trúc 3 Tab

| Tab | Nội dung | Hành động |
|-----|---------|----------|
| Mã KVC | Danh sách KVC gốc (cha) | Thêm / Sửa / Xoá |
| Danh mục KVC con nạp trước | Loại vé của KVC nhóm nạp tiền | Thêm / Sửa / Xoá |
| Danh mục KVC con công nợ | Loại vé của KVC nhóm công nợ | Thêm / Sửa / Xoá |

---

## Use Cases

### UC-MA-01 – Xem danh sách Mã KVC

- **Điều kiện:** Người dùng đang ở tab "Mã KVC" (mặc định)
- **Luồng chính:**
  1. Hệ thống hiển thị toàn bộ danh sách KVC gốc (cha)
  2. Phân biệt nhóm Nạp trước / Công nợ qua cột Loại
  3. Có thể lọc và tìm kiếm
- **Kết quả:** Người dùng thấy tất cả KVC đang có trong hệ thống

### UC-MA-02 – Tìm kiếm và lọc Mã KVC

- **Điều kiện:** Người dùng đang ở tab bất kỳ
- **Luồng chính:**
  1. Nhập từ khoá vào ô tìm kiếm (lọc theo mã hoặc tên KVC)
  2. Chọn **Loại**: Tất cả / Nạp trước / Công nợ
  3. Chọn **Trạng thái**: Tất cả / Đang hoạt động / Ngừng hoạt động
  4. Các bộ lọc kết hợp với nhau, lọc real-time
- **Kết quả:** Danh sách thu hẹp theo tiêu chí đã chọn

### UC-MA-03 – Thêm KVC mới

- **Điều kiện:** Admin đang ở tab Mã KVC
- **Luồng chính:**
  1. Admin nhấn **+ Thêm KVC**
  2. Modal "Thêm KVC mới" mở ra với các trường:
     - **Mã KVC** *(bắt buộc)* — mã số định danh trong hệ thống API
     - **Loại** — Nạp trước / Công nợ
     - **Tên khu vui chơi** *(bắt buộc)* — tên đầy đủ
     - **Mã định danh tìm kiếm** — mã dùng để tra cứu trong API
     - **Mã ngân hàng / TK** — số tài khoản ngân hàng nhận tiền
     - **Trạng thái** — Hoạt động / Ngừng hoạt động
  3. Admin nhấn **Lưu**
- **Kết quả:** KVC mới xuất hiện trong danh sách

### UC-MA-04 – Sửa thông tin KVC

- **Điều kiện:** Admin đang xem danh sách
- **Luồng chính:**
  1. Admin nhấn icon **✏️ Sửa** ở dòng cần chỉnh
  2. Modal chỉnh sửa mở ra, điền sẵn thông tin hiện tại
  3. Mã KVC ở chế độ **readonly** khi sửa (không đổi được mã)
  4. Admin chỉnh sửa các trường còn lại và nhấn **Lưu**
- **Kết quả:** Thông tin KVC được cập nhật ngay trong bảng

### UC-MA-05 – Xoá KVC

- **Điều kiện:** Admin đang xem danh sách
- **Luồng chính:**
  1. Admin nhấn icon **🗑️ Xoá** ở dòng cần xoá
  2. Modal xác nhận hiển thị: *"Bạn có chắc muốn xoá mục này không? Hành động này không thể hoàn tác."* kèm tên KVC
  3. Admin nhấn **Xoá** (nút đỏ) để xác nhận
- **Kết quả:** Dòng bị xoá khỏi bảng
- **Lưu ý:** Nên kiểm tra KVC đó còn được dùng trong màn hình khác không trước khi xoá

### UC-MA-06 – Quản lý KVC con nạp trước

- **Điều kiện:** Người dùng chọn tab "Danh mục KVC con nạp trước"
- **Luồng chính:**
  1. Danh sách hiển thị các loại vé (KVC con) thuộc KVC nạp trước
  2. Mỗi KVC con liên kết với một KVC cha qua Mã KVC cha
  3. Admin có thể thêm, sửa, xoá KVC con
- **Kết quả:** Người dùng quản lý được chi tiết từng loại vé của các KVC nạp trước
- **Ghi chú:** Đơn giá vốn ở KVC con là cơ sở tính Tiền vốn trong Chi tiết giá vốn vé bán

### UC-MA-07 – Quản lý KVC con công nợ

- **Điều kiện:** Người dùng chọn tab "Danh mục KVC con công nợ"
- **Luồng chính:** *(Giống UC-MA-06 nhưng cho nhóm công nợ)*
- **Kết quả:** Người dùng quản lý được chi tiết từng loại vé của các KVC công nợ

---

## Cấu trúc bảng — Tab Mã KVC

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| Mã KVC | Mã số định danh KVC trong hệ thống API | Mono, VD: `11681` |
| Tên khu vui chơi | Tên đầy đủ | |
| Loại | Nạp trước / Công nợ | Badge màu |
| Mã định danh tìm kiếm | Mã dùng để gọi API (thường = Mã KVC) | Mono |
| Mã ngân hàng / TK | Số tài khoản ngân hàng thụ hưởng | Mono |
| Trạng thái | Hoạt động / Ngừng hoạt động | Badge màu |
| Hành động | Sửa / Xoá | Chỉ Admin |

## Cấu trúc bảng — Tab KVC con (cả 2 tab)

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| Mã KVC con | Mã định danh loại vé con | Mono, VD: `347015` |
| Tên KVC con | Tên mô tả loại vé | VD: "SJC - Người lớn T7-CN" |
| Mã KVC cha | Mã KVC gốc | Mono |
| Tên KVC cha | Tên KVC gốc | |
| Mã loại vé | Mã định danh loại vé trong API | Mono |
| Nhóm loại vé | Tên nhóm | VD: "SJC- Thủy cung Timescity" |
| Đơn giá vốn | Giá vốn mỗi vé (₫) | |
| Trạng thái | Hoạt động / Ngừng hoạt động | Badge màu |
| Hành động | Sửa / Xoá | Chỉ Admin |

---

## Form Thêm / Sửa — Tab Mã KVC

| Trường | Bắt buộc | Loại | Ghi chú |
|--------|---------|------|---------|
| Mã KVC | ✓ | Text (readonly khi sửa) | Không thay đổi sau khi tạo |
| Loại | ✓ | Select: Nạp trước / Công nợ | |
| Tên khu vui chơi | ✓ | Text (full width) | |
| Mã định danh tìm kiếm | | Text | Mã dùng trong API, thường = Mã KVC |
| Mã ngân hàng / TK | | Text | Số TK nhận tiền nạp / thanh toán |
| Trạng thái | ✓ | Select: Hoạt động / Ngừng hoạt động | |

## Form Thêm / Sửa — Tab KVC con

| Trường | Bắt buộc | Loại | Ghi chú |
|--------|---------|------|---------|
| Mã KVC con | ✓ | Text | |
| Mã KVC cha | ✓ | Text | Liên kết với KVC gốc |
| Tên KVC cha | | Text (full width) | |
| Tên KVC con | ✓ | Text (full width) | |
| Mã loại vé | ✓ | Text | |
| Đơn giá vốn | ✓ | Number (VNĐ) | |
| Nhóm loại vé | | Text (full width) | |
| Trạng thái | ✓ | Select: Hoạt động / Ngừng hoạt động | |

---

## Dữ liệu mẫu — Tab Mã KVC (17 bản ghi)

**Nhóm Nạp trước:**

| Mã KVC | Tên | Mã định danh | Mã ngân hàng / TK |
|--------|-----|-------------|------------------|
| 11681 | Bản Mòng | 11681 | 1213776969 |
| 6935 | Sun Group | 6935 | 1SB2B24 |
| 11438 | Đồi Rồng | 11438 | 0751040751058 |
| 11750 | Delight | 11750 | 110603463866 |
| 11462 | Samten Hills Dalat | 11462 | 1862867777 |
| 11810 | VinKE & Aquarium Times City | 11810 | 1SB2B24 |

**Nhóm Công nợ:**

| Mã KVC | Tên | Mã định danh | Mã ngân hàng / TK |
|--------|-----|-------------|------------------|
| 10952 | TLTY | 10952 | 5610039478828 |
| 10360 | Sơn Tiên | 10360 | 57457 |
| 11477 | Lumiere | 11477 | 8610023579 |
| 11423 | Mikazuki | 11423 | 200077779999 |
| 11588 | Mekong | 11588 | 60300641396 |
| 10933 | Tà Cú | 10933 | 1130000077689 |
| 11483 | Hồ Tràm | 11483 | 1027882298 |
| 11480 | Nova Phan Thiết | 11480 | 3336333979 |
| 11705 | Công viên nước Hồ Tây | 11705 | 11004009888 |
| 11807 | Sealinks | 11807 | 1100030038237 |
| 11762 | Sightseeing HN | 11762 | sightseeing |

---

## Dữ liệu mẫu — KVC con nạp trước (13 bản ghi)

| Mã con | Tên KVC con | KVC cha | Đơn giá vốn |
|--------|------------|---------|------------|
| 347015 | SJC - Người lớn (Trên 1.4m) T7-CN | VinKE (11810) | 175.000 |
| 347016 | SJC - Trẻ em (1.0–1.4m) T7-CN | VinKE (11810) | 117.000 |
| 347020 | SJC - Người lớn Ngày thường | VinKE (11810) | 191.250 |
| 347030 | SJC - Người lớn (Trên 1.4m) Ngày thường | VinKE (11810) | 173.750 |
| 198432 | Sunworld - Người lớn Ngày thường | Sun Group (6935) | 240.000 |
| 198433 | Sunworld - Trẻ em Ngày thường | Sun Group (6935) | 180.000 |
| 210100 | Bản Mòng - Người lớn Cuối tuần | Bản Mòng (11681) | 210.000 |
| 210101 | Bản Mòng - Trẻ em Cuối tuần | Bản Mòng (11681) | 150.000 |
| 220110 | Đồi Rồng - Người lớn Cuối tuần | Đồi Rồng (11438) | 315.000 |
| 220115 | Đồi Rồng - Người lớn Ngày thường | Đồi Rồng (11438) | 275.000 |
| 230500 | Samten Hills - Người lớn Ngày thường | Samten Hills (11462) | 290.000 |
| 230501 | Samten Hills - Trẻ em Ngày thường | Samten Hills (11462) | 200.000 |
| 240100 | Delight - Người lớn Ngày thường | Delight (11750) | 135.000 |

## Dữ liệu mẫu — KVC con công nợ (13 bản ghi)

| Mã con | Tên KVC con | KVC cha | Đơn giá vốn |
|--------|------------|---------|------------|
| 180001 | Sơn Tiên - Người lớn Ngày thường | Sơn Tiên (10360) | 83.000 |
| 180002 | Sơn Tiên - Trẻ em Ngày thường | Sơn Tiên (10360) | 55.000 |
| 180010 | Mikazuki - Người lớn Cuối tuần | Mikazuki (11423) | 117.000 |
| 180011 | Mikazuki - Trẻ em Cuối tuần | Mikazuki (11423) | 78.000 |
| 180020 | Mekong - Người lớn Ngày thường | Mekong (11588) | 93.000 |
| 180050 | Hồ Tràm - Người lớn Cuối tuần | Hồ Tràm (11483) | 146.000 |
| 180055 | Nova - Người lớn Ngày thường | Nova Phan Thiết (11480) | 176.000 |
| 180060 | TLTY - Người lớn Ngày thường | TLTY (10952) | 105.000 |
| 180070 | Lumiere - Người lớn Ngày thường | Lumiere (11477) | 185.000 |
| 180080 | Tà Cú - Người lớn Ngày thường | Tà Cú (10933) | 68.000 |
| 180090 | Sealinks - Người lớn Ngày thường | Sealinks (11807) | 107.000 |
| 180095 | CVNHT - Người lớn Ngày thường | Công viên nước Hồ Tây (11705) | 125.000 |
| 180099 | Sightseeing - Người lớn Ngày thường | Sightseeing HN (11762) | 88.000 |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `so-du-kvc.html` | Dùng Mã KVC để gọi API lấy số dư hàng ngày |
| `danh-sach-kvc.html` | Hiển thị tên và thông tin từ danh mục Mã KVC |
| `chi-tiet-gia-von-ve-ban.html` | Mã KVC con → xác định Đơn giá vốn của từng loại vé |
| `doi-soat-kvc.html` | Mã KVC và TK ngân hàng dùng để đối soát số dư |

---

## Ghi chú thiết kế

- **Mã KVC = Mã định danh tìm kiếm** trong hầu hết trường hợp — chỉ khác nhau khi KVC dùng alias trong API
- **KVC con** là cấp 2 của KVC — mỗi loại vé (người lớn/trẻ em, ngày thường/cuối tuần) là một KVC con riêng
- **Đơn giá vốn** tại KVC con là cơ sở để tính cột "Tiền vốn" trong `chi-tiet-gia-von-ve-ban.html`
- Sightseeing HN có mã ngân hàng là `sightseeing` (không phải số) — đây là dữ liệu thực từ hệ thống, cần xác nhận lại
