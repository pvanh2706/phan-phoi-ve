# Plan khớp icon frontend với thiết kế HTML gốc

Ngày tạo: 2026-06-27

## 1. Bối cảnh

Frontend hiện tại đã có icon trở lại sau commit `0a87e3d Restore frontend icons`, nhưng icon chưa giống thiết kế ban đầu trong thư mục `HTML`.

Lần sửa trước đã thêm `frontend/src/components/ui/AppIcon.vue` và chuyển nhiều vị trí sang SVG line icon ổn định. Cách này giúp không bị mất icon, nhưng chưa chắc khớp với visual gốc vì HTML ban đầu có thể dùng icon dạng ký tự, emoji, SVG inline hoặc style riêng.

Mục tiêu lần tiếp theo là không chỉ "có icon", mà phải "giống thiết kế HTML gốc" theo từng khu vực UI.

## 2. Mục tiêu

- Đối chiếu toàn bộ icon trong frontend với thiết kế gốc trong `HTML`.
- Ghi rõ vị trí nào lệch, lệch kiểu gì, sửa ở file nào.
- Chỉnh icon theo từng phần nhỏ để dễ kiểm tra.
- Không sửa logic nghiệp vụ, API, DB, auth, job hoặc backend.
- Không sửa text tiếng Việt hoặc encoding nếu không liên quan trực tiếp đến icon.
- Sau khi sửa xong phải build frontend, đồng bộ `dist` sang backend `wwwroot`, build backend và commit riêng.

## 3. Nguồn chuẩn

Thư mục HTML gốc:

- `HTML/danh-sach-kvc.html`
- `HTML/ma-kvc.html`
- `HTML/so-du-kvc.html`
- `HTML/nap-tien-kvc.html`
- `HTML/chi-tiet-gia-von-ve-ban.html`
- `HTML/doi-soat-kvc.html`
- `HTML/kvc-hoan-tien.html`
- `HTML/hoan-tien-kvc.html`
- `HTML/trang-thai-hoan-tien-kh.html`
- `HTML/system.html`

Frontend hiện tại cần kiểm tra:

- `frontend/src/components/layout/AppShell.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/components/ui/DataTable.vue`
- `frontend/src/components/ui/KanbanBoard.vue`
- `frontend/src/components/ui/ReportTableCard.vue`
- `frontend/src/data/navigation.ts`
- `frontend/src/data/reports.ts`
- `frontend/src/data/workflows.ts`
- `frontend/src/style.css`
- `frontend/src/views/ParkCodesView.vue`
- `frontend/src/views/ReportView.vue`
- `frontend/src/views/JobErrorsView.vue`
- `frontend/src/views/SystemSettingsView.vue`
- `frontend/src/views/RefundProcessView.vue`
- `frontend/src/views/TopUpWorkflowView.vue`
- `frontend/src/views/AuditLogView.vue`

## 4. Nguyên tắc sửa

1. HTML gốc là chuẩn ưu tiên.
2. Nếu HTML gốc dùng icon ký tự/emoji và visual đó là chủ ý thiết kế, frontend nên khôi phục đúng kiểu đó hoặc tạo component render tương đương.
3. Nếu HTML gốc dùng SVG inline, frontend nên dùng SVG/component tương đương với cùng nét, kích thước, màu và vị trí.
4. Không tự đổi sang icon line-style khác nếu thiết kế gốc không dùng kiểu đó.
5. Icon phải ổn định trong sidebar thu gọn, mobile, hover, active và dark/light theme.
6. Không để icon làm thay đổi chiều cao dòng, chiều rộng cột hoặc gây nhảy layout.
7. Với nút thao tác, phải giữ `title` hoặc `aria-label` nếu icon-only.
8. Không chỉnh backend ngoài việc cập nhật lại file build trong `backend/src/PpvRecon.Api/wwwroot`.

## 5. Audit cần làm trước khi code

### 5.1. Tìm icon trong HTML

Chạy các lệnh gợi ý:

```powershell
rg "icon|svg|emoji|nav|sidebar|button|tab|modal-close|action|act-btn|device" HTML -n
```

Nếu cần xem cụ thể từng file:

```powershell
Get-Content -Raw HTML\danh-sach-kvc.html
Get-Content -Raw HTML\ma-kvc.html
Get-Content -Raw HTML\system.html
```

Ghi lại mapping theo mẫu:

