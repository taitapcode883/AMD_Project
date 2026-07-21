# CHANGES.md

Lịch sử thay đổi của project PastebinProject (AMD201 group assignment).

---

## 2026-07-21 — Hoàn thiện khung AuthService, dựng PasteService, cấu hình Ocelot Gateway

**Lý do:** Tiếp tục theo roadmap 2 tuần (chốt hôm nay: nhóm 4 người, 2 FE/2 BE; tuần 1 dựng tính năng core, tuần 2 Docker/CI/test + học sâu lại code). Mục tiêu: có khung 3 service chạy được, nối qua Gateway, trước khi viết logic nghiệp vụ thật (Register/Login, tạo/xem paste).

### AuthService
- `Data/AppDbContext.cs`: sửa lỗi `DbSet<Auth> Auths` (class `Auth` không tồn tại) → đúng thành `DbSet<User> Users`.
- `Program.cs`: thêm `builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=auth.db"))` + 2 `using` (`Microsoft.EntityFrameworkCore`, `AuthService.Data`).
- Cài package `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.VisualStudio.Web.CodeGeneration.Design`, `Swashbuckle.AspNetCore` (một số như `SqlServer` cài dư, không dùng tới — cần dọn sau).
- Chạy `dotnet ef migrations add InitialCreate` → sinh `Migrations/`, tạo `auth.db`.
- Scaffold `Controllers/AuthController.cs` bằng `dotnet aspnet-codegenerator controller -name AuthController -m User -dc AppDbContext -api --relativeFolderPath Controllers`.
- **Trạng thái:** `AuthController.cs` hiện vẫn là bản CRUD scaffold thô (`GetUsers`/`PutUser`/`PostUser`/`DeleteUser` theo `id`, trả thẳng `User` gồm cả `PasswordHash`) — **CHƯA** phải Register/Login thật theo API contract. Đây là việc cần làm tiếp theo.

### PasteService
- Tạo `Models/Paste.cs`: `Id, Code, Content, Language, Visibility, CreatedAt, ExpiresAt (nullable), OwnerId (nullable), ViewCount`.
- Tạo `Data/AppDbContext.cs`: `DbSet<Paste> Pastes`.
- `Program.cs`: thêm `AddDbContext<AppDbContext>` — ban đầu bị lỗi copy nhầm connection string `"Data Source=auth.db"` từ AuthService, đã sửa thành `"Data Source=paste.db"`.
- Chạy migration `InitialCreate`, tạo `paste.db`.
- Scaffold `Controllers/PasteController.cs` (cùng cách với AuthController).
- **Trạng thái:** `PasteController.cs` vẫn là CRUD scaffold thô theo `id` (int) — **CHƯA** khớp API contract (cần đổi sang tra cứu theo `code` (string), sinh mã code ngắn khi tạo, tính `ExpiresAt` từ enum `expiry`, validate rỗng + max 500KB). Đây là việc cần làm tiếp theo.

### ApiGateway
- Cài package `Ocelot`.
- Tạo `ocelot.json` — ban đầu để nguyên bản mẫu của thầy (route `/gateway/students`, `/gateway/teachers`, sai port), đã viết lại toàn bộ route khớp API contract thật:
  - `BaseUrl`: `http://localhost:5179` (port thật của ApiGateway)
  - `POST /auth/register`, `POST /auth/login` → AuthService (port `5279`)
  - `POST /pastes`, `GET /pastes/mine`, `GET /pastes/{code}`, `DELETE /pastes/{code}` → PasteService (port `5065`)
- Xoá `Controllers/WeatherForecastController.cs` (leftover scaffold, gây lỗi build `CS0246` vì `WeatherForecast.cs` đã bị xoá trước đó nhưng Controller vẫn tham chiếu) — ApiGateway theo đúng style thầy không cần Controller nào, chỉ cấu hình Ocelot.
- Build sạch, 0 lỗi.

### Khác
- Đã thảo luận và chốt **API contract** (endpoint + JSON request/response cho Auth + Paste) trong hội thoại với Claude, dùng làm chuẩn cho cả FE lẫn BE — **nhưng chưa lưu thành file `API_CONTRACT.md` trong repo** (lần đầu bị huỷ thao tác ghi file). Cần quyết định có lưu file này không để gửi cho 2 bạn FE.
- Repo Git: `PastebinProject/` hiện vẫn **untracked** trong repo `amd201` (chưa track vào Git), và **chưa có `.gitignore`** — rủi ro commit nhầm `bin/`, `obj/`, `*.db` giống lỗi đã xảy ra ở project `MVC` bên cạnh.
- Đã chốt roadmap 2 tuần: tuần 1 dựng xong tính năng core (BE + FE song song), tuần 2 dành cho Docker/CI/CD/unit test/README + học sâu lại code đã viết.

