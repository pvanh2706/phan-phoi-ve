# ezCloud PPV Frontend

Vue 3 + TypeScript app được chuyển từ bộ HTML tĩnh trong `../HTML`.

## Chạy local

```bash
npm install
npm run dev
```

Dev server mặc định chạy tại `http://127.0.0.1:5173`.

## Kiểm tra build

```bash
npm run build
```

## Cấu trúc chính

- `src/components/layout`: shell dùng chung, sidebar, header, theme toggle.
- `src/components/ui`: table, pagination, Kanban, page header.
- `src/data`: dữ liệu demo có type, sẵn để thay bằng API/service.
- `src/router`: route Vue tương ứng các trang HTML cũ.
- `src/views`: màn hình nghiệp vụ.
