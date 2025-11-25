using ASTRASystem.DTO.User;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Profiles
{
    public class UserProfile : Profile 
    {
        public UserProfile()
        {
            // ApplicationUser -> UserDto
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()) 
                .ForMember(dest => dest.DistributorName, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseName, opt => opt.Ignore());

            // ApplicationUser -> UserListItemDto
            CreateMap<ApplicationUser, UserListItemDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.DistributorName, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); 

            // UpdateUserProfileDto -> ApplicationUser
            CreateMap<UpdateUserProfileDto, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorCodeHash, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorCodeExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorAttempts, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalMessage, opt => opt.Ignore());
        }
    }
}
