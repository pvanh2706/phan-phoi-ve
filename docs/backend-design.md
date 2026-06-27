# Backend Design - PpvRecon

Tai lieu nay tong hop cac quyet dinh da thong nhat cho backend cua du an
**Doi soat phan phoi ve**. Muc tieu la de sau nay, ke ca sau vai thang,
nguoi doc hoac AI co the hieu lai boi canh va tiep tuc code backend ma
khong can hoi lai tu dau.

Ngay tong hop: 2026-06-27 theo gio Asia/Bangkok.

---

## 1. Ten Du An Va Pham Vi V1

### 1.1. Ten du an

- Ten hien thi: **Doi soat phan phoi ve**
- Ten ky thuat: **PpvRecon**
- Solution .NET de xuat: `PpvRecon.sln`
- Namespace goc: `PpvRecon`

Ly do chon `PpvRecon`: ten ngan gon, de dat namespace, van giu duoc y nghia
PPV va nghiep vu doi soat.

### 1.2. Pham vi v1

V1 chi tap trung vao:

- Dang nhap that va quan ly tai khoan noi bo.
- Phan **Cai dat**.
- Menu cha **Khu vui choi**.
- Job dong bo API, tong hop du lieu va doi soat.
- Audit log va xu ly loi dong bo.

Tam thoi bo qua cac menu khac de nang cap sau:

- Dai ly
- Khach le
- Doi soat Vin
- Cac dai ly OTA
- Quy trinh doi soat ngoai pham vi Khu vui choi

### 1.3. Nguon thong tin da doc

Frontend hien la Vue/Vite trong thu muc `frontend`. Frontend co mock UI va
data demo trong `src/data`, co the dung de xac dinh man hinh, filter, modal,
bang du lieu va API can phuc vu.

Ngoai frontend con co:

- `docs/Cai dat/*`
- `docs/Khu vui choi/*`
- `API/API So du KVC.xlsx`

Khi thiet ke backend, uu tien nghiep vu trong `docs` va file Excel hon mock
data trong frontend, vi frontend dang don gian hoa nhieu truong.

File Excel `API/API So du KVC.xlsx` co endpoint va cau hinh quan trong cho API
so du KVC:

- Endpoint: `http://api-ezcmt.ezticket.com.vn/gw/common/check-ar`
- Cac truong cau hinh: ten KVC, ma KVC, tai khoan ngan hang, `siteID`,
  `profileID`, ghi chu.
- Mot so KVC co ghi chu xu ly dac biet nhu `balance * -1`.

---

## 2. Kien Truc Backend

### 2.1. Cong nghe

- Backend: **.NET 10**
- Database v1: **SQLite**
- ORM: **EF Core code-first + migrations**
- API style: Controller, khong dung Minimal API.
- API prefix: `/api`
- Logging: Serilog
- Health check: `/health`
- Swagger/OpenAPI: bat trong moi truong dev.

May dev hien da co .NET SDK 10.0.201, co the tao project .NET 10.

### 2.2. Thu muc backend

Backend dat trong thu muc moi:

```text
backend/
```

Khong dung thu muc `API` hien co vi `API` dang chua tai lieu/file Excel.

### 2.3. Kien truc solution

Dung modular monolith voi 4 project:

```text
backend/
  PpvRecon.sln
  src/
    PpvRecon.Api/
    PpvRecon.Application/
    PpvRecon.Domain/
    PpvRecon.Infrastructure/
```

Vai tro tung project:

- `PpvRecon.Api`
  - Controllers
  - Middleware
  - Swagger
  - Authentication/JWT setup
  - CORS
  - Static file serving cho frontend khi deploy
  - Hosted services/scheduler
  - `appsettings`

- `PpvRecon.Application`
  - DTOs
  - Use cases/services nghiep vu
  - Validation
  - Response wrapper
  - Permission checks o muc ung dung
  - Orchestration job va import/manual entry

- `PpvRecon.Domain`
  - Entity core
  - Enum
  - Rule nghiep vu khong phu thuoc EF/API
  - Constants/domain errors neu can

- `PpvRecon.Infrastructure`
  - EF Core DbContext
  - SQLite provider
  - Migrations
  - Repository/data access neu can
  - SMTP email sender
  - External API clients
  - File storage/avatar storage
  - Password/token hashing implementation

Ly do: du an noi bo nhung se mo rong them module sau nay. Tach project tu dau
giup tranh viec API project qua lon, nhung van giu mot deployable monolith don
gian.

### 2.4. Frontend deploy

Co 2 che do:

- Development:
  - Vue/Vite chay rieng.
  - .NET chi la JSON API.
  - CORS cho:
    - `http://localhost:5173`
    - `http://127.0.0.1:5173`

- Deploy noi bo:
  - Vue build ra static files.
  - .NET serve static files tu `wwwroot`.
  - SPA fallback ve `index.html`.
  - API van o `/api/...`.

### 2.5. Logging

Them Serilog ngay tu v1:

- Console log
- Rolling file theo ngay
- Thu muc log: `backend/logs`

Can dam bao khong log token, password, refresh token raw, SMTP password.

---

## 3. Chuan API

### 3.1. Response wrapper

Moi API tra ve body thong nhat:

```json
{
  "success": true,
  "data": {},
  "message": "Thanh cong",
  "errors": []
}
```

Khi loi:

```json
{
  "success": false,
  "data": null,
  "message": "Du lieu khong hop le",
  "errors": [
    {
      "field": "email",
      "message": "Email khong dung dinh dang"
    }
  ]
}
```

Luu y:

