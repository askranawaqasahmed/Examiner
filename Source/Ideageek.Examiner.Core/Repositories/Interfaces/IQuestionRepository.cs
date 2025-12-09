using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface IQuestionRepository : IRepository<Question>
{
    Task<IEnumerable<Question>> GetByExamAsync(Guid examId);
    Task<bool> InsertManyAsync(IEnumerable<Question> questions);
}
