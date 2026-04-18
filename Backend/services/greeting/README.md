## Greeting Service

- Port: `5002` (container `8080`)
- Responsibility: template, tạo và gửi greeting, xử lý scheduled greeting
- Database: `egreetings_greeting_db`
- Publishes: `GreetingSentEvent`
- Consumes: none
- Health endpoints: `/health`, `/health/live`, `/health/ready`
