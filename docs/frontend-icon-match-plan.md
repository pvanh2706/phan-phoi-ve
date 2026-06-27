# Plan khớp giao diện và icon frontend với thiết kế HTML gốc

Ngày tạo: 2026-06-27

## 1. Bối cảnh

Frontend hiện tại đã có icon trở lại sau commit `0a87e3d Restore frontend icons`, nhưng icon và một số chi tiết giao diện chưa chắc đã giống thiết kế ban đầu trong thư mục `HTML`.

Lần sửa trước đã thêm `frontend/src/components/ui/AppIcon.vue` và chuyển nhiều vị trí sang SVG line icon ổn định. Cách này giúp không bị mất icon, nhưng chưa chắc khớp với visual gốc vì HTML ban đầu có thể dùng icon dạng ký tự, emoji, SVG inline hoặc style riêng.

Mục tiêu lần tiếp theo là không chỉ "có icon", mà phải đối chiếu toàn bộ giao diện với HTML gốc theo từng khu vực UI: bố cục, spacing, màu sắc, bảng, form, tab, nút, modal, responsive và icon.

## 2. Mục tiêu

- Đối chiếu giao diện frontend với HTML gốc theo từng màn và từng phần.
- Đối chiếu toàn bộ icon trong frontend với thiết kế gốc trong `HTML`.
- Ghi rõ vị trí nào lệch, lệch kiểu gì, sửa ở file nào.
- Chỉnh giao diện và icon theo từng phần nhỏ để dễ kiểm tra.
- Không sửa logic nghiệp vụ, API, DB, auth, job hoặc backend.
- Không sửa text tiếng Việt hoặc encoding nếu không liên quan trực tiếp đến giao diện/icon.
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
2. Khi so sánh, kiểm tra cả bố cục, khoảng cách, kích thước, màu sắc, font weight, border radius, shadow/blur, trạng thái hover/active/disabled và icon.
3. Nếu HTML gốc dùng icon ký tự/emoji và visual đó là chủ ý thiết kế, frontend nên khôi phục đúng kiểu đó hoặc tạo component render tương đương.
4. Nếu HTML gốc dùng SVG inline, frontend nên dùng SVG/component tương đương với cùng nét, kích thước, màu và vị trí.
5. Không tự đổi sang icon line-style khác nếu thiết kế gốc không dùng kiểu đó.
6. Icon phải ổn định trong sidebar thu gọn, mobile, hover, active và dark/light theme.
7. Không để icon làm thay đổi chiều cao dòng, chiều rộng cột hoặc gây nhảy layout.
8. Với nút thao tác, phải giữ `title` hoặc `aria-label` nếu icon-only.
9. Không chỉnh backend ngoài việc cập nhật lại file build trong `backend/src/PpvRecon.Api/wwwroot`.
10. Không sửa giao diện theo cảm tính nếu chưa đối chiếu HTML gốc tương ứng.

## 5. Audit cần làm trước khi code

### 5.0. Checklist đối chiếu giao diện cho từng phần

Khi kiểm tra bất kỳ màn nào, phải đối chiếu theo checklist này trước khi sửa:

| Nhóm cần kiểm tra | Cần đối chiếu với HTML gốc |
| --- | --- |
| Layout tổng thể | Sidebar, header, content width, padding ngoài, scroll area, vị trí card/table/form |
| Navigation | Icon, text, trạng thái active, hover, submenu, collapsed sidebar |
| Typography | Font size, font weight, line height, màu chữ, chữ hoa/thường trong table header |
| Màu sắc | Background, màu surface/card, màu border, màu badge, màu button, dark/light theme |
| Spacing | Gap giữa các block, padding card, padding toolbar, padding cell, khoảng cách icon-text |
| Border/radius | Radius của sidebar item, card, input, button, table row, modal |
| Buttons | Kiểu primary/secondary/danger/add/action, icon, text, hover, disabled, kích thước |
| Form controls | Input, select, textarea, placeholder, focus state, chiều cao, width |
| Table | Header, row height, cell padding, hover row, alignment, action column |
| Tabs | Tab container, active tab, inactive tab, icon trong tab, spacing |
| Badges/status | Text, màu, border, icon cảnh báo nếu có, kích thước |
| Modal | Overlay, modal width, header/footer, close button, padding, button order |
| Cards/stat cards | Background, border, blur/shadow, radius, title/value spacing |
| Responsive | Desktop, sidebar collapsed, mobile/narrow viewport nếu HTML có style |
| Icon | Kiểu icon, kích thước, stroke/fill, màu, vị trí, active/hover |

Kết quả audit nên ghi thành bảng:

| Màn/phần | HTML gốc | Frontend hiện tại | Lệch giao diện | Lệch icon | File cần sửa | Ưu tiên |
| --- | --- | --- | --- | --- | --- | --- |
| Ví dụ: Sidebar menu cha | `HTML/danh-sach-kvc.html` | `AppShell.vue` | Cần audit | Cần audit | `navigation.ts`, `AppShell.vue`, `style.css` | Cao |

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

