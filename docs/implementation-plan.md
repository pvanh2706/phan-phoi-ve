# Implementation Plan - PpvRecon

Tai lieu nay la ke hoach thi cong backend cho du an **Doi soat phan phoi ve**
(`PpvRecon`). Ke hoach dua tren:

- `docs/backend-design.md`
- `docs/database-design.md`

Muc tieu: sau khi mo lai sau vai gio/vai ngay, AI hoac developer co the tiep
tuc code theo tung phase ma khong can hoi lai boi canh tu dau.

Ngay lap plan: 2026-06-27 theo gio Asia/Bangkok.

---

## 0. Nguyen Tac Lam Viec

### 0.1. Khong lam tat ca mot luc

Lam theo vertical slices va commit/checkpoint nho:

1. Scaffold backend.
2. DB foundation.
3. Auth.
4. User/settings.
5. Khu vui choi master data.
6. Jobs/raw response/summary.
7. Reconciliation.
8. Email/audit cleanup.
9. Frontend integration.

Moi phase nen build/test duoc truoc khi sang phase tiep.

### 0.2. Tai lieu la source of truth truoc khi code

Neu gap mau thuan:

1. Uu tien `docs/database-design.md` cho schema.
2. Uu tien `docs/backend-design.md` cho kien truc/quyet dinh nghiep vu.
3. Neu API response thuc te khac tai lieu, cap nhat tai lieu truoc hoac cung
   commit voi code.

### 0.3. Cac quyet dinh da chot

- Backend .NET 10.
- Thu muc moi: `backend`.
- Solution: `PpvRecon.sln`.
- Projects:
  - `PpvRecon.Api`
  - `PpvRecon.Application`
  - `PpvRecon.Domain`
  - `PpvRecon.Infrastructure`
- SQLite + EF Core code-first migrations.
- Primary key v1: `int autoincrement`.
- Money: `long`, SQLite `INTEGER`, don vi VND.
- `BusinessDate`: `DateOnly`.
- Auth: access token 15 phut + refresh token 5 tieng.
- Refresh token luu hash, cookie HttpOnly, rotate moi lan refresh.
- Roles: `Admin`, `Member`, `Accountant`.
- `ParkTicketTypes` unique theo `ParkId + TicketTypeCode`.
- Doi soat resolved dung source hash va `SourceChangedAfterResolved`.

### 0.4. Yeu cau coding chung

- API prefix `/api`.
- Controller, khong dung Minimal API.
- Response wrapper `{ success, data, message, errors }`.
- Message loi tieng Viet.
- Pagination co dinh 100 item/trang, client chi truyen `page`.
- Serilog console + file rolling trong `backend/logs`.
- Swagger bat trong dev.
- Health check `/health`.
- CORS dev cho:
  - `http://localhost:5173`
  - `http://127.0.0.1:5173`

---

## 1. Phase 0 - Scaffold Backend

Muc tieu: tao solution/project backend chay duoc, build duoc, co logging,
Swagger, health check, CORS va static file setup co ban.

### 1.1. Tao solution va projects

Thu muc can tao:

```text
backend/
  PpvRecon.sln
  src/
    PpvRecon.Api/
    PpvRecon.Application/
    PpvRecon.Domain/
    PpvRecon.Infrastructure/
```

Lenh de xuat:

```powershell
mkdir backend
cd backend
dotnet new sln -n PpvRecon
mkdir src
cd src
dotnet new webapi -n PpvRecon.Api -f net10.0
dotnet new classlib -n PpvRecon.Application -f net10.0
dotnet new classlib -n PpvRecon.Domain -f net10.0
dotnet new classlib -n PpvRecon.Infrastructure -f net10.0
cd ..
dotnet sln add src/PpvRecon.Api/PpvRecon.Api.csproj
dotnet sln add src/PpvRecon.Application/PpvRecon.Application.csproj
dotnet sln add src/PpvRecon.Domain/PpvRecon.Domain.csproj
dotnet sln add src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj
```

Project references:

```text
Api -> Application, Infrastructure
Application -> Domain
Infrastructure -> Application, Domain
```

Lenh:

```powershell
dotnet add src/PpvRecon.Api/PpvRecon.Api.csproj reference src/PpvRecon.Application/PpvRecon.Application.csproj
dotnet add src/PpvRecon.Api/PpvRecon.Api.csproj reference src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj
dotnet add src/PpvRecon.Application/PpvRecon.Application.csproj reference src/PpvRecon.Domain/PpvRecon.Domain.csproj
dotnet add src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj reference src/PpvRecon.Application/PpvRecon.Application.csproj
dotnet add src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj reference src/PpvRecon.Domain/PpvRecon.Domain.csproj
```

