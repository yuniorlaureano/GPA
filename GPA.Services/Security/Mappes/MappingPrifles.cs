using AutoMapper;
using GPA.Common.Entities.Security;
using GPA.Dtos.Security;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Security.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<GPAUser, GPAUserDto>()
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => ""));                
                            
            CreateMap<GPAUserDto, GPAUser>();
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
