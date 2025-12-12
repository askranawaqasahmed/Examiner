using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface IQuestionOptionRepository : IRepository<QuestionOption>
{
    Task<IEnumerable<QuestionOption>> GetByExamAsync(Guid examId);
    Task<IEnumerable<QuestionOption>> GetByQuestionsAsync(IEnumerable<Guid> questionIds);
    Task DeleteByQuestionIdAsync(Guid questionId);
    Task DeleteByQuestionIdsAsync(IEnumerable<Guid> questionIds);
    Task InsertManyAsync(IEnumerable<QuestionOption> options);
}