- Message tra ve cho nguoi dung bang tieng Viet co dau khi code thuc te.
- Trong tai lieu nay co the khong dau o mot so noi de tranh loi encoding, nhung
  API/frontend can hien thi tieng Viet co dau.
- HTTP status van dung dung nghia: 200/201/400/401/403/404/409/500.

### 3.2. Pagination

Tat ca API list dung phan trang co dinh:

- Client chi truyen `page`.
- `pageSize` co dinh la **100**.
- Response can co:
  - `items`
  - `page`
  - `pageSize`
  - `totalItems`
  - `totalPages`

Vi du query:

```http
GET /api/parks?page=1&keyword=ban&status=Active
```

### 3.3. Filter/search

Dung query string don gian:

```http
GET /api/parks?page=1&keyword=sun&type=Prepaid&status=Active
GET /api/reconciliations?page=1&businessDate=2026-06-25&status=Variance
```

Khong can query DSL phuc tap trong v1.

### 3.4. Enum trong DB/API

Trong code/DB luu enum bang tieng Anh on dinh. Frontend hien thi tieng Viet.

Vi du:

- `Active` -> `Hoat dong`
- `Inactive` -> `Ngung su dung`
- `Prepaid` -> `Nap truoc`
- `Debt` -> `Cong no`
- `Matched` -> `Khop`
- `Variance` -> `Co lech`
- `MissingData` -> `Thieu du lieu`
- `Resolved` -> `Da xu ly`

---

## 4. Database Va Quy Tac Luu Tru

### 4.1. Database

- Dung SQLite trong v1.
- Dung EF Core code-first + migrations.
- Schema duoc quan ly bang migration, khong sua tay DB schema khi code da co
  migration.

### 4.2. Tien te

- So tien trong code/API dung `long`.
- SQLite luu bang `INTEGER` 64-bit.
- Don vi la VND.
- V1 tien dang chan, khong luu phan thap phan.
- Khong luu chuoi co dau phay, dau cham hoac ky hieu `d`.
- Format tien chi lam o frontend.
- Sau nay neu can tien le/thap phan, se migration sang kieu phu hop.

### 4.3. Ngay gio

- Timestamp he thong luu UTC:
  - `CreatedAtUtc`
  - `UpdatedAtUtc`
  - `DeletedAtUtc`
  - `StartedAtUtc`
  - `FinishedAtUtc`
  - `ResolvedAtUtc`

- Ngay nghiep vu/bao cao luu rieng:
  - `BusinessDate`
  - Theo ngay Viet Nam/Asia-Bangkok.

Ly do: job va doi soat chay theo ngay VN, con audit/log/session can timestamp
UTC de on dinh.

### 4.4. Soft delete

Khong xoa vat ly du lieu nghiep vu quan trong. Dung soft delete:

- `IsDeleted`
- `DeletedAtUtc`
- `DeletedByUserId`

Danh sach mac dinh phai loc `IsDeleted = false`.

Voi KVC, ma KVC khong duoc tai su dung sau khi xoa mem de tranh bao cao lich
su bi join nham. Neu xoa nham hoac KVC quay lai hop tac thi restore/cap nhat
ban ghi cu.

### 4.5. Audit fields chung

Nhung entity nghiep vu nen co:

- `CreatedAtUtc`
- `CreatedByUserId`
- `UpdatedAtUtc`
- `UpdatedByUserId`
- `IsDeleted`
- `DeletedAtUtc`
- `DeletedByUserId`

Khong phai bang nao cung bat buoc `IsDeleted`; bang log/raw response co the
khong can.

---

## 5. Authentication, Session Va Security

### 5.1. Dang nhap

V1 dung dang nhap noi bo:

- Email
- Password

Thiet ke nen de mo rong SSO/Google/Microsoft sau nay, nhung v1 chua tich hop.

### 5.2. Token

Dung ca access token va refresh token:

- Access token JWT: **15 phut**
- Refresh token/session: **5 tieng**
- Refresh token luu DB dang hash, khong luu token raw.
- Refresh token rotate moi lan refresh.
- Token cu bi revoke/thay bang token moi.

Co che cho SPA:

- Access token tra ve frontend va gui qua header:

```http
Authorization: Bearer <access_token>
```

- Refresh token luu trong **HttpOnly cookie**.
- Cookie `Secure` cau hinh theo moi truong:
  - Production: secure.
  - Dev local: cho phep chay HTTP neu can.
- Endpoint refresh doc refresh token tu cookie.

### 5.3. JWT claims

Access token chi chua claim toi thieu:

- UserId
- Email
- Role
- SessionId hoac JTI
- Exp

Permission chi tiet kiem tra trong backend service/DB khi can.

### 5.4. Sessions/Devices

Moi lan dang nhap tao session/refresh token record.

Session luu:

- UserId
- RefreshTokenHash
- ExpiresAtUtc
- RevokedAtUtc
- ReplacedByTokenHash hoac ReplacedBySessionId neu can
- CreatedAtUtc
- LastUsedAtUtc
- IpAddress
- UserAgent
- DeviceName/browser parsed neu co
- IsCurrent tinh o API theo current session

Moi user co the dang nhap nhieu thiet bi cung luc.

Tab Quan ly thiet bi:

- Hien thi cac session dang hoat dong.
- Cho user revoke tung thiet bi.
- Cho revoke ca thiet bi hien tai; frontend redirect ve login.

### 5.5. Password policy

Mat khau:

- Toi thieu 8 ky tu
- Co chu hoa
- Co chu thuong
- Co chu so
- Co ky tu dac biet

Sai mat khau lien tiep 3 lan:

