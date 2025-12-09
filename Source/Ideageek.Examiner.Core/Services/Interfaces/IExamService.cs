using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IExamService
{
    Task<IEnumerable<ExamDto>> GetAllAsync();
    Task<ExamDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExamDto>> GetByClassAsync(Guid classId);
    Task<Guid> CreateAsync(ExamRequestDto request);
    Task<bool> UpdateAsync(Guid id, ExamRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