### 1.2. Cai packages nen co

Api:

- `Serilog.AspNetCore`
- `Serilog.Sinks.File`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore`
- `Microsoft.AspNetCore.OpenApi` neu template can
- `Microsoft.AspNetCore.Diagnostics.HealthChecks`

Infrastructure:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.Options.ConfigurationExtensions`
- `BCrypt.Net-Next` hoac package hashing khac neu chon BCrypt

Application:

- Co the chua can package ngoai o Phase 0.

Lenh vi du:

```powershell
dotnet add src/PpvRecon.Api/PpvRecon.Api.csproj package Serilog.AspNetCore
dotnet add src/PpvRecon.Api/PpvRecon.Api.csproj package Serilog.Sinks.File
dotnet add src/PpvRecon.Api/PpvRecon.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/PpvRecon.Api/PpvRecon.Api.csproj package Swashbuckle.AspNetCore
dotnet add src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj package BCrypt.Net-Next
```

Can kiem tra version tuong thich .NET 10 khi code. Neu package version tu dong
lay preview/stable phu hop thi chap nhan.

### 1.3. Cau hinh appsettings

Trong `PpvRecon.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=App_Data/ppv-recon.db"
  },
  "Jwt": {
    "Issuer": "PpvRecon",
    "Audience": "PpvRecon.Web",
    "SigningKey": "CHANGE_ME_DEV_ONLY_MIN_32_CHARS",
    "AccessTokenMinutes": 15,
    "RefreshTokenHours": 5
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://127.0.0.1:5173"
    ]
  },
  "Email": {
    "SmtpHost": "",
    "SmtpPort": 587,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "",
    "EnableSsl": true
  }
}
```

Can them `appsettings.Development.json` neu muon signing key dev rieng.

### 1.4. Program.cs setup

Can setup:

- Serilog:
  - Console
  - File `../../logs/ppv-recon-.log` hoac `backend/logs/...`
- Controllers
- Swagger dev
- CORS
- Health check
- Static files
- SPA fallback khi production/deploy
- Exception middleware placeholder
- Response wrapper later

Checkpoint:

```powershell
dotnet build
dotnet run --project src/PpvRecon.Api/PpvRecon.Api.csproj
```

Verify:

- `/swagger` mo duoc trong dev.
- `/health` tra OK.
- Log file tao trong `backend/logs`.

### 1.5. Output cua Phase 0

Done khi:

- Solution build clean.
- API chay duoc.
- Swagger chay.
- `/health` chay.
- Serilog ghi file.
- Khong co DB migration/entity nghiep vu o phase nay neu muon tach gon.

---

## 2. Phase 1 - Database Foundation

Muc tieu: tao domain enums/entities, DbContext, EF config, migration dau tien.

### 2.1. Domain structure de xuat

Trong `PpvRecon.Domain`:

```text
Enums/
  UserRole.cs
  UserStatus.cs
  ParkPaymentType.cs
  RecordStatus.cs
  SourceType.cs
  JobRunStatus.cs
  JobRunItemStatus.cs
  ExternalApiSource.cs
  BankTransactionType.cs
  ReconciliationStatus.cs
  AuditAction.cs
  NotificationType.cs

Entities/
  Identity/
    User.cs
    UserSession.cs
    UserPreference.cs
    NotificationPreference.cs
  Settings/
    SystemSetting.cs
    NotificationRecipient.cs
  Parks/
    Park.cs
    ParkTicketType.cs
    ParkRefund.cs
  Jobs/
    JobRun.cs
    JobRunItem.cs
    ExternalApiRawResponse.cs
  Summaries/
    DailyParkBalanceSnapshot.cs
    DailyTicketCostSummary.cs
    DailyBankTransactionSummary.cs
  Reconciliation/
    ParkReconciliation.cs
  Auditing/
    AuditLog.cs
```

Base interfaces/classes:

```text
Common/
  IHasAuditFields.cs
  ISoftDelete.cs
```

### 2.2. Entity rules

- ID: `int Id`.
- FK: `int`, nullable FK: `int?`.
- Money: `long`.
- BusinessDate: `DateOnly`.
- Timestamp: `DateTime` UTC.
- Enum: co the luu enum type trong C#, configure conversion to string trong EF.

### 2.3. Infrastructure DbContext

Trong `PpvRecon.Infrastructure`:

