using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionDto>> GetByExamAsync(Guid examId);
    Task<QuestionDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(QuestionRequestDto request);
    Task<bool> UpdateAsync(Guid id, QuestionRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
