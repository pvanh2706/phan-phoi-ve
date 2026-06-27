# Database Design - PpvRecon

Tai lieu nay thiet ke database v1 cho du an **Doi soat phan phoi ve**
(`PpvRecon`). Tai lieu nay dua tren ket qua trao doi da luu trong
`docs/backend-design.md` va dung lam blueprint truoc khi scaffold backend .NET.

Ngay tong hop: 2026-06-27 theo gio Asia/Bangkok.

Muc tieu cua tai lieu:

- Chot nhom bang can co trong v1.
- Chot quan he, khoa chinh, unique index va soft delete.
- Chot enum/states de code va DB dung on dinh bang tieng Anh.
- Chot cach luu raw API response, summary theo ngay va doi soat.
- Giam rui ro vua code vua nghi DB dan den sai khoa hoac thieu audit.

---

## 1. Nguyen Tac Thiet Ke

### 1.1. Cach tiep can

Dung **EF Core code-first + migrations** voi SQLite.

DB design nay khong co nghia la khoa chet moi thu tu dau. Muc tieu la chot
"xuong song" v1:

- Identity/auth/session
- Settings/recipients
- Khu vui choi master data
- Job/raw response
- Daily summaries
- Reconciliation
- Audit log

Khi co Postman/response API that, co the dieu chinh mapping va them cot can
thiet bang migration tiep theo.

### 1.2. Naming convention

Ten table nen dung plural PascalCase:

- `Users`
- `UserSessions`
- `Parks`
- `ParkTicketTypes`
- `JobRuns`
- `AuditLogs`

Ten cot dung PascalCase theo .NET entity:

- `CreatedAtUtc`
- `BusinessDate`
- `PaymentType`

Enum luu string tieng Anh de de doc DB va tranh magic number:

- `Admin`, `Member`, `Accountant`
- `Prepaid`, `Debt`
- `Active`, `Inactive`, `Locked`
- `Api`, `Manual`

### 1.3. Primary key

Chot dung `int autoincrement` cho ID chinh cua cac table v1.

Trong SQLite, cot ID se la `INTEGER PRIMARY KEY AUTOINCREMENT`. Trong code .NET,
ID va foreign key dung `int`/`int?`.

Ly do:

- Du an noi bo, SQLite v1, ID dang so de doc/troubleshoot DB nhanh hon.
- User muon lam viec truc tiep voi DB/script khi can.
- Sau nay neu can public identifier rieng co the them cot `PublicId`/`ExternalId`
  bang migration moi.

### 1.4. Date/time

Timestamp he thong luu UTC:

- `CreatedAtUtc`
- `UpdatedAtUtc`
- `DeletedAtUtc`
- `StartedAtUtc`
- `FinishedAtUtc`
- `ResolvedAtUtc`

Ngay nghiep vu luu rieng:

- `BusinessDate` - date theo gio Viet Nam/Asia-Bangkok.

Trong EF:

- `DateTime` timestamp nen co `Kind = Utc`.
- `BusinessDate` chot dung `DateOnly` trong code.

Khi map SQLite, neu EF provider can converter thi cau hinh conversion trong
Infrastructure. API nhan/tra ngay dang `YYYY-MM-DD`.

### 1.5. Money

Nghiep vu yeu cau luu so tien dang so, khong luu chuoi format.

Logical type:

- `long` theo don vi VND.

Luu y:

- DB SQLite luu `INTEGER` 64-bit.
- Code entity/API dung `long`.
- Khong dung `int` 32-bit vi du lieu co the rat lon, vi du `9.996.172.999`.
- V1 tien dang chan theo VND, khong co phan thap phan.
- Sau nay neu phat sinh tien le/thap phan, co the migration sang decimal hoac
  luu theo don vi nho hon.

Ten cot money van la:

- `AvailableBalance`
- `TotalTicketCost`
- `AdjustmentAmount`

### 1.6. Common audit columns

Nhung bang nghiep vu co the sua/xoa nen co:

- `CreatedAtUtc`
- `CreatedByUserId`
- `UpdatedAtUtc`
- `UpdatedByUserId`

Nhung bang co soft delete nen them:

- `IsDeleted`
- `DeletedAtUtc`
- `DeletedByUserId`

Bang log/raw response/job item co the khong can soft delete.

### 1.7. Soft delete

Khong xoa vat ly du lieu nghiep vu quan trong.

Soft delete dung cho:

- `Parks`
- `ParkTicketTypes`
- Co the dung cho `ParkRefunds`
- User co the disable/lock thay vi delete; neu can delete thi cung nen soft
  delete.

Quy tac quan trong:

- `Park.Code` khong duoc tai su dung ke ca ban ghi da soft delete.
- Neu xoa nham thi restore.
- Danh sach van hanh mac dinh loc `IsDeleted = false`.

### 1.8. Raw response va summary

Voi API ben ngoai:

- Luu raw JSON nguyen van vao `ExternalApiRawResponses`.
- Code parse va luu bang summary da xu ly.
- V1 khong luu detail booking/ticket/giao dich ngan hang.

Summary v1:

- `DailyParkBalanceSnapshots`
- `DailyTicketCostSummaries`
- `DailyBankTransactionSummaries`

