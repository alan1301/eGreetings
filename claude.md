# E-Greetings – Tài Liệu Ngữ Cảnh Dự Án (claude.md)

> File này là ngữ cảnh tổng hợp dành cho AI assistant. Đọc kỹ trước khi hỗ trợ bất kỳ tác vụ nào trong dự án.

---

## 1. Tổng Quan Dự Án

**E-Greetings** là ứng dụng web cho phép người dùng chọn thiệp điện tử từ danh mục trực tuyến, cá nhân hóa nội dung (tin nhắn, hình ảnh, video) và gửi đến người nhận qua email.

- Thiệp điện tử tương tự bưu thiếp/thiệp truyền thống, khác biệt ở chỗ được tạo và gửi bằng phương tiện kỹ thuật số.
- Có dịch vụ **Subscribe** (trả phí hàng tháng): tự động gửi 1 thiệp/ngày đến danh sách email đã đăng ký.

---

## 2. Kiến Trúc & Công Nghệ

### Backend
- **Framework**: ASP.NET Core (C#), kiến trúc **Clean Architecture**
- **Các layer**:
  - `EGreetings.API` – Controllers, Middleware, Program.cs
  - `EGreetings.Application` – Commands (CQRS), Queries, Behaviors
  - `EGreetings.Domain` – Entities, Domain logic
  - `EGreetings.Infrastructure` – EF Core, Email, Scheduled Jobs, Migrations
  - `EGreetings.Shared` – DTOs dùng chung
- **Database**: SQL Server (Azure SQL Edge chạy qua Docker)
- **ORM**: Entity Framework Core
- **Auth**: JWT Bearer Token
- **Email**: SMTP (dev: MailHog trên `localhost:1025`)
- **Logging**: Serilog

### Frontend
- **Framework**: Angular 21
- **Ngôn ngữ**: TypeScript 5.9
- **CSS**: TailwindCSS 3
- **Build tool**: Angular CLI
- **State**: RxJS

### Infrastructure
- **Docker**: SQL Server chạy qua `docker-compose.yml`
- **Email dev**: MailHog (`start-mailhog.sh`)

---

## 3. Cách Chạy Dự Án

### Bước 1 – Khởi động SQL Server
```bash
docker compose up -d
```
SQL Server chạy tại `localhost:1433` | SA Password: `Admin123@`

### Bước 2 – Khởi động Backend
```bash
cd Backend
dotnet run --project src/EGreetings.API
```
API chạy tại `https://localhost:7xxx` (xem launchSettings.json)

### Bước 3 – Khởi động Frontend
```bash
cd Frontend
npm install   # lần đầu
npm start     # ng serve → http://localhost:4200
```

### Bước 4 – Khởi động Email (dev, tuỳ chọn)
```bash
./start-mailhog.sh
```
MailHog UI: `http://localhost:8025` | SMTP: `localhost:1025`

---

## 4. Cấu Hình Dev (appsettings.Development.json)

| Key | Giá trị |
|---|---|
| Connection String | `Server=localhost,1433;Database=EGreetingsDb_Dev;User Id=sa;Password=Admin123@;TrustServerCertificate=True` |
| JWT Secret | `DEV_SECRET_KEY_CHANGE_IN_PRODUCTION_MIN32CHARS!!` |
| JWT Issuer | `EGreetings.API.Local` |
| JWT Audience | `EGreetings.Client.Local` |
| Access Token TTL | 1440 phút (24h) |
| RememberMe TTL | 30 ngày |
| SMTP Host | `localhost:1025` (MailHog) |
| CORS Origins | `localhost:3000`, `3001`, `4200`, `5173` |

---

## 5. Các Tác Nhân (Actors)

| Tác nhân | Loại | Vai trò |
|---|---|---|
| **Admin** | Quản trị viên | Quản lý mẫu thiệp, xem báo cáo, xác nhận thanh toán, vô hiệu hóa tài khoản |
| **User** | Đã đăng ký | Duyệt, chọn, cá nhân hóa, gửi thiệp; quản lý danh bạ, bản nháp; đăng ký Subscribe |
| **Guest** | Chưa đăng ký | Duyệt danh mục, xem thiệp, đăng ký tài khoản |
| **Cổng thanh toán (GW)** | Hệ thống bên thứ ba | VNPay/MoMo/PayPal – xử lý giao dịch, gửi Webhook xác nhận |
| **System** | Tự động | Gửi thiệp Subscribe hàng ngày, vô hiệu hóa dịch vụ hết hạn, retry email thất bại |

---

## 6. Yêu Cầu Chức Năng

### 6.1 Website (Giao diện chung)
- Hiển thị danh mục thiệp phân loại rõ ràng: **Sinh nhật, Đám cưới, Năm mới, Lễ hội** (có thể thêm)
- Navigation bar hiển thị xuyên suốt, gồm: `TRANG CHỦ | SINH NHẬT | ĐÁM CƯỚI | NĂM MỚI | LỄ HỘI | ĐĂNG KÝ DỊCH VỤ | PHẢN HỒI | ĐĂNG KÝ TÀI KHOẢN | ĐĂNG NHẬP`
- Footer hiển thị xuyên suốt trên mọi trang

### 6.2 Người Dùng (User)
- Đăng ký tài khoản → xác thực email → đăng nhập
- Duyệt & chọn mẫu thiệp
- Cá nhân hóa thiệp (tin nhắn ≤500 ký tự, ảnh JPG/PNG/GIF ≤5MB, video MP4 ≤20MB)
- Gửi thiệp ngay hoặc hẹn giờ (≥5 phút sau hiện tại)
- Lưu bản nháp (auto-save mỗi 30 giây)
- Xem lịch sử gửi
- Quản lý danh bạ (tối đa 200 liên hệ)
- Gửi phản hồi (tối đa 5 phản hồi/ngày)
- Đăng ký dịch vụ Subscribe (trả phí hàng tháng)
- Quên mật khẩu / đặt lại mật khẩu
- Quản lý hồ sơ cá nhân

### 6.3 Admin
- Thêm / sửa / ẩn / xóa mẫu thiệp (không xóa cứng nếu có lịch sử giao dịch)
- Quản lý danh mục thiệp
- Xem & trả lời phản hồi từ người dùng
- Xem báo cáo giao dịch (bao gồm thông tin thiệp đã gửi, địa chỉ nhận, thời gian)
- Xác nhận thanh toán Subscribe
- Vô hiệu hóa / khóa tài khoản người dùng
- Xem nhật ký hệ thống (System Log), lọc, xuất CSV/Excel
- Quản lý nội dung trang web

### 6.4 Dịch Vụ Subscribe
- User cung cấp ≥10 địa chỉ email để đăng ký
- Thanh toán hàng tháng để kích hoạt
- Hệ thống tự động gửi 1 thiệp/ngày/người nhận (lúc 08:00)
- Tự động vô hiệu hóa khi hết hạn chưa gia hạn
- Admin xác nhận thanh toán thủ công hoặc qua Webhook

---

## 7. Vòng Đời Trạng Thái Subscribe

```
[Guest đăng ký] → Pending → (Admin xác nhận thanh toán) → Active
                                                            ↓ (hết hạn)
                                                          Expired → (gia hạn) → Active
                                                            ↓ (Admin vô hiệu)
                                                          Disabled (phải đăng ký mới)
```

| Trạng thái | Mô tả |
|---|---|
| **Pending** | Chờ xác nhận thanh toán, dịch vụ chưa kích hoạt |
| **Active** | Đang hoạt động, gửi thiệp tự động hàng ngày |
| **Expired** | Hết hạn, chưa gia hạn – có thể gia hạn |
| **Disabled** | Bị vô hiệu hóa bởi Admin – phải đăng ký mới |

---

## 8. Danh Sách Use Case (UC01 – UC30)

| Mã | Tên | Nhóm | Tác nhân |
|---|---|---|---|
| UC01 | Đăng ký tài khoản | Auth | Guest |
| UC02 | Đăng nhập hệ thống | Auth | User/Admin |
| UC03 | Duyệt danh mục thiệp | Thiệp | User/Guest |
| UC04 | Chọn mẫu thiệp | Thiệp | User |
| UC05 | Cá nhân hóa thiệp | Thiệp | User |
| UC06 | Gửi thiệp điện tử | Thiệp | User |
| UC07 | Gửi phản hồi | Thiệp | User |
| UC08 | Đăng ký dịch vụ Subscribe | Subscribe | User/Guest |
| UC09 | Admin – Thêm mẫu thiệp | Admin | Admin |
| UC10 | Admin – Ẩn/Xóa mẫu thiệp | Admin | Admin |
| UC11 | Admin – Xem phản hồi | Admin | Admin |
| UC12 | Admin – Xem báo cáo giao dịch | Admin | Admin |
| UC13 | Admin – Xác nhận thanh toán | Admin | Admin |
| UC14 | Admin – Vô hiệu hóa tài khoản | Admin | Admin |
| UC15 | Hệ thống – Tự động vô hiệu hóa Subscribe hết hạn | System | System |
| UC16 | Quản lý danh bạ | Thiệp | User |
| UC17 | Bản nháp & Lịch sử gửi | Thiệp | User |
| UC18 | Đăng xuất hệ thống | Auth | User/Admin |
| UC19 | Quản lý hồ sơ cá nhân | Auth | User |
| UC20 | Hệ thống – Gửi thiệp tự động (Subscribe Job) | System | System |
| UC21 | Admin – Quản lý người dùng / Subscribe | Admin | Admin |
| UC22 | Quên & Đặt lại mật khẩu | Auth | User/Guest |
| UC23 | Xem Trang Chủ | Thiệp | User/Guest |
| UC24 | Xem Chi Tiết Thiệp (Guest) | Thiệp | Guest |
| UC25 | Xem Lịch Sử Thanh Toán | Subscribe | User |
| UC26 | Gia Hạn Subscribe | Subscribe | User |
| UC27 | Admin – Quản Lý Danh Mục | Admin | Admin |
| UC28 | Admin – Quản Lý Nội Dung Web | Admin | Admin |
| UC29 | Hệ thống – Retry Gửi Email Thất Bại | System | System |
| UC30 | Hệ thống – Ghi Log & Kiểm Tra (Audit) | System | System/Admin |

---

## 9. Quy Tắc Nghiệp Vụ (Business Rules)

| Mã | Quy tắc | Use Case |
|---|---|---|
| BR-01 | Mật khẩu ≥8 ký tự: hoa + thường + số + ký tự đặc biệt | UC01 |
| BR-02 | Mỗi email chỉ đăng ký được 1 tài khoản | UC01 |
| BR-03 | Tài khoản kích hoạt sau xác thực email. TTL token 24h, dùng 1 lần | UC01 |
| BR-04 | Khóa tài khoản 15 phút sau 5 lần đăng nhập sai. CAPTCHA từ lần thứ 3 | UC02 |
| BR-05 | "Ghi nhớ đăng nhập" lưu phiên tối đa 30 ngày | UC02 |
| BR-06 | Hình ảnh đính kèm: JPG/PNG/GIF, ≤5 MB | UC05 |
| BR-07 | Video đính kèm: MP4, ≤20 MB | UC05 |
| BR-08 | Tin nhắn cá nhân ≤500 ký tự | UC05 |
| BR-09 | Auto-save bản nháp mỗi 30 giây | UC05, UC17 |
| BR-10 | User không Subscribe: tối đa 50 thiệp/ngày | UC06 |
| BR-11 | Hẹn giờ gửi thiệp: ≥5 phút sau thời điểm hiện tại | UC06 |
| BR-12 | Mọi giao dịch gửi thiệp phải lưu vào CSDL | UC06, UC12 |
| BR-13 | Tối đa 5 phản hồi / tài khoản / ngày | UC07 |
| BR-14 | Subscribe: danh sách gửi ≥10 địa chỉ email | UC08 |
| BR-15 | Dịch vụ Subscribe chỉ kích hoạt sau khi thanh toán xác nhận | UC08, UC13 |
| BR-16 | Hệ thống tự vô hiệu hóa dịch vụ hết hạn chưa gia hạn | UC08, UC15 |
| BR-17 | Chỉ Admin có quyền thêm/sửa/ẩn/xóa mẫu thiệp | UC09, UC10 |
| BR-18 | Không xóa cứng mẫu thiệp đã có lịch sử giao dịch | UC10 |
| BR-19 | Mẫu Ẩn vẫn lưu CSDL để tham chiếu lịch sử; không hiển thị cho User | UC10 |
| BR-20 | Danh bạ: tối đa 200 liên hệ / tài khoản | UC16 |
| BR-21 | Tên liên hệ ≤100 ký tự; email phải hợp lệ | UC16 |
| BR-22 | Guest đăng ký Subscribe → tạo tài khoản tự động sau thanh toán thành công | UC08 |
| BR-23 | Subscribe gửi 1 thiệp/ngày/người nhận, chọn ngẫu nhiên từ danh mục "Nổi bật" hoặc thiết lập User | UC20 |
| BR-24 | Mỗi Subscribe chỉ gửi tối đa 1 lần/ngày cho cùng 1 email nhận (chống spam) | UC20 |
| BR-25 | Logout xóa session/JWT ngay lập tức | UC18 |
| BR-26 | Token đặt lại mật khẩu (Forgot Password): TTL 15 phút, chỉ dùng 1 lần | UC22 |
| BR-27 | Navigation bar & Footer bắt buộc hiển thị xuyên suốt, chứa đúng các liên kết quy định | UC02, UC03 |
| BR-28 | Email gửi từ `noreply@e-greetings.com`, header `Reply-To` = email User gửi | UC06, UC20 |
| BR-29 | Frontend & Backend đều phải validate form: không Submit nếu sai format, chống XSS/SQL Injection | UC01–UC07 |
| BR-30 | Gia hạn Subscribe: +30 ngày từ ngày hết hạn (còn hạn) hoặc từ ngày thanh toán (đã hết hạn) | UC26 |
| BR-31 | Không xóa cứng danh mục đang chứa mẫu thiệp hoạt động; phải dùng chức năng Ẩn | UC27 |
| BR-32 | Retry gửi email thất bại: tối đa 3 lần, cách 5 phút/lần; sau 3 lần → đánh dấu "Failed" + alert Admin | UC29, UC20 |
| BR-33 | System Log ghi ≥5 nhóm sự kiện; lưu tối thiểu 30 ngày; không cho sửa/xóa log đã ghi | UC30 |

---

## 10. Yêu Cầu Hệ Thống (Non-Functional)

- Dữ liệu động chỉ được cập nhật bởi Admin
- Constraint validation trên tất cả các trường nhập liệu
- Điều hướng giữa các trang phải mượt mà
- Admin xem toàn bộ dữ liệu hệ thống
- System Log ghi nhận 5 nhóm sự kiện: đăng nhập/xuất, thanh toán, gửi thiệp, hành động Admin, lỗi hệ thống
- Log lưu tối thiểu 30 ngày, không thể sửa/xóa
- Giao diện xem log Admin: pagination 50 dòng/trang
- Batch log (Subscribe job) lưu tối thiểu 90 ngày
- Retry email: tối đa 3 lần, mỗi lần cách 5 phút (BR-32)

---

## 11. Cơ Chế Hệ Thống Tự Động

### Subscribe Job (UC20)
- Chạy lúc **08:00 hàng ngày**
- Truy vấn tất cả Subscribe `Active`
- Chọn thiệp ngẫu nhiên từ danh mục "Nổi bật" (hoặc thiết lập User)
- Kiểm tra không gửi trùng email trong ngày (BR-24)
- Gửi email, lưu giao dịch vào CSDL
- Ghi batch log; Admin xem log trong Dashboard

### Retry Email (UC29)
- Phát hiện email thất bại qua polling mỗi 5 phút
- Retry tối đa 3 lần (BR-32)
- Sau 3 lần thất bại: đánh dấu `Failed` + gửi alert cho Admin

### Auto Disable Subscribe (UC15)
- Tự động chuyển trạng thái `Active → Expired` khi hết hạn chưa gia hạn

---

## 12. Cấu Trúc Thư Mục

```
eGreeting/
├── docker-compose.yml          # SQL Server (Azure SQL Edge)
├── start-mailhog.sh            # Khởi động MailHog dev email
├── Backend/
│   ├── EGreetings.sln
│   └── src/
│       ├── EGreetings.API/         # Controllers, Middleware, Program.cs
│       ├── EGreetings.Application/ # CQRS Commands/Queries, Behaviors
│       ├── EGreetings.Domain/      # Entities, Domain logic
│       ├── EGreetings.Infrastructure/ # EF Core, Email, Jobs, Migrations
│       └── EGreetings.Shared/      # DTOs dùng chung
└── Frontend/
    ├── src/
    │   └── app/
    │       ├── core/               # Services, Guards, Interceptors
    │       ├── features/           # Admin, Auth, Cards, Subscribe...
    │       └── shared/             # Components dùng chung
    ├── angular.json
    ├── package.json
    └── tailwind.config.js
```

---

## 13. Tính Năng Bổ Sung (Ngoài Tài Liệu Gốc)

> Các tính năng dưới đây được phát sinh trong quá trình coding, **không có trong usecase gốc**. AI cần biết để không tạo ra code xung đột.

---

### 13.1 User Dashboard (`/design`)

**Route:** `GET /design` (Frontend) — Component: `DesignComponent`

Trang tổng quan cá nhân sau khi User đăng nhập. Bao gồm:
- **Danh sách bản nháp gần đây** (tối đa 4): lấy từ `GET /api/me/drafts?pageSize=4`
- **Sự kiện sắp tới từ danh bạ** (trong 30 ngày): lấy từ `GET /api/contacts/upcoming`
- **Banner thông báo thành công** khi gửi thiệp xong (redirect kèm `?sent=true&to=email`)

> Đây là trang "hub" chính của User sau login, thay thế cho việc redirect về trang chủ.

---

### 13.2 Contact Occasion Reminder (`/api/contacts/upcoming`)

**Endpoint:** `GET /api/contacts/upcoming` — Controller: `ContactsController`

Tính năng nhắc nhở dịp đặc biệt từ danh bạ:
- Lọc các contact có `OccasionDate` trong vòng **30 ngày tới**
- So sánh theo ngày/tháng, **bỏ qua năm** → tái diễn hàng năm (sinh nhật, kỷ niệm...)
- Trả về danh sách sắp xếp theo `DaysLeft` tăng dần
- Mỗi contact có thể lưu: `OccasionDate` (ngày/tháng), `OccasionLabel` (nhãn tự đặt, ví dụ: "Sinh nhật", "Kỷ niệm cưới"), `Group` (Friends/Family/Colleagues/Other)

**Model Contact mở rộng:**

| Field | Kiểu | Mô tả |
|---|---|---|
| `Name` | string (≤100 ký tự) | Tên liên hệ (BR-21) |
| `Email` | string | Email hợp lệ (BR-21) |
| `Group` | enum | Friends / Family / Colleagues / Other |
| `OccasionDate` | DateOnly? | Ngày dịp đặc biệt (tái diễn hàng năm) |
| `OccasionLabel` | string? | Nhãn tự đặt cho dịp đặc biệt |

---

### 13.3 Admin Dashboard Stats (`/api/admin/dashboard`)

**Endpoint:** `GET /api/admin/dashboard` — Controller: `CompatController`

Trả về số liệu tổng hợp nhanh cho trang Admin Dashboard:

| Field | Mô tả |
|---|---|
| `totalUsers` | Tổng số tài khoản trong hệ thống |
| `activeSubscriptions` | Số Subscribe đang Active |
| `pendingPayments` | Số Subscribe đang chờ xác nhận thanh toán (Pending) |
| `greetingsSentToday` | Số thiệp đã gửi trong ngày hôm nay |
| `unreadFeedbacks` | Số phản hồi chưa đọc/xử lý |
| `revenueThisMonth` | Doanh thu tháng này (tạm thời = 0, chưa implement) |

> **Lưu ý:** Endpoint này nằm trong `CompatController` (không phải `AdminController`) vì được implement nhanh không qua CQRS handler riêng.

---

### 13.4 Compatibility API Layer (`CompatController`)

**Controller:** `CompatController` — Route prefix: `/api`

Layer tương thích cho phép Frontend cũ (Frontend2) gọi API mà không cần đổi URL:

| Compat Endpoint | Trỏ đến |
|---|---|
| `GET /api/templates` | `GET /api/cards` (param `keyword` → `search`) |
| `GET /api/templates/{id}` | `GET /api/cards/{id}` |
| `GET /api/greetings/my` | `GET /api/me/greeting-history` |
| `POST /api/greetings/send` | `POST /api/greetings` |
| `GET /api/subscriptions/plans` | Trả về plan tĩnh (Standard: 5.000đ/email/tháng, min 10 email, 30 ngày) |
| `GET /api/subscriptions/my` | Subscription hiện tại của User (truy vấn trực tiếp DB) |

> **Quan trọng:** Khi thêm endpoint mới cho thiệp/gửi thiệp, cần kiểm tra `CompatController` để tránh xung đột route.

---

### 13.5 Admin – Mở Khóa Tài Khoản (Unlock User)

**Endpoint:** `POST /api/admin/users/{id}/unlock` — Nằm trong `AdminController`

Tính năng bổ sung cho UC21: Admin có thể **mở khóa** tài khoản đã bị khóa, không chỉ khóa.
- Đây là thao tác ngược của `LockUser` (UC14/UC21)
- Frontend: trang `admin/users` có nút Unlock tương ứng

---

### 13.6 Admin – Đánh Dấu Phản Hồi Đã Xử Lý

**Endpoint:** `PATCH /api/admin/feedbacks/{id}/read` — Nằm trong `AdminController`

Tính năng bổ sung cho UC11: Admin có thể đánh dấu phản hồi là **"Đã xử lý"** (Unread → Read).
- Phản hồi có trạng thái: `Unread` / `Read`
- Admin Dashboard hiển thị badge đếm số phản hồi `Unread`

---

### 13.7 Trang Giới Thiệu (`/about`)

**Route:** `GET /about` — Component: `AboutComponent`

Trang tĩnh giới thiệu về dự án E-Greetings. Không cần đăng nhập, accessible với cả Guest và User.

---

### 13.8 Guard Routing (Frontend)

4 loại route guard được implement:

| Guard | Điều kiện cho phép | Dùng cho |
|---|---|---|
| `authGuard` | Đã đăng nhập (có JWT hợp lệ) | Profile, Personalize, History, Contacts |
| `adminGuard` | Đăng nhập + Role = Admin | Tất cả route `/admin/*` |
| `guestGuard` | Chưa đăng nhập | Login, Register (tránh vào lại khi đã login) |
| `nonAdminGuard` | Không phải Admin (Guest hoặc User thường) | Trang chủ, Cards, Subscribe... |

