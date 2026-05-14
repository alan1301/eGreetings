namespace EGreetings.Domain.Enums;

public enum TransactionStatus
{
    Pending,
    Sent,
    Failed,
    Scheduled,
    Cancelled   // UC06 – User cancelled a scheduled greeting before it was sent
}
