using AutoMapper;
using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Application.Features.Greetings.DTOs;
using EGreetings.Application.Features.Subscriptions.DTOs;
using EGreetings.Application.Features.Templates.DTOs;
using EGreetings.Application.Features.Users.DTOs;
using EGreetings.Domain.Entities;

namespace EGreetings.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ─── User ───────────────────────────────────────────────────────────
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            // UC19: SubscribeStatus/SubscribeExpiredAt set manually in handler (requires Subscription query)
            .ForMember(d => d.SubscribeStatus, o => o.Ignore())
            .ForMember(d => d.SubscribeExpiredAt, o => o.Ignore());

        CreateMap<User, UserListItemDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // ─── Contact ────────────────────────────────────────────────────────
        CreateMap<Contact, ContactDto>();

        // ─── Draft ──────────────────────────────────────────────────────────
        CreateMap<Draft, DraftDto>()
            .ForMember(d => d.TemplateName, o => o.MapFrom(s => s.Template != null ? s.Template.Name : string.Empty));

        // ─── Category ───────────────────────────────────────────────────────
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.TemplateCount, o => o.MapFrom(s => s.Templates.Count(t => t.IsActive)));

        // ─── GreetingTemplate ───────────────────────────────────────────────
        CreateMap<GreetingTemplate, TemplateDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty));

        CreateMap<GreetingTemplate, TemplateDetailDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty));

        // ─── Greeting ───────────────────────────────────────────────────────
        CreateMap<Greeting, GreetingDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.TemplateName, o => o.MapFrom(s => s.Template != null ? s.Template.Name : string.Empty));

        // ─── Subscription ───────────────────────────────────────────────────
        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.CurrentRecipientCount, o => o.MapFrom(s => s.Recipients.Count(r => r.IsActive)));

        // ─── Payment ────────────────────────────────────────────────────────
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            // UC25: Ngày hết hạn Subscribe lấy từ navigation property
            .ForMember(d => d.SubscriptionExpiredAt, o => o.MapFrom(s => s.Subscription != null ? s.Subscription.ExpiredAt : (DateTime?)null));

        // ─── AuditLog ───────────────────────────────────────────────────────
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(d => d.EventType, o => o.MapFrom(s => s.EventType.ToString()));

        // ─── Feedback ───────────────────────────────────────────────────────
        CreateMap<Feedback, FeedbackDto>();
    }
}
