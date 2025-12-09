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
}
