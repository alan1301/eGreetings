# E-Greetings Backend — API Documentation

> **Version:** v6.0 | **Stack:** ASP.NET Core 9 · Clean Architecture · CQRS (MediatR) · EF Core · SQLite

---

## Base URL

```
https://localhost:5001/api
```

---

## Authentication

All protected endpoints require a JWT Bearer token in the `Authorization` header:

```
Authorization: Bearer <token>
```

Tokens are returned from the `/api/auth/login` endpoint.

**Roles:**
- `User` — default role after registration
- `Admin` — platform administrator (full access)

---

## Response Format

All responses follow a consistent envelope:

```json
{
  "success": true,
  "data": { ... },
  "message": "Optional human-readable message"
}
```

Errors:

```json
{
  "success": false,
  "message": "Mô tả lỗi"
}
```

---

## Error Handling

| HTTP Code | Meaning |
|-----------|---------|
| 200 | OK |
| 201 | Created |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (missing/invalid JWT) |
| 403 | Forbidden (insufficient role) |
| 404 | Not Found |
| 409 | Conflict (duplicate) |
| 500 | Internal Server Error |

---

## Endpoints

---

### 🔐 Auth — `/api/auth`

#### `POST /api/auth/register`
**UC01 · Đăng ký tài khoản mới**

No auth required.

**Request body:**
```json
{
  "fullName": "Nguyen Van A",
  "email": "user@example.com",
  "password": "Abc@12345",
  "confirmPassword": "Abc@12345",
  "phone": "0901234567"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
  "data": { "id": 1, "email": "user@example.com" }
}
```

**Business Rules:** BR-01 (email unique), BR-02 (password >= 8 chars, complexity), BR-07 (email verification required).

---

#### `POST /api/auth/login`
**UC02 · Đăng nhập**

No auth required.

**Request body:**
```json
{
  "email": "user@example.com",
  "password": "Abc@12345"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGci...",
    "userId": 1,
    "email": "user@example.com",
    "fullName": "Nguyen Van A",
    "role": "User",
    "expiresAt": "2026-04-13T00:00:00Z"
  }
}
```

**Business Rules:** BR-03 (email must be verified before login), BR-04 (lockout 15 min after 5 failed attempts).

---

#### `POST /api/auth/logout`
**UC18 · Đăng xuất**

No server-side action (JWT is stateless). Client must delete the token locally.

**Response:**
```json
{ "success": true, "message": "Đăng xuất thành công." }
```

---

#### `GET /api/auth/verify-email?token={token}`
**UC01 · Xác thực email**

No auth required.

**Query params:** `token` (string) — verification token from email.

**Response:**
```json
{ "success": true, "message": "Email đã được xác thực thành công." }
```

---

#### `POST /api/auth/forgot-password`
**UC22 · Quên mật khẩu**

No auth required.

**Request body:**
```json
{ "email": "user@example.com" }
```

**Response:** Always returns 200 (to prevent email enumeration):
```json
{ "success": true, "message": "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu." }
```

---

#### `POST /api/auth/reset-password`
**UC22 · Đặt lại mật khẩu**

No auth required.

**Request body:**
```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewAbc@123",
  "confirmPassword": "NewAbc@123"
}
```

**Business Rules:** BR-02 (password complexity), BR-06 (token expires after 1 hour).

---

### 📁 Categories — `/api/categories`

#### `GET /api/categories`
**UC03 · Xem danh sách danh mục (Public)**

No auth required.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Sinh nhật",
      "description": "Thiệp chúc mừng sinh nhật",
      "iconUrl": "https://...",
      "isActive": true,
      "templateCount": 12,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

Returns only active categories. `templateCount` = number of active templates in each category.

---

### 🎴 Templates — `/api/templates`

#### `GET /api/templates`
**UC03/UC23 · Xem danh sách mẫu thiệp (Public)**

No auth required.

