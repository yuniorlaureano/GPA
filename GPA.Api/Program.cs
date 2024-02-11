using GPA.Business.Inventory.Extensions;
using GPA.Bussiness.Services.Inventory.Mappers;
using GPA.Data.Inventory.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInventoryMappers();
builder.Services.AddInventoryValidators();
builder.Services.AddDataInventoryRepositories(builder.Configuration);
builder.Services.AddBusinessInventoryServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