Idempotency:

- Chay lai cung ngay/KVC thi update summary cu.
- Raw response co the tao ban ghi moi moi lan chay, gan voi `JobRunId`.

---

## 2. Enum Can Co

### 2.1. UserRole

```text
Admin
Member
Accountant
```

Hien thi:

- `Admin` -> `Quan tri`
- `Member` -> `Thanh vien`
- `Accountant` -> `Ke toan`

### 2.2. UserStatus

```text
Active
Inactive
Locked
```

Y nghia:

- `Active`: dang hoat dong.
- `Inactive`: bi vo hieu hoa boi Admin.
- `Locked`: bi khoa do sai mat khau 3 lan hoac Admin khoa.

### 2.3. ParkPaymentType

```text
Prepaid
Debt
```

Hien thi:

- `Prepaid` -> `Nap truoc`
- `Debt` -> `Cong no`

### 2.4. RecordStatus

```text
Active
Inactive
```

Dung cho:

- Park
- ParkTicketType
- Cac danh muc nghiep vu khac

### 2.5. SourceType

```text
Api
Manual
```

Y nghia:

- `Api`: du lieu lay tu API/job.
- `Manual`: du lieu do Admin/Accountant nhap tay de xu ly loi.

### 2.6. JobName

```text
SyncParkBalances
SyncTicketCosts
SyncBankTransactions
BuildParkReconciliation
SendDailySyncErrorSummary
CleanupAuditLogs
```

Co the luu dang string trong `JobRuns.JobName`.

### 2.7. JobRunStatus

```text
Running
Succeeded
CompletedWithErrors
Failed
Canceled
```

### 2.8. JobRunItemStatus

```text
Pending
Running
Succeeded
Failed
Skipped
ManualResolved
```

### 2.9. ExternalApiSource

```text
ParkBalance
TicketCost
BankTransaction
```

### 2.10. BankTransactionType

```text
TopUp
DebtPayment
Refund
Other
```

V1 chu yeu dung:

- `TopUp`
- `DebtPayment`

### 2.11. ReconciliationStatus

```text
Matched
Variance
MissingData
Resolved
```

Hien thi frontend:

- `Matched` -> `Khop`
- `Variance` -> `Co lech`
- `MissingData` -> `Thieu du lieu`
- `Resolved` -> `Da xu ly`

### 2.12. AuditAction

```text
Create
Update
SetInactive
Restore
SoftDelete
RunJob
ManualEntry
ResolveVariance
Login
Logout
LockUser
UnlockUser
ResetPassword
RevokeSession
```

Co the them sau.

### 2.13. NotificationType

```text
SyncErrorSummary
DailyReport
ReconciliationVarianceAlert
```

V1 dung `SyncErrorSummary`.

### 2.14. Refund statuses

Park refund status:

```text
Completed
Processing
Rejected
```

Customer refund status:

```text
Refunded
Transferring
WaitingConfirmation
NotProcessed
NoRefund
```

---

## 3. Identity/Auth Tables

## 3.1. Users

Luu tai khoan noi bo.

Table: `Users`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `FullName` | string(200) | Yes | Ho ten |
| `Email` | string(320) | Yes | Email hien thi, readonly sau khi tao |
| `NormalizedEmail` | string(320) | Yes | Uppercase/normalized, unique |
| `PhoneNumber` | string(30) | No | So dien thoai |
| `PasswordHash` | string | Yes | Hash, khong audit raw |
| `Role` | UserRole string | Yes | Admin/Member/Accountant |
| `Status` | UserStatus string | Yes | Active/Inactive/Locked |
| `FailedLoginCount` | int | Yes | Reset ve 0 khi login thanh cong |
| `LockedAtUtc` | DateTime | No | Khi bi khoa |
| `LockReason` | string(500) | No | Vi du sai password 3 lan |
| `AvatarPath` | string(500) | No | `/uploads/avatars/...` |
| `LastLoginAtUtc` | DateTime | No | Lan login thanh cong gan nhat |
| `PasswordChangedAtUtc` | DateTime | No | Doi/reset password |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | Null voi admin seed |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK nullable |
| `IsDeleted` | bool | Yes | Mac dinh false, neu can xoa user sau |
| `DeletedAtUtc` | DateTime | No | Soft delete |
| `DeletedByUserId` | int | No | FK nullable |

Indexes:

- Unique: `NormalizedEmail`
- Index: `Role`
- Index: `Status`
- Index: `IsDeleted`

Rules:

- Email unique va khong duoc doi sau khi tao.
- Sai password 3 lan: set `Status = Locked`, `LockedAtUtc`, `LockReason`.
- Admin unlock: set `Status = Active`, `FailedLoginCount = 0`,
  clear lock fields.
- Admin disable: set `Status = Inactive`, revoke all sessions.
- Reset password: update `PasswordHash`, `PasswordChangedAtUtc`, reset failed
  count neu can.

## 3.2. UserSessions

Moi refresh token/session la mot record.