```text
Persistence/
  PpvReconDbContext.cs
  Configurations/
    UserConfiguration.cs
    ParkConfiguration.cs
    ...
  DesignTimeDbContextFactory.cs
```

DbSets:

- `Users`
- `UserSessions`
- `UserPreferences`
- `NotificationPreferences`
- `SystemSettings`
- `NotificationRecipients`
- `Parks`
- `ParkTicketTypes`
- `ParkRefunds`
- `JobRuns`
- `JobRunItems`
- `ExternalApiRawResponses`
- `DailyParkBalanceSnapshots`
- `DailyTicketCostSummaries`
- `DailyBankTransactionSummaries`
- `ParkReconciliations`
- `AuditLogs`

### 2.4. EF configuration

Can config:

- Table names plural.
- Key autoincrement.
- Enum to string.
- DateOnly conversion cho SQLite neu can.
- Max lengths theo `database-design.md`.
- Unique indexes:
  - `Users.NormalizedEmail`
  - `UserSessions.RefreshTokenHash`
  - `NotificationRecipients.NotificationType + Email`
  - `Parks.Code`
  - `ParkTicketTypes.ParkId + TicketTypeCode`
  - `DailyParkBalanceSnapshots.BusinessDate + ParkId`
  - `DailyTicketCostSummaries.BusinessDate + ParkId`
  - `DailyBankTransactionSummaries.BusinessDate + ParkId + TransactionType`
  - `ParkReconciliations.BusinessDate + ParkId`
- Indexes theo checklist database design.
- DeleteBehavior.Restrict cho cac FK nghiep vu de tranh cascade xoa nham.

### 2.5. Seed system settings co ban

Co the seed bang EF `HasData` hoac script SQL. De don gian:

- Seed qua migration/DbContext cho `SystemSettings`.
- Hoac de script sau.

Seed de xuat:

- `Jobs.ScheduleTime = 23:59`
- `Jobs.ApiTimeoutSeconds = 30`
- `Jobs.ApiRetryCount = 2`
- `Jobs.ApiRetryDelaySeconds = 5`
- `Audit.RetentionDays = 365`
- `Email.EnableSyncErrorSummary = true`

### 2.6. Migration

Lenh de xuat:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate `
  --project src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj `
  --startup-project src/PpvRecon.Api/PpvRecon.Api.csproj `
  --output-dir Persistence/Migrations

dotnet ef database update `
  --project src/PpvRecon.Infrastructure/PpvRecon.Infrastructure.csproj `
  --startup-project src/PpvRecon.Api/PpvRecon.Api.csproj
```

Can tao thu muc `App_Data` khi app chay neu chua co.

### 2.7. Verify Phase 1

Done khi:

- `dotnet build` pass.
- Migration tao duoc.
- SQLite DB tao duoc.
- Check DB co table/index chinh.
- App van chay `/health`.

---

## 3. Phase 2 - Response, Errors, Auth Foundation

Muc tieu: co wrapper response, exception middleware, auth service, login/refresh/logout/me.

### 3.1. Application common models

Trong `PpvRecon.Application/Common`:

- `ApiResponse<T>`
- `ApiError`
- `PagedResult<T>`
- `Result<T>` hoac `AppResult<T>` internal

Format response:

```json
{
  "success": true,
  "data": {},
  "message": "Thanh cong",
  "errors": []
}
```

### 3.2. Exception middleware

Trong API:

- Middleware catch exceptions.
- Log error.
- Tra wrapper:
  - 500: "Co loi he thong, vui long thu lai sau."
  - 400 validation.
  - 401/403 custom neu can.

Can tranh leak stack trace ra response.

### 3.3. Auth contracts

Application DTOs:

```text
Auth/LoginRequest.cs
Auth/LoginResponse.cs
Auth/RefreshResponse.cs
Auth/MeResponse.cs
```

Login request:

- `email`
- `password`

Login response:

- `accessToken`
- `expiresAtUtc`
- `user`

Refresh token nam trong HttpOnly cookie, khong tra body.

### 3.4. Password hashing

Service:

```text
IPasswordHasher
PasswordHasher
```

Dung BCrypt hoac ASP.NET PasswordHasher. Neu dung BCrypt:

- Hash khi seed/create/reset.
- Verify khi login.

### 3.5. JWT service

Service:

```text
ITokenService
TokenService
```

Can tao:

- Access token 15 phut.
- Refresh token random cryptographic string.
- Hash refresh token truoc khi luu DB.
- Cookie options:
  - HttpOnly true
  - Secure theo environment
  - SameSite Lax hoac Strict tuy frontend dev; neu cross-origin local can can nhac `SameSite=None` + Secure, nhung HTTP local co the kho. Co the dev dung Lax va API same-site qua proxy sau.

