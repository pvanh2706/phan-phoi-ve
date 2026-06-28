# Chức năng menu "Mã khu vui chơi"

Menu **Mã khu vui chơi** dùng để quản lý danh mục KVC cha và danh mục KVC con/loại vé. Màn hình có 3 tab:

| Tab trên giao diện | Bảng dữ liệu chính | Chức năng |
| --- | --- | --- |
| Mã KVC | `Parks` | Quản lý danh sách khu vui chơi gốc/cha, loại thanh toán, mã gọi API, tài khoản ngân hàng, trạng thái sử dụng. |
| Danh mục KVC con nạp trước | `ParkTicketTypes` + liên kết `Parks` | Quản lý loại vé/KVC con thuộc các KVC có `PaymentType = Prepaid`. |
| Danh mục KVC con công nợ | `ParkTicketTypes` + liên kết `Parks` | Quản lý loại vé/KVC con thuộc các KVC có `PaymentType = Debt`. |

Khi người dùng thêm, sửa, ngừng sử dụng hoặc xóa mềm dữ liệu trong màn này, hệ thống còn ghi lịch sử thao tác vào bảng `AuditLogs`.

## Bảng `Parks`

**Chức năng bảng:** Lưu danh mục khu vui chơi cha. Đây là bảng nguồn để màn "Mã KVC" hiển thị, lọc theo từ khóa, lọc theo loại thanh toán, lọc theo trạng thái, thêm mới, cập nhật, ngừng sử dụng, xóa mềm và khôi phục dữ liệu.

**Ràng buộc chính:**

| Nội dung | Giá trị |
| --- | --- |
| Khóa chính | `Id` |
| Khóa duy nhất | `Code` |
| Chỉ mục | `Name`, `PaymentType`, `Status`, `IsDeleted` |
| Xóa dữ liệu | Xóa mềm bằng `IsDeleted`, `DeletedAtUtc`, `DeletedByUserId` |

| Tên cột | Kiểu dữ liệu/giới hạn | Bắt buộc | Chức năng của cột |
| --- | --- | --- | --- |
| `Id` | `int` | Có | Khóa chính tự tăng của khu vui chơi. |
| `Code` | `string`, tối đa 50 ký tự | Có | Mã KVC hiển thị trên màn hình, dùng định danh khu vui chơi và liên kết nghiệp vụ. Không được trùng. |
| `Name` | `string`, tối đa 300 ký tự | Có | Tên đầy đủ của khu vui chơi. Dùng để hiển thị, tìm kiếm và sắp xếp danh sách. |
| `PaymentType` | enum `ParkPaymentType` | Có | Loại thanh toán của KVC: `Prepaid` là nạp trước, `Debt` là công nợ. Dùng để phân nhóm và lọc dữ liệu. |
| `SearchCode` | `string`, tối đa 100 ký tự | Không | Mã tìm kiếm hoặc alias dùng khi gọi API bên ngoài; thường trùng với `Code`. |
| `Location` | `string`, tối đa 300 ký tự | Không | Địa điểm hoặc mô tả vị trí của khu vui chơi. |
| `BankAccount` | `string`, tối đa 100 ký tự | Không | Số tài khoản hoặc mã tài khoản nhận tiền/thanh toán, dùng trong đối soát và tra cứu giao dịch. |
| `BankName` | `string`, tối đa 200 ký tự | Không | Tên ngân hàng của tài khoản nhận tiền/thanh toán. |
| `CreditLimit` | `long?` | Không | Hạn mức công nợ của KVC, áp dụng chủ yếu cho nhóm `Debt`. |
| `ApiSiteId` | `string`, tối đa 100 ký tự | Không | Mã site dùng cho kết nối API của KVC. |
| `ApiProfileId` | `string`, tối đa 100 ký tự | Không | Mã profile dùng cho kết nối API của KVC. |
| `BalanceTransformRule` | `string`, tối đa 100 ký tự | Không | Quy tắc biến đổi số dư khi đồng bộ từ API, ví dụ đảo dấu hoặc giữ nguyên tùy nguồn dữ liệu. |
| `ApiNote` | `string`, tối đa 1000 ký tự | Không | Ghi chú kỹ thuật hoặc lưu ý khi gọi API của KVC. |
| `Status` | enum `RecordStatus` | Có | Trạng thái bản ghi: `Active` là đang hoạt động, `Inactive` là ngừng sử dụng. |
| `CreatedAtUtc` | `DateTime` | Có | Thời điểm tạo bản ghi theo UTC. |
| `CreatedByUserId` | `int?` | Không | Người dùng tạo bản ghi. Liên kết tới bảng người dùng nếu có dữ liệu. |
| `UpdatedAtUtc` | `DateTime?` | Không | Thời điểm cập nhật gần nhất theo UTC. |
| `UpdatedByUserId` | `int?` | Không | Người dùng cập nhật gần nhất. |
| `IsDeleted` | `bool` | Có | Cờ xóa mềm. API danh sách chỉ lấy bản ghi có `IsDeleted = false`. |
| `DeletedAtUtc` | `DateTime?` | Không | Thời điểm xóa mềm theo UTC. |
| `DeletedByUserId` | `int?` | Không | Người dùng thực hiện xóa mềm. |