Table: `UserSessions`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `UserId` | int | Yes | FK Users |
| `RefreshTokenHash` | string(512) | Yes | Unique, khong luu token raw |
| `JwtId` | string(100) | No | JTI/access token/session tracking |
| `ExpiresAtUtc` | DateTime | Yes | 5 tieng tu luc tao/refresh |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByIp` | string(100) | No | IP login |
| `UserAgent` | string(1000) | No | Browser raw |
| `DeviceName` | string(300) | No | Parsed/display |
| `LastUsedAtUtc` | DateTime | No | Update khi refresh/API me? |
| `LastUsedIp` | string(100) | No | IP gan nhat |
| `RevokedAtUtc` | DateTime | No | Khi logout/revoke/rotate |
| `RevokedByUserId` | int | No | User/Admin revoke |
| `RevokeReason` | string(300) | No | Logout, Rotate, PasswordChanged... |
| `ReplacedBySessionId` | int | No | Session moi khi rotate |

Indexes:

- Unique: `RefreshTokenHash`
- Index: `UserId`
- Index: `ExpiresAtUtc`
- Index: `RevokedAtUtc`

Rules:

- Refresh token rotate moi lan refresh:
  - Session cu set `RevokedAtUtc`, `RevokeReason = Rotated`.
  - Tao session/token moi hoac update record theo thiet ke code.
- Logout current: revoke current session va clear cookie.
- Doi password: revoke all sessions khac.
- Disable/lock user: revoke all active sessions.

## 3.3. UserPreferences

Luu preference ca nhan neu muon backend quan ly theme.

Table: `UserPreferences`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `UserId` | int | Yes | FK Users, unique |
| `ThemeMode` | string | Yes | Dark/Light/System |
| `Language` | string(20) | No | `vi-VN`, `en-US` sau nay |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `UpdatedAtUtc` | DateTime | No | UTC |

Indexes:

- Unique: `UserId`

Note:

- Frontend hien co the van luu localStorage. Bang nay giup mo rong sau.

## 3.4. NotificationPreferences

Luu cau hinh thong bao ca nhan cua user.

Table: `NotificationPreferences`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `UserId` | int | Yes | FK Users |
| `EventType` | string(100) | Yes | Vi du TopUp, Refund, ReconciliationVariance |
| `EnableInApp` | bool | Yes | Default true |
| `EnableEmail` | bool | Yes | Default true |
| `EnableSound` | bool | Yes | Default false |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `UpdatedAtUtc` | DateTime | No | UTC |

Indexes:

- Unique: `UserId + EventType`

---

## 4. Settings/Notification Tables

## 4.1. SystemSettings

Luu cau hinh he thong trong DB. V1 chua can UI chinh sua.

Table: `SystemSettings`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Key` | string(200) | Yes | PK |
| `Value` | string | Yes | Gia tri dang string/JSON |
| `ValueType` | string(50) | Yes | String/Int/Bool/Decimal/Json |
| `Description` | string(1000) | No | Mo ta |
| `IsSensitive` | bool | Yes | Khong tra raw value neu true |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK nullable |

Seed de xuat:

| Key | Value | Note |
| --- | --- | --- |
| `Jobs.ScheduleTime` | `23:59` | Gio chay job theo Asia/Bangkok |
| `Jobs.ApiTimeoutSeconds` | `30` | Timeout tung API call |
| `Jobs.ApiRetryCount` | `2` | Retry 2 lan |
| `Jobs.ApiRetryDelaySeconds` | `5` | Delay 5 giay |
| `Audit.RetentionDays` | `365` | Luu audit 1 nam |
| `Email.EnableSyncErrorSummary` | `true` | Bat email tong hop loi |

Note:

- SMTP password khong luu DB. De appsettings/environment.

## 4.2. NotificationRecipients

Danh sach email nhan thong bao he thong theo loai thong bao.

Table: `NotificationRecipients`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `NotificationType` | string(100) | Yes | SyncErrorSummary |
| `Email` | string(320) | Yes | Email nhan |
| `DisplayName` | string(200) | No | Ten hien thi |
| `IsActive` | bool | Yes | Default true |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | FK nullable |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK nullable |

Indexes:

- Unique: `NotificationType + Email`
- Index: `IsActive`

Rules:

- Email loi job gui toi recipients `NotificationType = SyncErrorSummary` va
  `IsActive = true`.
- Khong mac dinh gui tat ca user role Accountant.

---

## 5. Park Master Data Tables

## 5.1. Parks

KVC cha/danh muc goc.

Table: `Parks`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `Code` | string(50) | Yes | Ma KVC/API, unique vinh vien |
| `Name` | string(300) | Yes | Ten KVC |
| `PaymentType` | string | Yes | Prepaid/Debt |
| `SearchCode` | string(100) | No | Ma dinh danh tim kiem/API alias |
| `Location` | string(300) | No | Tinh/thanh/dia diem |
| `BankAccount` | string(100) | No | Co the la chuoi nhu `sightseeing` |
| `BankName` | string(200) | No | Ten ngan hang |
| `CreditLimit` | long VND | No | Dung cho Debt |
| `ApiSiteId` | string(100) | No | Tu Excel/API |
| `ApiProfileId` | string(100) | No | Tu Excel/API |
| `BalanceTransformRule` | string(100) | No | Vi du `None`, `MultiplyMinusOne` |
| `ApiNote` | string(1000) | No | Ghi chu cau hinh API |
| `Status` | string | Yes | Active/Inactive |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | FK Users |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK Users |
| `IsDeleted` | bool | Yes | Soft delete |
| `DeletedAtUtc` | DateTime | No | UTC |
| `DeletedByUserId` | int | No | FK Users |