**Query params:**
| Param | Type | Description |
|-------|------|-------------|
| `categoryId` | int? | Filter by category |
| `keyword` | string? | Search by name/description |
| `isFree` | bool? | Filter free/paid templates |
| `page` | int | Default: 1 |
| `pageSize` | int | Default: 12 |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "categoryId": 1,
        "categoryName": "Sinh nhật",
        "name": "Happy Birthday Flowers",
        "thumbnailUrl": "https://...",
        "isFree": true,
        "isActive": true,
        "usageCount": 150,
        "createdAt": "2026-01-01T00:00:00Z"
      }
    ],
    "total": 50,
    "page": 1,
    "pageSize": 12
  }
}
```

---

#### `GET /api/templates/{id}`
**UC04 · Xem chi tiết mẫu thiệp**

No auth required (public). Inactive templates visible only to Admin.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "categoryId": 1,
    "categoryName": "Sinh nhật",
    "name": "Happy Birthday Flowers",
    "description": "Mẫu thiệp hoa sinh nhật",
    "thumbnailUrl": "https://...",
    "htmlContent": "<div>...</div>",
    "cssStyle": ".greeting { ... }",
    "isFree": true,
    "isActive": true,
    "usageCount": 150,
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

#### `POST /api/templates`
**UC12 · Tạo mẫu thiệp mới**

🔐 **Admin only.**

**Request body:**
```json
{
  "categoryId": 1,
  "name": "Christmas Special",
  "description": "Thiệp Giáng Sinh đặc biệt",
  "thumbnailUrl": "https://...",
  "htmlContent": "<div>...</div>",
  "cssStyle": ".greeting { color: red; }",
  "isFree": false
}
```

---

#### `PUT /api/templates/{id}`
**UC12 · Cập nhật mẫu thiệp**

🔐 **Admin only.**

**Request body (all fields optional):**
```json
{
  "name": "Updated Name",
  "description": "Updated description",
  "thumbnailUrl": "https://new-thumb.png",
  "htmlContent": "<div>new html</div>",
  "cssStyle": ".updated { }",
  "isFree": true,
  "isActive": true
}
```

---

#### `PATCH /api/templates/{id}/visibility`
**UC12 · Ẩn/Hiện mẫu thiệp**

🔐 **Admin only.**

**Request body:**
```json
{ "hide": true }
```

`hide: true` = ẩn | `hide: false` = hiện lại.

**Business Rules:** BR-32 (template đang được dùng trong scheduled greetings vẫn có thể ẩn; greetings đang chờ vẫn sẽ gửi).

---

### 💌 Greetings — `/api/greetings`

#### `POST /api/greetings/send`
**UC06 · Gửi thiệp (User) | UC08 · Guest checkout**

No auth required (works for both logged-in users and guests).

**Request body:**
```json
{
  "templateId": 1,
  "recipientEmail": "recipient@example.com",
  "recipientName": "Nguyen Thi B",
  "senderMessage": "Chúc mừng sinh nhật bạn!",
  "customHtml": null,
  "scheduledAt": null
}
```

- `scheduledAt`: ISO 8601 datetime — để trống = gửi ngay.
- `customHtml`: HTML tùy chỉnh (User/Admin), nếu null thì dùng template gốc.
- Guest (không đăng nhập) chỉ có thể dùng mẫu miễn phí (BR-09).

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 42,
    "token": "abc123def456",
    "viewUrl": "https://egreetings.vn/view/abc123def456",
    "scheduledAt": null
  },
  "message": "Thiệp đã được gửi thành công!"
}
```

**Business Rules:** BR-09 (guest chỉ dùng mẫu free), BR-10 (gửi email qua SMTP), BR-11 (thiệp có unique token), BR-13 (lịch tự động gửi).

---

#### `GET /api/greetings/my`
**UC07 · Xem lịch sử thiệp đã gửi**

🔐 Auth required.