## Bảng `ParkTicketTypes`

**Chức năng bảng:** Lưu danh mục KVC con/loại vé của từng KVC cha. Hai tab "Danh mục KVC con nạp trước" và "Danh mục KVC con công nợ" cùng dùng bảng này, sau đó lọc theo `PaymentType` của bảng `Parks`.

**Ràng buộc chính:**

| Nội dung | Giá trị |
| --- | --- |
| Khóa chính | `Id` |
| Khóa ngoại | `ParkId` tham chiếu `Parks.Id` |
| Khóa duy nhất | Cặp `ParkId`, `TicketTypeCode` |
| Chỉ mục | `ParkId`, `Status`, `IsDeleted`, cặp `ParkId` + `Code` |
| Xóa dữ liệu | Xóa mềm bằng `IsDeleted`, `DeletedAtUtc`, `DeletedByUserId` |

| Tên cột | Kiểu dữ liệu/giới hạn | Bắt buộc | Chức năng của cột |
| --- | --- | --- | --- |
| `Id` | `int` | Có | Khóa chính tự tăng của KVC con/loại vé. |
| `ParkId` | `int` | Có | Khóa ngoại liên kết KVC con với KVC cha trong bảng `Parks`. |
| `Code` | `string`, tối đa 100 ký tự | Có | Mã dòng hoặc mã KVC con hiển thị trên màn hình. |
| `TicketTypeCode` | `string`, tối đa 100 ký tự | Có | Mã loại vé trong API hoặc hệ thống nguồn. Không được trùng trong cùng một `ParkId`. |
| `Name` | `string`, tối đa 500 ký tự | Có | Tên loại vé/KVC con, ví dụ vé người lớn, vé trẻ em, ngày thường, cuối tuần. |
| `TicketGroupName` | `string`, tối đa 500 ký tự | Không | Nhóm loại vé dùng để gom các vé cùng khu hoặc cùng nhóm sản phẩm. |
| `CostPrice` | `long` | Có | Giá vốn của loại vé, dùng để tính giá vốn vé bán trong báo cáo chi tiết. Không được âm theo kiểm tra ở API. |
| `Status` | enum `RecordStatus` | Có | Trạng thái bản ghi: `Active` là đang hoạt động, `Inactive` là ngừng sử dụng. |
| `CreatedAtUtc` | `DateTime` | Có | Thời điểm tạo bản ghi theo UTC. |
| `CreatedByUserId` | `int?` | Không | Người dùng tạo bản ghi. |
| `UpdatedAtUtc` | `DateTime?` | Không | Thời điểm cập nhật gần nhất theo UTC. |
| `UpdatedByUserId` | `int?` | Không | Người dùng cập nhật gần nhất. |
| `IsDeleted` | `bool` | Có | Cờ xóa mềm. API danh sách chỉ lấy bản ghi có `IsDeleted = false`. |
| `DeletedAtUtc` | `DateTime?` | Không | Thời điểm xóa mềm theo UTC. |
| `DeletedByUserId` | `int?` | Không | Người dùng thực hiện xóa mềm. |

**Các cột hiển thị nhưng lấy từ bảng `Parks`:**

| Tên hiển thị trên màn hình | Nguồn dữ liệu | Chức năng |
| --- | --- | --- |
| Mã KVC cha | `Parks.Code` | Cho biết KVC cha của loại vé/KVC con. |
| Tên KVC cha | `Parks.Name` | Hiển thị tên đầy đủ của KVC cha. |
| Loại thanh toán | `Parks.PaymentType` | Dùng để tách tab nạp trước và công nợ. |

## Bảng `AuditLogs`

**Chức năng bảng:** Lưu lịch sử thao tác phát sinh từ màn "Mã khu vui chơi", bao gồm tạo mới, cập nhật, ngừng sử dụng, khôi phục và xóa mềm KVC hoặc loại vé. Đây là bảng liên quan để truy vết thay đổi, không phải bảng danh mục chính của màn.

