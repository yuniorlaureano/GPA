using FluentValidation;
using GPA.Common.DTOs.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public static class InventoryValidatorExtensions
    {
        public static void AddInventoryValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<CategoryDto>, CategoryValidator>();
            services.AddScoped<IValidator<CategoryDto>, CategoryCreationValidator>();
            services.AddScoped<IValidator<ProductCreationDto>, ProductCreationValidator>();
        }
    }
}
