﻿using FluentValidation;
using GPA.Common.DTOs.Inventory;
using GPA.Services.Invoice.Validators;
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
            services.AddScoped<IValidator<StockCycleCreationDto>, StockCycleCreationValidator>();
            services.AddScoped<IValidator<AddonDto>, AddonValidator>();
            services.AddScoped<IValidator<OutputCreationDto>, OutputCreationValidator>();
            services.AddScoped<IValidator<StockCreationDto>, StockCreationValidator>();
            services.AddScoped<IValidator<ProviderDto>, ProviderCreationValidator>();
        }
    }
}
