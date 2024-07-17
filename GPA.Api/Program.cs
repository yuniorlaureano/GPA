using GPA.Business.Common.Extensions;
using GPA.Business.Inventory.Extensions;
using GPA.Business.Invoice.Extensions;
using GPA.Business.Security.Extensions;
using GPA.Bussiness.Services.Common.Mappers;
using GPA.Bussiness.Services.Common.Validator;
using GPA.Bussiness.Services.Inventory.Mappers;
using GPA.Bussiness.Services.Inventory.Validator;
using GPA.Bussiness.Services.Invoice.Mappers;
using GPA.Bussiness.Services.Invoice.Validator;
using GPA.Bussiness.Services.Security.Mappers;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Data.Common.Extensions;
using GPA.Data.Inventory.Extensions;
using GPA.Data.Invoice.Extensions;
using GPA.Data.Security.Extensions;
using GPA.Services.Security.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//ToDo: move this to a extension method.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GPA Api",
        Version = "v1",
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingrese un token válido",
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetValue<string>("AllowedHosts")?.Split(",") ?? ["*"];
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddGPAJwtBearer(builder.Configuration);

builder.Services.AddDbContext<GPADbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"), p => p.MigrationsAssembly("GPA.Api")));

builder.Services.AddInventoryMappers();
builder.Services.AddInventoryValidators();
builder.Services.AddDataInventoryRepositories(builder.Configuration);
builder.Services.AddBusinessInventoryServices();

builder.Services.AddInvoiceMappers();
builder.Services.AddInvoiceValidators();
builder.Services.AddDataInvoiceRepositories();
builder.Services.AddBusinessInvoiceServices();

builder.Services.AddCommonMappers();
builder.Services.AddCommonValidators();
builder.Services.AddDataCommonRepositories();
builder.Services.AddBusinessCommonServices();

builder.Services.AddIdentity<GPAUser, GPARole>().AddEntityFrameworkStores<GPADbContext>();
builder.Services.AddSecurityMappers();
builder.Services.AddBusinessSecurityServices();
builder.Services.AddSecurityValidators();
builder.Services.AddDataSecurityRepositories();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
