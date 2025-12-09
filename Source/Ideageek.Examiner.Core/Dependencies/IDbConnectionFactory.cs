using System.Data;

namespace Ideageek.Examiner.Core.Dependencies;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
