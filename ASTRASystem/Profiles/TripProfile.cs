using ASTRASystem.DTO.Trip;
using ASTRASystem.Enum;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class TripProfile : Profile
    {
        public TripProfile()
        {
            // Trip -> TripDto
            CreateMap<Trip, TripDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
                .ForMember(dest => dest.DispatcherName, opt => opt.Ignore());

            // Trip -> TripListItemDto
            CreateMap<Trip, TripListItemDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
                .ForMember(dest => dest.DispatcherName, opt => opt.Ignore())
                .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Assignments.Count))
                .ForMember(dest => dest.TotalValue, opt => opt.Ignore()); 

            // TripAssignment -> TripAssignmentDto
            CreateMap<TripAssignment, TripAssignmentDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Order.Store.Name))
                .ForMember(dest => dest.StoreBarangay, opt => opt.MapFrom(src => src.Order.Store.Barangay))
                .ForMember(dest => dest.StoreCity, opt => opt.MapFrom(src => src.Order.Store.City))
                .ForMember(dest => dest.OrderTotal, opt => opt.MapFrom(src => src.Order.Total));

            // CreateTripDto -> Trip
            CreateMap<CreateTripDto, Trip>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TripStatus.Created))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.Assignments, opt => opt.Ignore());

            // UpdateTripDto -> Trip
            CreateMap<UpdateTripDto, Trip>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TripId))
                .ForMember(dest => dest.WarehouseId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.Assignments, opt => opt.Ignore());
        }
    }
}
