using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Infrastructure.Outbox;
using BudgetAppBackend.Infrastructure.Repositories;
using BudgetAppBackend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using BudgetAppBackend.Infrastructure.Jobs;
using BudgetAppBackend.Application.Configuration;

namespace BudgetAppBackend.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("dbConnectionString");
            services.AddSingleton<DomainEventInterceptor>();
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<DomainEventInterceptor>();
                options.UseNpgsql(connectionString,
                                  builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                       .AddInterceptors(interceptor);
            });
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("OutboxProcessor");
                q.AddJob<OutboxProcessor>(jobKey)
                 .AddTrigger(
                    trigger =>
                    trigger.ForJob(jobKey)
                        .WithSimpleSchedule(s =>
                            s.WithIntervalInSeconds(1)
                                .RepeatForever()));

                var resetBudgetJobKey = new JobKey("ResetBudgetJob");
                q.AddJob<ResetBudgetJob>(resetBudgetJobKey)
                 .AddTrigger(trigger => trigger.ForJob(resetBudgetJobKey)
                            //.WithCronSchedule("0 * * ? * *"));
                .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(1, 0, 0)));  //MonthlyOnDayAndHourAndMinute(1, 0, 0))
            });
            services.AddQuartzHostedService();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IBudgetRepository, BudgetRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPlaidTransactionRepository, PlaidTransactionRepository>();
            services.AddScoped<IPlaidSyncCursorRepository, PlaidSyncCursorRepository>();
            services.AddScoped<IPlaidAccountFingerprintRepository, PlaidAccountFingerprintRepository>();
            services.AddScoped<IDailySpendingForecaster, TorchSharpForecaster>();
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings").Bind);

            services.AddSingleton<IAuthService, AuthService>();

            // Configure EmailSettings
            services.Configure<EmailSettings>(options =>
            {
                options.SmtpServer = configuration.GetSection("EmailSettings")["SmtpServer"];
                options.SmtpPort = int.TryParse(configuration.GetSection("EmailSettings")["SmtpPort"], out var port) ? port : null;
                options.SenderEmail = configuration.GetSection("EmailSettings")["EmailSender"];
                options.EmailPassword = configuration.GetSection("EmailSettings")["EmailPassword"];
                options.SenderName = configuration.GetSection("EmailSettings")["SenderName"];
            });

            
            services.Configure<Geminisetting>(options =>
            {
                options.EndPoints = configuration.GetSection("Gemini")["Endpoint"] ;
                options.GeminiApiKey = configuration.GetSection("Gemini")["ApiKey"];
                options.stream = configuration.GetSection("Gemini")["stream"];
            });

            // Configure PlaidOptions
            services.Configure<PlaidOptions>(options =>
            {
                options.ClientId = configuration.GetSection("Plaid")["ClientId"];
                options.Secret = configuration.GetSection("Plaid")["Secret"];
                options.Environment = configuration.GetSection("Plaid")["Environment"];
                options.BaseUrl = configuration.GetSection("Plaid")["BaseUrl"];
            });

            // Validate Plaid configuration
            services.PostConfigure<PlaidOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.ClientId))
                    throw new InvalidOperationException("Plaid ClientId is not configured");
                if (string.IsNullOrEmpty(options.Secret))
                    throw new InvalidOperationException("Plaid Secret is not configured");
                if (string.IsNullOrEmpty(options.BaseUrl))
                    throw new InvalidOperationException("Plaid BaseUrl is not configured");
            });

            // Register Plaid service
            services.AddScoped<IPlaidService, PlaidService>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(InfrastructureServicesRegistration).Assembly);
            });

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IAIChatService, OllamaAIChatService>();
            services.AddSingleton<IPdfReportService, PdfReportService>();

            return services;
        }
    }
}
