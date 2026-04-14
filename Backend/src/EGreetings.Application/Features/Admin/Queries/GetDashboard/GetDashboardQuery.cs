using EGreetings.Application.Features.Admin.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetDashboard;

/// <summary>UC14 - Dashboard thống kê tổng quan (Admin)</summary>
public record GetDashboardQuery : IRequest<DashboardStatsDto>;