Indexes:

- Unique: `Code`
- Index: `PaymentType`
- Index: `Status`
- Index: `IsDeleted`
- Index: `Name`

Rules:

- `Code` khong duoc doi sau khi tao, tru khi Admin va co quy trinh dac biet.
- `Code` khong duoc tai su dung sau soft delete.
- Member/Accountant/Admin duoc them/sua/ngung su dung.
- Chi Admin duoc soft delete.
- Default list loc `IsDeleted = false`.

Notes tu Excel:

- Mot so KVC co `balance * -1`; map vao `BalanceTransformRule`.
- `BankAccount` phai la string, khong phai number, vi co gia tri nhu
  `1SB2B24` va `sightseeing`.

## 5.2. ParkTicketTypes

KVC con/loai ve thuoc KVC cha.

Table: `ParkTicketTypes`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `ParkId` | int | Yes | FK Parks |
| `Code` | string(100) | Yes | Ma KVC con/ma dong |
| `TicketTypeCode` | string(100) | Yes | Ma loai ve API |
| `Name` | string(500) | Yes | Ten loai ve |
| `TicketGroupName` | string(500) | No | Nhom loai ve |
| `CostPrice` | long VND | Yes | Don gia von |
| `Status` | string | Yes | Active/Inactive |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | FK Users |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK Users |
| `IsDeleted` | bool | Yes | Soft delete |
| `DeletedAtUtc` | DateTime | No | UTC |
| `DeletedByUserId` | int | No | FK Users |

Indexes:

- Index: `ParkId`
- Index: `Status`
- Index: `IsDeleted`
- Unique: `ParkId + TicketTypeCode`
- Index: `ParkId + Code`

Quyet dinh v1:

- `TicketTypeCode` chi can unique trong tung KVC, nen unique theo
  `ParkId + TicketTypeCode`.

Rules:

- Member/Accountant/Admin duoc them/sua/ngung su dung.
- Chi Admin duoc soft delete.

---

## 6. Job/API Tables

## 6.1. JobRuns

Luu moi lan job chay.

Table: `JobRuns`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `JobName` | string(100) | Yes | SyncParkBalances... |
| `BusinessDate` | date | No | Ngay du lieu, null voi cleanup |
| `TriggeredBy` | string(50) | Yes | Schedule/Manual/System |
| `TriggeredByUserId` | int | No | FK Users neu manual |
| `StartedAtUtc` | DateTime | Yes | UTC |
| `FinishedAtUtc` | DateTime | No | UTC |
| `Status` | string | Yes | Running/Succeeded/... |
| `TotalItems` | int | Yes | Tong item |
| `SuccessItems` | int | Yes | Thanh cong |
| `FailedItems` | int | Yes | Loi |
| `SkippedItems` | int | Yes | Bo qua |
| `ErrorMessage` | string | No | Loi tong neu co |
| `SummaryJson` | string | No | Thong tin tong hop them |

Indexes:

- Index: `JobName + BusinessDate`
- Index: `Status`
- Index: `StartedAtUtc`

Rules:

- Job co loi tung KVC thi `Status = CompletedWithErrors`, khong phai Failed
  neu job van hoan thanh vong lap.
- `Failed` dung cho loi he thong lam job khong the tiep tuc.

## 6.2. JobRunItems

Luu ket qua tung item/KVC trong job.

Table: `JobRunItems`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `JobRunId` | int | Yes | FK JobRuns |
| `BusinessDate` | date | No | Ngay du lieu |
| `ParkId` | int | No | FK Parks nullable |
| `Source` | string(100) | No | ParkBalance/TicketCost/BankTransaction |
| `Status` | string | Yes | Pending/Running/Succeeded/Failed/... |
| `AttemptCount` | int | Yes | Tong attempt da goi |
| `StartedAtUtc` | DateTime | No | UTC |
| `FinishedAtUtc` | DateTime | No | UTC |
| `DurationMs` | int | No | Tong duration |
| `ErrorCode` | string(100) | No | Timeout/Http500/ParseError |
| `ErrorMessage` | string | No | Message de hien UI |
| `RawResponseId` | int | No | FK ExternalApiRawResponses |
| `ResolvedByUserId` | int | No | Khi ke toan nhap tay xu ly |
| `ResolvedAtUtc` | DateTime | No | UTC |
| `ManualResolutionNote` | string(1000) | No | Ghi chu xu ly |

Indexes:

- Index: `JobRunId`
- Index: `BusinessDate + ParkId + Source`
- Index: `Status`
- Index: `ResolvedAtUtc`

Rules:

- API timeout/lai loi sau retry: item `Failed`.
- Ke toan nhap tay thanh cong: item co the set `ManualResolved`.
- Man `Loi dong bo can xu ly` lay item `Failed` chua resolved.

