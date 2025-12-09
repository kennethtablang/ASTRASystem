using ASTRASystem.DTO.Location;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            // City mappings
            CreateMap<City, CityDto>()
                .ForMember(dest => dest.BarangayCount, opt => opt.MapFrom(src => src.Barangays.Count))
                .ForMember(dest => dest.StoreCount, opt => opt.MapFrom(src => src.Stores.Count));

            CreateMap<City, CityListItemDto>()
                .ForMember(dest => dest.BarangayCount, opt => opt.MapFrom(src => src.Barangays.Count))
                .ForMember(dest => dest.StoreCount, opt => opt.MapFrom(src => src.Stores.Count));

            CreateMap<CreateCityDto, City>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Barangays, opt => opt.Ignore())
                .ForMember(dest => dest.Stores, opt => opt.Ignore());

            CreateMap<UpdateCityDto, City>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Barangays, opt => opt.Ignore())
                .ForMember(dest => dest.Stores, opt => opt.Ignore());

            // Barangay mappings
            CreateMap<Barangay, BarangayDto>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name))
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.City.Province))
                .ForMember(dest => dest.StoreCount, opt => opt.MapFrom(src => src.Stores.Count));

            CreateMap<Barangay, BarangayListItemDto>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name))
                .ForMember(dest => dest.StoreCount, opt => opt.MapFrom(src => src.Stores.Count));

            CreateMap<CreateBarangayDto, Barangay>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.Stores, opt => opt.Ignore());

            CreateMap<UpdateBarangayDto, Barangay>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.Stores, opt => opt.Ignore());
        }
    }
}
