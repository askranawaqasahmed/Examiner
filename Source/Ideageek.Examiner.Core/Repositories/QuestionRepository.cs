using System.Linq;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class QuestionRepository : SqlKataRepository<Question>, IQuestionRepository
{
    public QuestionRepository(QueryFactory queryFactory) : base(queryFactory, "Question")
    {
    }

    public Task<IEnumerable<Question>> GetByExamAsync(Guid examId)
        => QueryFactory.Query(TableName).Where("ExamId", examId).OrderBy("QuestionNumber").GetAsync<Question>();

    public async Task<bool> InsertManyAsync(IEnumerable<Question> questions)
    {
        var payload = questions.ToList();
        foreach (var q in payload)
        {
            EnsureEntityId(q);
        }

        if (payload.Count == 0)
        {
            return true;
        }

        var affected = 0;
        foreach (var q in payload)
        {
            affected += await QueryFactory.Query(TableName).InsertAsync(q);
        }

        return affected > 0;
    }
}