- Khoa tai khoan.
- Tai khoan bi khoa den khi Admin mo lai hoac reset password.
- Khong gui email cho Admin.
- Hien thi trang thai khoa trong man Quan ly nguoi dung.

Khi Admin reset password:

- Admin tu nhap mat khau moi.
- Khong bat user doi lai mat khau o lan dang nhap tiep theo.

### 5.6. Revoke session theo su kien

- User doi mat khau: revoke toan bo session khac.
- Admin vo hieu hoa/khoa user: revoke toan bo session cua user do.
- Logout: revoke current session va clear refresh cookie.
- Logout all devices: revoke tat ca session cua user.

### 5.7. Admin dau tien

Admin dau tien duoc tao bang script PowerShell rieng, khong tu tao ngam khi app
chay.

Script nen:

- Hoi nhap tuong tac: ho ten, email, so dien thoai neu can.
- Nhap password dang an.
- Hash password.
- Ghi vao SQLite.
- Neu da co Admin thi can canh bao, khong tao trung neu chua xac nhan.

---

## 6. Roles Va Permissions

### 6.1. Roles

Moi user chi co **mot role chinh**.

Roles v1:

- `Admin`
- `Member`
- `Accountant` (hien thi tieng Viet: `Ke toan`)

### 6.2. Quyen tong quat

`Admin`:

- Toan quyen.
- Quan ly user.
- Quan ly tai khoan khach hang neu v1 lam.
- Xoa mem du lieu Khu vui choi.
- Xem audit log day du.
- Cau hinh he thong bang DB/script.

`Member`:

- Duoc thao tac nghiep vu trong Khu vui choi:
  - Them
  - Sua
  - Ngung su dung
  - Chay lai doi soat thu cong
- Khong duoc xoa mem.
- Khong duoc nhap tay du lieu thieu sau loi API/job.
- Khong duoc quan tri user/cau hinh nhay cam.
- Duoc sua thong tin ca nhan, theme, thong bao, doi mat khau, quan ly thiet bi
  cua minh.

`Accountant`:

- Co tat ca quyen nhu `Member` trong Khu vui choi.
- Duoc nhap tay du lieu thieu khi job/API loi.
- Duoc xem va xu ly man Loi dong bo can xu ly.
- Duoc chay lai doi soat thu cong.

### 6.3. Cai dat

Trong phan Cai dat:

- Admin/Member/Accountant:
  - Sua thong tin ca nhan.
  - Doi theme.
  - Cau hinh thong bao ca nhan.
  - Doi mat khau.
  - Quan ly thiet bi cua minh.

- Chi Admin:
  - Quan ly nguoi dung.
  - Tao user role `Admin`, `Member`, `Accountant`.
  - Reset password user.
  - Mo khoa/khoa/vu hieu hoa user.
  - Quan ly tai khoan khach hang neu lam trong v1.

---

## 7. Module Cai Dat

### 7.1. Thong tin tai khoan

User xem/sua thong tin ca nhan:

- Ho ten
- So dien thoai
- Avatar

Email readonly, khong duoc doi sau khi tao.
Role readonly voi user hien tai.

### 7.2. Avatar

Avatar upload luu trong backend:

```text
backend/PpvRecon.Api/wwwroot/uploads/avatars
```

DB chi luu path, vi du:

```text
/uploads/avatars/{userId}/avatar.webp
```

Quy tac:

- Chi nhan JPG/PNG/WebP.
- Toi da 2MB.
- Upload avatar moi thi xoa file avatar cu sau khi cap nhat thanh cong.

### 7.3. Giao dien

Frontend hien dang luu theme localStorage. Backend co the luu user preference sau:

- `ThemeMode`: `Dark`, `Light`, `System`
- Language neu sau nay can.

V1 co the giu localStorage hoac them API preference tuy muc code.

### 7.4. Thong bao

User co notification preference ca nhan. V1 co the luu:

- Loai thong bao
- Kenh email
- Kenh in-app
- Bat/tat

Phan email loi job dung recipient rieng, khong mac dinh theo preference ca nhan.

### 7.5. Quan ly nguoi dung

Chi Admin:

- List user
- Create user
- Edit user profile/role/status
- Reset password
- Unlock account
- Disable user

Email:

- Unique
- Readonly sau khi tao

### 7.6. Quan ly thiet bi

Dung session/refresh token records de hien thi:

- Browser/device
- IP
- Last used
- Dang hoat dong/het han/revoked

User duoc logout tung thiet bi.

---

## 8. Module Khu Vui Choi

### 8.1. Cac man hinh trong menu cha Khu vui choi

Frontend/docs hien co cac man:

- Danh sach khu vui choi
- Ma khu vui choi
- So du khu vui choi hang ngay
- Danh sach nap tien KVC theo ngay
- Chi tiet gia von ve ban
- Doi soat Khu vui choi
- KVC hoan tien

Can bo sung menu/man:

- Nhat ky thay doi
- Loi dong bo can xu ly

### 8.2. Park - KVC cha

KVC cha la danh muc goc. Entity de xuat: `Park`.

Truong chinh:

- `Id`
- `Code` - ma KVC trong he thong/API, unique vinh vien
- `Name`
- `PaymentType` - `Prepaid` hoac `Debt`
- `SearchCode` - ma dinh danh tim kiem/API alias neu khac `Code`
- `BankAccount`
- `BankName`
- `Location`
- `CreditLimit` - dung cho KVC cong no
- `ApiSiteId`
- `ApiProfileId`
- `BalanceTransformRule` hoac `BalanceMultiplier` neu can xu ly `balance * -1`
- `Status` - `Active` hoac `Inactive`
- Audit fields
- Soft delete fields

