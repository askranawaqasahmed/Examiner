namespace Ideageek.Examiner.Core.DataSeeder;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
