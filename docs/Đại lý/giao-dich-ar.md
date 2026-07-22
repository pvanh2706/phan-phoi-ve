# Use Case: Giao dịch của các đại lý trên AR

**Màn hình:** `AgencyArTransactionsView.vue`
**Đường dẫn truy cập:** Sidebar → Đại lý → Giao dịch của các đại lý trên AR (`/dai-ly/giao-dich-ar`)
**Quyền truy cập:** Admin, Member, Accountant (xem); chỉ Admin/Kế toán được chạy đồng bộ tay.
**Nguồn dữ liệu:** ✅ **Thật** — backend tự đăng nhập AR (ar.ezcloud.vn), tải file Excel giao dịch, lọc và lưu vào bảng `AgencyArTransactions`; giao diện đọc qua API `GET /api/agency-ar-transactions`.

---

## Mô tả

Danh sách giao dịch **trừ tiền của các đại lý** trên hệ thống AR — chỉ các giao dịch **"Thanh toán tiền cho
booking"**. Mỗi dòng gồm mã booking, tên/mã đại lý, thời điểm giao dịch, số tiền (dương) và mã tài khoản công
nợ. Dữ liệu lấy tự động hằng ngày từ file Excel do AR trả về (base64), xử lý ở backend rồi lưu DB.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Xem, tìm kiếm, lọc theo ngày, **chạy đồng bộ tay** |
| Accountant | Xem, tìm kiếm, lọc theo ngày, **chạy đồng bộ tay** |
| Member | Xem, tìm kiếm, lọc theo ngày |

---

## Use Cases

### UC-GDAR-01 – Xem danh sách giao dịch AR
- Bảng 100 dòng/trang từ DB, sắp xếp theo ngày giao dịch giảm dần, kèm tổng số tiền của tập đã lọc.

### UC-GDAR-02 – Tìm kiếm theo mã booking / tên/mã đại lý
- Backend lọc theo `BookingId`, `TravelAgentCode`, `TravelAgentName`.

### UC-GDAR-03 – Lọc theo khoảng ngày
- Chọn Từ ngày / Đến ngày → lọc theo `BusinessDate` (ngày giao dịch).

### UC-GDAR-04 – Chạy đồng bộ tay (Admin/Kế toán)
- Nhấn **"Lấy dữ liệu"** → chọn ngày → backend đăng nhập AR, tải Excel ngày đó, xử lý + upsert →
  toast báo số bản ghi thêm mới/cập nhật/không đổi và số dòng lỗi.

---

## Cấu trúc bảng (giao diện §14)

| Cột giao diện | Trường API | Nguồn Excel |
|-----|-----|-----|
| Mã booking | `bookingId` | trích từ mô tả `__EMPTY_7` |
| Tên đại lý | `travelAgentName` | `__EMPTY_1` (cột B) |
| Ngày tạo giờ | `transactionDate` (ISO +07:00) | `__EMPTY_3` (cột D) |
| Số tiền | `amount` (dương) | `\|__EMPTY_4\|` (cột E) |
| Mã đại lý | `travelAgentCode` | `__EMPTY` (cột A) |
| Mã TK công nợ | `receivableAccountCode` | `__EMPTY_2` (cột C) |

> Mapping cột đọc theo **chỉ số cột** (A=0, B=1, C=2, D=3, E=4, H=7 — tên `__EMPTY_N` của n8n = chỉ số cột
> 0-based; cột G/`__EMPTY_6` có header, không dùng). Nội dung gốc lưu ở `Description`.

---

## Luồng đồng bộ (backend)

1. **Đăng nhập** AR: `POST /api/account-login/Process` → kiểm `Status="200"` + lấy `Data.Token`.
2. **Lấy dữ liệu**: `POST /api/AR_TATR01/Process` với `FromDate/ToDate` = 00:00:00–23:59:59 của ngày cần
   đồng bộ (giờ VN), `Lang` từ cấu hình, `Token` trong body → file Excel base64 (`Data.FileContents`).
3. **Đọc Excel** từ memory stream; **bỏ 3 dòng đầu** (tiêu đề/báo cáo §6).
4. **Lọc §7**: chỉ giữ dòng `__EMPTY_7` khớp `^Thanh\s*toán\s*tiền\s*cho\s*booking` (không phân biệt hoa
   thường, cho phép nhiều khoảng trắng).
5. **Trích bookingId §8**: dãy số sau `booking`; không trích được → ghi log cảnh báo (kèm số dòng) và bỏ qua.
6. **Chuẩn hóa**: ngày (§10) hỗ trợ serial Excel / `DD/MM/YYYY[ HH:mm:ss]` / ISO, ưu tiên DD/MM, không đảo
   ngày-tháng, không lệch ±7h; tiền (§11) lấy trị tuyệt đối, kiểu `long`, không tự làm tròn.