| Khu vực | File HTML gốc | Icon gốc | Frontend hiện tại | File frontend cần sửa | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| Sidebar menu cha Khu vui chơi | `HTML/danh-sach-kvc.html` | Ghi nguyên icon gốc sau khi audit | `AppIcon park` | `navigation.ts`, `AppShell.vue`, `AppIcon.vue` | Kiểm tra màu, kích thước, active |
| Sidebar Đại lý | `HTML/danh-sach-kvc.html` | Ghi nguyên icon gốc sau khi audit | `AppIcon handshake` | `navigation.ts`, `AppShell.vue`, `AppIcon.vue` |  |
| Sidebar Khách lẻ | `HTML/danh-sach-kvc.html` | Ghi nguyên icon gốc sau khi audit | `AppIcon user` | `navigation.ts`, `AppShell.vue`, `AppIcon.vue` |  |
| Sidebar Đối soát Vin | `HTML/danh-sach-kvc.html` | Ghi nguyên icon gốc sau khi audit | `AppIcon scale` | `navigation.ts`, `AppShell.vue`, `AppIcon.vue` |  |
| Sidebar OTA | `HTML/danh-sach-kvc.html` | Ghi nguyên icon gốc sau khi audit | `AppIcon globe` | `navigation.ts`, `AppShell.vue`, `AppIcon.vue` |  |
| Sidebar Hoàn tiền | `HTML/danh-sach-kvc.html` | Ghi nguyên icon gốc sau khi audit | `AppIcon refresh` | `navigation.ts`, `AppShell.vue`, `AppIcon.vue` |  |
| Sidebar Quy trình đối soát | `HTML/danh-sach-kvc.html` | Ghi nguyên icon gốc sau khi audit | `AppIcon clipboard` | `navigation.ts`, `AppShell.vue`, `AppIcon.vue` |  |
| Tab cài đặt | `HTML/system.html` | Ghi từng icon tab | `AppIcon user/palette/lock/...` | `SystemSettingsView.vue`, `AppIcon.vue`, `style.css` | Kiểm tra style tab |
| Thiết bị đăng nhập | `HTML/system.html` | Ghi icon thiết bị gốc | `AppIcon monitor` | `SystemSettingsView.vue`, `style.css` |  |
| Nút sửa/xóa/ngừng | `HTML/ma-kvc.html` | Ghi icon nút gốc | `AppIcon edit/trash/pause` | `ParkCodesView.vue`, `DataTable.vue`, `AppIcon.vue` | HTML có thể dùng SVG inline |
| Modal close | Các file HTML | Ghi icon close gốc | Có nơi dùng chữ `x` hoặc ký tự close | Các view có modal | Cần thống nhất nếu lệch |
| Search placeholder | Các file HTML | Ghi icon search gốc nếu có | Một số data còn icon trong placeholder | `reports.ts`, `ReportTableCard.vue` | Chỉ sửa nếu thiết kế yêu cầu |

### 5.2. Tìm icon trong frontend

Chạy:

```powershell
rg "AppIcon|icon:|action.icon|svg|modal-close|theme-toggle|nav-icon|sys-tab-icon|act-btn|device-icon|kanban-title-icon" frontend/src -n
```

Tìm icon dạng ký tự/emoji còn sót:

```powershell
rg "[\x{1F300}-\x{1FAFF}\x{2600}-\x{27BF}]" frontend/src -n
```

Lưu ý: nếu regex Unicode không chạy đúng trên PowerShell, có thể mở trực tiếp các file:

- `frontend/src/data/reports.ts`
- `frontend/src/data/workflows.ts`
- `frontend/src/views/RefundProcessView.vue`

## 6. Plan chỉnh sửa theo phase

### Phase 1: Sidebar và layout chung

Mục tiêu: sidebar/menu phải giống HTML gốc trước, vì đây là phần dễ thấy nhất.

File cần xem:

