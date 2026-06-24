# Use Case: KVC hoàn tiền

**Màn hình:** `kvc-hoan-tien.html`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → KVC hoàn tiền
**Quyền truy cập:** Admin (thêm/sửa), Member (xem)

---

## Mô tả

Màn hình quản lý các trường hợp **hoàn tiền vé** xảy ra sau khi vé đã được bán và giá vốn đã được ghi nhận. Mỗi bản ghi hoàn tiền ghi lại: vé nào bị hoàn, KVC nào thực hiện hoàn, số tiền hoàn cho KVC và số tiền hoàn lại cho khách hàng, kèm trạng thái xử lý của từng bên.

Màn hình phân theo 2 nhóm: **KVC nạp tiền** (KVC hoàn lại tiền vào tài khoản nạp trước) và **KVC công nợ** (KVC hoàn lại bằng cách trừ vào công nợ phải trả).

Dữ liệu được nhập thủ công bởi Admin khi có phát sinh hoàn tiền thực tế.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc, thêm bản ghi hoàn tiền |
| Member | Xem, tìm kiếm, lọc |

---

## Nguồn dữ liệu

- **Nhập thủ công** — Admin tạo từng bản ghi khi có phát sinh hoàn tiền
- Không lấy tự động từ API
- Liên kết ngược với `chi-tiet-gia-von-ve-ban.html` qua Mã đặt vé và Mã loại vé

---

## Cấu trúc 2 Tab

| Tab | Nhóm KVC | Cơ chế hoàn |
|-----|---------|------------|
| KVC nạp tiền | KVC đã nhận tiền nạp trước | KVC hoàn lại vào tài khoản nạp (tăng số dư) |
| KVC công nợ | KVC hạn mức tín dụng | KVC hoàn bằng cách trừ vào công nợ phải trả kỳ đó |

---

## Use Cases

### UC-HT-01 – Xem danh sách hoàn tiền

- **Điều kiện:** Người dùng đang ở màn hình KVC hoàn tiền
- **Luồng chính:**
  1. Tab "KVC nạp tiền" hiển thị mặc định
  2. Bảng liệt kê tất cả bản ghi hoàn tiền, sắp xếp theo ngày hoàn giảm dần
  3. Mỗi dòng là một đơn hoàn tiền, gồm 12 cột thông tin
  4. Badge màu phân biệt trạng thái của từng bên (KVC và khách hàng)
- **Kết quả:** Người dùng thấy toàn bộ lịch sử hoàn tiền

### UC-HT-02 – Xem hoàn tiền KVC công nợ

- **Điều kiện:** Người dùng nhấn tab "KVC công nợ"
- **Luồng chính:** Tương tự UC-HT-01, hiển thị dữ liệu nhóm KVC công nợ
- **Kết quả:** Người dùng thấy lịch sử hoàn tiền nhóm tín dụng

### UC-HT-03 – Tìm kiếm bản ghi hoàn tiền

- **Điều kiện:** Người dùng muốn tìm một đơn hoàn cụ thể
- **Luồng chính:**
  1. Nhập từ khoá vào ô **"🔍 Tìm mã đặt vé, mã KVC, tên KVC..."**
  2. Bảng lọc real-time, so khớp theo: Mã đặt vé, Mã KVC, Tên KVC, Tên loại vé
  3. Không phân biệt hoa/thường
- **Kết quả:** Danh sách thu hẹp theo từ khoá
- **Ví dụ tìm:** `"DV20260428"`, `"11681"`, `"bản mòng"`, `"người lớn"`

### UC-HT-04 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng muốn xem hoàn tiền trong một kỳ
- **Luồng chính:**
  1. Chọn **Từ ngày** và/hoặc **Đến ngày** ở thanh công cụ
  2. Bảng cập nhật ngay, chỉ hiện bản ghi trong khoảng thời gian đó
  3. Có thể kết hợp với tìm kiếm từ khoá
- **Kết quả:** Danh sách hoàn tiền trong kỳ cần đối soát

### UC-HT-05 – Lọc theo trạng thái KVC

- **Điều kiện:** Người dùng muốn xem các hoàn tiền theo tình trạng xử lý phía KVC
- **Luồng chính:**
  1. Chọn dropdown: **Tất cả trạng thái / Hoàn tất / Đang xử lý / Từ chối**
  2. Bảng lọc ngay
- **Kết quả:** Danh sách chỉ hiện bản ghi theo trạng thái KVC đã chọn

### UC-HT-06 – Thêm bản ghi hoàn tiền