### Việc cần làm tiếp theo (chưa làm)
1. Viết lại `AuthController` thật: `Register` (hash password bằng BCrypt), `Login` (kiểm tra + trả JWT).
2. Viết lại `PasteController` thật: tạo `code` ngắn, tính `ExpiresAt`, validate rỗng/500KB, tra cứu theo `code`.
3. Background job xoá paste hết hạn (`BackgroundService` trong PasteService).
4. Xử lý `.gitignore` + đưa project vào Git.
5. Frontend (React/Vue) — chưa bắt đầu, 2 bạn FE phụ trách.
6. Docker, GitHub Actions CI/CD, unit test project, README — chưa bắt đầu.

---

## 2026-07-21 (buổi chiều) — Scaffold Frontend bằng Vue + Vite

**Lý do:** Bắt đầu phần Frontend cho 2 bạn FE trong nhóm code tiếp. Ban đầu scaffold bằng React (Vite), nhưng bạn FE trong nhóm muốn dùng Vue nên đã xoá và tạo lại.

### Frontend
- Tạo scaffold ban đầu bằng `npm create vite@latest Frontend -- --template react` (chọn linter ESLint) — sau đó **xoá toàn bộ** vì đổi ý sang Vue (thư mục lúc xoá chưa track git, chưa có code tuỳ chỉnh nào).
- Tạo lại bằng `npm create vite@latest Frontend -- --template vue` (chọn linter ESLint), `npm install`, `npm run dev` chạy thành công tại `http://localhost:5173`.
- **Stack chốt cho Frontend:** Vue 3 + Vite + JavaScript thuần (không TypeScript) + ESLint.
- **Trạng thái:** Chỉ là scaffold mặc định của Vite (`App.vue`, `main.js`, `index.html`), chưa có component/page thật nào (chưa có paste editor, view page, dashboard, login/register).

### Việc cần làm tiếp theo cho Frontend (chưa làm)
1. Xoá nội dung mẫu (logo, counter) trong `App.vue`.
2. Cài `vue-router` cho các trang: paste editor, view page, dashboard, login/register.
3. Kết nối tới `ApiGateway` (`http://localhost:5179`) khi API contract đã lưu thành file.

---

## 2026-07-21 (tối) — Cài package Auth, gộp lại cấu trúc thư mục

**Lý do:** Chuẩn bị viết `AuthController` thật (Register/Login), đồng thời gọn lại cấu trúc project trước khi bắt đầu phần logic nghiệp vụ.

### AuthService
- Cài package `BCrypt.Net-Next` 4.2.0 — dùng `BCrypt.HashPassword` lúc Register, `BCrypt.Verify` lúc Login.
- Cài package `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.10 — sinh JWT lúc Login (qua `JwtSecurityTokenHandler`, kéo theo sub-dependency `System.IdentityModel.Tokens.Jwt`), sau này dùng `AddJwtBearer` ở `Program.cs` để `PasteService` verify token mà không cần gọi lại `AuthService`.
- `AuthController.cs` vẫn là CRUD scaffold thô, **chưa** viết Register/Login — 2 package trên chỉ mới là công cụ, chưa có chỗ nào gọi tới.

### Cấu trúc thư mục
- Gộp `AuthService/` và `PasteService/` vào chung một thư mục cha `Services/` cho gọn (`ApiGateway/` và `Frontend/` vẫn ở gốc project, không đổi).
- Cập nhật `PastebinSolution.slnx` trỏ đúng 2 đường dẫn mới (`Services/AuthService/AuthService.csproj`, `Services/PasteService/PasteService.csproj`).
- Đã `dotnet build PastebinSolution.slnx` lại toàn bộ — build sạch, 0 lỗi, không có file nào khác còn tham chiếu đường dẫn cũ.

Cấu trúc hiện tại:
```
PastebinProject/
├── ApiGateway/
├── Frontend/
├── Services/
│   ├── AuthService/
│   └── PasteService/
├── PastebinSolution.slnx
└── CHANGES.md
```

### Việc cần làm tiếp theo (chưa làm)
1. Viết lại `AuthController` thật: `RegisterRequest`/`LoginRequest`, `Register` (hash password), `Login` (verify + trả JWT).
2. Viết lại `PasteController` thật (như đã ghi ở mục trên, chưa đổi).
3. Background job xoá paste hết hạn.
4. `.gitignore` + đưa project vào Git (`PastebinProject/` vẫn untracked).
5. Frontend, Docker, CI/CD, unit test, README — chưa bắt đầu.

---

## 2026-07-21 (khuya) — Đẩy repo lên GitHub, viết lại `PasteController` thật

**Lý do:** Phân công lại vai trò trong nhóm BE — 1 bạn phụ trách `PasteService`, tự tay viết lại toàn bộ Controller theo hướng dẫn giải thích từng dòng (không viết hộ), đồng thời đưa code lên GitHub để cả nhóm cùng thấy.

### Git / GitHub
- Repo `PastebinProject/` giờ là git repo **riêng, độc lập** với repo `amd201` bên ngoài (repo mẫu của giảng viên, không liên quan).
- Thêm `.gitignore` gốc (`bin/`, `obj/`, `*.db`, `node_modules/`, `.vs/`, `.DS_Store`...).
- Commit đầu tiên (52 file) đã push lên `https://github.com/taitapcode883/AMD_Project`, branch `main`.

