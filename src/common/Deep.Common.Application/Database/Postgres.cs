using Deep.Common.Application.Auditing;
using Deep.Common.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.Application.Database;

public static class Postgres
{
    public static void ConfigureOptions(
        DbContextOptionsBuilder options,
        IConfiguration configuration,
        string schema,
        IServiceProvider serviceProvider
    ) =>
        options
            .UseNpgsql(
                configuration.GetConnectionString("deep-db")!,
                npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema)
            )
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(
                serviceProvider.GetRequiredService<InsertOutboxMessagesInterceptor>(),
                serviceProvider.GetRequiredService<WriteAuditLogInterceptor>()
            );
}
