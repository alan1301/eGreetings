## Identity Service

- Port: `5001` (container `8080`)
- Responsibility: đăng ký, đăng nhập, JWT, quản lý trạng thái user
- Database: `egreetings_identity_db`
- Publishes: `UserRegisteredEvent`, `UserBannedEvent`
- Consumes: `SendPasswordResetEmailCommand`
- Health endpoints: `/health`, `/health/live`, `/health/ready`
