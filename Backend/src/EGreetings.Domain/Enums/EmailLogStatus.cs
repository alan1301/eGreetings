namespace EGreetings.Domain.Enums;

public enum EmailLogStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Retrying = 4  // Đang retry (tối đa 3 lần, cách 5 phút - BR-32)
}
