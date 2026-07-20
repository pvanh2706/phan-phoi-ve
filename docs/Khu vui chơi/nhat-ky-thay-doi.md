# Use Case: Nhật ký thay đổi

**Màn hình:** `AuditLogView.vue`
**Đường dẫn truy cập:** Sidebar → Khu vui chơi → Nhật ký thay đổi (`/khu-vui-choi/nhat-ky`)
**Quyền truy cập:** Admin, Member (xem) — chỉ đọc, không có thao tác chỉnh sửa
**Nguồn dữ liệu:** API thật — `GET /api/audit-logs` (before/after JSON ghi tự động khi có thay đổi)

---

## Mô tả

Màn hình tra cứu nhật ký **before/after** cho toàn bộ thao tác thay đổi dữ liệu trong nghiệp vụ Khu vui chơi và các job đồng bộ liên quan. Mỗi dòng ghi lại: ai thao tác, vào thời điểm nào, trên module/đối tượng nào, hành động gì, cùng trạng thái dữ liệu **trước** và **sau** thay đổi (dạng JSON rút gọn).

Đây là công cụ audit/truy vết — phục vụ điều tra khi có sai lệch số liệu hoặc cần biết ai đã sửa gì.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, lọc |
| Member | Xem, lọc |

---

## Use Cases

### UC-NK-01 – Xem nhật ký mặc định

- **Điều kiện:** Người dùng mở màn hình Nhật ký thay đổi
- **Luồng chính:**
  1. Hệ thống gọi API với bộ lọc mặc định `module = Park`
  2. Danh sách hiển thị tối đa 100 dòng/trang, mới nhất trước
- **Kết quả:** Người dùng thấy các thay đổi gần đây nhất trong nghiệp vụ Khu vui chơi

### UC-NK-02 – Lọc theo module, đối tượng, hành động, khoảng ngày

- **Điều kiện:** Người dùng đang ở màn hình Nhật ký thay đổi
- **Luồng chính:**
  1. Chọn khoảng ngày (Từ ngày / Đến ngày)
  2. Chọn **Module**: Khu vui chơi / Jobs / Cài đặt / Đăng nhập
  3. Chọn **Đối tượng**: KVC, Loại vé, Số dư, Giá vốn, Giao dịch ngân hàng, Đối soát, Job (tuỳ theo module)
  4. Chọn **Hành động**: Tạo mới, Cập nhật, Ngừng sử dụng, Xóa mềm, Chạy job, Nhập tay, Xử lý lệch
  5. Nhấn **Lọc**
- **Kết quả:** Danh sách chỉ hiển thị bản ghi khớp điều kiện lọc

### UC-NK-03 – Xóa bộ lọc

- **Điều kiện:** Đang có bộ lọc áp dụng
- **Luồng chính:** Nhấn **Xóa lọc** → các ô lọc reset về mặc định (`module = Park`, các ô khác rỗng) và tải lại trang 1
- **Kết quả:** Danh sách trở về trạng thái mặc định

### UC-NK-04 – Xem chi tiết trước/sau

- **Điều kiện:** Dòng nhật ký có dữ liệu `beforeJson`/`afterJson`
- **Luồng chính:** Cột **Trước** và **Sau** hiển thị JSON rút gọn (compact) ngay trong bảng, không cần mở modal
- **Kết quả:** Người dùng đối chiếu nhanh giá trị cũ/mới của bản ghi bị thay đổi

### UC-NK-05 – Phân trang

- **Điều kiện:** Tổng số dòng > 100
- **Luồng chính:** Nhấn `‹`/`›` để chuyển trang, mỗi trang tải lại từ API
- **Kết quả:** Xem hết toàn bộ nhật ký theo trang

---

## Cấu trúc bảng

| Cột | Mô tả |
|-----|-------|
| Thời điểm | `occurredAtUtc`, định dạng ngày giờ đầy đủ |
| Người thao tác | Email người dùng (`userEmailSnapshot`), hoặc "Hệ thống" nếu do job tự động |
| Module | Park / Jobs / Settings / Auth |
| Đối tượng | Tên entity + mã ID, in đậm (VD: `Park #12`) |
| Hành động | Badge indigo — nhãn theo `auditActionLabel` |
| Trước | JSON rút gọn trạng thái trước thay đổi, cắt bớt nếu quá dài (max-width 280px) |
| Sau | JSON rút gọn trạng thái sau thay đổi |

## Danh sách giá trị lọc

| Bộ lọc | Giá trị |
|--------|---------|
| Module | Khu vui chơi (`Park`), Jobs, Cài đặt (`Settings`), Đăng nhập (`Auth`) |
| Đối tượng | Tất cả, KVC (`Park`), Loại vé (`ParkTicketType`), Số dư (`DailyParkBalanceSnapshot`), Giá vốn (`DailyTicketCostSummary`), Giao dịch ngân hàng (`DailyBankTransactionSummary`), Đối soát (`ParkReconciliation`), Job (`JobRun`) |
| Hành động | Tất cả, Tạo mới (`Create`), Cập nhật (`Update`), Ngừng sử dụng (`SetInactive`), Xóa mềm (`SoftDelete`), Chạy job (`RunJob`), Nhập tay (`ManualEntry`), Xử lý lệch (`ResolveVariance`) |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Danh sách khu vui chơi, Số dư hằng ngày, Đối soát Khu vui chơi | Các thao tác Create/Update/ResolveVariance trên các màn này sinh ra bản ghi nhật ký tương ứng |
| Lỗi đồng bộ cần xử lý | Thao tác "Nhập tay" và "Chạy job" sinh nhật ký `ManualEntry` / `RunJob` |

---

## Ghi chú thiết kế

- Đây là màn hình **chỉ đọc**, không có nút thêm/sửa/xóa
- Dữ liệu do backend tự động ghi khi API tương ứng xử lý xong — không có luồng nhập tay nhật ký
