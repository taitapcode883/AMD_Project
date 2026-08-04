# AuthService

Service xác thực cho đồ án **Pastebin & Code Snippet Sharer** (AMD201 — Advanced Microservices Deployment). Chịu trách nhiệm đăng ký/đăng nhập người dùng và phát hành JWT cho các service khác (PasteService) tin dùng.

## Vai trò trong hệ thống

```
Browser ─► Frontend ─► ApiGateway ──/auth/*──► AuthService ──► PostgreSQL (authdb)
```

- Frontend gọi AuthService gián tiếp qua ApiGateway (route `/auth/*`), không gọi trực tiếp.
- PasteService **không** gọi ngược lại AuthService để verify token — cả hai service dùng chung `Jwt:Key` nên tự xác thực chữ ký JWT độc lập.
- Access token (JWT) sống ngắn hạn (mặc định 60 phút, cấu hình ở `Jwt:ExpiresInMinutes`); refresh token sống dài hạn (mặc định 7 ngày, `Jwt:RefreshTokenExpiresInDays`) và áp dụng rotation: mỗi lần `/refresh` thành công, refresh token cũ bị thu hồi và một token mới được cấp thay thế.
- Mật khẩu được băm bằng BCrypt (`BCrypt.Net-Next`), không lưu plaintext.

## Công nghệ

- ASP.NET Core (net10.0)
- Entity Framework Core + Npgsql (PostgreSQL)
- JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- BCrypt.Net-Next để hash mật khẩu

## API

Base path khi gọi trực tiếp service: `/api/Auth/*`. Qua ApiGateway: `/auth/*`.

### `POST /api/Auth/register`

Request:

```json
{
  "username": "trungtai",
  "email": "trungtai@example.com",
  "password": "matkhau123"
}
```

- `username`: bắt buộc, 3–50 ký tự.
- `email`: bắt buộc, đúng định dạng email.
- `password`: bắt buộc, tối thiểu 6 ký tự.

Response `201 Created`:

```json
{
  "id": 1,
  "username": "trungtai",
  "email": "trungtai@example.com",
  "role": "User"
}
```

Response `409 Conflict` nếu email đã tồn tại:

```json
{ "message": "Email đã được sử dụng." }
```

### `POST /api/Auth/login`

Request:

```json
{
  "email": "trungtai@example.com",
  "password": "matkhau123"
}
```

Response `200 OK`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-08-04T15:30:00Z",
  "refreshToken": "b2FzZGZhc2RmYXNkZmFzZGY...",
  "user": {
    "id": 1,
    "username": "trungtai",
    "email": "trungtai@example.com",
    "role": "User"
  }
}
```

Response `401 Unauthorized` nếu sai email/mật khẩu:

```json
{ "message": "Email hoặc mật khẩu không đúng." }
```

### `POST /api/Auth/refresh`

Cấp access token mới từ refresh token còn hiệu lực. Refresh token cũ bị thu hồi (rotation) và một refresh token mới được cấp để thay thế.

Request:

```json
{ "refreshToken": "b2FzZGZhc2RmYXNkZmFzZGY..." }
```

Response `200 OK`: cùng cấu trúc như response của `/login`.

Response `401 Unauthorized` nếu refresh token không hợp lệ, đã bị thu hồi hoặc hết hạn:

```json
{ "message": "Refresh token không hợp lệ hoặc đã hết hạn." }
```

### `POST /api/Auth/logout`

Thu hồi refresh token ngay lập tức ở phía server — client không thể dùng nó để lấy access token mới nữa (khác với việc client chỉ tự xoá token phía client).

Request:

```json
{ "refreshToken": "b2FzZGZhc2RmYXNkZmFzZGY..." }
```

Response: `204 No Content`.

### `GET /api/Auth/me`

Yêu cầu header `Authorization: Bearer <token>`. Dùng để kiểm tra access token còn hợp lệ và lấy thông tin người dùng hiện tại đang đăng nhập.

Response `200 OK`:

```json
{
  "id": 1,
  "username": "trungtai",
  "email": "trungtai@example.com",
  "role": "User"
}
```

Response `401 Unauthorized` nếu thiếu token hoặc token không hợp lệ/hết hạn.

## Chạy local

Yêu cầu: .NET SDK 10, PostgreSQL đang chạy.

```bash
# Chạy database (chỉ cần 1 lần)
docker run -d --name pg-auth -e POSTGRES_DB=authdb -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16-alpine

# Chạy service
ConnectionStrings__Default="Host=localhost;Database=authdb;Username=postgres;Password=postgres" dotnet run
```

Service tự chạy migration (`Database.Migrate()`) khi khởi động lần đầu, tự tạo schema cho `authdb` — không cần bước `dotnet ef database update` thủ công.

Mặc định chạy ở `http://localhost:5279` khi khởi động qua `docker compose` ở project gốc; khi chạy trực tiếp bằng `dotnet run`, cổng do `Properties/launchSettings.json` quyết định.

## Cấu hình (`appsettings.json`)

| Key | Ý nghĩa |
|---|---|
| `ConnectionStrings:Default` | Chuỗi kết nối PostgreSQL (`authdb`) |
| `Jwt:Key` | Khoá bí mật ký JWT (HMAC-SHA256) — phải giống hệt bên PasteService để verify được token |
| `Jwt:Issuer` / `Jwt:Audience` | Issuer/Audience nhúng vào token, được validate khi verify |
| `Jwt:ExpiresInMinutes` | Thời hạn access token |
| `Jwt:RefreshTokenExpiresInDays` | Thời hạn refresh token |
