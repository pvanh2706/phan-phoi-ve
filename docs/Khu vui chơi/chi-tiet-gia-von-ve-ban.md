# Use Case: Chi tiết giá vốn vé bán

**Màn hình:** `chi-tiet-gia-von-ve-ban.html`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Chi tiết giá vốn vé bán
**Quyền truy cập:** Admin, Member (xem)

---

## Mô tả

Màn hình hiển thị chi tiết từng đơn đặt vé kèm giá vốn — bao gồm đơn giá, số lượng, tiền bán, tiền vốn, đại lý bán, đại lý mua, KVC và loại vé. Dữ liệu được lấy tự động từ API **mỗi ngày lúc 23h59**, phản ánh toàn bộ giao dịch bán vé trong ngày.

Đây là màn hình phục vụ theo dõi **giá vốn thực tế** của từng giao dịch, là cơ sở để tính lợi nhuận và đối soát với KVC.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc, tìm kiếm toàn bộ dữ liệu cả 2 tab |
| Member | Xem, lọc, tìm kiếm toàn bộ dữ liệu cả 2 tab |

---

## Nguồn dữ liệu API

- **Thời điểm gọi API:** Mỗi ngày lúc **23h59** (job tự động cuối ngày)
- **Nội dung:** Toàn bộ đơn đặt vé trong ngày, kèm giá vốn từng loại vé
- **Phân tách:** 2 tab riêng biệt theo loại hình KVC (Nạp tiền / Công nợ)
- **Không chỉnh sửa được:** Dữ liệu chỉ đọc, phản ánh đúng giao dịch thực tế

---

## Cấu trúc 2 Tab

| Tab | Nội dung | KVC |
|-----|---------|-----|
| KVC nạp tiền | Vé bán tại các KVC thanh toán trước | Sunworld, VinKE, Vinpearl, Thủy cung Lotte... |
| KVC công nợ | Vé bán tại các KVC hạn mức tín dụng | Sơn Tiên, Mikazuki, Mekong, Hồ Tràm, Nova... |

---

## Use Cases

### UC-GV-01 – Xem danh sách giá vốn vé bán

- **Điều kiện:** Người dùng đang ở màn hình Chi tiết giá vốn vé bán
- **Luồng chính:**
  1. Hệ thống hiển thị tab "KVC nạp tiền" mặc định
  2. Dữ liệu sắp xếp theo ngày đặt giảm dần (mới nhất trước)
  3. Mỗi dòng là một đơn đặt vé gồm 19 cột thông tin
  4. Dòng phân cách ngày được chèn tự động giữa các nhóm ngày khác nhau
- **Kết quả:** Người dùng thấy toàn bộ lịch sử đặt vé theo từng ngày

### UC-GV-02 – Xem giá vốn KVC công nợ

- **Điều kiện:** Người dùng muốn xem dữ liệu nhóm KVC công nợ
- **Luồng chính:**
  1. Người dùng nhấn tab **"KVC công nợ"**
  2. Hệ thống hiển thị các đơn đặt vé của KVC nhóm công nợ
  3. Cấu trúc cột và bộ lọc giống hệt tab KVC nạp tiền
- **Kết quả:** Người dùng thấy chi tiết giá vốn của từng đơn đặt vé KVC công nợ

### UC-GV-03 – Tìm kiếm đơn đặt vé

- **Điều kiện:** Người dùng muốn tìm đơn cụ thể
- **Luồng chính:**
  1. Người dùng nhập từ khoá vào ô **"🔍 Tìm tên KVC, đại lý, loại vé..."**
  2. Danh sách lọc real-time theo `data-search` của từng dòng
  3. `data-search` chứa: tên KVC, tên đại lý, loại vé, ngân hàng
- **Kết quả:** Danh sách thu hẹp theo từ khoá nhập
- **Ví dụ tìm:** `"sunworld"`, `"thủy cung"`, `"người lớn"`, `"vinpearl"`

### UC-GV-04 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng muốn xem dữ liệu trong một kỳ
- **Luồng chính:**
  1. Người dùng chọn **Từ ngày** và/hoặc **Đến ngày**
  2. Danh sách tự lọc chỉ hiển thị đơn đặt trong khoảng thời gian đó
  3. Có thể kết hợp với bộ lọc tìm kiếm từ khoá
- **Kết quả:** Danh sách đặt vé trong kỳ, hỗ trợ đối soát theo tháng/tuần