## 6.3. ExternalApiRawResponses

Luu raw request/response JSON.

Table: `ExternalApiRawResponses`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `Source` | string(100) | Yes | ParkBalance/TicketCost/BankTransaction |
| `BusinessDate` | date | No | Ngay du lieu |
| `ParkId` | int | No | FK Parks nullable |
| `JobRunId` | int | No | FK JobRuns |
| `JobRunItemId` | int | No | FK JobRunItems nullable |
| `RequestUrl` | string(1000) | No | URL API |
| `RequestPayloadJson` | string | No | Body da mask neu can |
| `ResponseStatusCode` | int | No | HTTP status |
| `ResponseBodyJson` | string | No | Raw response |
| `IsSuccess` | bool | Yes | Parse/got success |
| `ErrorMessage` | string | No | Loi neu co |
| `DurationMs` | int | No | Duration call |
| `ReceivedAtUtc` | DateTime | Yes | UTC |

Indexes:

- Index: `Source + BusinessDate + ParkId`
- Index: `JobRunId`
- Index: `ReceivedAtUtc`

Rules:

- Co the co nhieu raw response cho cung source/date/KVC vi chay lai job.
- Summary table se tro ve job/raw moi nhat qua `SourceJobRunId` va co the
  `RawResponseId` neu can them cot.
- Khong luu secret/token trong request/response neu API co tra.

---

## 7. Daily Summary Tables

## 7.1. DailyParkBalanceSnapshots

Snapshot so du moi ngay theo KVC.

Table: `DailyParkBalanceSnapshots`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `BusinessDate` | date | Yes | Ngay VN |
| `ParkId` | int | Yes | FK Parks |
| `PaymentType` | string | Yes | Snapshot tu Park luc ghi |
| `AvailableBalance` | long VND | Yes | So du kha dung |
| `CurrentDebt` | long VND | No | Neu API co/voi Debt |
| `BankAccountSnapshot` | string(100) | No | Snapshot de tra cuu |
| `SourceType` | string | Yes | Api/Manual |
| `SourceJobRunId` | int | No | FK JobRuns |
| `SourceJobRunItemId` | int | No | FK JobRunItems |
| `RawResponseId` | int | No | FK ExternalApiRawResponses |
| `ManualReason` | string(1000) | No | Bat buoc neu Manual |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | API/job co the null/system |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK Users |

Indexes:

- Unique: `BusinessDate + ParkId`
- Index: `PaymentType`
- Index: `SourceType`

Rules:

- Chay lai cung ngay/KVC thi update record cu.
- Neu API loi, Accountant/Admin co the nhap tay record.
- Manual update phai audit before/after.

## 7.2. DailyTicketCostSummaries

Tong gia von ve ban theo ngay + KVC.

Table: `DailyTicketCostSummaries`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `BusinessDate` | date | Yes | Ngay VN |
| `ParkId` | int | Yes | FK Parks |
| `PaymentType` | string | Yes | Snapshot tu Park |
| `TotalTicketCost` | long VND | Yes | Tong tien von |
| `TotalSalesAmount` | long VND | No | Tong tien ban neu API co |
| `TotalQuantity` | int | No | Tong so ve neu can |
| `SourceType` | string | Yes | Api/Manual |
| `SourceJobRunId` | int | No | FK JobRuns |
| `SourceJobRunItemId` | int | No | FK JobRunItems |
| `RawResponseId` | int | No | FK ExternalApiRawResponses |
| `ManualReason` | string(1000) | No | Bat buoc neu Manual |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | FK nullable |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK nullable |

Indexes:

- Unique: `BusinessDate + ParkId`
- Index: `PaymentType`
- Index: `SourceType`

Rules:

- V1 khong luu detail booking.
- Raw JSON giu de tra cuu khi can.

## 7.3. DailyBankTransactionSummaries

Tong giao dich ngan hang theo ngay + KVC + loai giao dich.

Table: `DailyBankTransactionSummaries`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `BusinessDate` | date | Yes | Ngay VN |
| `ParkId` | int | Yes | FK Parks |
| `PaymentType` | string | Yes | Snapshot tu Park |
| `TransactionType` | string | Yes | TopUp/DebtPayment/... |
| `TotalDebitAmount` | long VND | Yes | Tien ra |
| `TotalCreditAmount` | long VND | Yes | Tien vao neu co |
| `TransactionCount` | int | Yes | So giao dich parse duoc |
| `SourceType` | string | Yes | Api/Manual |
| `SourceJobRunId` | int | No | FK JobRuns |
| `SourceJobRunItemId` | int | No | FK JobRunItems |
| `RawResponseId` | int | No | FK ExternalApiRawResponses |
| `ManualReason` | string(1000) | No | Bat buoc neu Manual |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | FK nullable |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK nullable |

Indexes:

- Unique: `BusinessDate + ParkId + TransactionType`
- Index: `PaymentType`
- Index: `SourceType`

Rules:

- V1 chap nhan man nap tien/thanh toan cong no hien thi tong hop.
- Neu sau nay can tra cuu tung giao dich, them table detail bang migration moi.

---

## 8. Reconciliation Tables