- `HTML/danh-sach-kvc.html`
- `frontend/src/data/navigation.ts`
- `frontend/src/components/layout/AppShell.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- So sánh icon từng menu cha.
- Chỉnh `navigation.ts` nếu tên icon hiện tại không khớp.
- Nếu HTML dùng icon dạng ký tự/emoji, cân nhắc tạo `AppMenuIcon` hoặc mở rộng `AppIcon` để render đúng.
- Nếu HTML dùng SVG, thêm path tương ứng vào `AppIcon`.
- Kiểm tra trạng thái normal, hover, active, collapsed.
- Kiểm tra màu icon trong dark/light theme.

Hoàn thành khi:

- Sidebar menu cha nhìn giống HTML gốc.
- Menu thu gọn vẫn hiện icon đúng.
- Không còn chữ viết tắt kiểu `KV`, `DL`, `OTA` trừ khi HTML gốc thật sự dùng như vậy.

### Phase 2: Header, user actions và theme toggle

File cần xem:

- `HTML/danh-sach-kvc.html`
- `HTML/system.html`
- `frontend/src/components/layout/AppShell.vue`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu icon thu gọn menu.
- Đối chiếu icon thông báo.
- Đối chiếu icon cài đặt.
- Đối chiếu icon đăng xuất.
- Đối chiếu nút đổi theme.

Hoàn thành khi:

- Các icon topbar/user-card giống HTML gốc về kiểu, size, màu và vị trí.

### Phase 3: Menu Khu vui chơi và màn mã KVC

File cần xem:

- `HTML/ma-kvc.html`
- `frontend/src/views/ParkCodesView.vue`
- `frontend/src/components/ui/DataTable.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu nút thêm KVC/thêm loại vé.
- Đối chiếu action sửa.
- Đối chiếu action ngừng sử dụng.
- Đối chiếu action xóa mềm.
- Nếu HTML dùng icon-only thì frontend nên icon-only kèm `title`.
- Nếu HTML dùng icon + text thì frontend giữ icon + text.

Hoàn thành khi:

- Bảng KVC cha và loại vé/KVC con có action giống `HTML/ma-kvc.html`.
- Không làm rộng cột hành động quá mức.

### Phase 4: Các màn báo cáo trong Khu vui chơi

File HTML cần xem:

- `HTML/danh-sach-kvc.html`
- `HTML/so-du-kvc.html`
- `HTML/nap-tien-kvc.html`
- `HTML/chi-tiet-gia-von-ve-ban.html`
- `HTML/doi-soat-kvc.html`
- `HTML/kvc-hoan-tien.html`

File frontend cần xem:

- `frontend/src/views/ReportView.vue`
- `frontend/src/components/ui/ReportTableCard.vue`
- `frontend/src/components/ui/DataTable.vue`
- `frontend/src/data/reports.ts`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu icon search trong ô tìm kiếm nếu HTML có.
- Đối chiếu icon badge cảnh báo nếu HTML có.
- Đối chiếu icon action xem/sửa/xóa trong bảng hoàn tiền.
- Đối chiếu icon nút build đối soát.
- Đối chiếu icon các nút lọc/tải lại/xóa lọc nếu HTML gốc có.

Hoàn thành khi:

- Các màn báo cáo không còn icon lệch kiểu với HTML gốc.
- Không còn icon ký tự lạ do encoding sai.

### Phase 5: Lỗi đồng bộ và job thủ công

File cần xem:

- HTML tương ứng nếu có trong thư mục `HTML`
- `frontend/src/views/JobErrorsView.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu icon chạy job.
- Đối chiếu icon gửi email lỗi.
- Đối chiếu icon nhập tay.
- Đối chiếu icon trạng thái lỗi nếu HTML có.

Hoàn thành khi:

- Kế toán/admin nhìn được các action quan trọng rõ ràng và icon đồng bộ với HTML.

### Phase 6: Cài đặt hệ thống

File cần xem:

- `HTML/system.html`
- `frontend/src/views/SystemSettingsView.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu icon từng tab:
  - Thông tin tài khoản.
  - Giao diện.
  - Đổi mật khẩu.
  - Quản lý thiết bị.
  - Quản lý người dùng.
  - Email nhận lỗi.
- Đối chiếu icon lựa chọn theme.
- Đối chiếu icon thiết bị đăng nhập.
- Đối chiếu action người dùng:
  - Sửa.
  - Reset pass.
  - Mở khóa.
  - Kích hoạt/vô hiệu.
- Đối chiếu action email nhận lỗi:
  - Sửa.
  - Ngừng.

Hoàn thành khi:

- `SystemSettingsView` giống `HTML/system.html` nhất có thể về icon và spacing.

### Phase 7: Quy trình hoàn tiền và Kanban

File cần xem:

- `HTML/hoan-tien-kvc.html`
- `HTML/trang-thai-hoan-tien-kh.html`
- `frontend/src/views/RefundProcessView.vue`
- `frontend/src/views/TopUpWorkflowView.vue`
- `frontend/src/components/ui/KanbanBoard.vue`
- `frontend/src/data/workflows.ts`

Việc cần làm:

