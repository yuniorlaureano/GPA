using GPA.Business.Services.Security;
using GPA.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GPA.Business.Security.Extensions
{
    public static class BusinessSecurityExtensions
    {
        public static void AddBusinessSecurityServices(this IServiceCollection services)
        {
            services.AddTransient<IGPAJwtService, GPAJwtService>();
            services.AddTransient<IGPAUserService, GPAUserService>();
        }

        public static void AddGPAJwtBearer(this AuthenticationBuilder authenticationBuilder, IConfiguration configuration)
        {
            var audience = configuration.GetValue<string>("Jwt:Audience");
            var issuer = configuration.GetValue<string>("Jwt:Issuer");
            var key = configuration.GetValue<string>("Jwt:Key");
            var expires = configuration.GetValue<string>("Jwt:Expires");

            if (
                audience is string { Length: 0 } ||
                issuer is string { Length: 0 } ||
                key is string { Length: 0 } ||
                expires is string { Length: 0 })
            {
                throw new ArgumentNullException("Las opciones para jwt token son requeridas");
            }

            var jwtOptions = new JwtOptions()
            {
                Audience = audience!,
                Issuer = issuer!,
                Key = key!,
                Expires = int.Parse(expires!)
            };

            authenticationBuilder.Services.AddSingleton(jwtOptions);

            authenticationBuilder.AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
            });
        }
    }
}