## 8.1. ParkReconciliations

Ket qua doi soat theo ngay + KVC.

Table: `ParkReconciliations`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `BusinessDate` | date | Yes | Ngay T |
| `PreviousBusinessDate` | date | No | Ngay T-1 neu xac dinh duoc |
| `ParkId` | int | Yes | FK Parks |
| `PaymentType` | string | Yes | Prepaid/Debt snapshot |
| `PreviousBalance` | long VND | No | So du T-1 |
| `AdditionalAmount` | long VND | No | Nap them/thanh toan |
| `UsedAmount` | long VND | No | Tong gia von da dung |
| `ExpectedBalance` | long VND | No | (1)+(2)-(3) |
| `ActualBalance` | long VND | No | So du T |
| `VarianceAmount` | long VND | No | Actual - Expected |
| `AdjustmentAmount` | long VND | No | Am/duong khi xu ly lech |
| `AdjustmentNote` | string(2000) | No | Ghi chu xu ly |
| `Status` | string | Yes | Matched/Variance/MissingData/Resolved |
| `MissingPreviousBalance` | bool | Yes | Missing data flag |
| `MissingActualBalance` | bool | Yes | Missing data flag |
| `MissingTicketCost` | bool | Yes | Missing data flag |
| `MissingBankTransaction` | bool | Yes | Missing data flag |
| `ResolvedByUserId` | int | No | FK Users |
| `ResolvedAtUtc` | DateTime | No | UTC |
| `LastBuiltJobRunId` | int | No | FK JobRuns |
| `RebuildCount` | int | Yes | So lan build/rebuild record |
| `LastSourceHash` | string(128) | No | Hash nguon moi nhat |
| `ResolvedSourceHash` | string(128) | No | Hash nguon tai luc resolve |
| `SourceChangedAfterResolved` | bool | Yes | Nguon doi sau khi da resolve |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | System/user |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK nullable |

Indexes:

- Unique: `BusinessDate + ParkId`
- Index: `Status`
- Index: `PaymentType`
- Index: `ResolvedAtUtc`

Rules:

- Build/rebuild upsert theo `BusinessDate + ParkId`.
- Neu thieu nguon van tao record `MissingData`.
- Resolve variance cho nhap `AdjustmentAmount` am/duong va `AdjustmentNote`.
- Resolve khong sua raw response/summary nguon.
- Resolve phai audit before/after.
- Khi build/rebuild, tinh `LastSourceHash` tu cac nguon dau vao:
  - So du T-1
  - So du T
  - Bank transaction summary
  - Ticket cost summary
- Khi resolve, copy `LastSourceHash` vao `ResolvedSourceHash`.
- Neu record da `Resolved` va rebuild sau do lam `LastSourceHash` khac
  `ResolvedSourceHash`, set `SourceChangedAfterResolved = true`.
- Khong tu xoa adjustment va khong tu chuyen status ve `Variance`; UI can canh
  bao "du lieu nguon da thay doi sau khi xu ly".

---

## 9. Refund Tables

## 9.1. ParkRefunds

Quan ly hoan tien KVC nhap thu cong.

Table: `ParkRefunds`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `BookingCode` | string(100) | Yes | Ma dat ve |
| `RefundDate` | date | Yes | Ngay hoan |
| `BusinessDate` | date | No | Neu can gom theo ngay nghiep vu |
| `ParkId` | int | No | FK Parks neu map duoc |
| `ParkCodeSnapshot` | string(50) | No | Snapshot |
| `ParkNameSnapshot` | string(300) | Yes | Ten KVC luc ghi |
| `PaymentType` | string | Yes | Prepaid/Debt |
| `TicketTypeCode` | string(100) | No | Ma loai ve |
| `TicketTypeName` | string(500) | No | Ten loai ve |
| `Quantity` | int | Yes | So ve |
| `ParkRefundAmount` | long VND | No | So tien KVC hoan |
| `CustomerRefundAmount` | long VND | No | So tien hoan KH |
| `Reason` | string(2000) | No | Ly do |
| `ParkRefundStatus` | string | Yes | Completed/Processing/Rejected |
| `CustomerRefundStatus` | string | Yes | Refunded/... |
| `CreatedAtUtc` | DateTime | Yes | UTC |
| `CreatedByUserId` | int | No | FK Users |
| `UpdatedAtUtc` | DateTime | No | UTC |
| `UpdatedByUserId` | int | No | FK Users |
| `IsDeleted` | bool | Yes | Soft delete neu can |
| `DeletedAtUtc` | DateTime | No | UTC |
| `DeletedByUserId` | int | No | FK Users |

Indexes:

- Index: `BookingCode`
- Index: `RefundDate`
- Index: `ParkId`
- Index: `PaymentType`
- Index: `ParkRefundStatus`
- Index: `CustomerRefundStatus`
- Index: `IsDeleted`

Rules:

- Admin co the them/sua.
- Member xem.
- Neu sau nay cho Accountant/Member thao tac thi can audit.
- Hoan tien co the anh huong doi soat sau nay, nhung cong thuc v1 can chot khi
  implement.

---

## 10. Audit Tables

## 10.1. AuditLogs

