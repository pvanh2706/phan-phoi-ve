# Use Case: Giao dịch đại lý nạp tiền trên BIDV

**Màn hình:** `AgencyReportView.vue` (pageKey: `agencyBidvTransactions`)
**Đường dẫn truy cập:** Sidebar → Đại lý → Giao dịch đại lý nạp tiền trên BIDV (`/dai-ly/giao-dich-bidv`)
**Quyền truy cập:** Admin, Member (xem)
**Nguồn dữ liệu:** ⚠️ **Demo/mock** — dữ liệu tĩnh trong `data/reports.ts` (`reportPages.agencyBidvTransactions`). Không có nút "Gọi API test" (khác với AR/TA).

---

## Mô tả

Sao kê giao dịch **nạp tiền của đại lý** vào tài khoản BIDV của ezCloud, mô phỏng dữ liệu lấy tự động từ **email báo có** (theo mô hình đã áp dụng thật cho luồng "Danh sách nạp tiền KVC theo ngày"). Mỗi dòng là 1 giao dịch ngân hàng với đầy đủ thông tin sao kê: ngày ghi sổ, ngày hạch toán, mã giao dịch, số tiền, số chứng từ, diễn giải.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-GDBIDV-01 – Xem sao kê giao dịch BIDV

- **Điều kiện:** Người dùng mở màn hình
- **Luồng chính:** Bảng hiển thị các dòng sao kê ngân hàng dạng đầy đủ (giống format sao kê gốc)
- **Kết quả:** Danh sách giao dịch nạp tiền của đại lý qua BIDV

### UC-GDBIDV-02 – Tìm kiếm theo số chứng từ / nội dung

- **Điều kiện:** Người dùng đang ở màn hình
- **Luồng chính:** Nhập từ khoá vào ô "🔍 Tìm số chứng từ, nội dung..." → lọc real-time
- **Kết quả:** Danh sách thu hẹp theo từ khoá

### UC-GDBIDV-03 – Lọc theo khoảng ngày

- **Điều kiện:** Người dùng cần giới hạn khoảng thời gian
- **Luồng chính:** Chọn Từ ngày / Đến ngày
- **Kết quả:** Danh sách lọc theo ngày giao dịch

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| From mail | Địa chỉ email nguồn sao kê (VD: `insaoke@bidv.com`) |
| Ngày ghi sổ | Ngày ngân hàng ghi nhận |
| Ngày hạch toán | Ngày hạch toán kế toán |
| Mã giao dịch | Loại bút toán (VD: `DD`) |
| Phát sinh nợ | Số tiền ghi nợ (căn phải) |
| Phát sinh có | Số tiền ghi có — tô xanh (căn phải) |
| Số dư | Số dư tài khoản sau giao dịch (căn phải) |
| Số chứng từ | Mã chứng từ ngân hàng |
| Diễn giải | Nội dung chuyển khoản gốc |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Danh sách nạp tiền KVC theo ngày | Cùng cơ chế lấy sao kê từ email — đã có API thật (`syncBankTransactions`); màn BIDV đại lý này nên tái sử dụng cùng pipeline khi lên backend |
| Danh sách các đại lý | Tên đại lý trong nội dung sao kê dùng để đối chiếu với danh mục đại lý |

---

## Ghi chú thiết kế

- Cấu trúc cột giống hệt sao kê ngân hàng dùng ở "Đại lý nạp tiền trên BIDV" của Khách lẻ/OTA — nên cân nhắc dùng chung 1 component sao kê ngân hàng khi code hoá thật
- Chưa có nút đồng bộ/gọi API — khi có backend cần bổ sung nút "Lấy sao kê" tương tự màn Nạp tiền KVC
