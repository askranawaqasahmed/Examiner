using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Enums;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionDto>> GetByExamAsync(Guid examId);
    Task<QuestionDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(QuestionRequestDto request);
    Task<bool> UpdateAsync(QuestionUpdateRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