**Query params:** `page` (default 1), `pageSize` (default 10).

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 42,
        "templateName": "Happy Birthday Flowers",
        "recipientEmail": "recipient@example.com",
        "recipientName": "Nguyen Thi B",
        "status": "Sent",
        "scheduledAt": null,
        "sentAt": "2026-04-12T08:00:00Z",
        "viewUrl": "https://egreetings.vn/view/abc123def456"
      }
    ],
    "total": 5
  }
}
```

---

#### `GET /api/greetings/view/{token}`
**UC09/UC24 · Xem thiệp (người nhận / guest)**

No auth required.

**Path param:** `token` — unique token from thiệp link.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 42,
    "senderMessage": "Chúc mừng sinh nhật bạn!",
    "recipientName": "Nguyen Thi B",
    "htmlContent": "<div>rendered HTML</div>",
    "cssStyle": ".greeting { }",
    "sentAt": "2026-04-12T08:00:00Z"
  }
}
```

---

### 📋 Subscriptions — `/api/subscriptions`

> All subscription endpoints require authentication (`[Authorize]`).

#### `GET /api/subscriptions/my`
**UC11 · Xem gói Subscribe của tôi**

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 5,
    "userId": 1,
    "status": "Active",
    "price": 199000,
    "startDate": "2026-01-01T00:00:00Z",
    "expiredAt": "2027-01-01T00:00:00Z",
    "maxRecipients": 50,
    "currentRecipientCount": 12,
    "recipients": [
      {
        "id": 1,
        "email": "recipient@example.com",
        "name": "Nguyen Thi B",
        "birthday": "1990-05-15T00:00:00Z",
        "occasion": "Birthday",
        "isActive": true
      }
    ],
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

#### `POST /api/subscriptions`
**UC10 · Đăng ký gói Subscribe**

**Request body:**
```json
{
  "planType": "Annual",
  "paymentMethod": "BankTransfer"
}
```

**Business Rules:** BR-12 (chỉ 1 gói active tại một thời điểm), BR-16 (pending payment sau khi tạo).

---

#### `POST /api/subscriptions/{id}/renew`
**UC26 · Gia hạn Subscribe**

**Request body:**
```json
"BankTransfer"
```

**Business Rules:** BR-30 (không gia hạn nếu gói đang bị disable bởi Admin).

---

#### `GET /api/subscriptions/{id}/recipients`
**UC11 · Xem danh sách người nhận**

---

#### `POST /api/subscriptions/{id}/recipients`
**UC11 · Thêm người nhận**

**Request body:**
```json
{
  "name": "Tran Van C",
  "email": "tranvanc@example.com",
  "birthday": "1992-03-20T00:00:00Z"
}
```

**Business Rules:** BR-14 (email format), BR-20 (không thêm quá MaxRecipients), BR-21 (email không trùng trong subscription).

---

#### `DELETE /api/subscriptions/{id}/recipients/{recipientId}`
**UC11 · Xoá người nhận**

---

#### `GET /api/subscriptions/payments`
**UC25 · Lịch sử thanh toán**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 10,
      "subscriptionId": 5,
      "amount": 199000,
      "status": "Paid",
      "method": "BankTransfer",
      "transactionCode": "TXN202604120001",
      "paidAt": "2026-04-12T09:00:00Z",
      "createdAt": "2026-04-12T08:30:00Z"
    }
  ]
}
```

---

### 👤 Users — `/api/users`

> All user endpoints require authentication (`[Authorize]`).

#### `GET /api/users/profile`
**UC19 · Xem hồ sơ cá nhân**

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "phone": "0901234567",
    "avatarUrl": "https://...",
    "role": "User",
    "status": "Active",
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

#### `PUT /api/users/profile`
**UC19 · Cập nhật hồ sơ cá nhân**

**Request body (all fields optional):**
```json
{
  "fullName": "Nguyen Van A Updated",
  "phone": "0909876543",
  "avatarUrl": "https://new-avatar.png"
}
```

---

#### `POST /api/users/change-password`
**UC19 A1 · Đổi mật khẩu**

**Request body:**
```json
{
  "currentPassword": "OldAbc@123",
  "newPassword": "NewXyz@456",
  "confirmPassword": "NewXyz@456"
}
```

**Business Rules:** BR-02 (password complexity), BR-05 (verify current password before change).

---

#### `GET /api/users/contacts`
**UC16 · Xem danh bạ người nhận**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Tran Thi B",
      "email": "b@example.com",
      "phone": "0911222333",
      "birthday": "1990-03-15T00:00:00Z",
      "note": "Bạn thân",
      "group": "Friends"
    }
  ]
}
```

