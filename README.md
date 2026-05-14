# eGreetings

Ứng dụng full-stack gửi thiệp điện tử với subscription, admin dashboard, và scheduled jobs.

## 1. Stack

| Layer       | Công nghệ                                                     |
| ----------- | ------------------------------------------------------------- |
| Frontend    | Angular 21 (standalone components, signals), TailwindCSS      |
| Backend     | .NET 9, ASP.NET Core, Clean Architecture, CQRS + MediatR      |
| ORM         | EF Core 9 (Code-First migrations)                             |
| Database    | SQL Server 2022                                               |
| Background  | Hangfire (SQL Server storage)                                 |
| Email (dev) | MailHog (SMTP fake + Web UI)                                  |

## 2. Yêu cầu môi trường

**Chạy bằng Docker (khuyến nghị):**
- Docker Desktop ≥ 4.30 (đã bật Compose v2)
- ~4 GB RAM free cho SQL Server container
- Không cần cài .NET SDK hay Node.js trên máy

**Chạy local (dev mode):**
- .NET 9 SDK
- Node.js 20+ và npm 10+
- SQL Server (có thể chạy chỉ service `sqlserver` từ compose)

## 3. Quick start (Docker)

Một lệnh duy nhất:

```bash
docker compose up --build
```

Lần đầu sẽ pull image SQL Server (~1.5 GB) + build backend/frontend (~3-5 phút). Sau đó:

| Service       | URL                                                       |
| ------------- | --------------------------------------------------------- |
| Frontend      | http://localhost:4300                                     |
| Backend API   | http://localhost:5059  (Swagger: `/swagger`)              |
| MailHog UI    | http://localhost:8025                                     |
| SQL Server    | `localhost:1433`  (user `sa`, password `Admin123@`)       |

Dừng stack: `docker compose down`. Xoá luôn data DB: `docker compose down -v`.

## 4. Dev mode (chạy local, không qua Docker)

Chỉ chạy SQL + MailHog bằng Docker, code chạy native (hot reload nhanh hơn):

```bash
# Terminal 1 — DB + Mail
docker compose up -d sqlserver mailhog

# Terminal 2 — Backend
cd Backend
dotnet run --project src/EGreetings.API
# → http://localhost:5059

# Terminal 3 — Frontend
cd Frontend
npm install
npm start
# → http://localhost:4300  (Angular dev server, proxy /api → localhost:5059)
```

Backend tự chạy migration + seed lần đầu khi connect được SQL Server.

## 5. Admin credentials (sau khi seed)

| Email                       | Password         |
| --------------------------- | ---------------- |
| `admin@gmail.com`           | `Admin123@`      |
| `admin@e-greetings.com`     | `Admin@123456!`  |

Seed cũng tạo 11 simulated users (5 free + 4 monthly + 2 annual) và ~707 historical greeting transactions trải 30 ngày để dashboard có data.

## 6. Troubleshooting

**Port bị chiếm (1433 / 5059 / 4300 / 1025 / 8025):**
```bash
lsof -nP -iTCP:1433 -sTCP:LISTEN     # đổi port nếu cần
docker compose down                   # stop trước khi sửa
```

**SQL Server container không healthy:**
```bash
docker compose logs sqlserver --tail=50
```
Trên Apple Silicon, image `mssql/server:2022-latest` chạy qua Rosetta (amd64). Nếu chậm bất thường, cân nhắc đổi sang `mcr.microsoft.com/azure-sql-edge` (ARM native) trong `docker-compose.yml`.

**Lỗi "Login failed for user 'sa'" sau khi đổi password:**
Volume SQL còn giữ password cũ. Reset:
```bash
docker compose down -v && docker compose up -d
```

**Migration / seed fail khi khởi động backend:**
```bash
docker compose logs backend --tail=100
```
Hầu hết do SQL Server chưa healthy đúng lúc — backend đã có `depends_on: condition: service_healthy`, nhưng nếu image SQL khởi động chậm có thể vẫn timeout. Restart `backend`:
```bash
docker compose restart backend
```

**Reset toàn bộ DB (xoá data, seed lại từ đầu):**
```bash
docker compose down -v
docker compose up -d
```

**Frontend trắng trang / 404 trên refresh:**
Kiểm tra `Frontend/nginx.conf` đang fallback `try_files $uri $uri/ /index.html` cho SPA routing.
