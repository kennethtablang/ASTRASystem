using ASTRASystem.DTO.Payment;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            // Payment -> PaymentDto
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.RecordedByName, opt => opt.Ignore()); // Will be populated from UserManager

            // RecordPaymentDto -> Payment
            CreateMap<RecordPaymentDto, Payment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RecordedById, opt => opt.Ignore()) // Will be set from current user
                .ForMember(dest => dest.RecordedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());
                //.ForMember(dest => dest.Notes, opt => opt.Ignore()); // Notes not in Payment model

            // Invoice -> InvoiceDto
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.OrderReference, opt => opt.MapFrom(src => src.OrderId.HasValue ? $"ORD-{src.OrderId}" : null))
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Order != null ? src.Order.Store.Name : null));

            // GenerateInvoiceDto -> Invoice
            CreateMap<GenerateInvoiceDto, Invoice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.IssuedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.InvoiceUrl, opt => opt.Ignore()) // Generated after PDF creation
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());
                //.ForMember(dest => dest.TaxRate, opt => opt.Ignore()); // Not in Invoice model
        }
    }
}