---

#### `POST /api/users/contacts`
**UC16 · Thêm liên hệ vào danh bạ**

**Request body:**
```json
{
  "name": "Tran Thi B",
  "email": "b@example.com",
  "phone": "0911222333",
  "birthday": "1990-03-15T00:00:00Z",
  "note": "Bạn thân",
  "group": "Friends"
}
```

---

#### `GET /api/users/drafts`
**UC17 · Xem danh sách bản nháp**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 3,
      "templateId": 1,
      "title": "Nháp sinh nhật Lan",
      "recipientEmail": "lan@example.com",
      "recipientName": "Nguyen Thi Lan",
      "senderMessage": "Chúc...",
      "updatedAt": "2026-04-10T14:00:00Z"
    }
  ]
}
```

---

#### `POST /api/users/drafts`
**UC17 · Lưu bản nháp**

**Request body:**
```json
{
  "templateId": 1,
  "title": "Nháp sinh nhật Lan",
  "customHtml": "<div>...</div>",
  "recipientEmail": "lan@example.com",
  "recipientName": "Nguyen Thi Lan",
  "senderMessage": "Chúc mừng sinh nhật!"
}
```

---

### 💬 Feedback — `/api/feedback`

#### `POST /api/feedback`
**UC13 · Gửi phản hồi / báo cáo**

No auth required (works anonymously or authenticated).

**Request body:**
```json
{
  "subject": "Lỗi hiển thị thiệp",
  "content": "Khi mở thiệp trên mobile bị vỡ layout.",
  "contactEmail": "user@example.com",
  "starRating": 3
}
```

- `starRating`: 1–5 sao (optional).

---

#### `GET /api/feedback`
**UC13 · Xem tất cả phản hồi**

🔐 **Admin only.**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "userId": 2,
      "subject": "Lỗi hiển thị thiệp",
      "content": "...",
      "starRating": 3,
      "status": "New",
      "reply": null,
      "repliedAt": null,
      "createdAt": "2026-04-11T10:00:00Z"
    }
  ]
}
```

---

### 🛠️ Admin — `/api/admin`

> All Admin endpoints require authentication + `Admin` role (`[Authorize(Roles = "Admin")]`).

#### `GET /api/admin/dashboard`
**UC14 · Dashboard thống kê tổng quan**

**Response:**
```json
{
  "success": true,
  "data": {
    "totalUsers": 500,
    "activeSubscriptions": 120,
    "pendingPayments": 15,
    "greetingsSentToday": 89,
    "unreadFeedbacks": 7,
    "revenueThisMonth": 23880000
  }
}
```

---

#### `GET /api/admin/users?search=&page=1&pageSize=20`
**UC14 · Xem danh sách người dùng**

**Query params:** `search` (optional), `page`, `pageSize`.

---

#### `POST /api/admin/users/{id}/ban`
**UC21 A4 · Ban / Unban User**

**Request body:**
```json
{
  "targetUserId": 0,
  "isBanning": true,
  "reason": "Vi phạm điều khoản sử dụng"
}
```

`isBanning: true` = ban | `isBanning: false` = unban.

**Business Rules:** BR-27 (ban kéo theo disable tất cả subscription đang active).

---

#### `GET /api/admin/categories`
**UC27 · Xem danh sách danh mục (Admin)**

Trả về tất cả danh mục (kể cả inactive).

