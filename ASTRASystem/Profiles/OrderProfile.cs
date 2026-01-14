using ASTRASystem.DTO.Order;
using ASTRASystem.Enum;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Order -> OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store.Name))
                .ForMember(dest => dest.StoreAddressLine1, opt => opt.MapFrom(src => src.Store.AddressLine1))
                .ForMember(dest => dest.StoreAddressLine2, opt => opt.MapFrom(src => src.Store.AddressLine2))
                .ForMember(dest => dest.StoreBarangay, opt => opt.MapFrom(src => src.Store.Barangay != null ? src.Store.Barangay.Name : null))
                .ForMember(dest => dest.StoreCity, opt => opt.MapFrom(src => src.Store.City != null ? src.Store.City.Name : null))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore())
                .ForMember(dest => dest.DistributorName, opt => opt.MapFrom(src => src.Distributor != null ? src.Distributor.Name : null))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
                .ForMember(dest => dest.PaidByName, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPaid, opt => opt.MapFrom(src => src.TotalPaid))
                .ForMember(dest => dest.RemainingBalance, opt => opt.MapFrom(src => src.RemainingBalance))
                .ForMember(dest => dest.HasPartialPayment, opt => opt.MapFrom(src => src.HasPartialPayment))
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore()); // Calculated in service

            // Order -> OrderListItemDto
            CreateMap<Order, OrderListItemDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store.Name))
                .ForMember(dest => dest.StoreAddressLine1, opt => opt.MapFrom(src => src.Store.AddressLine1))
                .ForMember(dest => dest.StoreAddressLine2, opt => opt.MapFrom(src => src.Store.AddressLine2))
                .ForMember(dest => dest.StoreBarangay, opt => opt.MapFrom(src => src.Store.Barangay != null ? src.Store.Barangay.Name : null))
                .ForMember(dest => dest.StoreCity, opt => opt.MapFrom(src => src.Store.City != null ? src.Store.City.Name : null))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items != null ? src.Items.Count : 0))
                .ForMember(dest => dest.TotalPaid, opt => opt.MapFrom(src => src.TotalPaid))
                .ForMember(dest => dest.RemainingBalance, opt => opt.MapFrom(src => src.RemainingBalance))
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore()); // Calculated in service

            // OrderItem -> OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product.Sku))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

            // CreateOrderDto -> Order
            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AgentId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.Tax, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PaidAt, opt => opt.Ignore())
                .ForMember(dest => dest.PaidById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.Distributor, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryPhotos, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            // CreateOrderItemDto -> OrderItem
            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }
    }
}