### PasteService — `PasteController.cs` viết lại hoàn chỉnh (khớp API contract)
- `GetPaste`/`DeletePaste`: đổi tra cứu từ `int id` → `string code` (dùng `FirstOrDefaultAsync(p => p.Code == code)` thay vì `FindAsync`, vì `Code` không phải khoá chính).
- Xoá hẳn `PutPaste` (không có trong API contract, đề bài không yêu cầu sửa paste) và hàm `PasteExists` đi kèm (chỉ được `PutPaste` gọi, thành code chết sau khi xoá).
- Thêm `CreatePasteRequest` (class request riêng, đặt cạnh `PasteController` trong cùng namespace `PasteService.Controllers`, không tách thư mục DTOs — giữ đúng style tối giản của thầy): `Content`, `Language`, `Visibility`, `Expiry` (chuỗi thô `"1h"/"1d"/"1w"/"never"`, khác với `ExpiresAt` đã tính cụ thể lưu trong entity `Paste`).
- Thêm hàm `GenerateCode(int length = 8)`: sinh mã ngẫu nhiên 8 ký tự (chữ hoa/thường/số), dùng làm định danh công khai cho URL — tách biệt với `Id` (khoá nội bộ, tăng dần, không lộ ra ngoài để tránh bị dò URL tuần tự).
- `PostPaste(CreatePasteRequest request)` viết lại hoàn chỉnh:
  - Validate `Content` không rỗng (`string.IsNullOrWhiteSpace`) và không vượt 500KB (đếm byte thật qua `Encoding.UTF8.GetByteCount`, không dùng `.Length` vì ký tự tiếng Việt/Unicode chiếm nhiều hơn 1 byte khi mã hoá UTF-8).
  - Tính `ExpiresAt` từ `request.Expiry` bằng `switch` expression (`"1h"` → `+1 giờ`, `"1d"` → `+1 ngày`, `"1w"` → `+7 ngày`, còn lại/`"never"` → `null`).
  - Gọi `GenerateCode()` trong vòng `do-while`, kiểm tra trùng qua `AnyAsync` trước khi nhận mã, đảm bảo `Code` luôn duy nhất.
  - Tạo `Paste` mới bằng object initializer, lưu DB, trả `201 Created` kèm `Location` trỏ đúng `GetPaste` theo `code`.
- `GetPaste`: thêm `paste.ViewCount++` + `SaveChangesAsync()` sau khi tìm thấy paste — tăng lượt xem mỗi lần gọi thành công. Đã giải thích khái niệm **change tracking** của EF Core (vì `paste` lấy trực tiếp từ `_context` nên không cần gọi `Entry(paste).State = Modified` như cách cũ).
- Build sạch, 0 lỗi (chỉ còn cảnh báo `CS8618` nullable có từ đầu, không phải lỗi mới).

### Việc cần làm tiếp theo (chưa làm)
1. `AuthController` thật (`Register`/`Login`, hash + JWT) — package đã cài (`BCrypt.Net-Next`, `JwtBearer`), chưa viết logic.
2. `PasteController.GetMine` (`GET /pastes/mine`) — cần JWT xong bên AuthService mới đọc được `OwnerId` từ token.
3. Kiểm tra `Visibility == "private"` phải yêu cầu đăng nhập ở `GetPaste` — cũng phụ thuộc JWT.
4. Background job xoá paste hết hạn (`BackgroundService`).
5. Frontend, Docker, CI/CD, unit test, README — chưa bắt đầu.