Can test local voi Vite; neu cookie cross-origin kho, co the cau hinh dev proxy
hoac tam SameSite Lax tuy setup.

### 3.6. Auth rules

Login:

- Normalize email.
- Neu user khong ton tai: tra loi chung "Email hoac mat khau khong dung."
- Neu status `Inactive`: "Tai khoan da bi vo hieu hoa."
- Neu status `Locked`: "Tai khoan dang bi khoa. Vui long lien he Admin."
- Verify password.
- Sai password:
  - Tang `FailedLoginCount`.
  - Neu >= 3: set `Status = Locked`, `LockedAtUtc`, `LockReason`.
  - Revoke sessions neu bi lock.
  - Tra message tieng Viet.
- Dung password:
  - Reset `FailedLoginCount`.
  - Update `LastLoginAtUtc`.
  - Tao session refresh token.
  - Set refresh cookie.
  - Tra access token + user.

Refresh:

- Doc refresh token tu cookie.
- Hash va tim active session.
- Check expiry/revoked/user active.
- Rotate:
  - Revoke session cu.
  - Tao session moi hoac record moi.
  - Set cookie moi.
  - Tra access token moi.

Logout:

- Revoke current session.
- Clear cookie.

Me:

- Lay user tu token.
- Tra user profile/toi thieu.

### 3.7. Controllers

`AuthController`:

