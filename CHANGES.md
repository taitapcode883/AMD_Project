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

---

## 2026-07-25 — Gộp branch `authservice`, hoàn thiện `PasteController` (JWT, GetMine, quyền), background job, rotate JWT key

**Lý do:** Bạn cùng nhóm phụ trách `AuthService` đã viết xong `Register`/`Login`/`Refresh`/`Logout` thật (kèm JWT + refresh token rotation) và push lên branch riêng `authservice` — nhưng branch đó tạo bằng `git init` độc lập, không chung lịch sử với `paste-service`. Cần gộp lại để `PasteService` dùng được JWT thật, hoàn thành nốt 3 việc TODO còn treo từ bữa trước (`GetMine`, check `private`, check quyền xoá) và bổ sung background job xoá paste hết hạn.

### Rà soát `PasteController.cs` trước khi gộp
- Phát hiện `GetPaste` không chặn paste đã hết hạn (`ExpiresAt`), `GetPastes` liệt kê cả paste hết hạn lẫn `private` — lỗ hổng lộ dữ liệu, không nằm trong TODO cũ nhưng cần vá ngay vì độc lập với JWT.
- Vá bằng cách thêm điều kiện so `ExpiresAt` với `DateTime.UtcNow` ở cả 2 action (`GetPaste` trả `404` nếu hết hạn, `GetPastes` lọc bằng `.Where(...)` ngay trong query EF Core).
- Build sạch, commit `5c8bf68`.

### Rà soát branch `authservice` (code của bạn cùng nhóm)
- `AuthController.cs` đã có `Register` (BCrypt hash + check trùng email), `Login` (verify + sinh JWT + refresh token), `Refresh` (rotation: thu hồi token cũ, cấp token mới), `Logout` (thu hồi refresh token), `Me` (`[Authorize]`, test token).
- 2 vấn đề nhỏ ghi nhận lại, **chưa sửa** (không phải việc của mình, để bạn đó tự quyết): `Register` dùng `CreatedAtAction(nameof(Register), ...)` sai (không có action `GET` nào khớp để trỏ `Location` tới); không có unique index cho `Email` ở DB, chỉ check bằng `AnyAsync` ở tầng code (race condition lý thuyết).
- Phát hiện vấn đề bảo mật: `Jwt:Key` bị commit thẳng dạng plaintext trong `Services/AuthService/appsettings.json`, đã push lên GitHub — coi như đã lộ, cần rotate (xử lý ở phần dưới).

### Merge `authservice` vào `paste-service`
- 2 branch không có common ancestor (`git merge-base` báo lỗi) → phải dùng `git merge --allow-unrelated-histories`.
- Làm thử trên branch tạm `merge-auth-paste` trước (không đụng `paste-service` cho đến khi duyệt xong) — trong 47 file trùng tên giữa 2 branch, chỉ 8 file thật sự conflict (còn lại giống hệt, tự merge sạch): `CHANGES.md`, `AuthService.http`, `AuthController.cs`, `AppDbContext.cs` (AuthService), `AppDbContextModelSnapshot.cs`, `Program.cs` (AuthService), `appsettings.json` (AuthService), `PasteController.cs`.
- Cách giải quyết từng conflict: các file AuthService → giữ bản `authservice` (bản thật, bên `paste-service` chỉ có bản scaffold cũ do chưa ai động tới); `PasteController.cs` → giữ bản `paste-service` (bản `authservice` chỉ là snapshot cũ trước khi viết lại); `CHANGES.md` → gộp tay, giữ nguyên nội dung `paste-service` (bên kia không có gì mới hơn để mất).
- Build sạch sau merge → thay `paste-service` trỏ thẳng vào commit merge này (`git branch -f`), xoá branch tạm. Verify bằng `git diff` xác nhận `Services/PasteService/` không đổi 1 dòng nào so với trước merge. Commit merge `dfa3246`.

