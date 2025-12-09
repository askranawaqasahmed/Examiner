using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class ExamRepository : SqlKataRepository<Exam>, IExamRepository
{
    public ExamRepository(QueryFactory queryFactory) : base(queryFactory, "Exam")
    {
    }

    public Task<IEnumerable<Exam>> GetByClassAsync(Guid classId)
        => QueryFactory.Query(TableName).Where("ClassId", classId).GetAsync<Exam>();
}
