using System.Linq;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class QuestionOptionRepository : SqlKataRepository<QuestionOption>, IQuestionOptionRepository
{
    public QuestionOptionRepository(QueryFactory queryFactory) : base(queryFactory, "QuestionOption")
    {
    }

    public Task<IEnumerable<QuestionOption>> GetByExamAsync(Guid examId)
        => QueryFactory.Query(TableName)
            .Select($"{TableName}.*") // Only option columns to avoid Question.Text overriding option Text
            .Join("Question", "Question.Id", $"{TableName}.QuestionId")
            .Where("Question.ExamId", examId)
            .OrderBy($"{TableName}.Order")
            .GetAsync<QuestionOption>();

    public Task<IEnumerable<QuestionOption>> GetByQuestionsAsync(IEnumerable<Guid> questionIds)
        => QueryFactory.Query(TableName)
            .WhereIn("QuestionId", questionIds)
            .OrderBy("Order")
            .GetAsync<QuestionOption>();

    public Task DeleteByQuestionIdAsync(Guid questionId)
        => QueryFactory.Query(TableName).Where("QuestionId", questionId).DeleteAsync();

    public Task DeleteByQuestionIdsAsync(IEnumerable<Guid> questionIds)
        => QueryFactory.Query(TableName).WhereIn("QuestionId", questionIds).DeleteAsync();

    public async Task InsertManyAsync(IEnumerable<QuestionOption> options)
    {
        var payload = options.ToList();
        foreach (var option in payload)
        {
            EnsureEntityId(option);
        }

        if (payload.Count == 0)
        {
            return;
        }

        foreach (var option in payload)
        {
            await QueryFactory.Query(TableName).InsertAsync(option);
        }
    }
}
