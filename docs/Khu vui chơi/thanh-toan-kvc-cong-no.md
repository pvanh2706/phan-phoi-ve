# Use Case: Thanh toán KVC công nợ

**Màn hình:** `nap-tien-kvc.html` → Tab **Thanh toán KVC công nợ**
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Danh sách nạp tiền KVC theo ngày → Tab "Thanh toán KVC công nợ"
**Quyền truy cập:** Admin, Member (xem)

---

## Mô tả

Hiển thị lịch sử các giao dịch **thanh toán công nợ thực tế** từ tài khoản ngân hàng của ezCloud đến tài khoản các KVC thuộc nhóm **Công nợ** (sử dụng hạn mức tín dụng, trả sau theo kỳ). Dữ liệu được lấy tự động từ API ngân hàng.

Khác với nhóm Nạp tiền (nạp trước), nhóm Công nợ bán vé trước rồi ezCloud thanh toán định kỳ theo số vé thực bán. Tab này ghi nhận các đợt thanh toán đó.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc, tìm kiếm toàn bộ lịch sử thanh toán |
| Member | Xem, lọc, tìm kiếm toàn bộ lịch sử thanh toán |

---

## Nguồn dữ liệu API

- **Nguồn:** API ngân hàng liên kết với tài khoản chủ của ezCloud
- **Cơ chế:** Hệ thống tự động đồng bộ giao dịch theo thời gian thực hoặc định kỳ
- **Phân loại:** Chỉ hiển thị các giao dịch **Ghi nợ** (tiền ra) có nội dung liên quan đến thanh toán công nợ KVC
- **Tần suất:** Thanh toán công nợ thường theo kỳ (ngày, tuần, hoặc tháng tuỳ theo hợp đồng với từng KVC)

---

## Phân biệt Nạp tiền vs Công nợ

| Tiêu chí | KVC Nạp tiền | KVC Công nợ |
|---------|-------------|-------------|
| Cơ chế | Nạp trước, bán theo số dư | Bán trước, trả sau |
| Hạn mức | Không có — tuỳ số dư nạp | Có hạn mức tín dụng cố định |
| Tần suất CK | Theo từng đợt nạp | Định kỳ (thanh toán công nợ đã dùng) |
| Từ khoá nội dung | "topup", "Top up", "nhap lo" | "thanh toan cong no" |

---

## Use Cases

### UC-CN-01 – Xem danh sách giao dịch thanh toán công nợ

- **Điều kiện:** Người dùng đang ở tab Thanh toán KVC công nợ
- **Luồng chính:**
  1. Hệ thống tải và hiển thị toàn bộ giao dịch thanh toán công nợ, sắp xếp theo ngày giảm dần
  2. Mỗi dòng hiển thị: Ngày giờ giao dịch, Ghi nợ, Ghi có, Nội dung giao dịch
  3. Cột Ghi nợ hiển thị màu xanh (tiền đã chuyển đi)
  4. Cột Ghi có luôn = 0
- **Kết quả:** Người dùng thấy lịch sử đầy đủ các lần thanh toán công nợ KVC

### UC-CN-02 – Tìm kiếm theo nội dung giao dịch

- **Điều kiện:** Người dùng muốn tìm giao dịch của một KVC cụ thể
- **Luồng chính:**
  1. Người dùng nhập từ khoá vào ô **"🔍 Tìm nội dung..."**
  2. Lọc real-time theo nội dung giao dịch
  3. Chỉ hiển thị các dòng khớp từ khoá
- **Kết quả:** Danh sách thu hẹp theo KVC, ngân hàng, hoặc mã giao dịch
- **Ví dụ tìm:** "son tien", "mikazuki", "nova phan thiet", "vietcombank"

### UC-CN-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần xem thanh toán trong một kỳ cụ thể
- **Luồng chính:**
  1. Người dùng chọn **Từ ngày** và/hoặc **Đến ngày**
  2. Danh sách tự lọc theo khoảng thời gian
  3. Có thể kết hợp với bộ lọc nội dung
- **Kết quả:** Danh sách thanh toán trong kỳ muốn đối soát
- **Ghi chú:** Hữu ích khi cần xác nhận các KVC đã được thanh toán trong tháng

### UC-CN-04 – Phân trang kết quả

- **Điều kiện:** Số dòng vượt quá page size
- **Luồng chính:**
  1. Hệ thống hiển thị thanh phân trang phía dưới bảng
  2. Người dùng nhấn số trang hoặc nút ‹/› để chuyển trang

---

## Cấu trúc bảng

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| Ngày giờ giao dịch | Thời điểm thực hiện chuyển khoản | Định dạng `DD/MM/YYYY` |
| Ghi nợ | Số tiền đã chuyển đi (₫) | Màu xanh lá, căn phải |
| Ghi có | Luôn = 0 với loại giao dịch này | Màu xám, căn phải |
| Nội dung | Nội dung chuyển khoản từ ngân hàng | Dạng text dài, có thể tìm kiếm |

