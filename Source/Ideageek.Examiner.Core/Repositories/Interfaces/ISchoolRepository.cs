using System.Collections.Generic;
using System.Threading.Tasks;
using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface ISchoolRepository : IRepository<School>
{
    Task<School?> GetByCodeAsync(string code);
    Task<IEnumerable<School>> GetByIdsAsync(IEnumerable<Guid> ids);
}
