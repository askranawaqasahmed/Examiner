using System.Collections.Generic;
using System.Linq;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly ISchoolRepository _schoolRepository;

    public ClassService(IClassRepository classRepository, ISchoolRepository schoolRepository)
    {
        _classRepository = classRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<IEnumerable<ClassDto>> GetAllAsync()
    {
        var entities = (await _classRepository.GetAllAsync()).ToList();
        var schoolLookup = await BuildSchoolLookupAsync(entities.Select(e => e.SchoolId));
        return entities.Select(entity => Map(entity, schoolLookup));
    }

    public async Task<ClassDto?> GetByIdAsync(Guid id)
    {
        var entity = await _classRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return null;
        }

        var school = await _schoolRepository.GetByIdAsync(entity.SchoolId);
        var schoolName = school?.Name ?? string.Empty;
        return Map(entity, schoolName);
    }

    public async Task<IEnumerable<ClassDto>> GetBySchoolAsync(Guid schoolId)
    {
        var entities = (await _classRepository.GetBySchoolAsync(schoolId)).ToList();
        var school = await _schoolRepository.GetByIdAsync(schoolId);
        var schoolName = school?.Name ?? string.Empty;
        return entities.Select(entity => Map(entity, schoolName));
    }

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

    private static ClassDto Map(ClassEntity entity, string schoolName)
        => new()
        {
            Id = entity.Id,
            SchoolId = entity.SchoolId,
            Name = entity.Name,
            Section = entity.Section,
            SchoolName = schoolName
        };

    private static ClassDto Map(ClassEntity entity, IReadOnlyDictionary<Guid, string> schoolLookup)
    {
        var schoolName = schoolLookup.TryGetValue(entity.SchoolId, out var name) ? name : string.Empty;
        return Map(entity, schoolName);
    }

    private async Task<Dictionary<Guid, string>> BuildSchoolLookupAsync(IEnumerable<Guid> schoolIds)
    {
        var uniqueIds = schoolIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (uniqueIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var schools = await _schoolRepository.GetByIdsAsync(uniqueIds);
        return schools.ToDictionary(s => s.Id, s => s.Name);
    }
}
