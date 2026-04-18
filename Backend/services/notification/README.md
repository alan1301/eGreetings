## Notification Service

- Port: `5005` (container `8080`)
- Responsibility: gửi email, retry, idempotency log
- Database: `egreetings_notification_db`
- Publishes: none
- Consumes: `UserRegisteredEvent`, `GreetingSentEvent`, `PaymentConfirmedEvent`
- Health endpoints: `/health`, `/health/live`, `/health/ready`