Loai thanh toan:

- `Prepaid`: nap truoc
- `Debt`: cong no

Trang thai ngung su dung:

- `Inactive`

Quyen:

- Admin/Member/Accountant: them/sua/ngung su dung.
- Chi Admin: xoa mem.

### 8.3. ParkTicketType - KVC con / loai ve

"KVC con" trong docs thuc chat la loai ve/nhom ve thuoc KVC cha. Entity de
xuat: `ParkTicketType`.

Truong chinh:

- `Id`
- `ParkId`
- `ChildCode` hoac `Code`
- `TicketTypeCode`
- `Name`
- `TicketGroupName`
- `CostPrice`
- `Status` - `Active` hoac `Inactive`
- Audit fields
- Soft delete fields

Quyen:

- Admin/Member/Accountant: them/sua/ngung su dung.
- Chi Admin: xoa mem.

### 8.4. Danh sach KVC

Danh sach KVC la man tong hop tu du lieu hien tai:

- KVC nap truoc:
  - So du hien tai
  - Tong da nap
  - Nap gan nhat
  - Trang thai/canh bao sap het

- KVC cong no:
  - Cong no hien tai
  - Han muc
  - Ngay den han neu co
  - Trang thai/canh bao gan han/qua han

V1 co the lay tu:

- `Park`
- Snapshot so du moi nhat
- Bank transaction summary moi nhat/tong
- Ticket cost summary/tong hop neu can

### 8.5. So du KVC hang ngay

Luu snapshot moi ngay cho moi KVC.

Entity de xuat: `DailyParkBalanceSnapshot`.

Unique key:

- `BusinessDate`
- `ParkId`

Truong chinh:

- `BusinessDate`
- `ParkId`
- `PaymentType`
- `AvailableBalance`
- `CurrentDebt` - neu KVC cong no/API co tra
- `BankAccount`
- `SourceType` - `Api` hoac `Manual`
- `SourceJobRunId`
- `ManualReason`
- Audit fields

Quy tac:

- Chay lai cung ngay thi update/upsert ban ghi cu.
- Luu raw response rieng.

### 8.6. Tong gia von ve ban

Theo quyet dinh cuoi cung, v1 **khong luu detail tung booking/ticket**. Thay
vao do:

- Luu raw API response JSON nguyen van.
- Code xu ly response roi luu tong theo `BusinessDate + Park`.

Entity de xuat: `DailyTicketCostSummary`.

Unique key:

- `BusinessDate`
- `ParkId`

Truong chinh:

- `BusinessDate`
- `ParkId`
- `PaymentType`
- `TotalTicketCost`
- `TotalSalesAmount` neu API co va can
- `TotalQuantity` neu can
- `SourceType` - `Api` hoac `Manual`
- `SourceJobRunId`
- `ManualReason`
- Audit fields

### 8.7. Tong giao dich ngan hang

Theo quyet dinh cuoi cung, v1 **khong luu tung giao dich ngan hang detail**.

Thay vao do:

- Luu raw API response JSON nguyen van.
- Code xu ly response roi luu tong theo `BusinessDate + Park`.
- Man nap tien/thanh toan cong no chap nhan hien thi dang tong hop ngay + KVC.

Entity de xuat: `DailyBankTransactionSummary`.

Unique key:

- `BusinessDate`
- `ParkId`
- `TransactionType`

`TransactionType` de xuat:

- `TopUp` - nap tien cho KVC nap truoc
- `DebtPayment` - thanh toan KVC cong no
- Co the them `Refund` sau neu can.

Truong chinh:

- `BusinessDate`
- `ParkId`
- `PaymentType`
- `TransactionType`
- `TotalDebitAmount`
- `TotalCreditAmount`
- `TransactionCount`
- `SourceType`
- `SourceJobRunId`
- `ManualReason`
- Audit fields

### 8.8. KVC hoan tien

Man KVC hoan tien quan ly thu cong cac truong hop hoan tien sau khi ve da ban
va gia von da ghi nhan.

Entity de xuat: `ParkRefund`.

Truong chinh:

- `BookingCode`
- `RefundDate` hoac `BusinessDate`
- `ParkId`
- `ParkCodeSnapshot`
- `ParkNameSnapshot`
- `TicketTypeCode`
- `TicketTypeName`
- `Quantity`
- `ParkRefundAmount`
- `CustomerRefundAmount`
- `Reason`
- `ParkRefundStatus`
- `CustomerRefundStatus`
- `PaymentType`
- Audit fields
- Soft delete fields neu can

Status DB tieng Anh, UI tieng Viet.

### 8.9. Ngung su dung vs xoa mem

Can phan biet:

- `Inactive` / Ngung su dung:
  - La trang thai nghiep vu.
  - Admin/Member/Accountant duoc thao tac.
  - Ban ghi van ton tai va co the xem theo filter.
  - Khong dung trong dropdown/mac dinh neu khong can.

- Soft delete:
  - Chi Admin.
  - Set `IsDeleted = true`.
  - Mac dinh an khoi cac man van hanh.
  - Khong tai su dung ma KVC.
  - Audit log phai ghi before/after.

---

## 9. Raw API Response

### 9.1. Nguyen tac

Voi API ben ngoai, v1 uu tien:

- Luu raw JSON nguyen van de tra cuu/audit.
- Luu bang nghiep vu da xu ly o dang tong hop.
- Khong normalize detail neu chua can.

### 9.2. Entity de xuat

`ExternalApiRawResponse`

Truong chinh:

