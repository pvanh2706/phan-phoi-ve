# Use Case: Nạp tiền cho KVC nạp tiền

**Màn hình:** `nap-tien-kvc.html` → Tab **Nạp tiền cho KVC nạp tiền**
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Danh sách nạp tiền KVC theo ngày → Tab "Nạp tiền cho KVC nạp tiền"
**Quyền truy cập:** Admin, Member (xem)

---

## Mô tả

Hiển thị lịch sử các giao dịch **chuyển khoản thực tế** từ tài khoản ngân hàng của ezCloud vào tài khoản của các KVC thuộc nhóm **Nạp tiền** (thanh toán trước, không có hạn mức công nợ). Dữ liệu được lấy tự động từ API ngân hàng, phản ánh đúng dòng tiền thực.

Tab này phục vụ mục đích tra cứu và đối chiếu, không cho phép tạo hay chỉnh sửa giao dịch.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc, tìm kiếm toàn bộ lịch sử giao dịch |
| Member | Xem, lọc, tìm kiếm toàn bộ lịch sử giao dịch |

---

## Nguồn dữ liệu API

- **Nguồn:** API ngân hàng liên kết với tài khoản chủ của ezCloud
- **Cơ chế:** Hệ thống tự động đồng bộ giao dịch theo thời gian thực hoặc định kỳ
- **Phân loại:** Chỉ hiển thị các giao dịch **Ghi nợ** (tiền ra) có nội dung liên quan đến topup KVC
- **Không bao gồm:** Giao dịch nội bộ, hoàn tiền, hay thu tiền vào

---

## Use Cases

### UC-NT-01 – Xem danh sách giao dịch nạp tiền

- **Điều kiện:** Người dùng đang ở tab Nạp tiền cho KVC nạp tiền
- **Luồng chính:**
  1. Hệ thống tải và hiển thị toàn bộ giao dịch nạp tiền, sắp xếp theo ngày giảm dần
  2. Mỗi dòng hiển thị: Ngày giờ giao dịch, Ghi nợ, Ghi có, Nội dung giao dịch
  3. Cột Ghi nợ hiển thị màu xanh (tiền đã chuyển đi)
  4. Cột Ghi có luôn = 0 cho loại giao dịch này
- **Kết quả:** Người dùng thấy lịch sử đầy đủ các lần topup tài khoản KVC

### UC-NT-02 – Tìm kiếm theo nội dung giao dịch

- **Điều kiện:** Người dùng muốn tìm giao dịch cụ thể
- **Luồng chính:**
  1. Người dùng nhập từ khoá vào ô **"🔍 Tìm nội dung..."**
  2. Lọc real-time theo nội dung (nội dung giao dịch chứa tên KVC, số tài khoản, ngân hàng)
  3. Chỉ hiển thị các dòng khớp từ khoá (không phân biệt hoa/thường)
- **Kết quả:** Danh sách thu hẹp, dễ tìm giao dịch cụ thể
- **Ví dụ tìm:** "sunworld", "techcombank", "VIN PHU QUOC"

### UC-NT-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng muốn xem giao dịch trong một khoảng thời gian
- **Luồng chính:**
  1. Người dùng chọn **Từ ngày** và/hoặc **Đến ngày**
  2. Danh sách tự lọc chỉ hiển thị giao dịch trong khoảng thời gian đó
  3. Có thể kết hợp với bộ lọc tìm kiếm nội dung
- **Kết quả:** Danh sách giao dịch trong kỳ muốn xem, hỗ trợ đối soát theo tháng

### UC-NT-04 – Phân trang kết quả

- **Điều kiện:** Số dòng vượt quá page size
- **Luồng chính:**
  1. Hệ thống hiển thị thanh phân trang phía dưới bảng
  2. Người dùng nhấn số trang hoặc nút ‹/› để chuyển trang
- **Ghi chú:** Page size cố định

---

## Cấu trúc bảng

