using System.Data.Common;
using Npgsql;

namespace Deep.Common.Application.Dapper;

public sealed class NpgsqlConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync() =>
        await dataSource.OpenConnectionAsync();
}
