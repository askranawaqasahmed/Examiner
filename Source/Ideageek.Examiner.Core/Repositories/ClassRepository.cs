using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class ClassRepository : SqlKataRepository<ClassEntity>, IClassRepository
{
    public ClassRepository(QueryFactory queryFactory) : base(queryFactory, "Class")
    {
    }

    public Task<IEnumerable<ClassEntity>> GetBySchoolAsync(Guid schoolId)
        => QueryFactory.Query(TableName).Where("SchoolId", schoolId).GetAsync<ClassEntity>();

    public Task<IEnumerable<ClassEntity>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = (ids ?? Enumerable.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToList();
        if (idList.Count == 0)
        {
            return Task.FromResult<IEnumerable<ClassEntity>>(Enumerable.Empty<ClassEntity>());
        }

        return QueryFactory.Query(TableName).WhereIn("Id", idList).GetAsync<ClassEntity>();
    }
}
