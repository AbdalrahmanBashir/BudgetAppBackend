using System.Text;
using BudgetAppBackend.Infrastructure;
using BudgetAppBackend.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BudgetAppBackend.API.Middlewares;
using Microsoft.OpenApi.Models;
using BudgetAppBackend.API.Services;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Infrastructure.SignalR;
using BudgetAppBackend.Infrastructure.Services;
using BudgetAppBackend.Application.Configuration;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (string.IsNullOrEmpty(accessToken) == false)
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });


        builder.Services.AddHttpClient<IAIAnalysisService, OllamaAIService>();
        builder.Services.AddSingleton<IUrlGenerator, UrlGeneratorService>();
        builder.Services.RegisterInfrastructureServices(builder.Configuration);
        builder.Services.RegisterApplicationServices();

        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.WriteIndented = true;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] then token."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowOrigin", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://10.2.0.2:3000", "http://10.96.0.45:3000", "http://localhost:3001", "http://localhost:3002", "http://10.2.0.2:3001", "http://192.168.56.1:3000")
                      .AllowCredentials()
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithExposedHeaders("*");
            });
        });

        builder.Services.AddHttpsRedirection(options => { options.HttpsPort = 7105; });
        builder.WebHost.UseUrls("https://localhost:7105");

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseCors("AllowOrigin");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<NotificationHub>("/notifications");
        app.MapControllers();

        app.Run();
    }
}