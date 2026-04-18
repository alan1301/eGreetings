## Subscription Service

- Port: `5003` (container `8080`)
- Responsibility: quản lý gói subscription, payment, recipients
- Database: `egreetings_subscription_db`
- Publishes: `PaymentConfirmedEvent`
- Consumes: `UserBannedEvent`
- Health endpoints: `/health`, `/health/live`, `/health/ready`
