using AutoMapper;
using GPA.Common.DTOs.General;
using GPA.Dtos.General;
using GPA.Entities.General;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.General.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<Unit, UnitDto>();
            CreateMap<UnitDto, Unit>();

            CreateMap<EmailConfigurationCreationDto, EmailConfiguration>();
            CreateMap<EmailConfigurationUpdateDto, EmailConfiguration>();
            CreateMap<EmailConfiguration, EmailConfigurationDto>();

            CreateMap<BlobStorageConfigurationCreationDto, BlobStorageConfiguration>();
            CreateMap<BlobStorageConfigurationUpdateDto, BlobStorageConfiguration>();
            CreateMap<BlobStorageConfiguration, BlobStorageConfigurationDto>();
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