| Tên cột | Kiểu dữ liệu/giới hạn | Bắt buộc | Chức năng của cột |
| --- | --- | --- | --- |
| `Id` | `int` | Có | Khóa chính tự tăng của bản ghi lịch sử. |
| `OccurredAtUtc` | `DateTime` | Có | Thời điểm xảy ra thao tác theo UTC. |
| `UserId` | `int?` | Không | Người dùng thực hiện thao tác. |
| `UserEmailSnapshot` | `string`, tối đa 320 ký tự | Không | Email người dùng tại thời điểm thao tác. |
| `UserRoleSnapshot` | `string`, tối đa 50 ký tự | Không | Vai trò người dùng tại thời điểm thao tác. |
| `Module` | `string`, tối đa 100 ký tự | Có | Module nghiệp vụ; với màn này thường là `Park`. |
| `EntityName` | `string`, tối đa 200 ký tự | Có | Tên đối tượng bị thay đổi, ví dụ `Park` hoặc `ParkTicketType`. |
| `EntityId` | `string`, tối đa 100 ký tự | Không | Id của bản ghi bị tác động. |
| `Action` | enum `AuditAction` | Có | Hành động đã thực hiện: `Create`, `Update`, `SetInactive`, `Restore`, `SoftDelete`. |
| `BeforeJson` | `string?` | Không | Dữ liệu trước khi thay đổi, lưu dạng JSON. |
| `AfterJson` | `string?` | Không | Dữ liệu sau khi thay đổi, lưu dạng JSON. |
| `IpAddress` | `string`, tối đa 100 ký tự | Không | Địa chỉ IP của người thao tác. |
| `UserAgent` | `string`, tối đa 1000 ký tự | Không | Thông tin trình duyệt/thiết bị của người thao tác. |
| `CorrelationId` | `string`, tối đa 100 ký tự | Không | Mã liên kết request để hỗ trợ truy vết lỗi và log hệ thống. |

## Chức năng chính theo bảng

| Chức năng trên màn | Bảng tác động | Ghi chú |
| --- | --- | --- |
| Xem danh sách Mã KVC | `Parks` | Chỉ hiển thị bản ghi chưa xóa mềm. |
| Tìm kiếm Mã KVC | `Parks` | Tìm theo `Code`, `Name`, `SearchCode`, `BankAccount`. |
| Lọc Mã KVC theo loại | `Parks` | Lọc theo `PaymentType`. |
| Lọc Mã KVC theo trạng thái | `Parks` | Lọc theo `Status`. |
| Thêm KVC | `Parks`, `AuditLogs` | Tạo bản ghi KVC và ghi log `Create`. |
| Sửa KVC | `Parks`, `AuditLogs` | Cập nhật thông tin KVC và ghi log `Update`. |
| Ngừng sử dụng KVC | `Parks`, `AuditLogs` | Đặt `Status = Inactive` và ghi log `SetInactive`. |
| Xóa mềm KVC | `Parks`, `AuditLogs` | Đặt `IsDeleted = true`, `Status = Inactive` và ghi log `SoftDelete`. |
| Khôi phục KVC đã xóa | `Parks`, `AuditLogs` | Chỉ Admin, đặt lại `IsDeleted = false` và ghi log `Restore`. |
| Xem danh sách KVC con nạp trước | `ParkTicketTypes`, `Parks` | Lọc loại vé theo KVC cha có `PaymentType = Prepaid`. |
| Xem danh sách KVC con công nợ | `ParkTicketTypes`, `Parks` | Lọc loại vé theo KVC cha có `PaymentType = Debt`. |
| Tìm kiếm KVC con | `ParkTicketTypes`, `Parks` | Tìm theo mã KVC con, mã loại vé, tên loại vé, mã KVC cha, tên KVC cha. |
| Thêm KVC con/loại vé | `ParkTicketTypes`, `AuditLogs` | Tạo loại vé và ghi log `Create`. |
| Sửa KVC con/loại vé | `ParkTicketTypes`, `AuditLogs` | Cập nhật loại vé và ghi log `Update`. |
| Ngừng sử dụng KVC con/loại vé | `ParkTicketTypes`, `AuditLogs` | Đặt `Status = Inactive` và ghi log `SetInactive`. |
| Xóa mềm KVC con/loại vé | `ParkTicketTypes`, `AuditLogs` | Đặt `IsDeleted = true`, `Status = Inactive` và ghi log `SoftDelete`. |
