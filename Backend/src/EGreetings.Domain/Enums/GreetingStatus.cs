namespace EGreetings.Domain.Enums;

public enum GreetingStatus
{
    Draft = 1,      // Bản nháp
    Scheduled = 2,  // Đã lên lịch gửi
    Sent = 3,       // Đã gửi
    Failed = 4,     // Gửi thất bại
    Cancelled = 5   // Đã hủy
}