```http
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

### 3.8. Verify Phase 2

Can co seed Admin truoc khi test login. Neu script chua co, co the tam insert
bang code/dev seed nhung sau do thay bang script.

Done khi:

- Login dung tra access token va set cookie.
- Sai password 3 lan khoa user.
- Refresh rotate token.
- Logout revoke session.
- Swagger test duoc cac endpoint.

---

## 4. Phase 3 - Seed Admin Script

Muc tieu: co script PowerShell tao Admin dau tien rieng, khong auto seed ngam.

### 4.1. Script path

```text
scripts/seed-admin.ps1
```

### 4.2. Chuc nang script

Script nen:

- Hoi DB path, default:
  - `backend/src/PpvRecon.Api/App_Data/ppv-recon.db`
- Hoi:
  - Full name
  - Email
  - Phone optional
  - Password secure
  - Confirm password secure
- Validate:
  - Email format co ban.
  - Password policy.
  - Password confirm match.
- Kiem tra da co Admin chua.
  - Neu co, canh bao va hoi xac nhan co tao them Admin khong.
- Hash password.
- Insert vao `Users`.

### 4.3. Hashing trong script

Can quyet dinh phu thuoc backend hashing:

Option A:

- Script goi mot utility .NET console de hash.

Option B:

- Script tu implement BCrypt bang load assembly/package kho hon.

Option C de nhanh:

- Them command trong API project:
  - `dotnet run -- seed-admin`
  - Nhung user da chot muon file script rieng.
  - Script `.ps1` co the goi command nay tuong tac.

Khuyen nghi:

- Tao `scripts/seed-admin.ps1` la file rieng.
- Script goi `dotnet run --project backend/src/PpvRecon.Api -- seed-admin`
  hoac mot command internal tuong tac.
- Nhu vay password hashing dung chung code backend, khong duplicate BCrypt
  trong PowerShell.

Can dam bao yeu cau "file script rieng" van dung: nguoi dung chay `.ps1`.

### 4.4. Verify Phase 3

Done khi:

- Chay script tao Admin.
- Login bang Admin thanh cong.
- Script khong in password.
- Script canh bao neu da co Admin.

---

## 5. Phase 4 - User/Profile/Device Management

Muc tieu: hoan thien phan Cai dat ca nhan va user management Admin.

### 5.1. Me/Profile endpoints

```http
GET  /api/me/profile
PUT  /api/me/profile
POST /api/me/avatar
POST /api/me/change-password
GET  /api/me/sessions
POST /api/me/sessions/{sessionId}/revoke
POST /api/me/sessions/revoke-all
```

Rules:

- User sua full name, phone.
- Email readonly.
- Role readonly.
- Change password can current password.
- Change password revoke all sessions khac.
- Avatar:
  - JPG/PNG/WebP
  - Max 2MB
  - Luu `wwwroot/uploads/avatars/{userId}/...`
  - Xoa avatar cu sau khi update thanh cong.

### 5.2. User management endpoints - Admin

```http
GET  /api/users?page=1&keyword=&role=&status=
POST /api/users
GET  /api/users/{id}
PUT  /api/users/{id}
POST /api/users/{id}/reset-password
POST /api/users/{id}/unlock
POST /api/users/{id}/disable
POST /api/users/{id}/enable
```

Rules:

- Chi Admin.
- Create user:
  - Email unique.
  - Role: Admin/Member/Accountant.
  - Password policy.
- Edit user:
  - Email readonly.
  - Role/status editable.
- Reset password:
  - Admin tu nhap password moi.
  - Khong bat user doi lai.
- Disable/lock:
  - Revoke all sessions.
- Unlock:
  - Status Active.
  - FailedLoginCount = 0.

### 5.3. Audit/security log

Can audit:

- Reset password
- Unlock
- Disable/enable
- Revoke session

Khong audit password/hash raw.

### 5.4. Verify Phase 4

Done khi:

- User doi profile/avatar/password duoc.
- Device list hien session.
- Revoke current session logout duoc.
- Admin tao/sua/reset/disable/unlock user duoc.

---

## 6. Phase 5 - Audit Log Foundation

Muc tieu: co audit service dung chung truoc khi lam Khu vui choi.

### 6.1. Audit service

Application:

```text
IAuditService
AuditService
```

Hoac Infrastructure implementation neu can DbContext.

Method de xuat:

```csharp
Task LogAsync(AuditLogEntry entry, CancellationToken ct);
```

Can lay:

- Current user id/email/role
- IP
- User-Agent
- CorrelationId

### 6.2. Serialization

Before/After JSON:

- Dung System.Text.Json.
- Ignore password hash/token/secret.
- Co helper sanitize object.

### 6.3. Audit query API

```http
GET /api/audit-logs?page=1&module=&entityName=&entityId=&userId=&action=&fromDate=&toDate=
```

Quyen:

- Admin xem full.
- Member/Accountant chi xem module Khu vui choi o muc phu hop.

### 6.4. Verify Phase 5

Done khi:

- Audit log ghi duoc bang service.
- Query API phan trang 100.
- Filter co ban hoat dong.

---

## 7. Phase 6 - Khu Vui Choi Master Data

Muc tieu: CRUD Parks va ParkTicketTypes, inactive/soft delete, audit log.

### 7.1. Parks endpoints

```http
GET    /api/parks?page=1&keyword=&paymentType=&status=
POST   /api/parks
GET    /api/parks/{id}
PUT    /api/parks/{id}
POST   /api/parks/{id}/set-inactive
POST   /api/parks/{id}/restore
DELETE /api/parks/{id}
GET    /api/parks/{id}/audit-logs?page=1
```

Rules:

- Admin/Member/Accountant:
  - GET/POST/PUT/set-inactive.
- Chi Admin:
  - DELETE soft delete.
  - restore neu can.
- `Code` unique vinh vien.
- `Code` khong doi sau tao neu khong co yeu cau moi.
- Default list: `IsDeleted = false`.
- Status:
  - `Active`
  - `Inactive`

### 7.2. ParkTicketTypes endpoints

```http
GET    /api/park-ticket-types?page=1&parkId=&keyword=&paymentType=&status=
POST   /api/park-ticket-types
GET    /api/park-ticket-types/{id}
PUT    /api/park-ticket-types/{id}
POST   /api/park-ticket-types/{id}/set-inactive
DELETE /api/park-ticket-types/{id}
GET    /api/park-ticket-types/{id}/audit-logs?page=1
```

Rules:

- Unique `ParkId + TicketTypeCode`.
- Money `CostPrice` la `long`.
- Admin/Member/Accountant them/sua/inactive.
- Chi Admin soft delete.

### 7.3. Validation tieng Viet

Vi du:

- `Ma KVC la bat buoc.`
- `Ma KVC da ton tai.`
- `Ten khu vui choi la bat buoc.`
- `Loai thanh toan khong hop le.`
- `Khong tim thay khu vui choi.`

### 7.4. Seed parks script

Path:

```text
scripts/seed-parks.sql
```

Script la template de user tu dien/chay.

Can gom:

- Insert Prepaid KVC.
- Insert Debt KVC.
- Insert ParkTicketTypes mau.
- Comment ro cot `ApiSiteId`, `ApiProfileId`, `BalanceTransformRule`.

### 7.5. Verify Phase 6

Done khi:

- CRUD Parks/ParkTicketTypes qua Swagger.
- Audit log before/after co du.
- Soft delete an khoi list.
- Inactive van ton tai va loc duoc.
- Member khong soft delete duoc.
- Accountant thao tac nhu Member.

---

## 8. Phase 7 - Job Framework, Raw Responses, Summaries

Muc tieu: tao khung job, JobRun/JobRunItem, raw response, daily summaries,
manual entry va man loi dong bo API.

### 8.1. Job framework

Services:

```text
IJobRunner
IJobRunLogger
IExternalApiClient<T>
```

Job classes:

- `SyncParkBalancesJob`
- `SyncTicketCostsJob`
- `SyncBankTransactionsJob`
- `BuildParkReconciliationJob`
- `SendDailySyncErrorSummaryJob`
- `CleanupAuditLogsJob`

Hosted service/scheduler:

- Doc `Jobs.ScheduleTime`.
- Chay theo Asia/Bangkok.
- Cac sync jobs chay doc lap.

### 8.2. Retry policy

Cho tung API call:

- Timeout 30 giay.
- Retry 2 lan.
- Delay 5 giay.

Neu loi:

- Ghi `JobRunItem.Failed`.
- Tiep tuc item khac.

### 8.3. Raw response

Khi API tra response:

- Insert `ExternalApiRawResponses`.
- Gan `JobRunId`, `JobRunItemId`, `ParkId`, `BusinessDate`.
- Luu request/response JSON da mask secret neu can.

### 8.4. Summary upsert

Upsert theo:

- Balance: `BusinessDate + ParkId`.
- Ticket cost: `BusinessDate + ParkId`.
- Bank transaction: `BusinessDate + ParkId + TransactionType`.

Source:

- `Api` cho job.
- `Manual` cho nhap tay.

### 8.5. Manual entry endpoints

Chi Admin/Accountant:

```http
POST /api/park-balances/manual
POST /api/ticket-cost-summaries/manual
POST /api/bank-transaction-summaries/manual
```

Manual body can co:

- `businessDate`
- `parkId`
- Amount fields
- `manualReason`
- Optional `jobRunItemId` de resolve loi.

Rules:

- ManualReason bat buoc.
- Audit before/after.
- Neu co JobRunItem Failed lien quan, set `ManualResolved`.

### 8.6. Job APIs

```http
GET  /api/jobs/runs?page=1&jobName=&businessDate=&status=
GET  /api/jobs/runs/{id}
GET  /api/jobs/errors?page=1&businessDate=&source=&status=
POST /api/jobs/sync-park-balances/run
POST /api/jobs/sync-ticket-costs/run
POST /api/jobs/sync-bank-transactions/run
```

Manual run:

- Admin/Member/Accountant? Sync API manual run chua chot rieng.
- Doi soat manual run da chot Admin/Member/Accountant.
- De an toan: sync API manual run nen Admin/Accountant trong v1, co the hoi lai
  truoc khi code.

### 8.7. External API mapping

Chua code mapping cu the cho bank/ticket cost neu chua co Postman/response.

Co the lam abstraction va fake adapter:

- `IParkBalanceApiClient`
- `ITicketCostApiClient`
- `IBankTransactionApiClient`

Khi user gui Postman/response:

- Cap nhat DTO response.
- Cap nhat parser.
- Cap nhat raw response save.

### 8.8. Verify Phase 7

Done khi:

- JobRun/JobRunItem tao duoc.
- Fake/manual job chay duoc.
- Raw response luu duoc.
- Summary upsert dung.
- Loi item khong lam dung ca job.
- Manual entry resolve loi.

---

## 9. Phase 8 - Reconciliation

Muc tieu: build/rebuild doi soat theo ngay + KVC, MissingData, resolve variance.

### 9.1. Build logic

Input:

- `BusinessDate`.

For each active/not deleted Park:

1. Tim previous business date:
   - Co the la `BusinessDate.AddDays(-1)` trong v1.
   - Sau nay co the bo qua ngay nghi neu can.
2. Lay `DailyParkBalanceSnapshots` T-1.
3. Lay `DailyParkBalanceSnapshots` T.
4. Lay `DailyBankTransactionSummaries` T.
5. Lay `DailyTicketCostSummaries` T.
6. Tinh missing flags.
7. Neu thieu -> `MissingData`.
8. Neu du:
   - `ExpectedBalance = PreviousBalance + AdditionalAmount - UsedAmount`
   - `VarianceAmount = ActualBalance - ExpectedBalance`
   - 0 -> `Matched`
   - != 0 -> `Variance`
9. Tinh `LastSourceHash`.
10. Upsert `ParkReconciliations`.

### 9.2. Source hash

Hash input nen gom:

- ParkId
- BusinessDate
- PreviousBalance snapshot id/updatedAt/value
- ActualBalance snapshot id/updatedAt/value
- Bank summary id/updatedAt/value
- Ticket cost summary id/updatedAt/value

Can deterministic string JSON roi SHA256.

Khi resolve:

- `ResolvedSourceHash = LastSourceHash`.

Khi rebuild:

- Neu status `Resolved` va `ResolvedSourceHash != LastSourceHash`:
  - `SourceChangedAfterResolved = true`.
  - Giu adjustment/status.

### 9.3. Endpoints

```http
GET  /api/reconciliations?page=1&businessDate=&parkId=&paymentType=&status=
GET  /api/reconciliations/{id}
POST /api/reconciliations/build
POST /api/reconciliations/{id}/resolve
```

Build manual:

- Admin/Member/Accountant duoc bam.

Resolve:

- Chua chot role rieng, nen de Admin/Accountant la an toan hon.
- Neu user muon Member resolve thi hoi lai.

Resolve body:

```json
{
  "adjustmentAmount": 1250000,
  "adjustmentNote": "Dieu chinh do API KVC ghi nhan tre"
}
```

### 9.4. Audit

Audit:

- Build manual job.
- Resolve variance before/after.

### 9.5. Verify Phase 8

Done khi:

- Build tao records theo ngay.
- Missing data van tao record.
- Variance tinh dung.
- Resolve luu adjustment.
- Rebuild sau resolved voi source doi set `SourceChangedAfterResolved`.

---

## 10. Phase 9 - Email Summary And Cleanup

Muc tieu: gui email tong hop loi job va cleanup audit logs.

### 10.1. Notification recipients

Endpoints Admin optional v1:

```http
GET  /api/notification-recipients?page=1&notificationType=&active=
POST /api/notification-recipients
PUT  /api/notification-recipients/{id}
DELETE /api/notification-recipients/{id}
```

Neu chua lam UI/API, co the seed/chinh DB/script truoc.

### 10.2. Email sender

Infrastructure:

```text
IEmailSender
SmtpEmailSender
```

Config tu appsettings/environment:

- Host
- Port
- Username
- Password
- FromEmail
- EnableSsl

Khong luu SMTP password trong DB.

### 10.3. Sync error summary

Job:

- Lay JobRunItems failed chua manual resolved theo BusinessDate.
- Group theo JobName/Source/KVC.
- Lay recipients `SyncErrorSummary`.
- Gui email tong hop.
- Neu SMTP thieu config:
  - Log warning.
  - Khong fail job chinh.

### 10.4. Cleanup audit logs

Job:

- Doc `Audit.RetentionDays`, default 365.
- Xoa AuditLogs cu hon retention.
- Ghi JobRun.

### 10.5. Verify Phase 9

Done khi:

- Config SMTP trong dev co the disable.
- Job email khong fail khi SMTP chua config.
- Neu co fake SMTP/config, email body gom du loi.
- Cleanup xoa audit cu dung retention.

---

## 11. Phase 10 - Frontend Integration

Muc tieu: ket noi frontend Vue voi backend theo API moi.

Chi lam sau khi backend core on.

### 11.1. Frontend API layer

Tao trong `frontend/src`:

```text
services/
  apiClient.ts
  authApi.ts
  parksApi.ts
  jobsApi.ts
  reconciliationApi.ts