---

## Định dạng nội dung giao dịch (từ API ngân hàng)

```
HBK-TKThe :<SỐ_TÀI_KHOẢN>, tại <NGÂN_HÀNG>. ND ezCloud thanh toan cong no cho <TÊN_KVC> ngay <NGÀY> -<MÃ_GIAO_DỊCH>
```

**Ví dụ:**
```
HBK-TKThe :57457, tại Techcombank. ND ezCloud thanh toan cong no cho Son Tien ngay 16 09 2025 -CTLNHIDO-PMT-001
HBK-TKThe :200077779999, tại Vietcombank. ND ezCloud thanh toan cong no cho Mikazuki ngay 16 09 2025 -CTLNHIDO-PMT-002
HBK-TKThe :3336333979, tại Vietcombank. ND ezCloud thanh toan cong no cho Nova Phan Thiet ngay 16 09 2025 -CTLNHIDO-PMT-005
```

---

## Dữ liệu mẫu

| Ngày GD | Ghi nợ | KVC | Ngân hàng | TK thụ hưởng |
|---------|--------|-----|-----------|-------------|
| 16/09/2025 | 42.495.000 | Sơn Tiên | Techcombank | 57457 |
| 16/09/2025 | 35.953.000 | Mikazuki | Vietcombank | 200077779999 |
| 16/09/2025 | 49.833.500 | Mekong | Vietcombank | 60300641396 |
| 16/09/2025 | 22.850.000 | Hồ Tràm | Vietcombank | 1027882298 |
| 16/09/2025 | 119.995.000 | Nova Phan Thiết | Vietcombank | 3336333979 |
| 16/09/2025 | 32.082.500 | Công viên nước Hồ Tây | Vietcombank | 11004009888 |
| 16/09/2025 | 8.110.000 | Sealinks | Vietcombank | 1100030038237 |
| 16/09/2025 | 9.996.172.999 | Sightseeing HN | Vietcombank | sightseeing |

---

## KVC thuộc nhóm Công nợ

| Tên KVC | Mã KVC | Hạn mức | Số TK | Ngân hàng |
|---------|--------|---------|-------|-----------|
| TLTY | 10952 | 50.000.000 | — | — |
| Sơn Tiên | 10360 | 70.000.000 | 57457 | Techcombank |
| Lumiere | 11477 | 60.000.000 | — | — |
| Mikazuki | 11423 | 60.000.000 | 200077779999 | Vietcombank |
| Mekong | 11588 | 50.000.000 | 60300641396 | Vietcombank |
| Tà Cú | 10933 | 50.000.000 | — | — |
| Hồ Tràm | 11483 | 30.000.000 | 1027882298 | Vietcombank |
| Nova Phan Thiết | 11480 | 150.000.000 | 3336333979 | Vietcombank |
| Lâm Đồng | 11627 | 30.000.000 | — | — |
| Tiên Sa show | 11753 | 100.000.000 | — | — |
| Công viên nước Hồ Tây | 11705 | 50.000.000 | 11004009888 | Vietcombank |
| Sealinks | 11807 | 10.000.000 | 1100030038237 | Vietcombank |
| Sightseeing HN | 11762 | ~10 tỷ | sightseeing | Vietcombank |

---

## Ghi chú đặc biệt

- **Sightseeing HN** có hạn mức bất thường (~10 tỷ) và số tiền thanh toán rất lớn (`9.996.172.999 ₫`) — cần xác nhận lại hợp đồng và hạn mức thực tế
- Thanh toán công nợ thường diễn ra theo **đợt** (nhiều KVC cùng ngày), khác với nạp tiền là từng đợt riêng lẻ
- Nếu card kanban trong Tab 1 của KVC công nợ bị chuyển sang cột "Thất bại", kế toán cần kiểm tra lại ở tab này xem giao dịch ngân hàng có phát sinh hay không

---

## Mối liên hệ với Tab Quy trình

Tab "Thanh toán KVC công nợ" là **bằng chứng thực tế** từ ngân hàng khi xử lý các card có tag "Công nợ" trong Tab 1 (Quy trình nạp tiền KVC). Sau khi card công nợ hoàn thành trong kanban, kế toán đối chiếu bằng cách tra cứu ở tab này.

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `nap-tien-kvc.html` Tab 1 | Quy trình kanban — card loại "Thanh toán Công nợ" |
| `so-du-kvc.html` Tab "Công nợ" | Số dư và số nợ còn lại của từng KVC |
| `doi-soat-kvc.html` | Đối soát tổng hợp doanh số bán vé với khoản đã thanh toán |
| `kvc-hoan-tien.html` | Trường hợp KVC hoàn tiền sau thanh toán công nợ |
