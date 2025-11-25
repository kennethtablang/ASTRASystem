using ASTRASystem.DTO.Common;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class CommonProfile : Profile
    {
        public CommonProfile()
        {
            // Distributor -> DistributorDto
            CreateMap<Distributor, DistributorDto>();

            // Warehouse -> WarehouseDto
            CreateMap<Warehouse, WarehouseDto>()
                .ForMember(dest => dest.DistributorName, opt => opt.MapFrom(src => src.Distributor.Name));

            // CreateWarehouseDto -> Warehouse
            CreateMap<CreateWarehouseDto, Warehouse>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Distributor, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Trips, opt => opt.Ignore());

            // Notification -> NotificationDto
            CreateMap<Notification, NotificationDto>();

            // AuditLog -> AuditLogDto
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore()); // Will be populated from UserManager

            // Generic lookup mappings
            CreateMap<Distributor, LookupItemDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Address));

            CreateMap<Warehouse, LookupItemDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Address));

            CreateMap<Product, LookupItemDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => $"{src.Sku} - {src.Category}"));

            CreateMap<Store, LookupItemDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => $"{src.Barangay}, {src.City}"));
        }
    }
}
