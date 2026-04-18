## Admin Service

- Port: `5006` (container `8080`)
- Responsibility: dashboard/admin aggregate, audit log
- Database: `egreetings_admin_db`
- Publishes: none
- Consumes: `UserRegisteredEvent`, `GreetingSentEvent`, `PaymentConfirmedEvent`
- Health endpoints: `/health`, `/health/live`, `/health/ready`