7. **Chống trùng §13**: `DedupHash = SHA-256(BookingId|TransactionDate|Amount|TravelAgentCode|ReceivableAccountCode)`
   + unique index. Chạy lại: chưa có→insert, đổi tên ĐL/mô tả→update, giống hệt→bỏ qua.
8. Mỗi lần chạy ghi `JobRun`/`JobRunItem`/`ExternalApiRawResponse` + log tổng hợp (§16). Không log token/mật
   khẩu/base64.

### Lịch chạy
- **Tự động:** job `SyncArTransactions` chạy **23:59 hằng ngày**, lấy dữ liệu **chính ngày đó** (§2).
- **Chạy tay theo ngày:** nút "Lấy dữ liệu" (`POST /api/agency-ar-transactions/sync`) hoặc
  `POST /api/jobs/sync-ar-transactions/run` (ghi JobRun) — chỉ Admin/Kế toán (§15).
- Các lần đồng bộ cùng một ngày được **khóa tuần tự** trong tiến trình để không chạy chồng gây trùng (§15).

### Retry (§16)
Login/lấy dữ liệu có retry cho lỗi tạm thời (timeout, lỗi mạng, HTTP 5xx) theo `RetryCount`/`RetryDelaySeconds`;
không retry với lỗi xác thực/4xx.

---

## Cấu hình (không hard-code §2/§17)

Mặc định ở `appsettings.json` (`ExternalApis:Ar`), override qua DB (`Conn.Ar.*`) hoặc biến môi trường
(`ExternalApis__Ar__Username`, …). **Không đưa vào DTO lưu chung của màn Cấu hình kết nối** để tránh bị ghi đè
nhầm; sửa qua appsettings/biến môi trường/DB.

| Khóa | Mặc định | Ý nghĩa |
|------|----------|---------|
| `ExternalApis:Ar:LoginUrl` | `https://ar.ezcloud.vn/api/account-login/Process` | URL đăng nhập |
| `ExternalApis:Ar:DataUrl` | `https://ar.ezcloud.vn/api/AR_TATR01/Process` | URL lấy dữ liệu |
| `ExternalApis:Ar:Username` / `Password` | (trống) | Tài khoản AR — **đặt qua env/secret, không commit** |
| `ExternalApis:Ar:Lang` | `1000000` | Tham số Lang |
| `ExternalApis:Ar:TimeZone` | `Asia/Ho_Chi_Minh` | Múi giờ tạo FromDate/ToDate |
| `ExternalApis:Ar:TimeoutSeconds` | `60` | Timeout mỗi request |
| `ExternalApis:Ar:RetryCount` / `RetryDelaySeconds` | `2` / `5` | Retry lỗi tạm thời |
| `Jobs:ScheduleTimes:ArTransaction` | `23:59` | Giờ chạy job tự động |

---

## Kết nối với màn hình khác

| Màn hình | Liên kết |
|---------|---------|
| Giao dịch của các đại lý trên TA | Đối chiếu chéo theo mã booking (AR vs TA) |
| Đối soát AR - TA / Đối soát TA - AR | Hiện vẫn dùng dữ liệu demo (`reportPages.agencyArTransactions`); sẽ chuyển sang dùng `AgencyArTransactions` ở bước sau |

---

## Ghi chú thiết kế

- `TransactionDate` lưu **giờ tường VN** dạng `DateTime` (không quy đổi UTC để tránh lệch ±7h §10); API gắn
  offset `+07:00` khi trả về. Dùng `DateTime` thay vì `DateTimeOffset` vì SQLite không hỗ trợ `ORDER BY` trên
  `DateTimeOffset`.
- Tiền `long` (VND), không float/double.
- Đọc Excel bằng **ClosedXML** (MIT).
- Test: `backend/tests/PpvRecon.Tests/ArTransactionSyncServiceTests.cs` phủ 18 tình huống §18 (đăng nhập/lỗi,
  bỏ 3 dòng đầu, lọc/không phân biệt hoa thường/nhiều khoảng trắng, trích bookingId, mapping cột, tiền âm→dương,
  serial date, DD/MM HH:mm:ss, không đảo ngày-tháng, không lệch múi giờ, chạy lại không trùng, một dòng lỗi
  không dừng cả file, sắp xếp danh sách).
- ⚠️ Nếu cấu trúc Excel thực tế khác mapping trên (theo §19), cần dừng và đối chiếu lại các cột `__EMPTY_*`
  trước khi tin dữ liệu.
