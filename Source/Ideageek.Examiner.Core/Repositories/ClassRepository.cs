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
}