- `Id`
- `Source` - vi du `ParkBalance`, `TicketCost`, `BankTransaction`
- `BusinessDate`
- `ParkId` nullable neu response chung khong theo KVC
- `JobRunId`
- `RequestUrl`
- `RequestPayloadJson`
- `ResponseStatusCode`
- `ResponseBodyJson`
- `ResponseReceivedAtUtc`
- `DurationMs`
- `IsSuccess`
- `ErrorMessage`

Can can nhac mask/thay the thong tin nhay cam trong request/response neu API co
tra token/credential.

---

## 10. Jobs Va Dong Bo API

### 10.1. Tong quan

Can tich hop API that ngay tu v1. Khi den buoc code API client, user se gui
Postman collection va response mau de mapping adapter.

Job chay trong backend .NET bang hosted service/scheduler.

Timezone nghiep vu:

- Asia/Bangkok / gio Viet Nam.

### 10.2. Cac job v1

Tach job rieng de de theo doi va chay lai:

- `SyncParkBalances`
- `SyncTicketCosts`
- `SyncBankTransactions`
- `BuildParkReconciliation`
- `SendDailySyncErrorSummary`
- `CleanupAuditLogs`

### 10.3. Lich chay

Muc tieu chung:

- Cac API tong hop du lieu trong ngay chay vao khoang **23:59** hang ngay.
- Cac job sync nguon du lieu chay doc lap vi API khong phu thuoc nhau.
- Doi soat la job/tac vu tong hop rieng, lay du lieu da sync trong DB de cong
  tru va so sanh.

Quyet dinh:

- Cac job sync chay doc lap.
- Doi soat co ca:
  - Chay tu dong sau khi du lieu da sync.
  - Nut chay thu cong cho mot ngay cu the.
- Admin, Member, Accountant deu duoc bam chay lai doi soat.

Email loi:

- Gui sau khi cac job API trong ngay ket thuc.
- Neu job/API treo qua timeout item thi item do ghi loi, job tiep tuc.

### 10.4. Timeout va retry

Cho tung API call:

- Timeout: 30 giay
- Retry: 2 lan
- Delay giua retry: 5 giay co dinh

Neu van loi:

- Ghi loi cho item/KVC do.
- Tiep tuc item/KVC khac.
- Khong dung ca job.

### 10.5. JobRun va JobRunItem

Can co bang ghi nhan lan chay job.

`JobRun` de xuat:

- `Id`
- `JobName`
- `BusinessDate`
- `TriggeredBy` - `Schedule`, `Manual`
- `TriggeredByUserId` nullable
- `StartedAtUtc`
- `FinishedAtUtc`
- `Status` - `Running`, `Succeeded`, `CompletedWithErrors`, `Failed`, `Canceled`
- `TotalItems`
- `SuccessItems`
- `FailedItems`
- `ErrorMessage`

`JobRunItem` de xuat:

- `Id`
- `JobRunId`
- `ParkId` nullable
- `BusinessDate`
- `Source`
- `Status` - `Pending`, `Succeeded`, `Failed`, `Skipped`, `ManualResolved`
- `AttemptCount`
- `StartedAtUtc`
- `FinishedAtUtc`
- `DurationMs`
- `ErrorCode`
- `ErrorMessage`
- `RawResponseId` nullable
- `ResolvedByUserId` nullable
- `ResolvedAtUtc` nullable
- `ManualDataReference` neu can tro toi record nhap tay

### 10.6. Loi dong bo can xu ly

Can them menu/man trong **Khu vui choi**:

**Loi dong bo can xu ly**

Nguoi xem/xu ly:

- Admin
- Accountant

Quyet dinh chua chot hoan toan:

- Member thuong co the xem hay khong chua bat buoc. De an toan v1 nen chi cho
  Admin va Accountant.

Man nay hien:

- Ngay du lieu
- Job/source
- KVC
- Error message
- So lan retry
- Thoi diem loi
- Trang thai xu ly
- Nut nhap tay du lieu thieu

Ly do nhap tay:

- Khi goi lai API sau do, du lieu co the lech sang ngay hom sau.
- Ke toan se nhap tay du lieu dung cho ngay bi loi.

### 10.7. Nhap tay du lieu thieu

Chi role:

- Admin
- Accountant

Du lieu nhap tay:

- Co hieu luc ngay.
- Ghi audit log before/after.
- Luu `SourceType = Manual`.
- Luu `ManualReason`.
- Luu `CreatedByUserId`/`UpdatedByUserId`.
- Gan lien voi loi/job item neu co.

Nguon co the nhap tay:

- So du KVC
- Tong gia von ve ban
- Tong giao dich ngan hang

---

## 11. Doi Soat Khu Vui Choi

### 11.1. Nguyen tac

Doi soat lay du lieu da sync/nhap tay trong DB de cong tru va so sanh.

Nguon du lieu:

- So du KVC:
  - So du T-1
  - So du T
- Giao dich ngan hang:
  - So nap them voi KVC `Prepaid`
  - So thanh toan voi KVC `Debt`
- Tong gia von ve ban:
  - So da dung trong ngay
- Hoan tien KVC neu ap dung trong cong thuc sau nay.

### 11.2. Cong thuc co ban

Theo docs:

```text
Ly thuyet = So du T-1 + So nap them/thanh toan - So da dung
Lech      = So du T - Ly thuyet
```

Voi KVC cong no, ten cot va y nghia co the la han muc/so du tin dung/cong no,
nhung v1 van luu ket qua tong hop theo cung nguyen tac doi soat da thong nhat.

### 11.3. Entity de xuat

`ParkReconciliation`

Unique key:

- `BusinessDate`
- `ParkId`

