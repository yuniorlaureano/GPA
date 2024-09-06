using FluentValidation;
using GPA.Common.DTOs.General;
using GPA.Services.General.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.General.Validator
{
    public static class CommonValidatorExtensions
    {
        public static void AddCommonValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<UnitDto>, UnitValidator>();
        }
    }
}
