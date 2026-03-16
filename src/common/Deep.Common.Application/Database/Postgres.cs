using Deep.Common.Application.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.Application.Database;

public static class Postgres
{
    public static void ConfigureOptions<TOutboxInterceptor>(
        DbContextOptionsBuilder options,
        IConfiguration configuration,
        string schema,
        IServiceProvider serviceProvider
    )
        where TOutboxInterceptor : IInterceptor =>
        options
            .UseNpgsql(
                configuration.GetConnectionString("deep-db")!,
                npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema)
            )
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(
                serviceProvider.GetRequiredService<TOutboxInterceptor>(),
                serviceProvider.GetRequiredService<WriteAuditLogInterceptor>()
            );
}