- Đối chiếu width sidebar, padding, brand/logo, màu nền, border, blur, scroll.
- Đối chiếu layout khi sidebar mở rộng và thu gọn.
- Đối chiếu typography của brand, role, menu label, submenu.
- Đối chiếu spacing giữa icon và label, margin giữa menu item.
- So sánh icon từng menu cha.
- Chỉnh `navigation.ts` nếu tên icon hiện tại không khớp.
- Nếu HTML dùng icon dạng ký tự/emoji, cân nhắc tạo `AppMenuIcon` hoặc mở rộng `AppIcon` để render đúng.
- Nếu HTML dùng SVG, thêm path tương ứng vào `AppIcon`.
- Kiểm tra trạng thái normal, hover, active, collapsed.
- Kiểm tra màu icon trong dark/light theme.

Hoàn thành khi:

- Sidebar nhìn giống HTML gốc về layout, màu, spacing và trạng thái tương tác.
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

- Đối chiếu chiều cao header, padding trái/phải, màu nền, border bottom.
- Đối chiếu search input: width, placeholder, radius, màu, focus nếu HTML có.
- Đối chiếu user card: avatar, tên, email, nút con, layout khi sidebar thu gọn.
- Đối chiếu icon thu gọn menu.
- Đối chiếu icon thông báo.
- Đối chiếu icon cài đặt.
- Đối chiếu icon đăng xuất.
- Đối chiếu nút đổi theme.

Hoàn thành khi:

- Header, user-card, theme toggle và icon topbar giống HTML gốc về layout, size, màu và vị trí.

### Phase 3: Menu Khu vui chơi và màn mã KVC

File cần xem:

- `HTML/ma-kvc.html`
- `frontend/src/views/ParkCodesView.vue`
- `frontend/src/components/ui/DataTable.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu tab `KVC cha` và `Loại vé / KVC con`: kích thước, màu, active state.
- Đối chiếu toolbar: input, select, button, wrap trên màn nhỏ.
- Đối chiếu card/table: padding card, header, row height, cell alignment, hover.
- Đối chiếu modal thêm/sửa: width, header, body, form grid, footer, close button.
- Đối chiếu nút thêm KVC/thêm loại vé.
- Đối chiếu action sửa.
- Đối chiếu action ngừng sử dụng.
- Đối chiếu action xóa mềm.
- Nếu HTML dùng icon-only thì frontend nên icon-only kèm `title`.
- Nếu HTML dùng icon + text thì frontend giữ icon + text.

Hoàn thành khi:

- Màn mã KVC giống `HTML/ma-kvc.html` về tab, toolbar, table, modal, button và spacing.
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

- Đối chiếu layout page header, stat cards, filter toolbar, tabs, table card.
- Đối chiếu màu/kiểu số tiền âm dương, badge trạng thái, dòng cảnh báo.
- Đối chiếu table: column width, alignment, header uppercase, row hover, empty state.
- Đối chiếu nút add/build/filter/reload nếu HTML có.
- Đối chiếu icon search trong ô tìm kiếm nếu HTML có.
- Đối chiếu icon badge cảnh báo nếu HTML có.
- Đối chiếu icon action xem/sửa/xóa trong bảng hoàn tiền.
- Đối chiếu icon nút build đối soát.
- Đối chiếu icon các nút lọc/tải lại/xóa lọc nếu HTML gốc có.

Hoàn thành khi:

- Các màn báo cáo trong Khu vui chơi giống HTML gốc về layout, card, table, filter, badge và action.
- Các màn báo cáo không còn icon lệch kiểu với HTML gốc.
- Không còn icon ký tự lạ do encoding sai.

### Phase 5: Lỗi đồng bộ và job thủ công

File cần xem:

- HTML tương ứng nếu có trong thư mục `HTML`
- `frontend/src/views/JobErrorsView.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu layout filter, bảng lỗi, trạng thái, notice, modal nhập tay.
- Đối chiếu màu trạng thái lỗi/thành công/manual.
- Đối chiếu button group chạy job và gửi email.
- Đối chiếu form nhập tay theo từng loại lỗi.
- Đối chiếu icon chạy job.
- Đối chiếu icon gửi email lỗi.
- Đối chiếu icon nhập tay.
- Đối chiếu icon trạng thái lỗi nếu HTML có.

Hoàn thành khi:

- Màn lỗi đồng bộ giống thiết kế gốc về toolbar, table, modal và trạng thái.
- Kế toán/admin nhìn được các action quan trọng rõ ràng và icon đồng bộ với HTML.

### Phase 6: Cài đặt hệ thống

File cần xem:

- `HTML/system.html`
- `frontend/src/views/SystemSettingsView.vue`
- `frontend/src/components/ui/AppIcon.vue`
- `frontend/src/style.css`

Việc cần làm:

- Đối chiếu page header, tab bar, card content, form grid và spacing.
- Đối chiếu profile/avatar upload area.
- Đối chiếu theme cards: preview, selected state, check mark, label.
- Đối chiếu password form.
- Đối chiếu device list: icon, meta text, button đăng xuất.
- Đối chiếu users table và recipients table.
- Đối chiếu user/recipient modal.
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