Luu audit before/after JSON.

Table: `AuditLogs`

Columns:

| Column | Type logical | Required | Note |
| --- | --- | --- | --- |
| `Id` | int | Yes | PK autoincrement |
| `OccurredAtUtc` | DateTime | Yes | UTC |
| `UserId` | int | No | FK Users nullable |
| `UserEmailSnapshot` | string(320) | No | Snapshot |
| `UserRoleSnapshot` | string(50) | No | Snapshot |
| `Module` | string(100) | Yes | Park/Auth/Jobs/Settings |
| `EntityName` | string(200) | Yes | Park, ParkTicketType... |
| `EntityId` | string(100) | No | int/string |
| `Action` | string(100) | Yes | Create/Update/... |
| `BeforeJson` | string | No | Data truoc |
| `AfterJson` | string | No | Data sau |
| `IpAddress` | string(100) | No | IP |
| `UserAgent` | string(1000) | No | UA |
| `CorrelationId` | string(100) | No | Trace request |

Indexes:

- Index: `OccurredAtUtc`
- Index: `UserId`
- Index: `Module`
- Index: `EntityName + EntityId`
- Index: `Action`

Rules:

- Retention: 365 ngay.
- Khong luu password/token/secret.
- Audit log chi xem tren UI v1, chua export.
- Member/Accountant chi xem log nghiep vu Khu vui choi o muc phu hop.
- Admin xem day du.

---

## 11. Relationship Summary

### 11.1. Identity

```text
Users 1 - n UserSessions
Users 1 - 1 UserPreferences
Users 1 - n NotificationPreferences
```

### 11.2. Park master data

```text
Parks 1 - n ParkTicketTypes
Parks 1 - n DailyParkBalanceSnapshots
Parks 1 - n DailyTicketCostSummaries
Parks 1 - n DailyBankTransactionSummaries
Parks 1 - n ParkReconciliations
Parks 1 - n ParkRefunds (nullable)
```

### 11.3. Jobs/API

```text
JobRuns 1 - n JobRunItems
JobRuns 1 - n ExternalApiRawResponses
JobRunItems 0/1 - 1 ExternalApiRawResponses
```

### 11.4. Delete behavior

Khuyen nghi EF delete behavior:

- User -> sessions: restrict/no cascade physical delete.
- Park -> child/data: restrict physical delete.
- JobRun -> JobRunItems: cascade co the chap nhan neu xoa job run vat ly, nhung
  v1 khong nen xoa job run.
- AuditLogs khong FK cascade bat buoc; co the luu snapshot va nullable UserId.

Voi soft delete, khong can physical cascade.

---

## 12. Index/Constraint Checklist

Bat buoc nen co trong migration dau:

Identity:

- `Users.NormalizedEmail` unique.
- `UserSessions.RefreshTokenHash` unique.
- `UserSessions.UserId` index.

Settings:

- `SystemSettings.Key` primary key.
- `NotificationRecipients.NotificationType + Email` unique.

Parks:

- `Parks.Code` unique.
- `Parks.PaymentType`, `Status`, `IsDeleted` indexes.
- `ParkTicketTypes.ParkId + TicketTypeCode` unique.
- `ParkTicketTypes.ParkId + Code` index.

Jobs:

- `JobRuns.JobName + BusinessDate` index.
- `JobRunItems.BusinessDate + ParkId + Source` index.
- `ExternalApiRawResponses.Source + BusinessDate + ParkId` index.

Summaries:

- `DailyParkBalanceSnapshots.BusinessDate + ParkId` unique.
- `DailyTicketCostSummaries.BusinessDate + ParkId` unique.
- `DailyBankTransactionSummaries.BusinessDate + ParkId + TransactionType` unique.

Reconciliation:

- `ParkReconciliations.BusinessDate + ParkId` unique.
- `ParkReconciliations.Status` index.

Audit:

- `AuditLogs.OccurredAtUtc` index.
- `AuditLogs.EntityName + EntityId` index.

---

## 13. Data Flow Theo Job

### 13.1. SyncParkBalances

Input:

- Active Parks co `ApiSiteId`, `ApiProfileId`.
- `BusinessDate`.

Flow:

1. Tao `JobRun`.
2. Voi moi Park tao `JobRunItem`.
3. Goi API timeout 30s, retry 2 lan, delay 5s.
4. Luu raw response vao `ExternalApiRawResponses`.
5. Parse balance, apply `BalanceTransformRule`.
6. Upsert `DailyParkBalanceSnapshots`.
7. Update `JobRunItem` success/failed.
8. Update `JobRun` status.

Neu loi:

- `JobRunItem.Status = Failed`.
- Ke toan nhap tay vao `DailyParkBalanceSnapshots`.

### 13.2. SyncTicketCosts

Input:

- `BusinessDate`.
- API ticket cost response.

Flow:

1. Luu raw response.
2. Parse/tong hop theo ngay + KVC.
3. Upsert `DailyTicketCostSummaries`.

V1 khong luu detail booking.

### 13.3. SyncBankTransactions

Input:

- `BusinessDate`.
- API bank transaction response.

Flow:

