using GPA.Api.Extensions;
using GPA.Business.General.Extensions;
using GPA.Business.Inventory.Extensions;
using GPA.Business.Invoice.Extensions;
using GPA.Business.Security.Extensions;
using GPA.Bussiness.Services.General.Mappers;
using GPA.Bussiness.Services.General.Validator;
using GPA.Bussiness.Services.Inventory.Mappers;
using GPA.Bussiness.Services.Inventory.Validator;
using GPA.Bussiness.Services.Invoice.Mappers;
using GPA.Bussiness.Services.Invoice.Validator;
using GPA.Bussiness.Services.Security.Mappers;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Data.General.Extensions;
using GPA.Data.Inventory.Extensions;
using GPA.Data.Invoice.Extensions;
using GPA.Data.Security.Extensions;
using GPA.Services.General.Security;
using GPA.Services.Security.Validators;
using GPA.Utils;
using GPA.Utils.Extensions;
using GPA.Utils.Middleware;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGPASwagger();
builder.Services.AddGPACors(builder.Configuration);
builder.Services.AddGPAAuthentication(builder.Configuration);
builder.Services.AddGPADbContext(builder.Configuration);

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentity<GPAUser, IdentityRole<Guid>>().AddEntityFrameworkStores<GPADbContext>();
builder.Services.AddSecurityMappers();
builder.Services.AddBusinessSecurityServices();
builder.Services.AddSecurityValidators();
builder.Services.AddDataSecurityRepositories();
builder.Services.AddAuthorization();

builder.Services.AddScoped<IAesHelper, AesHelper>();
builder.Services.AddScoped<IEmailProviderHelper, EmailProviderHelper>();
builder.Services.AddScoped<IBlobStorageHelper, BlobStorageHelper>();

builder.Services.AddUtils(builder.Environment, builder.Configuration);
builder.Services.AddSendGridUrl(builder.Configuration);

var app = builder.Build();

//Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
