using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface ISchoolService
{
    Task<IEnumerable<SchoolDto>> GetAllAsync();
    Task<SchoolDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(SchoolRequestDto request);
    Task<bool> UpdateAsync(Guid id, SchoolRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
