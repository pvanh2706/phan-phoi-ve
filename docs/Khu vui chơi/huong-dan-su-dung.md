# Hướng dẫn sử dụng — Mục Khu vui chơi

Mục **Khu vui chơi** (sidebar bên trái) gồm 8 màn hình, phục vụ quản lý danh mục KVC, theo dõi số dư/giao dịch hằng ngày, và đối soát. Quyền: **Admin** (toàn quyền), **Accountant** (xử lý đối soát/nhập tay), **Member** (chỉ xem).

## Quy trình khuyến nghị mỗi ngày

1. Vào **Lỗi đồng bộ cần xử lý** → bấm "Chạy số dư / Chạy giá vốn / Chạy ngân hàng" để đồng bộ dữ liệu ngày hôm đó (hoặc job tự chạy theo lịch).
2. Nếu có dòng lỗi (API không lấy được), Kế toán bấm **Nhập tay** để nhập số liệu thủ công cho đúng KVC/ngày.
3. Vào **Đối soát Khu vui chơi** → bấm **Build đối soát** cho ngày cần kiểm tra để so khớp số dư.
4. Dòng nào bị lệch (cột "Lệch" khác 0) → bấm ô cảnh báo để mở modal **Xử lý lệch**, nhập số điều chỉnh + ghi chú.
5. Mọi thao tác Tạo/Sửa/Xóa/Xử lý đều được ghi lại — xem lại tại **Nhật ký thay đổi**.

## Tóm tắt từng màn hình

| Màn hình | Mục đích | Thao tác chính |
|---|---|---|
| **Danh sách khu vui chơi** | Xem toàn bộ KVC (chỉ đọc), lọc theo tên/loại thanh toán/trạng thái | Tìm kiếm, lọc |
| **Mã khu vui chơi** | Quản lý danh mục: tab *Mã KVC* (KVC cha), *Danh mục KVC con nạp trước/công nợ* | Thêm/Sửa/Xóa (Admin). Form KVC con chỉ còn: KVC cha, Mã KVC con, Tên KVC con, Trạng thái |
| **Số dư khu vui chơi hằng ngày** | Snapshot số dư tài khoản KVC theo ngày, 2 tab Nạp trước/Công nợ | Lọc theo KVC/ngày; nút "Gọi API test" lấy số dư thủ công |
| **Danh sách nạp tiền KVC theo ngày** | Lịch sử giao dịch ngân hàng nạp tiền/thanh toán công nợ | Nút "⤓ Get API" đồng bộ email ngân hàng; bấm vào nội dung để xem chi tiết giao dịch gộp |
| **Chi tiết giá vốn vé bán** | Chi tiết từng vé bán kèm giá vốn, theo đại lý/loại vé | Nút "Lấy dữ liệu" chọn ngày để đồng bộ từ Oneinventory |
| **Đối soát Khu vui chơi** | So sánh số dư T-1, nạp/dùng, số dư T tự tính vs thực tế → phát hiện lệch | Nút "Build đối soát"; xử lý lệch qua checkbox cảnh báo |
| **Nhật ký thay đổi** | Lịch sử before/after mọi thao tác trong Khu vui chơi và các job đồng bộ | Lọc theo module/đối tượng/hành động/ngày |
| **Lỗi đồng bộ cần xử lý** | Danh sách lỗi gọi API theo KVC/ngày (số dư, giá vốn, ngân hàng) | Chạy lại job theo nguồn; Kế toán/Admin nhập tay khi API vẫn lỗi; gửi email tổng hợp lỗi |

## Lưu ý

- Dữ liệu cùng ngày khi chạy lại job hoặc lấy dữ liệu thủ công sẽ **ghi đè** kết quả cũ.
- "Mã KVC" là mã định danh dùng xuyên suốt để gọi API và đối soát — cần khai báo chính xác trước khi dùng các màn hình còn lại.
- Xử lý lệch đối soát yêu cầu quyền Admin hoặc Accountant.