```

Api client:

- Base URL dev: `http://localhost:<api-port>/api`.
- Attach access token Bearer.
- `credentials: include` de gui refresh cookie.
- Xu ly 401:
  - Goi `/api/auth/refresh`.
  - Retry request.
  - Neu refresh fail -> redirect login.

### 11.2. Login UI

Can them route/login view:

- Email/password.
- Hien message tieng Viet.
- Luu access token trong memory hoac storage theo quyet dinh frontend.

Khuyen nghi:

- Access token trong memory neu co the.
- Neu reload page, goi refresh de lay access token moi.

### 11.3. Replace mock data

Thu tu:

1. Auth/login/me.
2. AppShell user info/logout.
3. System settings user/profile/device.
4. ParkCodesView -> Parks/ParkTicketTypes API.
5. Report pages Khu vui choi -> summary/reconciliation API.
6. Them menu:
   - Nhat ky thay doi
   - Loi dong bo can xu ly

### 11.4. Static deploy

Build frontend:

```powershell
cd frontend
npm run build
```

Copy dist vao:

```text
backend/src/PpvRecon.Api/wwwroot
```

.NET serve static files va fallback index.

---

## 12. Suggested Work Order For Next Coding Session

Neu sau 5 tieng quay lai va bat dau code, nen lam dung thu tu sau:

