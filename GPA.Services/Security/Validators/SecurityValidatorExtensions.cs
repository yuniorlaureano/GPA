using FluentValidation;
using GPA.Dtos.Security;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Services.Security.Validators
{
    public static class SecurityValidatorExtensions
    {
        public static void AddSecurityValidators(this IServiceCollection collections)
        {
            collections.AddScoped<IValidator<SignUpDto>, SignUpValidator>();
        }
    }
}
