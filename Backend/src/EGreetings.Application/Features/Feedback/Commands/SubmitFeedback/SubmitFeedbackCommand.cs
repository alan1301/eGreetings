using MediatR;

namespace EGreetings.Application.Features.Feedback.Commands.SubmitFeedback;

/// <summary>
/// UC13 - Gửi phản hồi / báo cáo
/// BR-13: Mỗi tài khoản chỉ được gửi tối đa 5 phản hồi/ngày
/// </summary>
public record SubmitFeedbackCommand(
    int? UserId,            // null nếu ẩn danh
    string Subject,
    string Content,
    string? ContactEmail,
    int? StarRating         // UC07: Đánh giá sao 1-5 (tùy chọn)
) : IRequest<int>;