### Session A - Scaffold only

1. Tao `backend` solution/projects.
2. Add references/packages.
3. Configure Serilog/Swagger/CORS/health.
4. Build/run.
5. Dung lai, verify.

Expected output:

- Backend skeleton chay duoc.
- Chua can entity/migration.

### Session B - DB Initial

1. Tao enums/entities theo `database-design.md`.
2. Tao DbContext/configurations.
3. Configure SQLite.
4. Tao migration `InitialCreate`.
5. Update database.
6. Verify schema.

Expected output:

- SQLite DB co schema.
- Build pass.

### Session C - Auth

1. Response wrapper + exception middleware.
2. Password hasher.
3. JWT/refresh token service.
4. AuthController login/refresh/logout/me.
5. Seed Admin script/command.
6. Test login/refresh/logout.

Expected output:

- Co the dang nhap that.

### Session D - User/Profile

1. Profile APIs.
2. Avatar upload.
3. Device/session APIs.
4. Admin user management.

### Session E - Parks + Audit

1. Audit service.
2. Parks CRUD.
3. ParkTicketTypes CRUD.
4. Inactive/soft delete permissions.
5. Seed parks SQL template.

Sau Session E, frontend co the bat dau thay mock cho danh muc KVC.

---

## 13. Test Checklist Chung