---

#### `POST /api/admin/categories`
**UC27 · Tạo danh mục mới**

**Request body:**
```json
{
  "name": "Tốt nghiệp",
  "description": "Thiệp chúc mừng tốt nghiệp",
  "iconUrl": "https://icons.example.com/graduation.svg"
}
```

---

#### `PUT /api/admin/categories/{id}`
**UC27 · Cập nhật danh mục**

**Request body (all optional):**
```json
{
  "name": "Updated Name",
  "description": "Updated description",
  "iconUrl": "https://new-icon.svg",
  "isActive": false
}
```

---

#### `DELETE /api/admin/categories/{id}`
**UC27 · Vô hiệu hóa danh mục (soft delete)**

**Business Rules:** BR-31 (không xóa vật lý, chỉ `IsActive = false`).

---

#### `GET /api/admin/subscriptions?status=&userId=&page=1&pageSize=20`
**UC21 · Xem tất cả gói Subscribe**

**Query params:** `status` (Pending/Active/Expired/Disabled), `userId`, `page`, `pageSize`.

---

#### `POST /api/admin/subscriptions/{id}/disable`
**UC21 · Vô hiệu hóa gói Subscribe**

**Request body:**
```json
{ "reason": "Vi phạm điều khoản sử dụng" }
```

---

#### `GET /api/admin/greetings?userId=&status=&page=1&pageSize=20`
**UC12 · Xem báo cáo thiệp gửi**

**Query params:** `userId`, `status` (Pending/Sent/Failed), `page`, `pageSize`.

---

#### `POST /api/admin/feedback/{id}/reply`
**UC11 · Trả lời phản hồi**

**Request body:**
```json
{ "reply": "Cảm ơn bạn đã phản hồi! Chúng tôi đã ghi nhận và sẽ khắc phục sớm." }
```

---

#### `PATCH /api/admin/feedback/{id}/read`
**UC11 · Đánh dấu phản hồi đã đọc**

No body required.

---

#### `GET /api/admin/payments?status=&page=1&pageSize=20`
**UC13 · Xem danh sách thanh toán**

**Query params:** `status` (Pending/Paid/Failed), `page`, `pageSize`.

---

#### `POST /api/admin/payments/{id}/confirm`
**UC13 · Xác nhận thanh toán (kích hoạt Subscribe)**

**Request body:**
```json
{ "transactionCode": "TXN202604120001" }
```

Sau khi confirm: Payment → `Paid`, Subscription → `Active`, gửi email thông báo.

---

#### `GET /api/admin/content/{key}`
**UC28 · Lấy nội dung website theo key**

**Supported keys:** `banner`, `footer`, `about`, `terms`, `privacy`, v.v.

---

#### `PUT /api/admin/content/{key}`
**UC28 · Cập nhật nội dung website**

**Request body:**
```json
{
  "title": "Banner Tháng 4",
  "content": "<h1>Chào mừng mùa hè!</h1>",
  "imageUrl": "https://cdn.example.com/banner-april.jpg"
}
```

Mỗi lần cập nhật tạo một version mới (versioning).

---

#### `GET /api/admin/content/{key}/versions`
**UC28 · Xem lịch sử phiên bản nội dung**

---

#### `POST /api/admin/content/{key}/rollback/{versionId}`
**UC28 · Rollback nội dung về phiên bản cũ**

No body required.

---

#### `GET /api/admin/audit-logs`
**UC30 · Xem Audit Logs**

**Query params:**
| Param | Type | Description |
|-------|------|-------------|
| `entityName` | string? | Filter by entity (User, Greeting, etc.) |
| `userId` | int? | Filter by user |
| `from` | datetime? | Start date |
| `to` | datetime? | End date |
| `page` | int | Default: 1 |
| `pageSize` | int | Default: 50 |

**Business Rules:** BR-33 (logs retained >= 30 days).

---