### UC-GV-05 – Phân trang kết quả

- **Điều kiện:** Tổng số dòng sau lọc > 100
- **Luồng chính:**
  1. Hệ thống hiển thị thanh phân trang phía dưới bảng
  2. Hiển thị `"X–Y / Z dòng"` và các nút số trang + ‹/›
  3. Người dùng nhấn chuyển trang
- **Ghi chú:** Page size cố định 100 dòng/trang

---

## Cấu trúc bảng — 19 cột

| # | Cột | Mô tả | Định dạng |
|---|-----|-------|-----------|
| 1 | Mã đặt vé | Mã đơn đặt hàng | Mono, VD: `60023151211` |
| 2 | Đơn giá | Giá vốn mỗi vé (₫) | Mono, căn phải |
| 3 | Tên loại vé | Tên cụ thể loại vé | Text, VD: "Người lớn (Trên 1.4m)-Cuối tuần T7-CN" |
| 4 | Tên nhóm loại vé | Nhóm vé theo KVC | Text, VD: "SJC- Thủy cung Timescity" |
| 5 | Tiền bán | Tổng doanh thu bán vé (₫) | Xanh lá, căn phải |
| 6 | Tiền vốn | Tổng giá vốn (₫) | Xanh dương, căn phải |
| 7 | Mã ĐL bán | Mã đại lý thực hiện bán | Mono |
| 8 | SL vé | Số lượng vé trong đơn | Số nguyên, căn phải |
| 9 | Mã ĐL mua | Mã đại lý mua vé từ KVC | Mono |
| 10 | Tên đại lý mua | Tên đại lý mua trung gian | Text, VD: "Oneinventory_SJC Thủy cung Lotte" |
| 11 | Mã KVC | Mã định danh KVC trong hệ thống | Mono |
| 12 | Tên Khu vui chơi | Tên KVC đầy đủ | Text |
| 13 | Tạm tính | Giá trị tạm tính (= Tiền bán) | Xanh lá, căn phải |
| 14 | ID | ID bản ghi nội bộ | Mono |
| 15 | Ngày đặt | Ngày thực hiện đặt vé | Xám, định dạng `YYYY-MM-DD` |
| 16 | Tên đại lý bán | Tên đại lý bán đầy đủ | Text, VD: "Thủy cung VINKE-EZCMT" |
| 17 | Mã loại vé | Mã định danh loại vé | Mono |
| 18 | Tên ĐL mua cấp trên | Tên đại lý mua ở cấp cao hơn | Text |
| 19 | Mã ĐL mua cấp trên | Mã đại lý mua cấp trên | Mono, thường = `5129` |

---

## Logic tính toán

| Trường | Công thức |
|--------|-----------|
| Tiền bán | `Đơn giá bán × SL vé` |
| Tiền vốn | `Đơn giá vốn × SL vé` |
| Tạm tính | = Tiền bán (giá trị trước khi đối soát chính thức) |
| Lợi nhuận đơn (tính ngoài UI) | `Tiền bán − Tiền vốn` |

---

## Màu sắc hiển thị

| Trường | Màu | Ý nghĩa |
|--------|-----|---------|
| Tiền bán, Tạm tính | Xanh lá (`#4ade80`) | Doanh thu |
| Tiền vốn | Xanh dương (`#60a5fa`) | Giá vốn |
| Đơn giá | Xám mono | Đơn giá mỗi vé |
| Ngày đặt | Xám (`#6b7280`) | Ngày tham chiếu |

---

## Dữ liệu mẫu — Tab KVC nạp tiền

| Ngày | KVC | Loại vé | Đơn giá | SL | Tiền bán | Tiền vốn |
|------|-----|---------|---------|-----|---------|---------|
| 2025-09-01 | VinKE & Aquarium Times City (11810) | Người lớn-Cuối tuần T7-CN | 177.200 | 5 | 886.000 | 875.000 |
| 2025-09-01 | VinKE & Aquarium Times City (11810) | Trẻ em (1.0–1.4m)-Cuối tuần | 120.000 | 4 | 480.000 | 468.000 |
| 2025-09-02 | Sunworld Hà Nội (6935) | Người lớn-Ngày thường | 245.000 | 5 | 1.225.000 | 1.200.000 |
| 2025-09-02 | Sunworld Hà Nội (6935) | Trẻ em-Ngày thường | 185.000 | 3 | 555.000 | 540.000 |
| 2025-09-03 | Vinpearl Cửa Hội (11682) | Người lớn-Cuối tuần | 320.000 | 3 | 960.000 | 945.000 |
| 2025-09-03 | Vinpearl Nam Hội An (11684) | Người lớn-Ngày thường | 280.000 | 4 | 1.120.000 | 1.100.000 |
| 2025-09-04 | Vinpearl Phú Quốc (11686) | Người lớn-Ngày thường | 420.000 | 5 | 2.100.000 | 2.070.000 |
| 2025-09-04 | Thủy cung Lotte HN (11810) | Người lớn-Ngày thường | 195.000 | 5 | 975.000 | 956.250 |
| 2025-09-05 | VinKE & Aquarium Times City (11810) | Người lớn-Ngày thường | 177.200 | 4 | 708.800 | 695.000 |