### `PasteService` — wire JWT + hoàn thành TODO còn lại
- Cài package `Microsoft.AspNetCore.Authentication.JwtBearer`, thêm section `Jwt` (`Key`/`Issuer`/`Audience`, khớp với `AuthService`) vào `appsettings.json`.
- `Program.cs`: thêm `AddAuthentication().AddJwtBearer(...)` với `MapInboundClaims = false` (khớp bên AuthService, nếu không claim `sub` sẽ bị đổi tên tự động, đọc `OwnerId` sau này sẽ ra `null`), `AddAuthorization()`, và **`app.UseAuthentication()` trước `app.UseAuthorization()`** (thiếu bước này thì `[Authorize]` luôn trả 401 dù token đúng — đã tự bắt lỗi này lúc build/test).
- `GetPaste`: thêm chặn `Visibility == "private"` — không đăng nhập → `401`, đăng nhập nhưng không phải chủ → `403 Forbid`.
- `PostPaste`: gán `OwnerId` từ claim `sub` trong token nếu người tạo đã đăng nhập; vẫn cho tạo ẩn danh nếu không có token (giữ đúng thiết kế `OwnerId` nullable).
- `DeletePaste`: paste có `OwnerId` → chỉ đúng chủ mới xoá được (401/403 tương tự); paste ẩn danh (`OwnerId == null`) → giữ hành vi cũ, ai biết `code` cũng xoá được (không ai chứng minh được quyền sở hữu qua token cho loại paste này).
- Thêm `GetMine` (`[Authorize]`, `GET api/Paste/mine`) — trả các paste có `OwnerId` khớp user hiện tại.
- Build sạch, commit `0412418`.

### Background job xoá paste hết hạn
- Thêm `ExpiredPasteCleanupService : BackgroundService` (`Services/PasteService/ExpiredPasteCleanupService.cs`) — vòng lặp mỗi 10 phút, xoá thẳng bằng `ExecuteDeleteAsync` (EF Core dịch thành 1 câu `DELETE` chạy trên DB, không tải entity vào bộ nhớ).
- Vì `AppDbContext` là Scoped còn `BackgroundService` chạy Singleton, không inject thẳng được — phải inject `IServiceProvider`, tự tạo `IServiceScope` mỗi vòng lặp để lấy `AppDbContext` tươi.
- Đăng ký `builder.Services.AddHostedService<ExpiredPasteCleanupService>()` trong `Program.cs` — phải thêm `using PasteService;` (namespace gốc project không tự động có sẵn trong file top-level statements như `System`/`Microsoft.AspNetCore.*` qua `ImplicitUsings`).
- Build sạch, commit `7160205`.

### Rotate `Jwt:Key`
- Khoá cũ đã lộ plaintext trên GitHub từ trước — sinh khoá mới 64 byte ngẫu nhiên bằng `openssl rand -base64 64`.
- Cập nhật ở `authservice` (`Services/AuthService/appsettings.json`, commit `273e938`), rồi đồng bộ đúng khoá đó sang cả `Services/AuthService/appsettings.json` **và** `Services/PasteService/appsettings.json` bên `paste-service` (commit `2a9e432`) — bắt buộc đồng bộ cả 2 chỗ vì AuthService ký / PasteService verify dùng chung 1 khoá, lệch nhau là JWT vỡ hoàn toàn (mọi request có `[Authorize]` đều bị từ chối).
- Đã push cả `authservice` và `paste-service` lên GitHub. Lưu ý: rotate chỉ chặn được việc dùng khoá cũ **từ giờ trở đi** — khoá cũ vẫn còn nằm trong lịch sử Git đã push trước đó, không tự xoá được trừ khi rewrite history (chưa làm, rủi ro cao, cần bàn với cả nhóm trước nếu muốn làm).

### Việc cần làm tiếp theo (chưa làm)
1. Sửa `CreatedAtAction` sai trong `AuthController.Register` (branch `authservice`).
2. Cân nhắc thêm unique index cho `User.Email` ở DB.
3. Bạn phụ trách AuthService cần `git pull` branch `authservice` trước khi push tiếp (đã có commit rotate key mới trên đó).
4. Frontend, Docker, CI/CD, unit test, README — chưa bắt đầu.

---

## 2026-07-28 — Hoàn thiện route ApiGateway, test end-to-end qua Postman, gộp có chọn lọc `frontend`/`authservice`, thêm CORS

