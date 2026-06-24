# Use Case: Số dư khu vui chơi hàng ngày

**Màn hình:** `so-du-kvc.html`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Số dư khu vui chơi hàng ngày

---

## Mô tả

Màn hình theo dõi số dư và công nợ của từng khu vui chơi (KVC) theo từng ngày. Dữ liệu được lấy từ API của từng KVC theo ngày và mã KVC, hiển thị dưới dạng 2 tab tương ứng 2 loại hình topup: **Nạp trước** (thanh toán trước) và **Công nợ** (giới hạn tín dụng).

Mỗi bản ghi là snapshot số dư của một KVC tại thời điểm đầu ngày (thường 00:00:xx), giúp đội ngũ theo dõi biến động số dư hàng ngày và phát hiện KVC âm số dư.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem toàn bộ số dư tất cả KVC, lọc theo tên/ngày |
| Member | Xem toàn bộ số dư tất cả KVC, lọc theo tên/ngày |

---

## Nguồn dữ liệu API

Dữ liệu được lấy từ API của từng KVC với các tham số:
- **Theo ngày:** ngày cụ thể (định dạng `YYYY-MM-DD`)
- **Theo mã KVC:** mã định danh KVC trong hệ thống (ví dụ: `11681`, `6935`, `11438`)
- **Tần suất gọi:** mỗi ngày một lần, thường tại thời điểm 00:00:xx (đầu ngày VN)
- **Kết quả:** snapshot số dư khả dụng + số nợ hiện tại (nếu có) tại thời điểm gọi API

---

## Use Cases

### UC-SD-01 – Xem danh sách số dư KVC nạp tiền theo ngày
- **Điều kiện:** Người dùng đang ở màn hình Số dư KVC hàng ngày
- **Luồng chính:**
  1. Hệ thống hiển thị tab "Số dư KVC nạp tiền" mặc định
  2. Dữ liệu được sắp xếp theo ngày giảm dần (mới nhất trước)
  3. Mỗi ngày có dòng phân cách hiển thị ngày định dạng `DD/MM/YYYY`
  4. Mỗi dòng là số dư của một KVC vào đầu ngày đó
- **Kết quả:** Người dùng thấy lịch sử số dư của các KVC loại nạp trước

### UC-SD-02 – Xem danh sách số dư KVC công nợ theo ngày
- **Điều kiện:** Người dùng đang ở màn hình Số dư KVC hàng ngày
- **Luồng chính:**
  1. Người dùng nhấn tab "Số dư KVC công nợ"
  2. Hệ thống hiển thị danh sách KVC loại công nợ, sắp xếp theo ngày giảm dần
  3. Mỗi dòng hiển thị thêm cột "Số nợ hiện tại" ngoài số dư khả dụng
- **Kết quả:** Người dùng thấy lịch sử số dư và công nợ của các KVC loại công nợ

### UC-SD-03 – Lọc theo tên KVC
- **Điều kiện:** Người dùng đang ở tab bất kỳ
- **Luồng chính:**
  1. Người dùng nhập tên KVC vào ô tìm kiếm "🔍 Tìm tên KVC..."
  2. Danh sách lọc real-time theo từ khoá (không phân biệt hoa/thường, có dấu)
  3. Chỉ hiển thị các dòng có tên KVC khớp với từ khoá
- **Kết quả:** Danh sách thu hẹp chỉ còn các KVC phù hợp

### UC-SD-04 – Lọc theo khoảng ngày
- **Điều kiện:** Người dùng đang ở tab bất kỳ
- **Luồng chính:**
  1. Người dùng chọn "Từ ngày" và/hoặc "Đến ngày" từ date picker
  2. Danh sách tự động lọc chỉ hiển thị các bản ghi trong khoảng ngày đã chọn
  3. Có thể kết hợp với bộ lọc tên KVC
- **Kết quả:** Danh sách thu hẹp theo khoảng thời gian, pagination tự điều chỉnh

### UC-SD-05 – Phân trang kết quả
- **Điều kiện:** Số bản ghi > 100 dòng/trang
- **Luồng chính:**
  1. Hệ thống hiển thị thanh pagination phía dưới bảng
  2. Thanh pagination hiển thị: "Hiển thị X–Y / Z dòng" + các nút trang
  3. Người dùng nhấn số trang hoặc nút ‹/› để chuyển trang
- **Ghi chú:** Page size cố định 100 dòng/trang

### UC-SD-06 – Nhận diện KVC âm số dư
- **Điều kiện:** API trả về số dư khả dụng < 0
- **Luồng chính:**
  1. Hệ thống hiển thị giá trị số dư với màu đỏ (class `amount-red`)
  2. Ví dụ: Delight có số dư `-22.155.000` hiển thị màu đỏ
- **Kết quả:** Nhân viên dễ dàng nhận ra KVC đang âm số dư cần xử lý

---

## Cấu trúc 2 Tab