#### `GET /api/admin/audit-logs/export`
**UC30 · Export Audit Logs (CSV)**

**Query params:** `from`, `to` (datetime).

**Response:** `Content-Type: text/csv` — file tải về `audit-logs-YYYYMMDD.csv`.

---

## Background Services

| Service | Trigger | Description |
|---------|---------|-------------|
| `AutoSendGreetingService` | Every minute | UC06 BR-13 — Gửi thiệp đã lên lịch khi đến giờ |
| `ExpireSubscriptionService` | Daily (midnight) | UC10 BR-17 — Đánh dấu gói Subscribe đã hết hạn |
| `RetryEmailService` | Every 5 mins | UC06 BR-10 — Thử lại gửi email thất bại (tối đa 3 lần) |
| `AuditLogCleanupService` | Daily | UC30 BR-33 — Xóa audit logs cũ hơn 30 ngày |

---

## Business Rules Summary

| Rule | Description |
|------|-------------|
| BR-01 | Email đăng ký phải là duy nhất |
| BR-02 | Password >= 8 ký tự, có chữ hoa + số + ký tự đặc biệt |
| BR-03 | Email phải được xác thực trước khi đăng nhập |
| BR-04 | Khóa tài khoản 15 phút sau 5 lần đăng nhập sai |
| BR-05 | Xác minh mật khẩu hiện tại khi đổi mật khẩu |
| BR-06 | Token reset mật khẩu hết hạn sau 1 giờ |
| BR-07 | Token xác thực email hết hạn sau 24 giờ |
| BR-09 | Guest chỉ được dùng mẫu thiệp miễn phí |
| BR-10 | Gửi email qua SMTP (MailKit); retry tối đa 3 lần |
| BR-11 | Mỗi thiệp có unique view token (GUID/random) |
| BR-12 | User chỉ có 1 gói Subscribe active tại một thời điểm |
| BR-13 | Thiệp lên lịch sẽ được gửi tự động khi đến giờ |
| BR-14 | Email người nhận phải đúng định dạng RFC |
| BR-16 | Subscription mới tạo ở trạng thái Pending (chờ thanh toán) |
| BR-17 | Subscription hết hạn → tự động chuyển Expired |
| BR-20 | Không thêm người nhận vượt quá MaxRecipients của gói |
| BR-21 | Email người nhận không được trùng trong cùng subscription |
| BR-27 | Ban User → Disable toàn bộ subscription đang active |
| BR-30 | Không gia hạn subscription đang bị Disabled bởi Admin |
| BR-31 | Xóa danh mục là soft delete (IsActive = false), không xóa vật lý |
| BR-32 | Ẩn template không hủy greetings đã lên lịch |
| BR-33 | Audit logs phải được lưu tối thiểu 30 ngày |

---

## Use Case Coverage

