using GPA.Business.Security.Extensions;
using GPA.Data;
using GPA.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace GPA.Api.Extensions
{
    public static class ApiExtensions
    {
        public static void AddGPASwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "GPA Api",
                    Version = "v1",
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Ingrese un token v�lido",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                     {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
        }

        public static void AddGPACors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", policy =>
                {
                    var origins = configuration.GetValue<string>("AllowedOriging")?.Split(",") ?? ["*"];
                    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
                });
            });
        }

        public static void AddGPAAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddGPAJwtBearer(configuration);
        }

        public static void AddGPADbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<GPADbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default"), p => p.MigrationsAssembly("GPA.Api")));
        }

        public static void AddSendGridUrl(this IServiceCollection services, IConfiguration configuration)
        {
            var sendGridUrl = configuration["Url:SendGrid"];
            if (sendGridUrl is { Length: > 0 })
            {
                services.AddHttpClient(UrlConstant.SENDGRID, options =>
                {
                    options.BaseAddress = new Uri(sendGridUrl);
                });
            }
        }
    }
}
