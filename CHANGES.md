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

## 2026-07-25 — Sửa PasteService (migrate + endpoint `/mine`), chuyển `Jwt:Key` khỏi appsettings.json

**Lý do:** Kiểm tra 3 service chạy thật (build + gọi qua Gateway) thì phát hiện `PasteService` lỗi 500 ở mọi request chạm DB, và `Jwt:Key` đang nằm plaintext trong file commit lên Git — đúng kiểu lỗi đã từng khiến key trước đó bị coi là lộ và phải rotate.

### PasteService
- `Program.cs` chưa từng gọi `Database.Migrate()` lúc khởi động (khác `AuthService`) → dù đã có migration `InitialCreate`, `paste.db` không bao giờ được tạo bảng `Pastes` → mọi query ném `SqliteException: no such table: Pastes`. Đã thêm `Database.Migrate()` giống `AuthService`.
- `ocelot.json` route `GET /pastes/mine` → `GET /api/Paste/mine`, nhưng controller không có action nào tên `mine` → bị `{code}` route nuốt mất, chạy `GetPaste(code: "mine")` rồi crash. Đã thêm `[Authorize] GetMine()` lọc theo `OwnerId` lấy từ claim `sub` trong JWT.
- Vì cần xác thực JWT ở `PasteService` để biết "mine" là của ai, đã thêm `AddAuthentication().AddJwtBearer(...)` giống hệt cấu hình bên `AuthService` (cùng `Key`/`Issuer`/`Audience` để token do `AuthService` phát hành verify được ở đây).
- `PostPaste` giờ gán `OwnerId` từ claim khi request có token hợp lệ; vẫn cho tạo paste ẩn danh nếu không có token.
- Test lại full flow qua Gateway: register → login → tạo paste (có token) → `GET /pastes/mine` (200 khi có token, 401 khi không) → `GET /pastes/{code}` (200, `viewCount` tăng đúng).

### Bảo mật: `Jwt:Key`
- Phát hiện nhánh `authservice` trên remote đã có commit rotate `Jwt:Key` từ trước (`273e938`, lý do: key cũ bị lộ plaintext trên GitHub) — nhưng key mới đó **vẫn** đang nằm plaintext trong `appsettings.json`, chỉ là giá trị khác. Cùng một vấn đề, chưa xử lý gốc.
- Xử lý gốc: sinh key mới (64 byte random, không tái dùng key nào từng bị commit), lưu bằng `dotnet user-secrets` (`AuthService` và `PasteService`, mỗi project một `UserSecretsId` riêng trong `.csproj`) — secret nằm ngoài repo, chỉ trên máy dev.
- Xoá `Jwt:Key` khỏi cả hai `appsettings.json` (chỉ còn `Issuer`/`Audience`, không phải secret).
- Thêm guard fail-fast ở cả hai `Program.cs`: throw `InvalidOperationException` rõ ràng nếu `Jwt:Key` rỗng, thay vì `NullReferenceException` mù mờ lúc runtime.
- **Lưu ý cho cả nhóm:** key mới không còn trong Git nữa — ai clone máy mới phải tự set lại bằng `dotnet user-secrets set "Jwt:Key" "<value>"` ở cả 2 project (giá trị lấy từ người đã có, gửi qua kênh riêng, không paste vào chat công khai/commit). Môi trường ngoài dev (nếu deploy sau này) dùng biến môi trường `Jwt__Key`.

### Việc cần làm tiếp theo (chưa làm)
1. Background job xoá paste hết hạn.
2. Frontend, Docker, CI/CD, unit test, README — chưa bắt đầu.
3. Cân nhắc rewrite lịch sử Git để xoá hẳn key cũ khỏi các commit trước (hiện chỉ ngừng commit key mới, các key cũ vẫn còn trong history) — cần cả nhóm đồng ý trước khi làm vì force-push sẽ ảnh hưởng tới máy người khác.
