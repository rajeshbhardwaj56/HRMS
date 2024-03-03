using HRMS.API.BusinessLayer;
using HRMS.API.BusinessLayer.ITF;
using HRMS.API.DataLayer;
using HRMS.API.DataLayer.ITF;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IDataLayer, DataLayer>();
builder.Services.AddSingleton<IJWTAuthentication, JWTAuthentication>();
builder.Services.AddSingleton<IBusinessLayer, BusinessLayer>();
builder.Services.AddSingleton<IJWTAuthentication, JWTAuthentication>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
