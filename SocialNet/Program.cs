using Microsoft.AspNetCore.Identity;
using SocialNet.Core;
using SocialNet.Core.Dtos.EmailDtos;
using SocialNet.Domain.Identity;
using SocialNet.Infrastructure;
using SocialNet.Infrastructure.DataAcces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationCore();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