**Lý do:** `ocelot.json` mới chỉ có route `register`/`login`/4 route paste, thiếu 3 action AuthService đã có sẵn (`refresh`/`logout`/`me`). Đồng thời 2 bạn cùng nhóm đã push thêm vào `frontend` (UI hoàn chỉnh) và `authservice` (unique index, đổi cách quản lý `Jwt:Key`) — cần lấy về nhưng không được đè mất `PasteService` đã viết.

### ApiGateway — bổ sung route còn thiếu
- Tự viết thêm 3 route vào `ocelot.json` theo mẫu route `login`/`register` có sẵn: `POST /auth/refresh`, `POST /auth/logout`, `GET /auth/me`.
- Bug tự phát hiện lúc viết: route `/auth/me` gõ nhầm `UpstreamHttpMethod: ["Post"]` trong khi `AuthController.Me()` là `[HttpGet("me")]` — sửa lại thành `"Get"`, nếu không Gateway trả `404`/`405` cho mọi request `GET /auth/me`.
- Commit `68a4451`.

### Test end-to-end qua Postman (cả 3 service chạy song song: Gateway `:5179`, AuthService `:5279`, PasteService `:5065`)
- Set up collection với biến `base_url`, `access_token`, `refresh_token`, `paste_code` + script `Post-response` tự lưu token sau mỗi lần login/refresh — tránh copy tay.
- Xác nhận toàn bộ luồng chạy đúng qua Gateway: `register` (201) → `login` (200) → `me` (200, cần đúng Bearer token) → `refresh` (200, rotation — token cũ dùng lại bị `401`) → `logout` (204) → `pastes` create/get/mine/delete (201/200/200/204).
- Debug 2 lỗi thao tác Postman gặp phải (không phải lỗi code): biến `base_url` bị dư dấu `/` cuối gây `//auth/me` → `404`; URL `login` dính khoảng trắng thừa ở cuối (`%20`) → `404`. Dùng Postman Console (`View → Show Postman Console`) để soi request thật gửi đi, phát hiện ra 2 lỗi này.

