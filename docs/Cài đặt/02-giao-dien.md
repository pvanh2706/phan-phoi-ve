# Use Case: Giao diện

**Màn hình:** `system.html` → Tab **Giao diện**
**Đường dẫn truy cập:** Sidebar → Cài đặt → Tab "🎨 Giao diện"

---

## Mô tả

Cho phép người dùng tuỳ chỉnh chế độ hiển thị (theme) của toàn bộ hệ thống ezCloud PPV và ngôn ngữ giao diện. Lựa chọn được lưu vào `localStorage` và áp dụng ngay lập tức, đồng thời duy trì xuyên suốt khi điều hướng sang các màn hình khác.

---

## Actors

| Actor | Quyền |
|-------|-------|
| Admin | Đổi theme và ngôn ngữ cho tài khoản của mình |
| Member | Đổi theme và ngôn ngữ cho tài khoản của mình |

---

## Use Cases

### UC-02.1 – Chọn chế độ hiển thị Tối (Dark)
- **Điều kiện:** Người dùng đang ở tab Giao diện
- **Luồng chính:**
  1. Người dùng nhấn vào card "🌙 Tối"
  2. Hệ thống set `data-theme="dark"` trên toàn trang
  3. Hệ thống lưu `ppv-theme = "dark"` vào `localStorage`
  4. Card "Tối" được highlight với border màu indigo, hiển thị dấu ✓
- **Kết quả:** Toàn bộ giao diện chuyển sang nền tối (dark mode), áp dụng cho tất cả 10 màn hình

### UC-02.2 – Chọn chế độ hiển thị Sáng (Light)
- **Điều kiện:** Người dùng đang ở tab Giao diện
- **Luồng chính:**
  1. Người dùng nhấn vào card "☀️ Sáng"
  2. Hệ thống set `data-theme="light"` trên toàn trang
  3. Hệ thống lưu `ppv-theme = "light"` vào `localStorage`
  4. Card "Sáng" được highlight, hiển thị dấu ✓
- **Kết quả:** Toàn bộ giao diện chuyển sang nền sáng (light mode)

### UC-02.3 – Chọn chế độ Theo hệ thống (System)
- **Điều kiện:** Người dùng đang ở tab Giao diện
- **Luồng chính:**
  1. Người dùng nhấn vào card "💻 Hệ thống"
  2. Hệ thống đọc `prefers-color-scheme` của trình duyệt/OS
  3. Nếu OS đang Dark → áp dụng dark; nếu Light → áp dụng light
  4. Lưu `ppv-theme = "system"` vào `localStorage`
- **Kết quả:** Giao diện tự động theo cài đặt hệ thống của thiết bị

### UC-02.4 – Chọn ngôn ngữ
- **Điều kiện:** Người dùng đang ở tab Giao diện
- **Luồng chính:**
  1. Người dùng mở dropdown Ngôn ngữ
  2. Chọn "🇻🇳 Tiếng Việt" hoặc "🇺🇸 English"
- **Ghi chú:** Tính năng ngôn ngữ hiện ở trạng thái UI mockup, chưa có logic đổi ngôn ngữ động

---

## Tuỳ chọn giao diện

| Chế độ | Icon | Mô tả |
|--------|------|-------|
| Tối | 🌙 | Nền tối, phù hợp môi trường ánh sáng thấp |
| Sáng | ☀️ | Nền sáng, phù hợp môi trường ánh sáng tốt |
| Hệ thống | 💻 | Theo cài đặt OS/trình duyệt của thiết bị |

---

## Cơ chế lưu trữ & áp dụng theme

- **Lưu tại:** `localStorage` với key `ppv-theme`
- **Đọc khi load:** Tất cả 10 file HTML đều có inline script đọc `localStorage` ngay trong `<head>` trước khi render CSS (tránh flash of unstyled content)
- **Mặc định:** `dark` nếu chưa từng cài đặt
- **Phạm vi áp dụng:** Tất cả màn hình trong hệ thống

---

## Danh sách màn hình hỗ trợ theme

| File | Màn hình |
|------|---------|
| `danh-sach-kvc.html` | Danh sách khu vui chơi |
| `ma-kvc.html` | Mã khu vui chơi |
| `so-du-kvc.html` | Số dư KVC hàng ngày |
| `nap-tien-kvc.html` | Nạp tiền KVC |
| `chi-tiet-gia-von-ve-ban.html` | Chi tiết giá vốn vé bán |
| `doi-soat-kvc.html` | Đối soát KVC |
| `kvc-hoan-tien.html` | KVC hoàn tiền |
| `hoan-tien-kvc.html` | Quy trình hoàn tiền (Kanban) |
| `trang-thai-hoan-tien-kh.html` | Trạng thái hoàn tiền KH |
| `system.html` | Cài đặt hệ thống |

---

## Kết nối với màn hình khác

- **Ảnh hưởng đến:** Toàn bộ 10 màn hình trong hệ thống
- **Nút phụ:** Nút "🌓 Giao diện" cố định góc phải dưới ở tất cả màn hình (toggle nhanh giữa dark/light)
- **Truy cập nhanh từ:** Tất cả màn hình qua nút "Cài đặt" ở user card
