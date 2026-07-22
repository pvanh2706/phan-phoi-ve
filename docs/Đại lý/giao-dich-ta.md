# Use Case: Giao dịch của các đại lý trên TA

**Màn hình:** `AgencyTaTransactionsView.vue`
**Đường dẫn truy cập:** Sidebar → Đại lý → Giao dịch của các đại lý trên TA (`/dai-ly/giao-dich-ta`)
**Quyền truy cập:** Admin, Member, Accountant (xem); chỉ Admin/Kế toán được chạy đồng bộ tay.
**Nguồn dữ liệu:** ✅ **Thật** — backend tự đồng bộ từ OneInventory (`rp_booking_list`) và lưu vào bảng `AgencyBookings`; giao diện đọc qua API `GET /api/agency-ta-transactions`.

---

## Mô tả

Danh sách giao dịch (booking) của các đại lý ghi nhận trên hệ thống **TA** (OneInventory). Mỗi dòng gồm mã
booking, tên/mã đại lý, ngày tạo và số tiền. Dữ liệu được lấy tự động hằng ngày, lưu vào database và liên kết
với **Danh sách các đại lý** qua mã đại lý mua. Dùng để đối chiếu chéo với dữ liệu trên AR nhằm phát hiện
booking bị thiếu ở một trong hai hệ thống.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, **chạy đồng bộ tay** |
| Accountant | Xem, tìm kiếm, lọc theo ngày, **chạy đồng bộ tay** |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-GDTA-01 – Xem danh sách giao dịch TA
- Bảng hiển thị 100 dòng/trang từ database, sắp xếp theo ngày đặt giảm dần, kèm tổng số tiền của tập đã lọc.

### UC-GDTA-02 – Tìm kiếm theo mã booking / tên/mã đại lý
- Nhập từ khoá → backend lọc theo `BookingCode`, `BuyingAgentCode`, `BuyingAgentName`.

### UC-GDTA-03 – Lọc theo khoảng ngày
- Chọn Từ ngày / Đến ngày → lọc theo `BookingDate` (ngày đặt).

### UC-GDTA-04 – Chạy đồng bộ tay (Admin/Kế toán)
- Nhấn **"Lấy dữ liệu"** → chọn ngày → backend gọi OneInventory cho ngày đó, lọc + upsert vào DB →
  toast báo số bản ghi thêm mới/cập nhật/không đổi và số dòng chưa map được đại lý.

---

## Cấu trúc bảng (giao diện)

| Cột | Nguồn (OneInventory) |
|-----|------|
| Mã booking | `MaDatVe` |
| Tên đại lý | `TenDaiLyMua` |
| Mã đại lý | `MaDaiLyMua` |
| Ngày tạo | `NgayDat` |
| Số tiền (đỏ) | `TienBan` |
| Trạng thái | Đã/Chưa map được đại lý nội bộ |

Ngoài các cột hiển thị, bảng `AgencyBookings` còn lưu đầy đủ trường nguồn để phục vụ đối soát: mã/tên đại lý
cấp trên, mã/tên đại lý bán, mã/tên khu vui chơi, mã/tên/nhóm loại vé, số lượng, đơn giá, tiền vốn, tạm tính,
giảm giá, ID giao dịch nguồn, thời điểm đồng bộ.

---

## Luồng đồng bộ (backend)

1. **Đăng nhập** OneInventory (`/api/v1/admin/user/login`) lấy token — tái dùng `OneInventoryBookingApiClient`.
2. **Lấy booking** (`/api/v1/admin/procedure?function=rp_booking_list&startDate=…&endDate=…`) cho đúng 1 ngày.
3. **Lọc §7:** chỉ giữ dòng có `MaDaiLyMuaCapTren = ParentAgencyCode` (cấu hình, mặc định `5129`).
4. **Map đại lý §9:** `MaDaiLyMua` → `Agency.Code` → `BuyingAgentId`. Không thấy thì để `NULL`, đánh dấu
   `IsAgencyMatched = false`, ghi nhận mã chưa map (không làm hỏng job).
5. **Chống trùng §10:** upsert theo `(SourceSystem = "OneInventory", SourceTransactionId = ID)` — chạy lại
   cùng ngày sẽ cập nhật/bỏ qua, không tạo bản ghi trùng (có unique index bảo đảm).
6. Mỗi lần chạy được ghi `JobRun`/`JobRunItem`/`ExternalApiRawResponse` (không log token/mật khẩu).

### Lịch chạy
- **Tự động:** job `SyncAgencyBookings` chạy **23:59 hằng ngày**, lấy dữ liệu **chính ngày đó** (phương án đã chốt).
- **Chạy tay theo ngày:** nút "Lấy dữ liệu" (`POST /api/agency-ta-transactions/sync`) hoặc
  `POST /api/jobs/sync-agency-bookings/run` (có ghi JobRun) — chỉ Admin/Kế toán.
- Nếu 23:59 sót booking phát sinh muộn hoặc OneInventory trả trễ, dùng chạy tay lại đúng ngày để bù/đối soát.

---

## Cấu hình (không hard-code)

Mặc định ở `appsettings.json`, có thể override trong DB qua màn **Cấu hình kết nối** (prefix `Conn.`), áp dụng ngay:

| Khóa | Mặc định | Ý nghĩa |
|------|----------|---------|
| `ExternalApis:OneInventory:BaseUrl` | `https://admin.oneinventory.com` | URL OneInventory |
| `ExternalApis:OneInventory:Username` / `Password` | (secret) | Tài khoản BI — **không commit secret thật** |
| `ExternalApis:OneInventory:ParentAgencyCode` | `5129` | Mã đại lý cấp trên để lọc (§7) |
| `Jobs:ScheduleTimes:AgencyBooking` | `23:59` | Giờ chạy job tự động |

> Lưu ý bảo mật: username/password/token OneInventory không được ghi vào source code (nên dùng biến môi
> trường/secret khi triển khai), log, tài liệu Git hay response trả về frontend.

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên AR | Nguồn đối chiếu chéo theo mã booking |
| Đối soát AR - TA / Đối soát TA - AR | Hiện vẫn dùng dữ liệu demo (`reportPages.agencyTaTransactions`); sẽ chuyển sang dùng `AgencyBookings` ở bước sau |

---

## Ghi chú thiết kế

- Tiền lưu kiểu `long` (VND), không dùng float/double; các mã định danh lưu chuỗi (§6).
- Bảng `AgencyBookings` giữ đủ dữ liệu nguồn để hạn chế phải gọi lại API khi giao diện cần thêm cột.
- Test: `backend/tests/PpvRecon.Tests/AgencyBookingSyncServiceTests.cs` phủ đăng nhập/booking thành công,
  thất bại, rỗng, sai định dạng, đúng/sai mã cấp trên, trùng, không/không tìm thấy đại lý, chạy lại cùng ngày.