1. Luu raw response.
2. Parse/tong hop theo ngay + KVC + transaction type.
3. Upsert `DailyBankTransactionSummaries`.

V1 khong luu detail transaction.

### 13.4. BuildParkReconciliation

Input:

- `BusinessDate`.
- Summary tables.

Flow:

1. Xac dinh danh sach Parks can doi soat.
2. Lay previous balance T-1.
3. Lay actual balance T.
4. Lay bank transaction summary.
5. Lay ticket cost summary.
6. Neu thieu nguon, tao/update record `MissingData`.
7. Neu du nguon, tinh expected va variance.
8. Upsert `ParkReconciliations`.

Status:

- Variance = 0 -> `Matched`
- Variance != 0 -> `Variance`
- Thieu nguon -> `MissingData`

### 13.5. SendDailySyncErrorSummary

Input:

- `BusinessDate`.
- `JobRunItems` failed chua manual resolved.
- Recipients `SyncErrorSummary`.

Flow:

1. Gom loi theo job/source/KVC.
2. Neu khong co loi thi khong gui.
3. Neu SMTP chua cau hinh thi log warning.
4. Gui email tong hop.

### 13.6. CleanupAuditLogs

Input:

- `Audit.RetentionDays`, default 365.

Flow:

1. Xoa audit logs cu hon retention.
2. Ghi `JobRun`.

---

## 14. Manual Entry Rules

Manual entry dung khi API loi va ke toan can nhap tay.

Role duoc phep:

- Admin
- Accountant

Bang co manual entry:

- `DailyParkBalanceSnapshots`
- `DailyTicketCostSummaries`
- `DailyBankTransactionSummaries`

Bat buoc khi manual:

- `SourceType = Manual`
- `ManualReason` khong rong
- `UpdatedByUserId` la user thao tac
- Audit log before/after
- Neu lien quan `JobRunItem` loi thi mark `ManualResolved`

Khong can Admin duyet; co hieu luc ngay.

---

## 15. Seed Data

### 15.1. Seed Admin

PowerShell script rieng:

```text
scripts/seed-admin.ps1
```

Script:

- Hoi nhap email, ho ten, phone.
- Nhap password dang secure.
- Validate password policy.
- Hash password.
- Insert `Users` role `Admin`, status `Active`.
- Neu da co Admin thi canh bao.

### 15.2. Seed system settings

Co the seed bang migration hoac SQL script:

```text
scripts/seed-system-settings.sql
```

### 15.3. Seed parks

User muon tu chay script DB va tu dien du lieu.

De xuat:

```text
scripts/seed-parks.sql
```

Script nen co template:

```sql
-- Insert KVC nap truoc
-- Code, Name, PaymentType, BankAccount, BankName, ApiSiteId, ApiProfileId,
-- BalanceTransformRule, Status

-- Insert KVC cong no
-- Code, Name, PaymentType, CreditLimit, BankAccount, BankName, ApiSiteId,
-- ApiProfileId, Status
```

Khong can import Excel v1.

---

## 16. Migration De Xuat

Co the tao mot migration dau `InitialCreate` gom tat ca table v1.

Neu muon tach de review de hon:

1. `InitialIdentity`
2. `InitialSettings`
3. `InitialParkMasterData`
4. `InitialJobsAndRawResponses`
5. `InitialDailySummariesAndReconciliation`
6. `InitialAuditLogs`

Trong thuc te EF Core thuong don gian hon voi 1 migration dau, mien la review
ky schema truoc khi apply.

---

## 17. Diem Can Hoi Lai Khi Code

Nhung diem da chot truoc khi code:

1. Primary key:
   - Dung `int autoincrement`, khong dung Guid trong v1.

2. Money:
   - Dung `long` trong code.
   - SQLite luu `INTEGER` 64-bit.
   - Don vi VND, khong co phan thap phan trong v1.

3. `BusinessDate`:
   - Dung `DateOnly` trong code.
   - API nhan/tra `YYYY-MM-DD`.

4. `ParkTicketTypes` unique:
   - Unique theo `ParkId + TicketTypeCode`.

5. Rebuild reconciliation sau khi da `Resolved`:
   - Giu status/adjustment da resolve.
   - Tinh source hash.
   - Neu nguon doi sau khi resolve, set `SourceChangedAfterResolved = true`.
   - UI can canh bao, khong tu dua ve `Variance`.

Mot so diem con can hoi lai khi code chi tiet:

1. `ParkRefunds` anh huong doi soat:
   - V1 co the luu rieng.
   - Khi cong vao cong thuc doi soat can chot voi nghiep vu.

---

## 18. Checklist Truoc Khi Code Backend

Truoc khi scaffold/code:

- [ ] Review tai lieu `docs/backend-design.md`.
- [ ] Review tai lieu nay.
- [x] Chot `int autoincrement` lam primary key.
- [x] Chot money dung `long`/SQLite `INTEGER` theo VND.
- [x] Chot `DateOnly` cho `BusinessDate`.
- [x] Chot unique `ParkTicketTypes` theo `ParkId + TicketTypeCode`.
- [x] Chot reconciliation resolved dung source hash va `SourceChangedAfterResolved`.
- [ ] Sau do tao solution/project backend.
