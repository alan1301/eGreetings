using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.ManageCategory;

/// <summary>UC27 - Xóa danh mục - BR-31: Không xóa nếu có template đang hoạt động</summary>
public record DeleteCategoryCommand(int CategoryId) : IRequest<bool>;
