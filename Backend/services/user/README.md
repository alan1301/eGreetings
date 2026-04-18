## User Service

- Port: `5004` (container `8080`)
- Responsibility: profile, dữ liệu user nội bộ theo bounded context user
- Database: `egreetings_user_db`
- Publishes: none
- Consumes: `UserRegisteredEvent`
- Health endpoints: `/health`, `/health/live`, `/health/ready`
