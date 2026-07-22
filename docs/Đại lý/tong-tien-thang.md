# Use Case: Tổng tiền các đại lý đã dùng theo tháng

**Màn hình:** `AgencyMonthlyUsageView.vue`
**Đường dẫn truy cập:** Sidebar → Đại lý → Tổng tiền các đại lý đã dùng theo tháng (`/dai-ly/tong-tien-thang`) — mục cuối cùng trong menu Đại lý, ngay dưới "Giao dịch đại lý nạp tiền trên BIDV"
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock, nhưng tính toán tự động (computed)** — không phải bảng tĩnh. Lấy trực tiếp toàn bộ dòng `reportPages.agencyArTransactions.tabs[0].rows` trong `data/reports.ts` và cộng dồn theo (Đại lý × Tháng) mỗi khi trang render.

---

## Mô tả

Tổng hợp **toàn bộ booking bị trừ tiền trên hệ thống AR**, cộng dồn số tiền đã dùng theo từng **đại lý** trong từng **tháng**. Vì phép tính là `computed()` chạy tự động trên dữ liệu AR hiện có, mỗi khi có giao dịch AR mới phát sinh trong ngày, tổng của tháng tương ứng sẽ tự cập nhật theo — không cần bấm nút "Build" thủ công như một số trang đối soát khác trong hệ thống.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc theo tháng/đại lý, tìm kiếm |
| Member | Xem, lọc theo tháng/đại lý, tìm kiếm |

---

## Use Cases

### UC-TTT-01 – Xem tổng tiền theo tháng

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:**
  1. Hệ thống duyệt toàn bộ dòng AR (`agencyArTransactions`)
  2. Với mỗi dòng, parse tháng từ cột "Ngày giờ giao dịch" (định dạng `dd/MM/yyyy HH:mm:ss` → khoá `yyyy-MM`)
  3. Cộng dồn số tiền và đếm số booking theo khoá (Đại lý, Tháng)
  4. Sắp xếp: tháng mới nhất trước, cùng tháng thì tổng tiền cao hơn lên trước
- **Kết quả:** Bảng tổng hợp mỗi dòng là 1 (đại lý, tháng), kèm dòng **Tổng cộng** ở cuối bảng

### UC-TTT-02 – Lọc theo tháng

- **Điều kiện:** Người dùng muốn xem riêng 1 tháng
- **Luồng chính:** Chọn tháng trong dropdown **"Tất cả các tháng"** (danh sách tháng tự sinh từ dữ liệu thực tế, không cố định)
- **Kết quả:** Bảng chỉ hiển thị các dòng thuộc tháng đã chọn

### UC-TTT-03 – Lọc theo đại lý

- **Điều kiện:** Người dùng muốn xem riêng 1 đại lý
- **Luồng chính:** Chọn tên đại lý trong dropdown **"Tất cả đại lý"** (danh sách tự sinh từ dữ liệu, không dùng danh mục 114 đại lý ở "Danh sách các đại lý")
- **Kết quả:** Bảng chỉ hiển thị các dòng thuộc đại lý đã chọn

### UC-TTT-04 – Tìm kiếm theo tên đại lý

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm tên đại lý..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Tháng | Hiển thị dạng "Tháng MM/YYYY" |
| Đại lý | Tên đại lý, in đậm |
| Số booking | Tổng số dòng AR được cộng dồn vào (căn phải) |
| Tổng tiền đã dùng (trừ trên AR) | Tổng số tiền, tô đỏ (căn phải) |

Dòng **Tổng cộng** ở `<tfoot>` cộng tổng số booking và tổng tiền của toàn bộ dòng đang hiển thị (sau khi áp dụng bộ lọc).

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên AR | Nguồn dữ liệu duy nhất — mỗi dòng AR là 1 booking được cộng vào tổng tháng tương ứng |
| Danh sách các đại lý | Danh mục đầy đủ 114 đại lý; màn này chỉ hiện các đại lý thực sự có giao dịch AR trong dữ liệu mẫu |

---

## Ghi chú thiết kế

- Vì dữ liệu mẫu `agencyArTransactions` hiện tại chỉ có 23 dòng, toàn bộ đều thuộc cùng 1 ngày (`2026-04-28`), nên hiện chỉ có **1 tháng duy nhất** (Tháng 04/2026) xuất hiện trong bộ lọc — khi dữ liệu AR trải dài nhiều tháng hơn, dropdown tháng sẽ tự động có thêm lựa chọn mà không cần sửa code
- Không có nút "Build" thủ công — đúng tinh thần yêu cầu "tự động cộng tổng hàng ngày": chỉ cần dữ liệu AR có thêm dòng mới, `computed()` sẽ tự tính lại ngay khi trang render, không cần thao tác thêm
- Khi có API AR thật, thay `reportPages.agencyArTransactions.tabs[0].rows` bằng dữ liệu tổng hợp từ backend (nên cân nhắc tính sẵn ở backend theo job hằng ngày thay vì tính lại trên toàn bộ transaction mỗi lần load trang, để tránh chậm khi số lượng booking lớn)
