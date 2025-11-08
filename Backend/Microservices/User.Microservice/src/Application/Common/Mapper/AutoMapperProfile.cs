using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Users.Commands;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Constants;

namespace Application.Common.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterUserCommand, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => false));
            
            CreateMap<User, RegisterUserCommand>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name));

            CreateMap<GetUserResponse, User>();
            CreateMap<User, GetUserResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.FromBool(src.IsActive)))
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src =>
                        src.UserRoles != null
                            ? src.UserRoles
                                .Select(ur => ur.Role != null ? ur.Role.RoleName : null)
                                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                                .Select(roleName => roleName.Trim())
                                .FirstOrDefault()
                            : null));
        }
        
    }
}
