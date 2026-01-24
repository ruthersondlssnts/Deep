using System.Data.Common;

namespace Deep.Common.Dapper
{
    public interface IDbConnectionFactory
    {
        ValueTask<DbConnection> OpenConnectionAsync();
    }
}
