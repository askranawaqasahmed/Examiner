using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllAsync();
    Task<StudentDto?> GetByIdAsync(Guid id);
    Task<StudentDto?> GetByStudentNumberAsync(string studentNumber);
    Task<IEnumerable<StudentDto>> GetByClassAsync(Guid classId);
    Task<Guid> CreateAsync(StudentRequestDto request);
    Task<bool> UpdateAsync(Guid id, StudentRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