## Dữ liệu mẫu — Tab KVC công nợ

| Ngày | KVC | Loại vé | Đơn giá | SL | Tiền bán | Tiền vốn |
|------|-----|---------|---------|-----|---------|---------|
| 2025-09-16 | Sơn Tiên (10360) | Người lớn-Ngày thường | 85.000 | 5 | 425.000 | 415.000 |
| 2025-09-16 | Mikazuki (11423) | Người lớn-Cuối tuần | 120.000 | 4 | 480.000 | 468.000 |
| 2025-09-16 | Mekong (11588) | Người lớn-Ngày thường | 95.000 | 3 | 285.000 | 278.000 |
| 2025-09-17 | Hồ Tràm (11483) | Người lớn-Cuối tuần | 150.000 | 4 | 600.000 | 585.000 |
| 2025-09-17 | Nova Phan Thiết (11480) | Người lớn-Ngày thường | 180.000 | 5 | 900.000 | 880.000 |
| 2025-09-18 | Sealinks (11807) | Người lớn-Ngày thường | 110.000 | 3 | 330.000 | 322.000 |

---

## Cơ chế cập nhật dữ liệu

```
23h59 hàng ngày
  └─ Job tự động gọi API
       └─ Lấy toàn bộ đơn đặt vé trong ngày
            ├─ Phân loại vào Tab KVC nạp tiền
            └─ Phân loại vào Tab KVC công nợ
```

- Dữ liệu hôm nay xuất hiện sớm nhất vào 23h59 cùng ngày
- Nếu job thất bại, ngày đó sẽ không có dữ liệu (cần kiểm tra log)
- Dữ liệu lịch sử giữ nguyên, không bị ghi đè

---

## Cấu trúc đại lý trong hệ thống

| Vai trò | Mô tả | Ví dụ |
|---------|-------|-------|
| Mã ĐL mua cấp trên | Đại lý gốc của ezCloud mua từ KVC | `5129` – Oneinventory_Q R_ezCloud Mua_PL |
| Mã ĐL mua | Đại lý trung gian phân phối | `7034` – Oneinventory_SJC Thủy cung Lotte |
| Mã ĐL bán | Đơn vị thực tế bán vé cho khách | `7310` – VD: Thủy cung VINKE-EZCMT |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| `doi-soat-kvc.html` | Đối soát tổng hợp — dùng Tiền vốn tại đây để tính toán |
| `so-du-kvc.html` | Số dư KVC — đối chiếu doanh số bán với số tiền đã nạp/tín dụng |
| `nap-tien-kvc.html` | Lịch sử nạp tiền — đảm bảo số dư đủ để bán vé trong ngày |
| `kvc-hoan-tien.html` | Trường hợp đơn đặt vé cần hoàn tiền sau khi đã ghi nhận giá vốn |

---

## Ghi chú thiết kế

- **Tiền vốn < Tiền bán** ở mọi dòng — phần chênh lệch là doanh thu của ezCloud
- Cột **Tạm tính** bằng Tiền bán do chưa điều chỉnh chiết khấu cuối kỳ — giá trị chính thức sau đối soát có thể khác
- **Mã ĐL mua cấp trên = 5129** xuất hiện trong tất cả bản ghi — đây là account gốc của ezCloud trên hệ thống Oneinventory
- Loại vé phân biệt **Ngày thường / Cuối tuần T7-CN** và **Người lớn / Trẻ em** — cùng KVC nhưng đơn giá khác nhau
- Một đơn đặt vé có thể gồm nhiều loại vé khác nhau, mỗi loại là một dòng riêng trong bảng