- Đối chiếu icon cột Kanban.
- Đối chiếu modal close.
- Đối chiếu nút thêm/đóng/lưu nếu HTML gốc có icon.
- Kiểm tra icon trong dữ liệu workflow hiện tại có còn lệch với HTML.

Hoàn thành khi:

- Kanban không bị lệch icon so với thiết kế gốc.
- Modal close đúng style HTML.

### Phase 8: Component icon chung

File cần xem:

- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- Quyết định giữ `AppIcon` SVG hay bổ sung mode render icon gốc.
- Nếu cần khớp emoji/ký tự gốc, có thể thêm prop hoặc component mới, ví dụ:

```ts
type IconKind = 'svg' | 'text'
```

Hoặc giữ mapping riêng:

```ts
const originalMenuIcons = {
  park: 'icon gốc từ HTML',
  agency: 'icon gốc từ HTML',
}
```

Chỉ triển khai khi audit chứng minh HTML gốc không phải line SVG.

Hoàn thành khi:

- Icon được quản lý tập trung.
- Không copy SVG rải rác nếu có thể tránh.
- Không phá các icon đã ổn định.

## 7. Kiểm thử sau mỗi nhóm sửa

Sau mỗi phase lớn, chạy:

```powershell
npm run build
```

Từ thư mục:

```powershell
frontend
```

Sau khi hoàn tất toàn bộ frontend:

```powershell
$workspace = (Resolve-Path '.').Path
$dist = (Resolve-Path 'frontend\dist').Path
$www = (Resolve-Path 'backend\src\PpvRecon.Api\wwwroot').Path
if (-not $dist.StartsWith($workspace) -or -not $www.StartsWith($workspace)) { throw 'Resolved paths are outside workspace.' }
if (-not $www.EndsWith('backend\src\PpvRecon.Api\wwwroot')) { throw "Unexpected wwwroot path: $www" }
Get-ChildItem -LiteralPath $www -Force | Remove-Item -Recurse -Force
Copy-Item -Path (Join-Path $dist '*') -Destination $www -Recurse -Force
```

Sau đó build backend:

```powershell
dotnet build backend\PpvRecon.sln
```

## 8. Kiểm tra thủ công nên làm

Nếu có thể chạy app, kiểm tra các route:

- `/khu-vui-choi/danh-sach`
- `/khu-vui-choi/ma-kvc`
- `/khu-vui-choi/so-du`
- `/khu-vui-choi/nap-tien`
- `/khu-vui-choi/gia-von-ve-ban`
- `/khu-vui-choi/doi-soat`
- `/khu-vui-choi/kvc-hoan-tien`
- `/khu-vui-choi/loi-dong-bo`
- `/system`
- `/hoan-tien/quy-trinh`
- `/hoan-tien/trang-thai-khach-hang`

Cần kiểm tra:

- Sidebar mở rộng.
- Sidebar thu gọn.
- Dark theme.
- Light theme.
- Màn hình desktop.
- Màn hình hẹp/mobile nếu có thời gian.
- Hover/active của menu.
- Hover của nút action.
- Icon không bị mất màu hoặc lệch dòng.

## 9. Quy tắc commit

Sau khi hoàn tất và build xanh:

```powershell
git status --short
git diff --check
git add frontend/src backend/src/PpvRecon.Api/wwwroot
git commit -m "Match original HTML icon design"
```

Không commit nếu:

- `npm run build` lỗi.
- `dotnet build backend\PpvRecon.sln` lỗi.
- Có thay đổi ngoài phạm vi icon mà chưa được người dùng đồng ý.

## 10. Ghi chú cho AI tiếp theo

- Repo hiện tại sau commit `0a87e3d` có `AppIcon.vue`. Đừng xóa ngay component này nếu chưa audit xong, vì nhiều màn đang dùng nó.
- Người dùng muốn giống thiết kế HTML gốc, không chỉ muốn có icon.
- Hãy đọc HTML trước khi sửa.
- Nếu phát hiện HTML gốc dùng emoji/ký tự icon, cần cân nhắc giữ đúng visual đó thay vì thay bằng SVG line icon.
- Nếu phát hiện HTML gốc dùng SVG, hãy đưa SVG vào component chung để dễ bảo trì.
- Không sửa backend logic.
- Không sửa DB.
- Không sửa text tiếng Việt nếu không liên quan đến icon.
- Sau mỗi nhóm sửa lớn nên commit hoặc ít nhất build để tránh dồn lỗi.