Moi phase can toi thieu:

- `dotnet build`
- Neu co tests thi `dotnet test`
- Chay API local
- Swagger endpoint duoc
- Health check duoc
- Kiem tra git diff khong co file rac

Auth manual test:

- Login dung.
- Login sai 1,2 lan.
- Login sai lan 3 -> locked.
- Locked user khong login duoc.
- Admin unlock.
- Refresh token rotate.
- Logout revoke.

DB manual test:

- Unique email.
- Unique Park.Code.
- Unique ParkTicketType ParkId + TicketTypeCode.
- Summary upsert khong tao trung.
- Reconciliation upsert khong tao trung.

Permissions manual test:

- Member khong soft delete Park.
- Accountant manual entry duoc.
- Member manual entry khong duoc.
- Admin xem audit full.

---

## 14. Cac Viec Chua Nen Lam Ngay

De tranh qua tai v1:

- Chua can SSO/Google/Microsoft.
- Chua can export Excel/CSV audit log.
- Chua can import Excel danh muc KVC.
- Chua can luu detail booking/ticket/giao dich ngan hang.
- Chua can UI chinh system settings DB.
- Chua can tach worker service rieng.
- Chua can multi-tenant.
- Chua can approval workflow cho Member/Ketoan, vi thay doi co hieu luc ngay.

---

## 15. Rủi Ro Can De Y

### 15.1. Cookie refresh token voi Vue dev

Refresh token dung HttpOnly cookie. Khi frontend Vite va backend khac origin,
cookie policy co the can tinh ky:

- `credentials: include`
- CORS allow credentials
- SameSite/Secure phu hop local/prod

Neu local HTTP gap kho, co the dung Vite proxy de frontend goi cung origin.

### 15.2. SQLite DateOnly

Can test EF Core SQLite mapping `DateOnly`.

Neu gap loi:

- Configure converter DateOnly <-> string `yyyy-MM-dd`
- Hoac DateOnly <-> DateTime 00:00:00, nhung domain van giu DateOnly.

### 15.3. Long money

VND dang chan nen `long` ok. Neu API tra tien co decimal:

- Can chot rounding.
- Hoac migration sang decimal/string/integer minor unit.

### 15.4. Raw response size

SQLite luu raw JSON co the phinh to. V1 chap nhan. Sau nay neu raw response lon:

- Nen nen gzip.
- Hoac luu file/object storage va DB luu path/hash.

### 15.5. Source hash

Can hash deterministic. Dung sorted JSON/string format on dinh.

Neu hash tinh tu UpdatedAt, moi update audit co the doi hash khong mong muon.
Nen hash tu gia tri nghiep vu va source record ids/version can thiet, khong dua
nhung timestamp khong lien quan.

---

## 16. Quick Start For Next AI

Neu ban la AI tiep theo, hay lam nhu sau:

1. Doc `docs/backend-design.md`.
2. Doc `docs/database-design.md`.
3. Doc file nay.
4. Kiem tra repo status.
5. Neu user bao "bat dau code backend", thuc hien **Session A - Scaffold only**.
6. Khong nhay thang vao auth/job/frontend.
7. Sau moi session, build/test va bao tom tat file da tao.

