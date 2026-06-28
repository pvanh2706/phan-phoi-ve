# Danh mục bảng dữ liệu - PpvRecon

Tài liệu liệt kê toàn bộ các bảng trong database và **tác dụng chính** của từng
bảng. Tài liệu chỉ mô tả ở mức bảng, **không đi vào chi tiết từng cột** — xem
`docs/database-design.md` nếu cần chi tiết cột, khóa, quan hệ.

- Loại DB: SQLite (EF Core code-first + migrations).
- Tổng số bảng nghiệp vụ: **21** (chưa tính bảng hệ thống của EF Core).
- Nguồn chuẩn: `PpvReconDbContext` trong `PpvRecon.Infrastructure`.

---

## 1. Tài khoản & phiên đăng nhập (Identity)

| Bảng | Tác dụng chính |
|------|----------------|
| `Users` | Tài khoản người dùng hệ thống (Admin / Member / Accountant): thông tin đăng nhập, vai trò, trạng thái, khóa tài khoản, soft-delete. |
| `UserSessions` | Phiên đăng nhập / refresh token theo từng thiết bị. Phục vụ màn "Quản lý thiết bị", thu hồi phiên, đăng xuất từ xa. |
| `UserPreferences` | Tùy chọn cá nhân của từng user (giao diện sáng/tối, ngôn ngữ). |
| `NotificationPreferences` | Cấu hình bật/tắt từng loại thông báo theo từng user. |

## 2. Cấu hình hệ thống (Settings)

| Bảng | Tác dụng chính |
|------|----------------|
| `SystemSettings` | Cấu hình toàn hệ thống dạng key-value (giờ chạy job, số ngày giữ audit log, timeout/retry khi gọi API, bật email tổng hợp lỗi…). |
| `NotificationRecipients` | Danh sách email nhận thông báo theo loại (tổng hợp lỗi đồng bộ, báo cáo ngày, cảnh báo lệch đối soát). |

## 3. Khu vui chơi - dữ liệu gốc (Master data)

| Bảng | Tác dụng chính |
|------|----------------|
| `Parks` | Danh mục khu vui chơi (KVC): mã, tên, loại thanh toán (nạp tiền/công nợ), tài khoản ngân hàng, cấu hình API. |
| `ParkTicketTypes` | Danh mục loại vé của từng KVC. |
| `ParkRefunds` | Bản ghi hoàn tiền của KVC theo từng booking. |

## 4. Job & dữ liệu API thô (Jobs)

| Bảng | Tác dụng chính |
|------|----------------|
| `JobRuns` | Mỗi lần chạy job (đồng bộ hằng ngày, gửi email lỗi, dọn audit log…): trạng thái, thời gian bắt đầu/kết thúc, số liệu tổng hợp. |
| `JobRunItems` | Từng đầu việc con trong một job run (theo KVC/nguồn dữ liệu): trạng thái thành công/lỗi, số lần retry, ghi chú xử lý tay. |
| `ExternalApiRawResponses` | Log thô mỗi lần gọi API ngoài (URL, payload gửi, HTTP status, response body, lỗi, thời gian gọi). Phục vụ tra cứu và đối chiếu khi cần. |

## 5. Dữ liệu tổng hợp theo ngày (Summaries)

| Bảng | Tác dụng chính |
|------|----------------|
| `DailyParkBalanceSnapshots` | Số dư KVC được chốt theo từng ngày (lấy từ API hoặc nhập tay). |
| `DailyTicketCostSummaries` | Tổng hợp giá vốn vé bán theo ngày và theo KVC. |
| `TicketSaleCostDetails` | Chi tiết từng dòng vé bán kèm giá vốn (theo booking). |
| `BankTransactionDetails` | Chi tiết giao dịch ngân hàng (nạp tiền cho KVC / thanh toán công nợ). |
| `DailyBankTransactionSummaries` | Tổng hợp giao dịch ngân hàng theo ngày, KVC và loại giao dịch. |

## 6. Đối soát (Reconciliation)

| Bảng | Tác dụng chính |
|------|----------------|
| `ParkReconciliations` | Kết quả đối soát số dư theo ngày/KVC: số dư trước, nạp thêm, đã dùng, số kỳ vọng, số thực tế, chênh lệch và trạng thái khớp/lệch. |

## 7. Nhật ký (Audit)

| Bảng | Tác dụng chính |
|------|----------------|
| `AuditLogs` | Nhật ký thao tác (before/after) của người dùng và job: ai, làm gì, lúc nào, IP, user agent. |

## 8. Quy trình nạp tiền KVC - Kanban (Workflow)

| Bảng | Tác dụng chính |
|------|----------------|
| `WorkflowColumns` | Các cột/bước trong bảng quy trình nạp tiền KVC, kèm cấu hình trường hiển thị trên thẻ và phân quyền được chuyển task. |
| `WorkflowTasks` | Các nhiệm vụ nạp tiền / thanh toán công nợ, di chuyển qua các bước trong bảng quy trình. |

---

## Bảng hệ thống (EF Core)

| Bảng | Tác dụng chính |
|------|----------------|
| `__EFMigrationsHistory` | Do EF Core tự quản lý, ghi lại các migration đã áp dụng vào DB. Không phải dữ liệu nghiệp vụ. |
| `__EFMigrationsLock` | Khóa tạm thời do EF Core dùng để tránh chạy migration đồng thời. Không phải dữ liệu nghiệp vụ. |
