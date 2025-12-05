using ASTRASystem.DTO.Inventory;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class InventoryProfile : Profile
    {
        public InventoryProfile()
        {
            // Inventory -> InventoryDto
            CreateMap<Inventory, InventoryDto>()
                .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product.Sku))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Product.Category != null ? src.Product.Category.Name : null))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // Calculated in service

            // InventoryMovement -> InventoryMovementDto
            CreateMap<InventoryMovement, InventoryMovementDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Inventory.Product.Name))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Inventory.Warehouse.Name))
                .ForMember(dest => dest.CreatedByName, opt => opt.Ignore()); // Will be populated from UserManager

            // CreateInventoryDto -> Inventory
            CreateMap<CreateInventoryDto, Inventory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StockLevel, opt => opt.MapFrom(src => src.InitialStock))
                .ForMember(dest => dest.LastRestocked, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.Movements, opt => opt.Ignore());

            // AdjustInventoryDto -> InventoryMovement (for tracking)
            CreateMap<AdjustInventoryDto, InventoryMovement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src => src.InventoryId))
                .ForMember(dest => dest.MovementType, opt => opt.MapFrom(src => src.MovementType))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QuantityAdjustment))
                .ForMember(dest => dest.PreviousStock, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.NewStock, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Reference))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.MovementDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore());

            // RestockInventoryDto -> InventoryMovement
            CreateMap<RestockInventoryDto, InventoryMovement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryId, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.MovementType, opt => opt.MapFrom(src => "Restock"))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.PreviousStock, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.NewStock, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Reference))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.MovementDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore());
        }
    }
}
