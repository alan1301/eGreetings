# E-Greetings – Project Summary (Claude.ai Projects)

Ứng dụng web gửi **thiệp điện tử** (e-greeting cards). User chọn mẫu, cá nhân hóa, gửi email đến người nhận. Có dịch vụ Subscribe trả phí: tự động gửi 1 thiệp/ngày.

---

## Tech Stack

| Layer | Công nghệ |
|---|---|
| Frontend | Angular 21, TailwindCSS 3, TypeScript 5.9 |
| Backend | ASP.NET Core (C#), Clean Architecture, CQRS (MediatR) |
| Database | SQL Server (Docker – Azure SQL Edge) |
| Auth | JWT Bearer + HttpOnly cookie (refresh token) |
| Email (dev) | MailHog `localhost:1025` |
| Jobs | Hangfire (scheduled jobs) |
| ORM | Entity Framework Core |

## Cách Chạy

```bash
docker compose up -d                          # SQL Server :1433
cd Backend && dotnet run --project src/EGreetings.API   # API
cd Frontend && npm start                      # Angular :4200
./start-mailhog.sh                            # Email dev UI :8025
```

**DB:** `Server=localhost,1433;Database=EGreetingsDb_Dev;User Id=sa;Password=Admin123@`

---

## Kiến Trúc Backend

```
EGreetings.API          → Controllers, Middleware
EGreetings.Application  → Commands/Queries (CQRS), Validators
EGreetings.Domain       → Entities, Enums, Business logic
EGreetings.Infrastructure → EF Core, Email, Hangfire Jobs, Migrations
EGreetings.Shared       → DTOs, ApiResponse<T>, PagedResult<T>
```

⚠️ **CompatController** (`/api/templates`, `/api/greetings/send`, `/api/subscriptions/my`, `/api/admin/dashboard`) là alias layer cho Frontend cũ — kiểm tra trước khi thêm route mới tránh conflict.

---

## Roles & Route Guards

| Role | Truy cập |
|---|---|
| **Admin** | `/admin/*` (adminGuard) — KHÔNG thấy trang user |
| **User** | Tất cả trang user (authGuard) |
| **Guest** | Public pages; bị chặn khi vào `/login`, `/register` nếu đã login (guestGuard) |

---

## Trạng Thái Implement

### ✅ Hoàn thành

**Auth**
- Đăng ký (BR-01,02,03) + xác thực email
- Đăng nhập JWT + RememberMe cookie 30 ngày (BR-04,05)
- Đăng xuất (BR-25), Forgot/Reset password (BR-26)
- Cập nhật hồ sơ cá nhân

**Cards**
- Danh sách + lọc theo category/search/sort/featured, phân trang
- Chi tiết thiệp (public — Guest xem được)
- Danh mục (`GET /api/categories`)
- Cá nhân hóa: text, màu sắc, theme (có color-wheel component)
- ⚠️ **Premium template** check: backend có guard `PREMIUM_TEMPLATE` nhưng frontend chưa phân tier rõ ràng

**Bản nháp & Lịch sử**
- Tạo bản nháp, auto-save 30 giây (BR-09)
- Danh sách bản nháp của User (`GET /api/me/drafts`)
- Lịch sử gửi (`GET /api/me/greeting-history`)

**Gửi thiệp**
- Gửi ngay (BR-10: max 50/ngày nếu không Subscribe)
- Hẹn giờ gửi (BR-11: ≥5 phút)
- Cơ chế Send on behalf of (BR-28: Reply-To = email User)

**Contacts**
- Danh sách, thêm contact (BR-20: max 200, BR-21)
- **Nhắc dịp đặc biệt 30 ngày** (`GET /api/contacts/upcoming`) — tái diễn hàng năm
- Phân nhóm: Friends/Family/Colleagues/Other

**Subscribe**
- Đăng ký (min 10 email, BR-14), trạng thái: Pending→Active→Expired→Disabled
- Gia hạn +30 ngày (BR-30)
- Xem trạng thái hiện tại

**System Jobs (Hangfire)**
- UC15: Auto-disable Subscribe hết hạn (chạy 00:01 hàng ngày)
- UC20: Gửi thiệp tự động cho Subscribe Active (08:00 hàng ngày)
- UC29: Retry email thất bại (max 3 lần, cách 5 phút — BR-32)

**Admin**
- Dashboard: totalUsers, activeSubscriptions, pendingPayments, greetingsSentToday, unreadFeedbacks
- Quản lý mẫu thiệp: Thêm, sửa, ẩn (BR-19), xóa (BR-18)
- Quản lý danh mục: Thêm, ẩn (BR-31)
- Quản lý Users: Danh sách, Lock (BR-04), **Unlock**
- Quản lý Subscriptions: Danh sách, Activate (UC13), Disable (UC14)
- Phản hồi: Xem danh sách, đánh dấu đã đọc
- Báo cáo giao dịch: Filter theo ngày, search, phân trang
- System Logs: Filter theo eventType/status/date, phân trang 50/trang (BR-33)

**Frontend Pages**

| Route | Component | Guard |
|---|---|---|
| `/` | HomeComponent | nonAdminGuard |
| `/cards` | CardListComponent | nonAdminGuard |
| `/cards/:id` | CardDetailComponent | nonAdminGuard |
| `/categories/:slug` | CardListComponent | nonAdminGuard |
| `/cards/:id/personalize` | PersonalizeComponent | authGuard |
| `/history` | HistoryComponent | authGuard |
| `/contacts` | ContactsComponent | authGuard |
| `/design` | DesignComponent (User Hub) | authGuard |
| `/profile` | ProfileComponent | authGuard |
| `/subscribe` | SubscribeComponent | nonAdminGuard |
| `/login` `/register` | Auth components | guestGuard |
| `/about` | AboutComponent | nonAdminGuard |
| `/admin` | AdminDashboard | adminGuard |
| `/admin/users` | AdminUsers | adminGuard |
| `/admin/subscriptions` | AdminSubscriptions | adminGuard |
| `/admin/feedback` | AdminFeedback | adminGuard |
| `/admin/cards` | AdminCards | adminGuard |
| `/admin/logs` | AdminLogs | adminGuard |

---

### ⏳ Chưa Hoàn Thành / TODO

| Feature | Ghi chú |
|---|---|
| **Payment Gateway** (VNPay/MoMo) | Spec có đề cập, chưa implement — Admin xác nhận thanh toán thủ công |
| **`revenueThisMonth`** | Hardcode = 0 trong dashboard |
| **Export log CSV/Excel** | UC30 có đề cập, chưa có endpoint |
| **CAPTCHA** (BR-04) | Backend check số lần sai, CAPTCHA frontend chưa có |
| **Forgot Password frontend page** | Backend endpoint có, chưa rõ frontend page |
| **Premium Template tier** | Backend có guard, frontend chưa phân loại Free/Premium rõ ràng |
| **Danh bạ: Sửa/Xóa contact** | Chỉ có GET + POST, chưa có PUT/DELETE |

---

## Business Rules Quan Trọng Nhất

| BR | Quy tắc |
|---|---|
| BR-02 | 1 email = 1 tài khoản |
| BR-03 | Phải xác thực email mới login được (TTL 24h) |
| BR-04 | Khóa 15 phút sau 5 lần sai |
| BR-09 | Auto-save draft mỗi 30 giây |
| BR-10 | User không Subscribe: max 50 thiệp/ngày |
| BR-14 | Subscribe cần ≥10 email |
| BR-18 | Không xóa cứng thiệp có lịch sử giao dịch |
| BR-26 | Token reset password TTL 15 phút, dùng 1 lần |
| BR-28 | Email gửi từ noreply@, Reply-To = email User |
| BR-32 | Retry email max 3 lần, cách 5 phút |
| BR-33 | Log lưu ≥30 ngày, không sửa/xóa |

---

## Lưu Ý Khi Coding

1. **Thêm endpoint mới** → kiểm tra `CompatController` tránh route conflict
2. **CQRS pattern**: mọi business logic qua `IMediator.Send()`, không viết logic trực tiếp trong Controller
3. **Response format**: luôn dùng `ApiResponse<T>.Ok()` / `ApiResponse<T>.Created()` / `ApiResponse.Fail()`
4. **Phân trang**: dùng `PagedResult<T>` với `Meta` (Page, PageSize, Total)
5. **Admin routes** bắt đầu bằng `/api/admin/` — xử lý trong `AdminController`, không phải `CompatController`
6. **User Dashboard** ở route `/design` (không phải `/dashboard`)