| Cột | Mô tả | Ghi chú |
|-----|-------|---------|
| Ngày giờ giao dịch | Thời điểm thực hiện chuyển khoản | Định dạng `DD/MM/YYYY HH:mm:ss` hoặc `DD/MM/YYYY` |
| Ghi nợ | Số tiền đã chuyển đi (₫) | Màu xanh lá, căn phải |
| Ghi có | Luôn = 0 với loại giao dịch này | Màu xám, căn phải |
| Nội dung | Nội dung chuyển khoản từ ngân hàng | Dạng text dài, có thể tìm kiếm |

---

## Định dạng nội dung giao dịch (từ API ngân hàng)

```
HBK-TKThe :<SỐ_TÀI_KHOẢN>, tại <NGÂN_HÀNG>. ND <MÔ_TẢ> -<MÃ_GIAO_DỊCH>
```

**Ví dụ:**
```
HBK-TKThe :1SB2B24, tại NCB. ND Top up Sunworld - ezCloud 17.09.2025 -CTLNHIDO000012817233009-1/1-PMT-002
HBK-TKThe :19139932758899, tại Techcombank. ND ezCloud topup trien khai ban ve bang he thong API cho VIN CUA HOI ngay 28 04 2026 -CTLNHIDO000015124913428-1/1-PMT-002 244
HBK-TKThe :700029610000, tại Shinhan Bank V. ND ezCloud thanh toan nhap lo cho THUY CUNG LOTTE ngay 29 04 2026 lan 1 -CTLNHIDO000015134693760-1/1-PMT-002 243
```

---

## Dữ liệu mẫu

| Ngày giờ GD | Ghi nợ | KVC | Ngân hàng | TK thụ hưởng |
|------------|--------|-----|-----------|-------------|
| 17/09/2025 17:33:32 | 490.000.000 | Sunworld | NCB | 1SB2B24 |
| 28/04/2026 | 50.000.000 | Vin Cửa Hội | Techcombank | 19139932758899 |
| 28/04/2026 | 50.000.000 | Vin Nam Hội An | Vietcombank | 1029876329 |
| 28/04/2026 | 490.000.000 | Sunworld | NCB | 1SB2B24 |
| 28/04/2026 | 100.000.000 | Vin Phú Quốc | Vietcombank | 0091000593278 |
| 29/04/2026 | 365.625.000 | Thủy Cung Lotte (Lần 1) | Shinhan Bank | 700029610000 |
| 29/04/2026 | 237.375.000 | Thủy Cung Lotte (Lần 2) | Shinhan Bank | 700029610000 |

---

## KVC thuộc nhóm Nạp tiền (trả trước)

| Tên KVC | Số TK | Ngân hàng |
|---------|-------|-----------|
| Bản Mòng | 1213776969 | NCB |
| Sunworld (Sun Group) | 1SB2B24 | NCB |
| Đồi Rồng | 0751040751058 | — |
| Delight | 110603463866 | — |
| Samten Hills Dalat | 1862867777 | — |
| Vin Nha Trang | 19139932758899 | Techcombank |
| Vin Cửa Hội | 19139932758899 | Techcombank |
| Vin Phú Quốc | 0091000593278 | Vietcombank |
| Vin Nam Hội An | 1029876329 | Vietcombank |
| Thủy Cung Lotte | 700029610000 | Shinhan Bank |

---

## Mối liên hệ với Tab Quy trình

Tab "Nạp tiền cho KVC nạp tiền" là **bằng chứng thực tế** (API ngân hàng) tương ứng với các card đã **Hoàn thành** trong tab Quy trình nạp tiền KVC. Khi một card trong kanban chuyển sang cột "Hoàn thành", kế toán có thể đối chiếu bằng cách tìm giao dịch tương ứng ở tab này.

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `nap-tien-kvc.html` Tab 1 | Quy trình kanban thủ công tương ứng |
| `so-du-kvc.html` Tab "Nạp tiền" | Số dư KVC sau khi topup |
| `doi-soat-kvc.html` | Đối soát số liệu bán vé với số tiền đã nạp |
