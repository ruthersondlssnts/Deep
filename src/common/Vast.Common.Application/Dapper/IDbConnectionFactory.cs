using System.Data.Common;

namespace Vast.Common.Application.Dapper;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
