using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class SchoolRepository : SqlKataRepository<School>, ISchoolRepository
{
    public SchoolRepository(QueryFactory queryFactory) : base(queryFactory, "School")
    {
    }

    public async Task<School?> GetByCodeAsync(string code)
    {
        var entity = await QueryFactory.Query(TableName).Where("Code", code).FirstOrDefaultAsync<School>();
        return entity;
    }

    public Task<IEnumerable<School>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = (ids ?? Enumerable.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToList();
        if (idList.Count == 0)
        {
            return Task.FromResult<IEnumerable<School>>(Enumerable.Empty<School>());
        }

        return QueryFactory.Query(TableName).WhereIn("Id", idList).GetAsync<School>();
    }
}