Truong chinh:

- `BusinessDate`
- `ParkId`
- `PaymentType`
- `PreviousBalance`
- `AdditionalAmount`
- `UsedAmount`
- `ExpectedBalance`
- `ActualBalance`
- `VarianceAmount`
- `Status`
- `MissingDataFlags` hoac bo boolean chi tiet:
  - `MissingPreviousBalance`
  - `MissingActualBalance`
  - `MissingTicketCost`
  - `MissingBankTransaction`
- `AdjustmentAmount`
- `AdjustmentNote`
- `ResolvedByUserId`
- `ResolvedAtUtc`
- `LastBuiltJobRunId`
- Audit fields

### 11.4. Trang thai doi soat

DB/code:

- `Matched`
- `Variance`
- `MissingData`
- `Resolved`

Frontend hien thi:

- `Matched` -> `Khop`
- `Variance` -> `Co lech`
- `MissingData` -> `Thieu du lieu`
- `Resolved` -> `Da xu ly`

### 11.5. Upsert/chay lai

Ket qua doi soat luu theo:

- `BusinessDate + Park`

Chay lai:

- Update ban ghi cu.
- Khong tao trung.
- Neu record da `Resolved`:
  - Van giu thong tin adjustment/resolution.
  - Rebuild lai cac so nguon.
  - Tinh `LastSourceHash` moi.
  - Neu `LastSourceHash` khac `ResolvedSourceHash`, set
    `SourceChangedAfterResolved = true`.
  - Khong tu xoa adjustment va khong tu chuyen status ve `Variance`.
  - UI can canh bao "du lieu nguon da thay doi sau khi xu ly".

### 11.6. Xu ly lech

Khi dong doi soat co lech:

- User nhap `AdjustmentAmount` am/duong.
- User nhap `AdjustmentNote`.
- Luu:
  - `ResolvedByUserId`
  - `ResolvedAtUtc`
  - `Status = Resolved`
- Khong sua raw response hoac du lieu nguon.
- Ghi audit log before/after.

Adjustment amount dung de ghi nhan phan dieu chinh. Khi hien thi co the tinh:

```text
VarianceAfterAdjustment = VarianceAmount + AdjustmentAmount
```

Neu `VarianceAfterAdjustment = 0` thi coi la da xu ly hop ly.

---

## 12. Audit Log

### 12.1. Nguyen tac

Moi thao tac quan trong trong Khu vui choi va security can ghi audit log.

Audit log phai ghi:

- Ai thao tac
- Luc nao
- Tu IP nao
- User-Agent nao
- Doi tuong nao
- Hanh dong nao
- Du lieu truoc
- Du lieu sau

### 12.2. Retention

Audit log luu **1 nam**.

Co job `CleanupAuditLogs` xoa log cu hon retention.

Retention days nen co trong System Settings DB:

- `Audit.RetentionDays = 365`

### 12.3. Entity de xuat

`AuditLog`

Truong chinh:

- `Id`
- `OccurredAtUtc`
- `UserId`
- `UserEmailSnapshot`
- `UserRoleSnapshot`
- `Module` - vi du `Park`, `Settings`, `Auth`, `Jobs`
- `EntityName`
- `EntityId`
- `Action` - `Create`, `Update`, `SetInactive`, `SoftDelete`, `Restore`,
  `RunJob`, `ManualEntry`, `ResolveVariance`, ...
- `BeforeJson`
- `AfterJson`
- `IpAddress`
- `UserAgent`
- `CorrelationId`

Khong ghi password/token vao before/after.

### 12.4. Cac thao tac can audit

Trong Khu vui choi:

- Them/sua/ngung su dung/xoa mem KVC.
- Them/sua/ngung su dung/xoa mem ParkTicketType.
- Chay job thu cong.
- Nhap tay du lieu thieu.
- Chay/tong hop lai doi soat.
- Danh dau xu ly lech va nhap adjustment.

Auth/security:

- Login thanh cong/that bai co the ghi security log rieng.
- Khoa user do sai password.
- Admin unlock/reset password/disable user.
- Revoke session.

### 12.5. Giao dien audit log

V1 can co audit log chi xem tren giao dien, chua can export Excel/CSV.

De xuat 2 lop giao dien:

1. Menu con chung trong **Khu vui choi**: `Nhat ky thay doi`
   - Cho tra cuu toan bo thay doi nghiep vu Khu vui choi.
   - Filter theo ngay, user, action, entity, KVC.

2. Nut `Lich su` tren tung ban ghi
   - Vi du trong dong KVC `Ban Mong`, bam `Lich su` de xem log rieng cua KVC.

Quyen xem:

- Admin: xem day du.
- Member/Accountant: xem audit log nghiep vu Khu vui choi o muc phu hop.
- Khong cho Member xem log nhay cam phan Cai dat/security.

---

## 13. Email Thong Bao Loi Job

### 13.1. Nguyen tac

Neu job API co loi, he thong gui email tong hop cho ke toan biet can vao sua
thu cong.

Khong gui email tung loi rieng le.

### 13.2. Thoi diem gui

Gui sau khi cac job API trong ngay ket thuc.

Khong can gio co dinh cung nhu 00:15. Dieu kien la job sync trong ngay da ket
thuc voi trang thai:

- `Succeeded`
- `CompletedWithErrors`
- `Failed` neu job fail tong

Neu SMTP chua cau hinh:

- Ghi log.
- Khong lam fail job chinh.

### 13.3. SMTP config

Thong tin nhay cam de trong `appsettings` hoac environment variable:

- Host
- Port
- Username
- Password
- From email
- Enable SSL

