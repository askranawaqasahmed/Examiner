using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface ISchoolRepository : IRepository<School>
{
    Task<School?> GetByCodeAsync(string code);
}
