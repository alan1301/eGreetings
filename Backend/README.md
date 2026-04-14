# E-Greetings API — .NET 9 CQRS

Hệ thống thiệp điện tử E-Greetings, xây dựng theo kiến trúc **CQRS + Clean Architecture** với **.NET 9** và **Entity Framework Core 9 (Code First)**.

## Kiến trúc

```
EGreetings/
└── src/
    ├── EGreetings.Domain/          # Entities, Enums, Common
    ├── EGreetings.Application/     # Commands, Queries, DTOs, Validators, Interfaces
    ├── EGreetings.Infrastructure/  # EF Core, AppDbContext, Seed, Services, BackgroundServices
    └── EGreetings.API/             # Controllers, Middleware, Program.cs
```

## Use Cases được triển khai (30 UC)

| Nhóm | UC | Mô tả |
|------|-----|--------|
| Nhóm 1: Xác thực | UC01–UC02, UC18, UC22 | Đăng ký, Đăng nhập, Đăng xuất, Quên mật khẩu |
| Nhóm 2: Thiệp | UC03–UC09, UC20, UC23, UC24 | Xem/Tùy chỉnh/Gửi/Xem thiệp |
| Nhóm 3: Đăng ký dịch vụ | UC10–UC11, UC15, UC25, UC26 | Subscribe, Quản lý, Gia hạn, Lịch sử |
| Nhóm 4: Admin | UC12–UC14, UC21, UC27–UC28 | Template, Feedback, Users, Category, Content |
| Nhóm 5: Admin nâng cao | UC16–UC17, UC19, UC29–UC30 | Danh bạ, Nháp, Hồ sơ, Retry, Audit |

## Cài đặt & Chạy

### Yêu cầu
- .NET 9 SDK
- (Tùy chọn) Docker cho SMTP dev

### Bước 1: Clone & Restore
```bash
cd EGreetings
dotnet restore
```

### Bước 2: Tạo Migration & Database
```bash
cd src/EGreetings.API
dotnet ef migrations add InitialCreate \
  --project ../EGreetings.Infrastructure \
  --startup-project . \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project ../EGreetings.Infrastructure \
  --startup-project .
```

### Bước 3: Chạy API
```bash
dotnet run --project src/EGreetings.API
```

API chạy tại: https://localhost:7001
Swagger UI: https://localhost:7001/swagger

### Bước 4: Seed Data (tự động khi chạy)
Dữ liệu mẫu được tự động seed khi khởi động lần đầu:
- **1 Admin**: admin@egreetings.vn / Admin@123456
- **5 Users**: hoanguyen@gmail.com / minhtvn@yahoo.com / ... (Password: User@123456)
- **8 Danh mục** (Sinh nhật, Tết, Đám cưới, ...)
- **8 Mẫu thiệp** (4 miễn phí, 4 trả phí)
- **3 Subscriptions** (Active, Active, Expired)
- **4 Thiệp** (Sent, Scheduled, Sent, Guest)
- **Web contents** (banner, footer, about)

## Công nghệ

| Thành phần | Thư viện |
|------------|----------|
| CQRS | MediatR 12.4 |
| Validation | FluentValidation 11 |
| ORM | Entity Framework Core 9 (SQLite) |
| Auth | JWT Bearer + BCrypt.Net |
| Email | MailKit |
| Background | IHostedService |
| API Docs | Swashbuckle (Swagger) |

## Business Rules quan trọng

- **BR-28**: Reply-To header = email người gửi thiệp
- **BR-30**: Renew Subscription = +30 ngày từ ngày ExpiredAt (không phải từ hôm nay)
- **BR-31**: Không xóa Category có Template đang hoạt động; không xóa Category hệ thống
- **BR-32**: Retry email tối đa 3 lần, cách nhau 5 phút
- **BR-33**: AuditLog immutable, lưu 30 ngày, hỗ trợ export CSV

## Subscription State Machine

```
Pending → Active → Expired → (Disabled nếu Admin ban user)
```

## Background Services

| Service | Mô tả |
|---------|-------|
| `ExpireSubscriptionService` | Tự động chuyển Active→Expired mỗi giờ (UC15) |
| `AutoSendGreetingService` | Gửi thiệp sinh nhật tự động lúc 07:00 UTC (UC20) |
| `RetryEmailService` | Retry email thất bại mỗi 5 phút, tối đa 3 lần (UC29/BR-32) |
| `AuditLogCleanupService` | Xóa AuditLog cũ hơn 30 ngày mỗi ngày (BR-33) |

## API Endpoints tổng hợp

```
POST   /api/auth/register           UC01 - Đăng ký
POST   /api/auth/login              UC02 - Đăng nhập
POST   /api/auth/logout             UC18 - Đăng xuất
POST   /api/auth/forgot-password    UC22 - Quên mật khẩu
POST   /api/auth/reset-password     UC22 - Đặt lại mật khẩu

GET    /api/templates               UC03/UC23 - Danh sách mẫu thiệp
POST   /api/templates               UC12 - Tạo mẫu (Admin)

GET    /api/greetings/view/{token}  UC09/UC24 - Xem thiệp qua link
GET    /api/greetings/my            UC07 - Thiệp đã gửi
POST   /api/greetings/send          UC06/UC08 - Gửi thiệp (User + Guest)

GET    /api/users/profile           UC19 - Hồ sơ cá nhân
PUT    /api/users/profile           UC19 - Cập nhật hồ sơ
GET    /api/users/contacts          UC16 - Danh bạ
POST   /api/users/contacts          UC16 - Thêm liên hệ
GET    /api/users/drafts            UC17 - Bản nháp
POST   /api/users/drafts            UC17 - Lưu bản nháp

POST   /api/subscriptions           UC10 - Đăng ký gói
POST   /api/subscriptions/{id}/renew UC26 - Gia hạn (BR-30)
GET    /api/subscriptions/payments  UC25 - Lịch sử thanh toán

POST   /api/feedback                UC13 - Gửi phản hồi

GET    /api/admin/users             UC14 - Danh sách users
POST   /api/admin/users/{id}/ban    UC21 - Ban/Unban user
GET    /api/admin/categories        UC27 - Danh mục
DELETE /api/admin/categories/{id}   UC27 - Xóa danh mục (BR-31)
PUT    /api/admin/content/{key}     UC28 - Cập nhật nội dung
GET    /api/admin/audit-logs        UC30 - Audit logs (BR-33)
GET    /api/admin/audit-logs/export UC30 - Export CSV
GET    /api/admin/dashboard         Dashboard tổng quan
```
