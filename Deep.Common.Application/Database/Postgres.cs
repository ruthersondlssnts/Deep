using Deep.Common.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deep.Common.Application.Database;

public static class Postgres
{
    public static Action<IServiceProvider, DbContextOptionsBuilder> StandardOptions(IConfiguration configuration, string schema) =>
        (serviceProvider, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("deep-db")!,
                optionsBuilder =>
                {
                    optionsBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema);
                })
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(
                serviceProvider.GetRequiredService<PublishDomainEventsInterceptor>());
        };
}