- **Điều kiện:** Admin có thông tin một đơn hoàn tiền mới phát sinh
- **Luồng chính:**
  1. Admin nhấn **+ Thêm hoàn tiền** (ở tab tương ứng)
  2. Modal "Thêm hoàn tiền — KVC nạp tiền/công nợ" mở ra
  3. Admin điền đầy đủ các trường (xem bảng form bên dưới)
  4. Trạng thái mặc định: KVC = "Đang xử lý", Khách hàng = "Chưa xử lý"
  5. Admin nhấn **Lưu**
- **Validation:** Mã đặt vé và Tên KVC là bắt buộc
- **Kết quả:** Bản ghi mới xuất hiện trong bảng, có thể tìm kiếm/lọc ngay

### UC-HT-07 – Phân trang kết quả

- **Điều kiện:** Số dòng sau lọc > 100
- **Luồng chính:**
  1. Hệ thống hiển thị thanh phân trang phía dưới bảng
  2. Hiển thị `"Hiển thị X–Y / Z dòng"` và các nút số trang + ‹/›
  3. Người dùng nhấn chuyển trang
- **Ghi chú:** Page size cố định 100 dòng/trang

---

## Cấu trúc bảng — 12 cột

| # | Cột | Mô tả | Định dạng |
|---|-----|-------|-----------|
| 1 | Mã đặt vé | Mã đơn đặt vé gốc cần hoàn | Mono, VD: `DV20250916001` |
| 2 | Ngày hoàn | Ngày xử lý hoàn tiền | Mono, `DD/MM/YYYY` |
| 3 | Mã KVC | Mã định danh KVC | Mono |
| 4 | Tên KVC | Tên khu vui chơi | In đậm, màu trắng |
| 5 | Mã loại vé | Mã loại vé bị hoàn | Mono |
| 6 | Tên loại vé | Tên mô tả loại vé | Text |
| 7 | SL vé | Số lượng vé trong đơn hoàn | Số nguyên, căn giữa |
| 8 | Số tiền hoàn KVC (₫) | Số tiền KVC hoàn trả | Màu đỏ, căn phải |
| 9 | Lý do hoàn | Lý do hoàn tiền | Text nhiều dòng, max-width 200px |
| 10 | Trạng thái KVC | Tình trạng xử lý phía KVC | Badge màu |
| 11 | Số tiền hoàn cho KH (₫) | Số tiền hoàn lại cho khách | Căn phải; `—` nếu chưa xác định |
| 12 | Tình trạng hoàn cho KH | Tình trạng xử lý phía khách hàng | Badge màu |

---

## Form Thêm hoàn tiền

| Trường | Bắt buộc | Loại | Ghi chú |
|--------|---------|------|---------|
| Mã đặt vé | ✓ | Text | VD: `DV20260001` |
| Ngày hoàn | | Date | |
| Mã KVC | | Text | VD: `11681` |
| Tên KVC | ✓ | Text | Tên đầy đủ |
| Mã loại vé | | Text | VD: `VNL001` |
| Tên loại vé | | Text | VD: `Vé người lớn` |
| Số lượng vé | | Number | Tối thiểu 1 |
| Số tiền hoàn KVC (₫) | | Number | |
| Số tiền hoàn cho KH (₫) | | Number | |
| Trạng thái KVC | | Select | Mặc định: Đang xử lý |
| Tình trạng hoàn cho KH | | Select | Mặc định: Chưa xử lý |
| Lý do hoàn | | Textarea | Mô tả chi tiết lý do |

---

## Trạng thái và màu badge

### Trạng thái KVC

| Giá trị | Màu badge | Ý nghĩa |
|---------|-----------|---------|
| Hoàn tất | Xanh lá | KVC đã xử lý hoàn thành |
| Đang xử lý | Amber/Vàng | KVC đang xem xét |
| Từ chối | Đỏ | KVC không chấp nhận hoàn |

### Tình trạng hoàn cho khách hàng

| Giá trị | Màu badge | Ý nghĩa |
|---------|-----------|---------|
| Đã hoàn | Xanh lá | Tiền đã chuyển về cho khách |
| Đang chuyển | Xanh dương (sky) | Đang trong quá trình chuyển |
| Chờ xác nhận | Amber/Vàng | Chờ khách xác nhận thông tin hoàn |
| Chưa xử lý | Xám | Chưa bắt đầu xử lý phía khách |
| Không hoàn | Đỏ | Không đủ điều kiện hoàn cho khách |

---

