using System.Data;
using Ideageek.Examiner.Core.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Ideageek.Examiner.Core.Dependencies;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public DbConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
