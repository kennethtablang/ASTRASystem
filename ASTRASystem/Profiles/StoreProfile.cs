using ASTRASystem.DTO.Store;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class StoreProfile : Profile
    {
        public StoreProfile()
        {
            // Store -> StoreDto
            CreateMap<Store, StoreDto>()
                .ForMember(dest => dest.BarangayName, opt => opt.MapFrom(src => src.Barangay != null ? src.Barangay.Name : null))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null))
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.City != null ? src.City.Province : null));

            // Store -> StoreListItemDto
            CreateMap<Store, StoreListItemDto>()
                .ForMember(dest => dest.BarangayName, opt => opt.MapFrom(src => src.Barangay != null ? src.Barangay.Name : null))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null));

            // Store -> StoreWithBalanceDto
            CreateMap<Store, StoreWithBalanceDto>()
                .ForMember(dest => dest.BarangayName, opt => opt.MapFrom(src => src.Barangay != null ? src.Barangay.Name : null))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null))
                .ForMember(dest => dest.OutstandingBalance, opt => opt.Ignore())
                .ForMember(dest => dest.OverdueInvoiceCount, opt => opt.Ignore());

            // CreateStoreDto -> Store
            CreateMap<CreateStoreDto, Store>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Barangay, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            // UpdateStoreDto -> Store
            CreateMap<UpdateStoreDto, Store>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Barangay, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());
        }
    }
}