### Gộp có chọn lọc từ nhánh nhóm (không merge nguyên branch)
- `git fetch` phát hiện nhánh `frontend` mới (`origin/frontend`) và `authservice` có thêm 3 commit — cả 3 đứng tên tác giả `taitapcode883` (trùng git identity với user hiện tại, cần xác nhận lại với bạn AuthService xem có dùng chung máy/account không).
- **`frontend`**: merge thẳng, sạch, không conflict (chỉ đụng `Frontend/`, không đụng `Services/`/`ApiGateway/`). Commit merge `4a1139d`.
- **`authservice`**: KHÔNG merge nguyên branch — trong 3 commit mới có 1 commit (`5e4b775`) viết lại luôn cả `PasteController.cs`/`Program.cs` bên PasteService bằng bản **cũ hơn, kém hơn** bản đang có (thiếu check `private`/ownership). Thử `git cherry-pick` thấy conflict đúng như dự đoán ở `PasteController.cs`/`Program.cs`/`appsettings.json` → `git cherry-pick --abort`, `git reset --hard` về trạng thái sạch.
- Thay vào đó lấy tay từng file AuthService-only bằng `git show <commit>:<path> > <path>` (không đụng gì bên PasteService): `AuthController.cs`, `AppDbContext.cs`, 2 file migration `AddUniqueEmailIndex` — mang lại unique index DB cho `User.Email` + trả `409 Conflict` ở tầng DB (`catch (DbUpdateException)`) thay vì chỉ check `AnyAsync` như cũ. Build lại `AuthService` xác nhận sạch trước khi commit `0ffe74f`.
- **Chủ động bỏ qua** 1 phần khác của `authservice` (commit `e25ae05`): đổi `Jwt:Key` từ `appsettings.json` sang `dotnet user-secrets` — user-secrets là local trên máy, không đồng bộ qua git, lấy vào sẽ làm AuthService không khởi động được (thiếu key) trên máy khác ngoài máy người tạo ra thay đổi đó. Còn treo trên `origin/authservice`, chưa merge, cần bàn với team cách chia sẻ key an toàn hơn (ví dụ file `.env`/`appsettings.Local.json` thêm vào `.gitignore`).
- Nhân tiện kiểm tra lại thấy bug `CreatedAtAction(nameof(Register), ...)` sai (ghi nhận từ 2026-07-25, mục TODO #1) **đã được teammate tự sửa** trong chính commit `5e4b775` (nay đổi thành `StatusCode(StatusCodes.Status201Created, response)`) — lấy về cùng lúc với phần unique index, không cần sửa tay nữa.

### Rà soát toàn bộ hệ thống để tìm việc còn thiếu trước khi "thành web hoàn chỉnh"
Phát hiện Backend (3 service) đã ổn định, nhưng `Frontend/` mới chỉ là UI, **chưa nối API thật**:
- `Login.vue`/`Register.vue`: không gọi `POST /auth/login`/`register` — chỉ đọc/ghi `localStorage` giả lập, không có JWT nào được lưu ở bất kỳ đâu trong Frontend.
- `Dashboard.vue`: `pastes` là mảng hardcode cứng trong code, không gọi `GET /pastes/mine`.
- `PasteEditor.vue`: bug thật — gửi field `expiresAt` (tính sẵn ISO date ở client) nhưng `CreatePasteRequest` bên backend chỉ đọc field `Expiry` (`"1h"/"1d"/"1w"`) → paste tạo từ Frontend **không bao giờ hết hạn** dù chọn gì trên UI. Chưa sửa (thuộc code FE, không phải phần mình).
- `vite.config.js` chỉ proxy `/pastes`, thiếu `/auth/*`; không có `.env`/`VITE_API_URL` — sẽ vỡ khi `vite build` production (hết dev-server để proxy).

### ApiGateway — thêm CORS
- Thêm `AddCors`/`UseCors("Frontend")` (origin `http://localhost:5173`, đúng port mặc định Vite dev) vào `Program.cs`, đặt `UseCors` trước `UseOcelot()` (bắt buộc theo thứ tự middleware, nếu không header CORS không kịp gắn vào response). Verify bằng `curl -X OPTIONS` giả lập preflight — nhận đủ 3 header `Access-Control-Allow-*`.
- Lý do cần: hiện `/pastes` "gọi được" từ Frontend chỉ nhờ proxy dev-server của Vite (né CORS vì browser thấy same-origin), nhưng cách đó không có rule cho `/auth/*` và không hoạt động ở bản build production. Thêm CORS ở Gateway giải quyết tận gốc, không phụ thuộc dev-server.
- Commit `c4b3748`.

### Trạng thái cuối ngày
- `paste-service` đã có đủ: PasteService (của mình) + AuthController thật (đã lấy phần cải tiến) + Frontend UI (chưa nối API) + ApiGateway đủ route + CORS. Tất cả đã push lên `origin/paste-service`.
- Backend (AuthService/PasteService/ApiGateway) coi như xong việc, đã test qua Postman.
- Việc chặn "web hoàn chỉnh" duy nhất còn lại: Frontend chưa nối API thật (mục Login/Register/Dashboard/PasteEditor ở trên) — thuộc phần 2 bạn FE.

### Việc cần làm tiếp theo (chưa làm)
1. Nối `Login.vue`/`Register.vue`/`Dashboard.vue` vào API thật, lưu JWT token, gắn `Authorization: Bearer` cho các request cần — việc của FE.
2. Sửa `PasteEditor.vue` gửi đúng field `expiry` thay vì `expiresAt` — việc của FE.
3. Thêm `.env`/`VITE_API_URL` + proxy rule `/auth/*` (hoặc bỏ hẳn proxy, dùng CORS đã có) — việc của FE.
4. Team quyết định cách chia sẻ `Jwt:Key` không qua git (thay cho hướng user-secrets hiện đang treo trên `authservice`).
5. Docker, CI/CD, unit test, README — chưa bắt đầu.

---

## 2026-08-01 — Đồng bộ code nhóm, đối chiếu đề bài, unit test, Docker hóa, CI/CD, PostgreSQL, deploy Render, fix bug CRUD Frontend

**Lý do:** Kiểm tra lại toàn bộ code so với `AMD201 - Assignment Brief.docx` (file đề bài nằm sẵn ở gốc repo, chưa đọc kỹ trước đó) để biết còn thiếu gì trước khi nộp bài. Phát hiện thiếu gần hết phần DevOps (Docker, CI/CD, unit test, README, deploy) — đề bài ghi rõ "A non-working or undeployed application cannot score above 4" dù code chạy được, nên đây là việc ưu tiên nhất.

### Đồng bộ code nhóm trước khi bắt đầu
- `git fetch` thấy `origin/frontend` có thêm 1 commit (`c030e8f`): fix đúng bug đã ghi nhận hôm 2026-07-28 — `PasteEditor.vue` gửi `expiresAt` thay vì `expiry`. Cherry-pick về `paste-service` (`a68f6c5`).
- Phát hiện nhánh mới `origin/frontend-login` (5 commit) — có `Frontend/src/services/api.js` (fetch wrapper thật, tự gắn JWT), và `Login.vue`/`Register.vue`/`Dashboard.vue` đã nối API thật, đúng thứ đang thiếu theo audit 2026-07-28. Nhưng `PasteController.cs` trên nhánh này còn nguyên conflict marker `<<<<<<<`/`=======`/`>>>>>>>` chưa resolve — không build được.
- Lấy chọn lọc 6 file Frontend (`api.js`, `Login.vue`, `Register.vue`, `Dashboard.vue`, `router/index.js`, `vite.config.js`) bằng `git checkout <branch> -- <paths>`, loại hẳn `PasteController.cs`. Đối chiếu field JSON của `AuthResponse`/`UserResponse` (backend) với những gì `Login.vue` đọc — khớp chính xác (camelCase mặc định ASP.NET Core). Commit `b31fd80`.

### Unit test cho `PasteController`
- Xác nhận lại kế hoạch tạm dừng hôm 2026-07-28 (xUnit + EF Core InMemory) trước khi làm, đúng theo thói quen "không giả định đề xuất cũ đã được chấp nhận".
- Dựng project `Services/PasteService.Tests`, hướng dẫn viết từng bước (helper `CreateContext()` dùng `Guid` làm tên DB ảo riêng mỗi test, helper `SetUser()` giả lập `ClaimsPrincipal` vì gọi controller trực tiếp không qua ASP.NET pipeline nên không tự có `HttpContext`/`User`).
- Sau khi giải thích xong bước 1, được yêu cầu "làm luôn cho tôi chỉ giải thích chi tiết" — viết trực tiếp 16 test bao phủ `PostPaste`/`GetPaste`/`GetMine`/`DeletePaste` (validate, expiry, private/ownership, view count, xoá ẩn danh). Cả 16 pass ngay lần đầu. Commit `7531a1f`.

### Docker hóa 4 service + docker-compose
- Dockerfile multi-stage (SDK build → ASP.NET runtime) cho AuthService/PasteService/ApiGateway, Dockerfile 2 stage (node build → nginx) cho Frontend.
- **Bug tự phát hiện lúc build thử:** thiếu `.dockerignore` khiến `obj/`/`bin/` đã restore trên host bị `COPY` đè lên `obj/` restore trong container → publish lỗi `NETSDK1064`. Thêm `.dockerignore` là fix.
- **Bug tự phát hiện khác:** `PasteService` chưa từng gọi `Database.Migrate()` lúc khởi động (AuthService có từ lâu) — vì `*.db` bị gitignore nên container/máy mới clone sẽ crash ngay khi gọi API do bảng chưa tồn tại. Thêm gọi `Database.Migrate()` giống AuthService.
- `docker-compose.yml`: build cả 4 service, volume riêng cho từng DB. Build + chạy thử thật bằng `docker compose up`, test full flow qua `curl` (register → login → tạo paste → GetMine) xác nhận chạy đúng trước khi commit. Commit `d01b8a8`.

### GitHub Actions CI/CD
- `.github/workflows/ci-cd.yml`: job `test` (dotnet test) → job `docker` (build + push 4 image lên Docker Hub, tag `latest` + SHA) → job `deploy` (gọi Render Deploy Hook, tự skip nếu secret rỗng).
- Chạy thử thật, phát hiện lỗi `unauthorized: access token has insufficient scopes` — token Docker Hub lưu trong secret bị tạo với quyền Read-only thay vì Read & Write. Tạo lại token đúng quyền, update secret, chạy lại pass. Commit `5a5926e`.
- Đẩy 4 image lên Docker Hub (`trungtai8803/pastebin-{authservice,pasteservice,apigateway,frontend}`) thủ công lần đầu qua `docker login`/`tag`/`push`.

### Chuyển SQLite → PostgreSQL
- Cả AuthService lẫn PasteService trước đó dùng SQLite (đề bài yêu cầu SQL Server hoặc PostgreSQL), còn cài dư package `EntityFrameworkCore.SqlServer` chưa từng dùng.
- Đổi sang `Npgsql.EntityFrameworkCore.PostgreSQL`, xoá migration cũ (không tương thích schema Postgres), tạo migration `InitialCreate` mới cho cả 2 service, verify bằng cách chạy thật vào 2 container Postgres tạm (`docker run postgres:16-alpine`) — xác nhận đúng unique index `Email`, FK `RefreshTokens → Users` cascade, cột `DateTime` map đúng `timestamp with time zone`.
- Thêm 2 service `postgres-auth`/`postgres-paste` vào `docker-compose.yml` (đúng kiểu database-per-service), connection string đọc qua `ConnectionStrings:Default` (fallback về giá trị cũ nếu không set). Test full flow qua Postgres thật (kể cả `409 Conflict` khi trùng email) trước khi commit `85501a0`.

### README.md
- Mô tả project, sơ đồ kiến trúc ASCII, hướng dẫn chạy bằng `docker compose` hoặc `dotnet run`/`npm run dev`, cách chạy test, tóm tắt pipeline CI/CD. Commit `5f2bec9`, cập nhật lại link deploy thật ở cuối buổi (`0e33b4d`).

### Deploy lên Render — phần tốn thời gian nhất, 4 lỗi thật phải debug
- **Free tier chỉ cho 1 Postgres/account.** Tạo `pastedb` bằng cách connect vào server Postgres free đã có (`authdb_ls4q`) qua `psql` rồi `CREATE DATABASE pastedb;` thủ công — không cần trả phí, vẫn giữ đúng kiến trúc "mỗi service 1 database", chỉ là chung 1 server vật lý.
- **AuthService crash lặp lại** ngay sau khi Render "tự phát hiện port 8080 rồi restart để cập nhật cấu hình mạng" — restart đó thất bại vì hết quota `inotify` (xem mục dưới), nhưng fix tức thời là set biến môi trường `PORT=8080` (Render không cần tự dò port nữa) — áp dụng cho cả 4 service (`PORT=80` riêng cho Frontend vì nginx nghe cổng khác).
- **Gateway gọi downstream nội bộ Render bị `502` fail nhanh** (dùng tên service kiểu `pastebin-authservice:8080` theo docs Render private networking) — không xác định được nguyên nhân chính xác dù đã thử nhiều cách; đổi sang gọi qua **URL công khai HTTPS** của AuthService/PasteService (đã verify từng service tự hoạt động đúng qua URL riêng) — hoạt động ngay.
- **Nguyên nhân gốc thật sự của toàn bộ các lần crash `exit 139`:** ASP.NET Core mặc định bật `FileSystemWatcher` (dùng `inotify` của Linux) để tự reload `appsettings.json`, ApiGateway còn tự thêm 2 watcher nữa cho `ocelot.json`/`ocelot.{Env}.json`. Container Render free tier giới hạn tài nguyên, quota `inotify` (128) hết sau vài lần restart → `System.IO.IOException: configured user limit (128) on inotify instances reached` → exit 139, Render tự restart liên tục càng làm cạn quota nhanh hơn. Chẩn đoán bằng cách thêm tạm 1 endpoint debug (`/debug/ping-auth`, đã xoá sau khi xong) tự gọi HttpClient để lộ lỗi thật, kết hợp đọc kỹ log Render (thấy `Unhandled exception` + `Exited with status 139` ngay sau dòng "New primary port detected... Restarting"). **Fix:** `ENV DOTNET_hostBuilder__reloadConfigOnChange=false` ở cả 3 Dockerfile backend + `reloadOnChange: false` cho 2 lệnh `AddJsonFile` trong `ApiGateway/Program.cs`. Sau fix, cả 3 service live ổn định, gọi qua Gateway trả `201`/`200` đúng.
- **CORS sai domain:** Render tự thêm hậu tố ngẫu nhiên `-68mk` vào URL Frontend vì tên `pastebin-frontend` bị trùng toàn cục trên Render (URL phải duy nhất toàn hệ thống, không chỉ trong 1 account). Cập nhật lại CORS đúng `https://pastebin-frontend-68mk.onrender.com`.
- Verify cuối cùng bằng flow thật qua domain production: `register` → `login` → tạo paste private → `GetMine` → CORS preflight đúng origin thật. Tất cả `201`/`200`/`204` đúng như kỳ vọng. Commit các bước: `3e088e4`, `8a850df` (debug tạm), `d543157` (fix inotify), `b01e3bf` (fix CORS + xoá debug).

### Bug lớn phát hiện lúc user tự test: `PasteEditor.vue` không gắn JWT token
- User tự test trên trang live, báo "tạo được nhưng không xoá được, reload mất hết".
- Nguyên nhân: `PasteEditor.vue` (trang tạo paste) vẫn dùng `fetch()` trần thay vì `apiRequest()` — không gắn header `Authorization`. Mọi paste tạo qua UI đều thành ẩn danh (`OwnerId: null`) dù đang đăng nhập → không bao giờ hiện trong `GetMine` (Dashboard) → không có nút Delete nào để bấm vì paste không nằm trong danh sách. File này không nằm trong 6 file đã lấy từ `frontend-login` hôm đầu buổi nên bị bỏ sót.
- Sửa `PasteEditor.vue` dùng `apiRequest()` giống các trang khác. Đồng thời phát hiện và sửa luôn `PasteView.vue` (trang xem lại paste) có lỗi tương tự — dùng `fetch` trần nên xem paste **private** của chính mình sẽ bị `401` và hiển thị nhầm thành "service unavailable" thay vì phân biệt rõ 404 (không tồn tại) vs 401/403 (private, cần đăng nhập đúng chủ).
- Verify lại bằng flow thật cả local (docker-compose) lẫn production Render: tạo paste private khi đã login → `ownerId` gán đúng → hiện trong `GetMine` → xem lại được → xoá được (`204`) → biến mất khỏi `GetMine`. Commit `134cdf0`.

### Trạng thái cuối ngày
- **App đang chạy live thật** tại https://pastebin-frontend-68mk.onrender.com (API qua Gateway: https://pastebin-apigateway.onrender.com), verify end-to-end đầy đủ qua cả curl lẫn (đang chờ) test tay trên trình duyệt.
- Đối chiếu rubric: unit test ✅, Docker ✅, CI/CD (test+build+push tự động) ✅, PostgreSQL ✅, README ✅, deploy thật ✅ — hết bị chặn điểm dưới 4.
- Docker Hub: `hub.docker.com/u/trungtai8803`, image `trungtai8803/pastebin-{authservice,pasteservice,apigateway,frontend}:latest`.

### Việc cần làm tiếp theo (chưa làm)
1. Lấy Render Deploy Hook cho cả 4 service, lưu thành 4 GitHub secret (`RENDER_DEPLOY_HOOK_AUTH/PASTE/GATEWAY/FRONTEND`) để job `deploy` trong CI tự chạy — hiện vẫn phải tự bấm "Manual Deploy" trên Render sau mỗi lần push. Đang làm dở, dừng lại giữa chừng vì phát hiện bug CRUD ở trên.
2. Merge `paste-service` vào `main` — đề bài yêu cầu CI/CD chạy "on every push to main", hiện `main` vẫn là bản scaffold cũ chưa merge.
3. User đang tự test tay trên trình duyệt theo 6 kịch bản (đăng ký/đăng nhập, tạo paste đủ biến thể, Dashboard, view counter, hết hạn tự động, đăng xuất/bảo mật) — chưa có kết quả.
4. Merit/Distinction (không bắt buộc): syntax highlighting (highlight.js/Prism), diff viewer, public paste feed có pagination — chưa làm.