Khong luu SMTP password trong DB.

### 13.4. NotificationRecipients

Nguoi nhan email loi luu trong DB bang nhieu dong, khong mac dinh gui tat ca
role Accountant.

Entity de xuat: `NotificationRecipient`

Truong:

- `Id`
- `NotificationType`
- `Email`
- `DisplayName`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`

`NotificationType` v1:

- `SyncErrorSummary`

Sau nay co the them:

- `DailyReport`
- `ReconciliationVarianceAlert`

### 13.5. Noi dung email loi

Email tong hop nen co:

- Ngay du lieu
- Tong so loi
- So loi theo job/source
- Danh sach KVC loi
- Link hoac huong dan vao man `Loi dong bo can xu ly`
- Nhac rang ke toan can nhap tay du lieu thieu

---

## 14. System Settings

### 14.1. Nguyen tac

Co bang cau hinh he thong trong DB, nhung v1 chua can giao dien chinh sua.
Admin co the chinh bang DB/script truoc.

Thong tin nhay cam van de trong `appsettings`/environment.

### 14.2. Entity de xuat

`SystemSetting`

Truong:

- `Key`
- `Value`
- `ValueType` - `String`, `Int`, `Bool`, `Decimal`, `Json`
- `Description`
- `IsSensitive` - neu true thi khong tra raw value qua API
- `UpdatedAtUtc`
- `UpdatedByUserId`

### 14.3. Settings v1

De xuat seed:

- `Jobs.ScheduleTime` = `23:59`
- `Jobs.ApiTimeoutSeconds` = `30`
- `Jobs.ApiRetryCount` = `2`
- `Jobs.ApiRetryDelaySeconds` = `5`
- `Audit.RetentionDays` = `365`
- `Email.EnableSyncErrorSummary` = `true`

SMTP password khong luu DB.

---

## 15. Seed Va Script

### 15.1. Admin dau tien

Tao bang PowerShell script rieng:

```text
scripts/seed-admin.ps1
```

Script tuong tac, khong dua password vao command history.

### 15.2. Du lieu KVC ban dau

User khong uu tien import Excel trong v1. Co the dung SQL script de seed truc
tiep vao SQLite.

De xuat:

```text
scripts/seed-parks.sql
```

Script nen co comment ro:

- Cot bat buoc
- Cot co the bo trong
- Vi du KVC nap truoc
- Vi du KVC cong no
- Vi du `siteID`/`profileID`
- Vi du `BalanceTransformRule`

Sau nay neu can, co the thiet ke Excel template chuan de import.

---

## 16. Goi Y Entity Tong Hop

Danh sach entity de code v1. Ten co the dieu chinh khi implement nhung nen giu
y nghia.

Auth/settings:

- `User`
- `UserSession`
- `UserPreference`
- `NotificationPreference`
- `NotificationRecipient`
- `SystemSetting`
- `AuditLog`

Khu vui choi:

- `Park`
- `ParkTicketType`
- `DailyParkBalanceSnapshot`
- `DailyTicketCostSummary`
- `DailyBankTransactionSummary`
- `ParkRefund`
- `ParkReconciliation`

External API/job:

- `ExternalApiRawResponse`
- `JobRun`
- `JobRunItem`

Optional/security:

- `SecurityLog` neu muon tach auth events khoi audit log.

---

## 17. Goi Y API Endpoints V1

Day la goi y endpoint de code sau. Co the dieu chinh theo controller.

### 17.1. Auth

```http
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
POST /api/auth/logout-all
GET  /api/auth/me
```

### 17.2. Profile/settings ca nhan

```http
GET  /api/me/profile
PUT  /api/me/profile
POST /api/me/avatar
POST /api/me/change-password
GET  /api/me/sessions
POST /api/me/sessions/{sessionId}/revoke
GET  /api/me/preferences
PUT  /api/me/preferences
```

### 17.3. User management - Admin

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

### 17.4. Parks

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

`DELETE` o day la soft delete, chi Admin.

### 17.5. Park ticket types

```http
GET    /api/park-ticket-types?page=1&parkId=&keyword=&paymentType=&status=
POST   /api/park-ticket-types
GET    /api/park-ticket-types/{id}
PUT    /api/park-ticket-types/{id}
POST   /api/park-ticket-types/{id}/set-inactive
DELETE /api/park-ticket-types/{id}
GET    /api/park-ticket-types/{id}/audit-logs?page=1
```

### 17.6. Daily summaries

```http
GET /api/park-balances?page=1&businessDate=&parkId=&paymentType=&sourceType=
GET /api/ticket-cost-summaries?page=1&businessDate=&parkId=&paymentType=&sourceType=
GET /api/bank-transaction-summaries?page=1&businessDate=&parkId=&paymentType=&sourceType=
```

Manual entry endpoints for Admin/Accountant:

```http
POST /api/park-balances/manual
POST /api/ticket-cost-summaries/manual
POST /api/bank-transaction-summaries/manual
```

### 17.7. Jobs

```http
GET  /api/jobs/runs?page=1&jobName=&businessDate=&status=
GET  /api/jobs/runs/{id}
GET  /api/jobs/errors?page=1&businessDate=&source=&status=
POST /api/jobs/sync-park-balances/run
POST /api/jobs/sync-ticket-costs/run
POST /api/jobs/sync-bank-transactions/run
POST /api/jobs/reconciliation/run
POST /api/jobs/errors/{jobRunItemId}/resolve-manual
```

### 17.8. Reconciliation

```http
GET  /api/reconciliations?page=1&businessDate=&parkId=&paymentType=&status=
GET  /api/reconciliations/{id}
POST /api/reconciliations/build
POST /api/reconciliations/{id}/resolve
```

Resolve body gom:

```json
{
  "adjustmentAmount": 1250000,
  "adjustmentNote": "Dieu chinh do API KVC ghi nhan tre"
}
```

### 17.9. Audit logs

```http
GET /api/audit-logs?page=1&module=&entityName=&entityId=&userId=&action=&fromDate=&toDate=
```

Phan quyen view log can filter theo role.

---

## 18. Cac Diem Can Chu Y Khi Code

### 18.1. Khong luu du lieu nhay cam vao log/audit

Tuyet doi khong luu:

- Password raw
- Password hash trong audit before/after neu khong can
- Refresh token raw
- Access token
- SMTP password
- External API credentials

### 18.2. Idempotency

Nhung job chay lai phai idempotent:

- Balance snapshot: upsert theo `BusinessDate + ParkId`.
- Ticket cost summary: upsert theo `BusinessDate + ParkId`.
- Bank transaction summary: upsert theo `BusinessDate + ParkId + TransactionType`.
- Reconciliation: upsert theo `BusinessDate + ParkId`.
- Raw response: co the luu moi moi lan chay de tra cuu, gan voi `JobRunId`.

### 18.3. Missing data

Doi soat van tao record neu thieu nguon:

- Status = `MissingData`
- Ghi ro missing flags
- Hien tren UI de ke toan xu ly

### 18.4. Manual data

Du lieu nhap tay can phan biet voi API:

- `SourceType = Manual`
- `ManualReason`
- `CreatedByUserId`
- `UpdatedByUserId`
- Audit log before/after

### 18.5. Unique constraints voi soft delete

Voi `Park.Code`, khong cho trung ke ca ban ghi soft deleted.

Voi `ParkTicketType`, v1 chot constraint:

- Unique theo `ParkId + TicketTypeCode`.
- `ParkId + Code` co the de index phuc vu tra cuu.

### 18.6. Rebuild reconciliation after resolved

Neu dong doi soat da `Resolved`, sau do job rebuild lai va du lieu nguon thay
doi:

- Giu `AdjustmentAmount` va `AdjustmentNote`.
- Cap nhat so nguon moi.
- Tinh source hash tu cac nguon dau vao.
- Khi resolve, luu `ResolvedSourceHash`.
- Khi rebuild, neu `LastSourceHash` khac `ResolvedSourceHash`, set
  `SourceChangedAfterResolved = true`.
- Khong tu xoa adjustment va khong tu chuyen status ve `Variance`.
- UI can canh bao "du lieu nguon da thay doi sau khi xu ly".

---

## 19. Thu Tu Code De Xuat Sau Tai Lieu Nay

Day la plan goi y, chua phai yeu cau da thuc hien.

Phase 1 - Scaffold backend:

- Tao solution/project 4 lop.
- Cai package EF Core SQLite, JWT, Swagger, Serilog.
- Cau hinh response wrapper, exception middleware, health check, CORS.

Phase 2 - Auth/user:

- Entity User/UserSession.
- Password hashing.
- JWT + refresh cookie + rotation.
- Login/refresh/logout/me.
- Script tao Admin dau tien.

Phase 3 - Settings/user management:

- Profile/avatar.
- Change password.
- Device/session management.
- Admin user management.

Phase 4 - Khu vui choi danh muc:

- Park.
- ParkTicketType.
- Audit log.
- Soft delete/inactive.
- Seed SQL mau.

Phase 5 - Jobs/raw response/summaries:

- JobRun/JobRunItem.
- ExternalApiRawResponse.
- Summary tables.
- Scheduler shell.
- Manual entry.
- Error screen API.

Phase 6 - Reconciliation:

- Build reconciliation.
- Run manual/auto.
- MissingData.
- Resolve with adjustment.

Phase 7 - Email summary:

- NotificationRecipients.
- SMTP sender.
- Sync error summary job.

Phase 8 - Frontend integration:

- Login UI.
- API services.
- Replace mock data by API.
- Them menu Nhat ky thay doi va Loi dong bo can xu ly.

---

## 20. Tom Tat Quyet Dinh Chinh

- Backend .NET 10 trong thu muc `backend`.
- Modular monolith 4 project.
- SQLite + EF Core code-first migrations.
- Controller API, prefix `/api`.
- Response wrapper thong nhat.
- Pagination co dinh 100 item/trang.
- Login noi bo voi access token 15 phut + refresh token 5 tieng.
- Refresh token hash trong DB, HttpOnly cookie, rotate moi lan refresh.
- Roles: Admin, Member, Accountant; moi user chi mot role.
- Member duoc thao tac Khu vui choi nhung khong xoa mem/khong nhap tay loi job.
- Accountant = Member + quyen nhap tay du lieu thieu sau loi job.
- Chi Admin xoa mem.
- KVC xoa mem khong cho tai su dung ma.
- Audit log before/after JSON, IP, User-Agent, luu 1 nam.
- API raw response luu JSON, bang nghiep vu luu tong hop ngay + KVC.
- Job API timeout 30 giay/call, retry 2 lan, delay 5 giay.
- Loi item nao ghi item do, job tiep tuc.
- Ke toan nhap tay du lieu thieu vi goi lai API co the lech ngay.
- Email loi job gui tong hop sau khi cac job API ket thuc.
- SMTP password de appsettings/environment, recipient email luu DB nhieu dong.
- Doi soat luu theo BusinessDate + Park, chay lai update, co MissingData.
- Xu ly lech bang AdjustmentAmount am/duong va AdjustmentNote.
