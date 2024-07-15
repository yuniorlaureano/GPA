using AutoMapper;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Security;
using GPA.Dtos.Security;
using GPA.Entities.Unmapped;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Security.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<GPAUser, GPAUserDto>();
            CreateMap<GPAUser, GPAUserUpdateDto>();
            CreateMap<GPAUserDto, GPAUser>();
            CreateMap<GPAUserUpdateDto, GPAUser>();
            CreateMap<RawUser, GPAUserDto>();

            CreateMap<RawProfile, RawProfileDto>();
            CreateMap<RawProfileDto, RawProfile>();
        }
    }

    public static class SecurityMapperExtensions
    {
        public static void AddSecurityMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
