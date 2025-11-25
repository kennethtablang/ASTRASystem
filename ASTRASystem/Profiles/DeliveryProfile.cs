using ASTRASystem.DTO.Delivery;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class DeliveryProfile : Profile
    {
        public DeliveryProfile()
        {
            // DeliveryPhoto -> DeliveryPhotoDto
            CreateMap<DeliveryPhoto, DeliveryPhotoDto>()
                .ForMember(dest => dest.UploadedByName, opt => opt.Ignore()); // Will be populated from UserManager

            // UploadDeliveryPhotoDto -> DeliveryPhoto
            CreateMap<UploadDeliveryPhotoDto, DeliveryPhoto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Url, opt => opt.Ignore()) // Will be set after file upload
                .ForMember(dest => dest.UploadedById, opt => opt.Ignore()) // Will be set from current user
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());
                //.ForMember(dest => dest.Photo, opt => opt.Ignore()); // Ignore IFormFile
        }
    }
}
