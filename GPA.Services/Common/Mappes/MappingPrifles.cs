using AutoMapper;
using GPA.Common.DTOs.Common;
using GPA.Entities.Common;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Common.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<Unit, UnitDto>();
        }
    }

    public static class CommonMapperExtensions
    {
        public static void AddCommonMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
