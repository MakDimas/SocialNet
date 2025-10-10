using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialNet.Core;
using SocialNet.Core.Dtos.EmailDtos;
using SocialNet.Core.Models.HealthCheckModels;
using SocialNet.Core.Options;
using SocialNet.Domain.Identity;
using SocialNet.Infrastructure;
using SocialNet.Infrastructure.DataAcces;
using SocialNet.Infrastructure.Hubs;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;

const string CORS_POLICY_NAME = "SocialNetCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

var aspNetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (string.IsNullOrWhiteSpace(aspNetCoreUrls))
{
    var isContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
    if (isContainer)
    {
        var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS") ?? "8080";
        var portToUse = httpPorts.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        builder.WebHost.UseUrls($"http://*:{portToUse}");
    }
    else
    {
        var port = Environment.GetEnvironmentVariable("PORT")
                   ?? Environment.GetEnvironmentVariable("WEBSITES_PORT")
                   ?? "5000";
        builder.WebHost.UseUrls($"http://*:{port}");
    }
}

builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection("Frontend"));

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CORS_POLICY_NAME, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtIssuer = Environment.GetEnvironmentVariable("JWT__ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT__AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY") ?? builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT signing key is not configured. Set configuration 'Jwt:Key' or env 'JWT__KEY'.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
        ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

var connectionString = builder.Configuration.GetConnectionString("ConnectionString");
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: connectionString,
        name: "azure-sql-db",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["database", "sql"]
     );

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationCore();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = true,
    ResponseWriter = async (context, report) =>
    {
        var healthCheckModel = new HealthCheckModel
        {
            Status = report.Status.ToString(),
            Results = report.Entries.Select(entry => new HealthCheckResultModel
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                Error = entry.Value.Exception?.Message
            })
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(healthCheckModel));

        if (report.Status == HealthStatus.Unhealthy)
        {
            logger.LogError($"Health status is unhealthy. Details: {healthCheckModel}");
        }
    }
});
app.MapHub<HealthHub>("/healthhub");

app.MapGet("/", () => Results.Ok(new { status = "ok" }));

if (string.Equals(app.Configuration["ASPNETCORE_URLS"], "https", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

app.UseCors(CORS_POLICY_NAME);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
