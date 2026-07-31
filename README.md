# Pastebin & Code Snippet Sharer

Đồ án nhóm môn **AMD201 — Advanced Microservices Deployment**. Bản clone đơn giản của Pastebin/GitHub Gist: người dùng paste text/code, nhận về link ngắn để chia sẻ, có thể đặt hạn dùng, chế độ riêng tư, và quản lý các paste của mình qua dashboard sau khi đăng nhập.

## Kiến trúc

Hệ thống gồm 4 thành phần độc lập, giao tiếp qua HTTP:

```
                        ┌─────────────┐
   Browser ───────────► │  Frontend    │  Vue 3 SPA (Vite), nginx phục vụ static build
                        │  :5173       │
                        └──────┬───────┘
                               │ REST (JSON)
                               ▼
                        ┌─────────────┐
                        │  ApiGateway  │  ASP.NET Core + Ocelot — route request
                        │  :5179       │  tới đúng service phía sau, xử lý CORS
                        └──┬───────┬───┘
                   /auth/* │       │ /pastes*
                           ▼       ▼
                  ┌─────────────┐ ┌──────────────┐
                  │ AuthService │ │ PasteService │
                  │   :5279     │ │    :5065     │
                  └──────┬──────┘ └──────┬───────┘
                         ▼               ▼
                  ┌─────────────┐ ┌──────────────┐
                  │ PostgreSQL  │ │  PostgreSQL  │
                  │  (authdb)   │ │  (pastedb)   │
                  └─────────────┘ └──────────────┘
```

- **AuthService** — đăng ký/đăng nhập, phát hành JWT (access token + refresh token rotation), lưu vào PostgreSQL riêng (`authdb`).
- **PasteService** — tạo/xem/xoá paste, kiểm tra quyền sở hữu và visibility qua JWT do AuthService ký (không gọi ngược lại AuthService để verify — cả 2 service dùng chung `Jwt:Key`), có background job tự xoá paste hết hạn, lưu vào PostgreSQL riêng (`pastedb`).
- **ApiGateway** — điểm vào duy nhất cho Frontend, dùng [Ocelot](https://ocelot.readthedocs.io/) để route `/auth/*` → AuthService, `/pastes*` → PasteService, xử lý CORS cho origin của Frontend.
- **Frontend** — Vue 3 + Vite, gồm trang đăng nhập/đăng ký, paste editor, trang xem paste, dashboard liệt kê paste của người dùng.

## Tính năng

- Paste text/code → sinh link ngắn duy nhất (`/p/{code}`)
- Đặt hạn dùng: 1 giờ / 1 ngày / 1 tuần / không hết hạn (background job tự dọn paste hết hạn mỗi 10 phút)
- Paste riêng tư (`private`) chỉ chủ sở hữu xem được (yêu cầu đăng nhập)
- Đếm lượt xem mỗi paste
- Dashboard liệt kê, tìm kiếm, xoá paste của chính mình
- Validate: từ chối nội dung rỗng, giới hạn 500 KB

## Chạy local

### Cách 1 — Docker Compose (khuyến nghị, không cần cài .NET/Node)

```bash
docker compose up --build
```

Sau khi build xong (lần đầu mất vài phút):
- Frontend: http://localhost:5173
- ApiGateway: http://localhost:5179
- AuthService: http://localhost:5279 (thường gọi qua Gateway, không gọi trực tiếp)
- PasteService: http://localhost:5065 (thường gọi qua Gateway, không gọi trực tiếp)

Dừng: `docker compose down` (thêm `-v` nếu muốn xoá luôn dữ liệu SQLite trong volume).

### Cách 2 — Chạy trực tiếp bằng .NET / Node (khi cần debug từng service)

Yêu cầu: .NET SDK 10, Node.js 20+, PostgreSQL đang chạy (local hoặc container riêng).

```bash
# Chạy 2 database (chỉ cần làm 1 lần)
docker run -d --name pg-auth -e POSTGRES_DB=authdb -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16-alpine
docker run -d --name pg-paste -e POSTGRES_DB=pastedb -e POSTGRES_PASSWORD=postgres -p 5433:5432 postgres:16-alpine

# Terminal 1 — AuthService
cd Services/AuthService
ConnectionStrings__Default="Host=localhost;Database=authdb;Username=postgres;Password=postgres" dotnet run

# Terminal 2 — PasteService
cd Services/PasteService
ConnectionStrings__Default="Host=localhost;Port=5433;Database=pastedb;Username=postgres;Password=postgres" dotnet run

# Terminal 3 — ApiGateway
cd ApiGateway && dotnet run

# Terminal 4 — Frontend
cd Frontend && npm install && npm run dev
```

Mỗi service tự chạy migration (`Database.Migrate()`) khi khởi động lần đầu, tự tạo schema — không cần bước `dotnet ef database update` thủ công.

## Chạy test

```bash
cd Services/PasteService.Tests
dotnet test
```

16 unit test cho `PasteController` (xUnit + EF Core InMemory), bao phủ validate input, tính hạn dùng, kiểm tra quyền sở hữu/visibility, đếm lượt xem, xoá paste.

## CI/CD

`.github/workflows/ci-cd.yml` chạy tự động trên mỗi push:
1. **test** — build + `dotnet test` (mọi push và mọi PR vào `main`)
2. **docker** — build 4 Docker image, push lên Docker Hub (`trungtai8803/pastebin-{authservice,pasteservice,apigateway,frontend}`), chỉ chạy sau khi test pass
3. **deploy** — gọi Render Deploy Hook để deploy bản mới nhất (chỉ chạy khi push vào `main`)

## Deploy

- Docker images: https://hub.docker.com/u/trungtai8803
- Live URL: _(đang cập nhật — chưa deploy lên Render)_

## Cấu trúc thư mục

```
PastebinProject/
├── ApiGateway/              # Ocelot reverse proxy
├── Frontend/                 # Vue 3 SPA
├── Services/
│   ├── AuthService/          # Đăng ký/đăng nhập, JWT
│   ├── PasteService/         # Logic paste (CRUD, expiry, visibility)
│   └── PasteService.Tests/   # Unit test (xUnit)
├── docker-compose.yml         # Orchestrate 4 service cho local dev
└── .github/workflows/         # CI/CD pipeline
```
