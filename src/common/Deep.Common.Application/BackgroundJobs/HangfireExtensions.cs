using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.BackgroundJobs;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireInternal(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<HangfireOptions>()
            .Bind(configuration.GetSection(HangfireOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddHangfire(
            (sp, config) =>
            {
                HangfireOptions options = sp.GetRequiredService<IOptions<HangfireOptions>>().Value;

                string connectionString =
                    options.ConnectionString
                    ?? configuration.GetConnectionString("deep-db")
                    ?? throw new InvalidOperationException(
                        "Hangfire connection string not configured."
                    );

                config.UsePostgreSqlStorage(
                    opts => opts.UseNpgsqlConnection(connectionString),
                    new PostgreSqlStorageOptions
                    {
                        SchemaName = options.Schema,
                        PrepareSchemaIfNecessary = true,
                        QueuePollInterval = TimeSpan.FromSeconds(5),
                    }
                );

                config.UseSimpleAssemblyNameTypeSerializer();
                config.UseRecommendedSerializerSettings();
            }
        );

        services.AddHangfireServer(
            (sp, serverOptions) =>
            {
                HangfireOptions hangfireOptions = sp.GetRequiredService<
                    IOptions<HangfireOptions>
                >().Value;
                serverOptions.WorkerCount = hangfireOptions.WorkerCount;
            }
        );

        return services;
    }

    public static IApplicationBuilder UseHangfireInternal(
        this IApplicationBuilder app,
        bool enableDashboard = false
    )
    {
        if (enableDashboard)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [] });
        }

        return app;
    }
}
