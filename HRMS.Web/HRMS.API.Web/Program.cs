using HRMS.API.BusinessLayer;
using HRMS.API.BusinessLayer.ITF;
using HRMS.API.DataLayer;
using HRMS.API.DataLayer.ITF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var Confbuilder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

IConfigurationRoot Configuration = Confbuilder.Build();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IDataLayer, DataLayer>();
builder.Services.AddSingleton<IAttandanceDataLayer, AttandanceDataLayer>();
builder.Services.AddSingleton<IJWTAuthentication, JWTAuthentication>();
builder.Services.AddSingleton<IBusinessLayer, BusinessLayer>();
builder.Services.AddSingleton<IJWTAuthentication, JWTAuthentication>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
        };
    });
var app = builder.Build();

app.UseExceptionHandler("/error"); // Add this

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
