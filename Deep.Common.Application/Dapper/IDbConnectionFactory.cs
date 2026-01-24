using System.Data.Common;

namespace Deep.Common.Application.Dapper;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
