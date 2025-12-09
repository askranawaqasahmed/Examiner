using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IClassService
{
    Task<IEnumerable<ClassDto>> GetAllAsync();
    Task<ClassDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ClassDto>> GetBySchoolAsync(Guid schoolId);
    Task<Guid> CreateAsync(ClassRequestDto request);
    Task<bool> UpdateAsync(Guid id, ClassRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