## Dữ liệu mẫu — Tab KVC nạp tiền

| Mã đặt vé | Ngày hoàn | KVC | Loại vé | SL | Hoàn KVC | Lý do | TT KVC | Hoàn KH | TT KH |
|-----------|---------|-----|---------|-----|---------|-------|--------|---------|-------|
| DV20250916001 | 16/09/2025 | Bản Mòng (11681) | Vé người lớn | 2 | 360.000 ₫ | Khách hủy trước ngày sử dụng | Hoàn tất | 360.000 ₫ | Đã hoàn |
| DV20250916002 | 16/09/2025 | Bản Mòng (11681) | Vé trẻ em | 1 | 120.000 ₫ | Khách hủy trước ngày sử dụng | Hoàn tất | 120.000 ₫ | Đã hoàn |
| DV20250917003 | 17/09/2025 | Vin Nam Hội An (11682) | Vé người lớn | 3 | 750.000 ₫ | Lỗi hệ thống trùng đặt vé | Đang xử lý | — | Chờ xác nhận |
| DV20250918004 | 18/09/2025 | Vin Phú Quốc (11683) | Vé gia đình | 1 | 1.200.000 ₫ | Khách không đến, yêu cầu hoàn toàn bộ | Hoàn tất | 1.200.000 ₫ | Đã hoàn |
| DV20250920005 | 20/09/2025 | Thủy Cung Lotte (11684) | Vé người lớn | 2 | 400.000 ₫ | Yêu cầu sau ngày sử dụng, không đủ điều kiện | Từ chối | — | Không hoàn |
| DV20260428006 | 28/04/2026 | Vin Cửa Hội (11685) | Vé người lớn | 4 | 1.600.000 ₫ | KVC đóng cửa đột xuất do bảo trì | Hoàn tất | 1.600.000 ₫ | Đang chuyển |
| DV20260429007 | 29/04/2026 | Sealinks (11686) | Vé người lớn | 2 | 500.000 ₫ | Khách hủy do thời tiết xấu | Đang xử lý | — | Chưa xử lý |

---

## Lý do hoàn tiền thường gặp

| Lý do | Kết quả thường gặp |
|-------|------------------|
| Khách hủy trước ngày sử dụng | KVC Hoàn tất, KH Đã hoàn |
| Lỗi hệ thống trùng đặt vé | KVC Đang xử lý, KH Chờ xác nhận |
| KVC đóng cửa đột xuất | KVC Hoàn tất (bắt buộc), KH Đang chuyển |
| Khách không đến | KVC Hoàn tất (tùy chính sách), KH Đã hoàn |
| Yêu cầu sau ngày sử dụng | KVC Từ chối, KH Không hoàn |
| Khách hủy do thời tiết | Tùy chính sách KVC |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `chi-tiet-gia-von-ve-ban.html` | Tra cứu đơn đặt vé gốc theo Mã đặt vé — đối chiếu giá vốn đã ghi nhận |
| `doi-soat-kvc.html` | Hoàn tiền làm tăng số dư KVC nạp tiền hoặc giảm công nợ phải trả — ảnh hưởng đến cột (2) hoặc (3) trong đối soát |
| `so-du-kvc.html` | Sau khi KVC hoàn tất, số dư tài khoản KVC nạp tiền sẽ tăng tương ứng |
| `nap-tien-kvc.html` | Với KVC công nợ, khoản hoàn có thể bù trừ vào đợt thanh toán công nợ kế tiếp |

---

## Ghi chú thiết kế

- **Hoàn tiền KVC nạp tiền:** Số tiền hoàn KVC làm **tăng số dư** tài khoản nạp trước của KVC đó — ảnh hưởng đến cột (2) Số nạp thêm trong Đối soát KVC
- **Hoàn tiền KVC công nợ:** Số tiền hoàn làm **giảm công nợ** phải thanh toán trong kỳ — ảnh hưởng đến tổng thanh toán trong đợt quyết toán
- **Từ chối hoàn (phía KVC):** Nếu KVC từ chối, khoản này không ảnh hưởng đến số dư/công nợ — ezCloud chịu phần chênh lệch hoặc thỏa thuận riêng với khách
- **Số tiền hoàn KVC ≠ Số tiền hoàn KH** trong một số trường hợp: KVC hoàn đầy đủ nhưng KH chỉ hoàn một phần do phí hủy
- Cột "Số tiền hoàn cho KH" hiển thị `—` khi chưa xác định hoặc khi trạng thái là "Không hoàn"
