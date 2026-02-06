// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.Application.Database;

public static class Postgres
{
    public static Action<IServiceProvider, DbContextOptionsBuilder> StandardOptions(
        IConfiguration configuration,
        string schema) =>
        (serviceProvider, options) =>
        {
            options
                .UseNpgsql(
                    configuration.GetConnectionString("deep-db")!,
                    npgsql =>
                        npgsql.MigrationsHistoryTable(
                            HistoryRepository.DefaultTableName,
                            schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetServices<IInterceptor>());
        };
}