| UC# | Use Case | Endpoints |
|-----|----------|-----------|
| UC01 | Đăng ký tài khoản | `POST /auth/register`, `GET /auth/verify-email` |
| UC02 | Đăng nhập | `POST /auth/login` |
| UC03 | Xem danh mục & mẫu thiệp | `GET /categories`, `GET /templates` |
| UC04 | Xem chi tiết mẫu thiệp | `GET /templates/{id}` |
| UC05 | Tùy chỉnh mẫu thiệp | (client-side, data sent via UC06) |
| UC06 | Gửi thiệp | `POST /greetings/send` |
| UC07 | Xem thiệp đã gửi | `GET /greetings/my` |
| UC08 | Guest gửi thiệp | `POST /greetings/send` (no auth) |
| UC09 | Xem thiệp (người nhận) | `GET /greetings/view/{token}` |
| UC10 | Đăng ký Subscribe | `POST /subscriptions` |
| UC11 | Quản lý Subscribe & Recipients | `GET/POST/DELETE /subscriptions/{id}/recipients` |
| UC12 | Quản lý template (Admin) | `POST/PUT/PATCH /templates` |
| UC13 | Quản lý thanh toán | `POST /feedback`, `GET/POST /admin/payments` |
| UC14 | Quản lý Users (Admin) | `GET /admin/users`, `GET /admin/dashboard` |
| UC16 | Quản lý danh bạ | `GET/POST /users/contacts` |
| UC17 | Lưu bản nháp | `GET/POST /users/drafts` |
| UC18 | Đăng xuất | `POST /auth/logout` |
| UC19 | Hồ sơ cá nhân | `GET/PUT /users/profile`, `POST /users/change-password` |
| UC21 | Quản lý Subscription (Admin) | `GET /admin/subscriptions`, `POST /admin/subscriptions/{id}/disable` |
| UC22 | Quên/Đặt lại mật khẩu | `POST /auth/forgot-password`, `POST /auth/reset-password` |
| UC23 | Trang chủ (công khai) | `GET /templates`, `GET /categories` |
| UC24 | Guest xem thiệp | `GET /greetings/view/{token}` |
| UC25 | Lịch sử thanh toán | `GET /subscriptions/payments` |
| UC26 | Gia hạn Subscribe | `POST /subscriptions/{id}/renew` |
| UC27 | Quản lý danh mục (Admin) | `GET/POST/PUT/DELETE /admin/categories` |
| UC28 | Quản lý nội dung website | `GET/PUT /admin/content/{key}`, rollback |
| UC29 | Phản hồi & Đánh giá | `POST /feedback`, `POST /admin/feedback/{id}/reply` |
| UC30 | Logging & Audit | `GET /admin/audit-logs`, `GET /admin/audit-logs/export` |

---

## MassTransit Events (Async)

| Event | Published by | Consumed by |
|-------|-------------|-------------|
| `GreetingSentEvent` | `SendGreetingCommandHandler` | Email notification consumer |
| `PaymentConfirmedEvent` | `ConfirmPaymentCommandHandler` | Subscription activation consumer |

---

## Project Structure

```
EGreetings.API/
  Controllers/        → 8 controllers (routes above)
  Program.cs          → DI registration, middleware

EGreetings.Application/
  Features/
    Auth/             → Register, Login, VerifyEmail, ForgotPassword, ResetPassword, ChangePassword
    Categories/       → GetPublicCategories
    Greetings/        → SendGreeting, GetUserGreetings, GetGreetingByToken
    Templates/        → GetTemplates, GetTemplateById, CreateTemplate, UpdateTemplate, HideTemplate
    Subscriptions/    → CreateSubscription, RenewSubscription, GetMySubscription,
                        AddRecipient, RemoveRecipient, GetRecipients, GetPaymentHistory
    Users/            → GetProfile, UpdateProfile, AddContact, GetContacts, SaveDraft, GetDrafts
    Feedback/         → SubmitFeedback, GetFeedbacks, ReplyFeedback, MarkFeedbackRead
    Admin/            → GetUsers, BanUser, GetDashboard, ManageCategory, ManageWebContent,
                        ConfirmPayment, DisableSubscription, GetAllGreetings, GetPayments,
                        GetAllSubscriptions, GetAuditLogs, RollbackWebContent, GetWebContent,
                        GetContentVersions

EGreetings.Domain/
  Entities/           → User, GreetingTemplate, Greeting, Category, Subscription,
                        SubscriptionRecipient, Payment, Feedback, Contact, Draft,
                        AuditLog, WebContent
  Enums/              → UserRole, UserStatus, SubscriptionStatus, PaymentStatus,
                        GreetingStatus, FeedbackStatus, AuditEventType

EGreetings.Infrastructure/
  Persistence/        → AppDbContext, EF Configurations, Migrations
  Services/           → EmailService (MailKit), CurrentUserService, AuditService,
                        JwtTokenService, TokenService
  BackgroundServices/ → AutoSendGreetingService, ExpireSubscriptionService,
                        RetryEmailService, AuditLogCleanupService
  Messaging/          → MassTransit consumers & events
```