- `SystemSettingsView` giống `HTML/system.html` nhất có thể về layout, tab, form, table, modal, icon và spacing.

### Phase 7: Quy trình hoàn tiền và Kanban

File cần xem:

- `HTML/hoan-tien-kvc.html`
- `HTML/trang-thai-hoan-tien-kh.html`
- `frontend/src/views/RefundProcessView.vue`
- `frontend/src/views/TopUpWorkflowView.vue`
- `frontend/src/components/ui/KanbanBoard.vue`
- `frontend/src/data/workflows.ts`

Việc cần làm:

- Đối chiếu Kanban board: column width, gap, header, count badge, task card.
- Đối chiếu drag/hover visual nếu HTML có.
- Đối chiếu modal chi tiết task và modal thêm task.
- Đối chiếu table/list trạng thái hoàn tiền khách hàng.
- Đối chiếu icon cột Kanban.
- Đối chiếu modal close.
- Đối chiếu nút thêm/đóng/lưu nếu HTML gốc có icon.
- Kiểm tra icon trong dữ liệu workflow hiện tại có còn lệch với HTML.

Hoàn thành khi:

- Quy trình hoàn tiền và Kanban giống HTML gốc về layout, card, modal, button, badge và icon.
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
- Layout tổng thể có khớp HTML gốc không.
- Card/table/form/modal có cùng spacing, radius, màu và kích thước với HTML gốc không.
- Không có text bị tràn, nút bị nhảy kích thước hoặc icon bị lệch baseline.
- Nếu có thể, mở song song HTML gốc và app frontend để so từng phần.

## 8.1. Gợi ý kiểm tra bằng screenshot

Nếu cần kiểm tra kỹ hơn, có thể chạy app và chụp screenshot từng route để so với HTML gốc:

1. Mở file HTML gốc tương ứng trong browser.
2. Mở frontend route tương ứng.
3. So theo checklist ở mục `5.0`.
4. Ghi lại các điểm lệch trước khi sửa.
5. Sửa từng nhóm nhỏ, build lại, rồi so lại.

Không nên sửa nhiều màn cùng lúc nếu chưa có danh sách điểm lệch rõ ràng.

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

- Người dùng muốn giống thiết kế HTML gốc theo từng phần giao diện, không chỉ muốn có icon.
- Khi làm tiếp, phải audit cả layout, spacing, màu, bảng, form, modal, responsive và icon.
- Hãy đọc HTML trước khi sửa.
- Sau phase 8, `AppIcon.vue` đã được xóa vì không còn chỗ dùng. Icon menu/sidebar và các action quan trọng đang bám theo HTML gốc: HTML dùng emoji/ký tự thì giữ emoji/ký tự, HTML dùng SVG thì dùng SVG inline.
- Nếu sau này thêm màn mới, không khôi phục `AppIcon.vue` theo quán tính. Chỉ tạo component icon chung khi có nhiều SVG giống nhau cần tái sử dụng thật sự.
- Không sửa backend logic.
- Không sửa DB.
- Không sửa text tiếng Việt nếu không liên quan đến icon.
- Sau mỗi nhóm sửa lớn nên commit hoặc ít nhất build để tránh dồn lỗi.

## 11. Trạng thái thực hiện

Cập nhật ngày 2026-06-28:

- Phase 1 đã hoàn thành: sidebar và menu chính đã bám thiết kế HTML gốc. Commit: `dad235a Match original sidebar design`.
- Phase 2 đã hoàn thành: header, user actions, thông báo và theme toggle đã bám HTML. Commit: `0f0be6c Match original shell header`.
- Phase 3 đã hoàn thành: màn mã KVC, tab, toolbar, bảng, action và modal đã chỉnh theo `HTML/ma-kvc.html`. Commit: `ac761aa Match park code screen design`.
- Phase 4 đã hoàn thành: bảng báo cáo, nút thêm/build và action xem/sửa/xóa đã dùng icon đúng kiểu HTML. Commit: `c35fe25 Match report table controls`.
- Phase 5 đã hoàn thành: màn lỗi đồng bộ, action chạy job/gửi mail/nhập tay và modal đã chỉnh theo thiết kế. Commit: `300c713 Match sync error controls`.
- Phase 6 đã hoàn thành: cài đặt hệ thống, tab, theme option, thiết bị, bảng user/email và action đã bám `HTML/system.html`. Commit: `3f8ca74 Match system settings design`.
- Phase 7 đã hoàn thành: Kanban quy trình hoàn tiền đã đổi về header màu theo trạng thái, cột 260px và bỏ icon header cột lệch HTML. Commit: `1ea9304 Match refund workflow board`.
- Phase 8 đã hoàn thành trong commit cuối của đợt chỉnh này: xóa `AppIcon.vue`, thay nút menu bằng SVG inline đúng HTML và dọn CSS selector icon không còn dùng.

Build nghiệm thu đã chạy:

```powershell
npm run build
dotnet build backend\PpvRecon.sln
```
