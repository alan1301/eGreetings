using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Feedback.Commands.SubmitFeedback;
using EGreetings.Application.Features.Feedback.Queries.GetFeedbacks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>UC13 - Gửi phản hồi / báo cáo</summary>
[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public FeedbackController(ICurrentUserService currentUser, IMediator mediator)
    {
        _currentUser = currentUser;
        _mediator = mediator;
    }

    /// <summary>UC13 - Gửi phản hồi (Authenticated hoặc Anonymous)</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitFeedbackRequest request, CancellationToken ct)
    {
        var command = new SubmitFeedbackCommand(
            UserId: _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            Subject: request.Subject,
            Content: request.Content,
            ContactEmail: request.ContactEmail,
            StarRating: request.StarRating
        );

        var id = await _mediator.Send(command, ct);
        return Ok(new { Success = true, Message = "Cảm ơn bạn đã gửi phản hồi!", Data = new { Id = id } });
    }

    /// <summary>UC13 - Xem phản hồi (Admin)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var feedbacks = await _mediator.Send(new GetFeedbacksQuery(), ct);
        return Ok(new { Success = true, Data = feedbacks });
    }
}

public record SubmitFeedbackRequest(
    string Subject,
    string Content,
    string? ContactEmail,
    int? StarRating       // UC07: Đánh giá sao 1-5 (tùy chọn)
);