### Tab 1 – Số dư KVC nạp tiền

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| Ngày VN | Ngày lấy dữ liệu | Định dạng `YYYY-MM-DD` |
| Giờ VN | Giờ lấy dữ liệu | Thường `00:00:xx` (đầu ngày) |
| Loại topup | Loại hình nạp | Badge xanh lá "Nạp trước" |
| Tên KVC | Tên khu vui chơi | In đậm |
| Số dư khả dụng | Số dư tại thời điểm API call | Xanh = dương, Đỏ = âm |
| Mã KVC | Mã định danh KVC trong hệ thống | Monospace font |
| Mã Ngân hàng | Tài khoản ngân hàng liên kết | Monospace font |

**KVC nạp tiền mẫu:**

| Tên KVC | Mã KVC | Mã NH |
|---------|--------|-------|
| Bản Mòng | 11681 | 1213776969 |
| Sun Group | 6935 | 1SB2B24 |
| Đồi Rồng | 11438 | 0751040751058 |
| Delight | 11750 | 110603463866 |
| Samten Hills Dalat | 11462 | 1862867777 |

### Tab 2 – Số dư KVC công nợ

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| Ngày VN | Ngày lấy dữ liệu | Định dạng `YYYY-MM-DD` |
| Giờ VN | Giờ lấy dữ liệu | Thường `00:00:xx` |
| Loại topup | Loại hình nạp | Badge tím "Công nợ" |
| Tên KVC | Tên khu vui chơi | In đậm |
| Số dư khả dụng | Số dư tín dụng còn lại | Xanh = còn dư, Đỏ = vượt hạn mức |
| Số nợ hiện tại | Phần đã dùng chưa thanh toán | Đỏ = có nợ, Xám = 0 |
| Mã KVC | Mã định danh KVC | Monospace font |
| TK Ngân hàng | Tài khoản ngân hàng | Monospace font |

**KVC công nợ mẫu:**

| Tên KVC | Mã KVC | Hạn mức |
|---------|--------|---------|
| TLTY | 10952 | 50.000.000 |
| Sơn Tiên | 10360 | 70.000.000 |
| Lumiere | 11477 | 60.000.000 |
| Mikazuki | 11423 | 60.000.000 |
| Mekong | 11588 | 50.000.000 |
| Tà Cú | 10933 | 50.000.000 |
| Hồ Tràm | 11483 | 30.000.000 |
| Nova Phan Thiết | 11480 | 150.000.000 |
| Lâm Đồng | 11627 | 30.000.000 |
| Tiên Sa show | 11753 | 100.000.000 |
| Công viên nước | 11705 | 50.000.000 |
| Sealinks | 11807 | 10.000.000 |
| Sightseeing HN | 11762 | ~10 tỷ |

---

## Logic hiển thị màu sắc số tiền

| Trạng thái | Class CSS | Màu |
|-----------|-----------|-----|
| Dương (> 0) | `amount-green` | Xanh lá |
| Âm (< 0) | `amount-red` | Đỏ |
| Bằng 0 | `amount-zero` | Xám |

---

## Dòng phân cách ngày (Date Separator)

Sau khi sort giảm dần theo ngày, hệ thống tự động chèn dòng phân cách (class `date-separator`) trước nhóm mỗi ngày mới, hiển thị "📅 DD/MM/YYYY". Dòng này không thể click và không bị ảnh hưởng bởi hover style.

---

## Bộ lọc và Phân trang

| Tham số | Loại | Mô tả |
|---------|------|-------|
| Tìm tên KVC | Text input | Lọc real-time, không phân biệt hoa/thường |
| Từ ngày | Date picker | Lọc bản ghi từ ngày này trở đi |
| Đến ngày | Date picker | Lọc bản ghi đến ngày này |
| Page size | Cố định 100 | Số dòng tối đa mỗi trang |

---

## Xử lý dữ liệu phía client

1. **Khi tải trang (DOMContentLoaded):** Sort 2 bảng giảm dần theo `data-date`, gọi `render('nap')` và `render('cn')`
2. **Khi lọc:** Reset về trang 1, lọc các dòng, chèn date separator, cập nhật pagination
3. **Khi chuyển trang:** Chỉ render lại bảng, không reset bộ lọc

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `danh-sach-kvc.html` | Danh sách tổng hợp các KVC và thông tin cơ bản |
| `ma-kvc.html` | Quản lý mã định danh KVC (Mã KVC dùng để gọi API) |
| `nap-tien-kvc.html` | Chi tiết các giao dịch nạp tiền theo ngày |
| `doi-soat-kvc.html` | Đối soát số liệu KVC theo kỳ |
| `kvc-hoan-tien.html` | Tình trạng hoàn tiền, liên quan số dư âm |

---

## Ghi chú thiết kế

- Số dư âm (màu đỏ) ở tab Nạp tiền — ví dụ Delight `-22.155.000` — là tín hiệu cần chú ý: KVC đã bán vé vượt số dư còn trong tài khoản
- Cột "Số nợ hiện tại" ở tab Công nợ = hạn mức tín dụng − số dư khả dụng còn lại
- Một số KVC (TLTY, Lumiere, Tiên Sa show) có số nợ = 0, nghĩa là chưa sử dụng hạn mức
- Giờ ghi nhận thường là 23:59:xx do job tự động gọi API cuối ngày, thường lấy dữ liệu vào 23:59 mỗi ngày
