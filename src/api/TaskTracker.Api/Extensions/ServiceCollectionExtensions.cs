using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Middleware;
using TaskTracker.Application.Configuration;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Services;
using TaskTracker.Infrastructure.AI;
using TaskTracker.Infrastructure.Data;
using TaskTracker.Infrastructure.Repositories;

namespace TaskTracker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITaskSummaryService, TaskSummaryService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        services.AddScoped<ITaskRepository, TaskRepository>();

        services.AddAiSummaryProvider(configuration);

        return services;
    }

    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static IServiceCollection AddAiSummaryProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(AiProviderSettings.SectionName)
            .Get<AiProviderSettings>() ?? new AiProviderSettings();

        var useExternalProvider =
            settings.Type.Equals("External", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(settings.ApiKey);

        if (useExternalProvider)
        {
            services.AddSingleton(settings);
            services.AddHttpClient<IAiSummaryProvider, ExternalAiSummaryProvider>();
        }
        else
        {
            services.AddScoped<IAiSummaryProvider, SimpleTaskSummaryProvider>();
        }

        return services;
    }
}
