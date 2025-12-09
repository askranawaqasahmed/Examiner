using System.Linq;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;

    public ClassService(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<IEnumerable<ClassDto>> GetAllAsync()
        => (await _classRepository.GetAllAsync()).Select(Map);

    public async Task<ClassDto?> GetByIdAsync(Guid id)
    {
        var entity = await _classRepository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<IEnumerable<ClassDto>> GetBySchoolAsync(Guid schoolId)
        => (await _classRepository.GetBySchoolAsync(schoolId)).Select(Map);

    public Task<Guid> CreateAsync(ClassRequestDto request)
    {
        var entity = new ClassEntity
        {
            Id = Guid.NewGuid(),
            SchoolId = request.SchoolId,
            Name = request.Name,
            Section = request.Section,
            CreatedAt = DateTime.UtcNow
        };

        return _classRepository.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, ClassRequestDto request)
    {
        var entity = await _classRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.SchoolId = request.SchoolId;
        entity.Name = request.Name;
        entity.Section = request.Section;

        return await _classRepository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id) => _classRepository.DeleteAsync(id);

    private static ClassDto Map(ClassEntity entity)
        => new()
        {
            Id = entity.Id,
            SchoolId = entity.SchoolId,
            Name = entity.Name,
            Section = entity.Section
        };
}
