using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IClassRepository _classRepository;

    public ExamService(
        IExamRepository examRepository,
        ISchoolRepository schoolRepository,
        IClassRepository classRepository)
    {
        _examRepository = examRepository;
        _schoolRepository = schoolRepository;
        _classRepository = classRepository;
    }

    public async Task<IEnumerable<ExamDto>> GetAllAsync()
    {
        var entities = (await _examRepository.GetAllAsync()).ToList();
        var schoolLookup = await BuildSchoolLookupAsync(entities.Select(e => e.SchoolId));
        var classLookup = await BuildClassLookupAsync(entities.Select(e => e.ClassId));
        return entities.Select(entity => Map(entity, schoolLookup, classLookup));
    }

    public async Task<ExamDto?> GetByIdAsync(Guid id)
    {
        var entity = await _examRepository.GetByIdAsync(id);
        return entity is null ? null : await MapWithNamesAsync(entity);
    }

    public async Task<IEnumerable<ExamDto>> GetByClassAsync(Guid classId)
    {
        var entities = (await _examRepository.GetByClassAsync(classId)).ToList();
        var schoolLookup = await BuildSchoolLookupAsync(entities.Select(e => e.SchoolId));
        var classLookup = await BuildClassLookupAsync(entities.Select(e => e.ClassId));
        return entities.Select(entity => Map(entity, schoolLookup, classLookup));
    }

    public Task<Guid> CreateAsync(ExamRequestDto request)
    {
        var entity = new Exam
        {
            Id = Guid.NewGuid(),
            SchoolId = request.SchoolId,
            ClassId = request.ClassId,
            Name = request.Name,
            Subject = request.Subject,
            TotalMarks = request.TotalMarks,
            QuestionCount = request.QuestionCount,
            ExamDate = request.ExamDate,
            CreatedAt = DateTime.UtcNow
        };

        return _examRepository.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, ExamRequestDto request)
    {
        var entity = await _examRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.SchoolId = request.SchoolId;
        entity.ClassId = request.ClassId;
        entity.Name = request.Name;
        entity.Subject = request.Subject;
        entity.TotalMarks = request.TotalMarks;
        entity.QuestionCount = request.QuestionCount;
        entity.ExamDate = request.ExamDate;

        return await _examRepository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id) => _examRepository.DeleteAsync(id);

    private async Task<ExamDto> MapWithNamesAsync(Exam entity)
    {
        var schoolName = (await _schoolRepository.GetByIdAsync(entity.SchoolId))?.Name ?? string.Empty;
        var className = (await _classRepository.GetByIdAsync(entity.ClassId))?.Name ?? string.Empty;
        return Map(entity, schoolName, className);
    }

    private static ExamDto Map(Exam entity, IReadOnlyDictionary<Guid, string> schoolLookup, IReadOnlyDictionary<Guid, string> classLookup)
    {
        var schoolName = schoolLookup.TryGetValue(entity.SchoolId, out var school) ? school : string.Empty;
        var className = classLookup.TryGetValue(entity.ClassId, out var cls) ? cls : string.Empty;
        return Map(entity, schoolName, className);
    }

    private static ExamDto Map(Exam entity, string schoolName, string className)
        => new()
        {
            Id = entity.Id,
            SchoolId = entity.SchoolId,
            ClassId = entity.ClassId,
            Name = entity.Name,
            Subject = entity.Subject,
            TotalMarks = entity.TotalMarks,
            QuestionCount = entity.QuestionCount,
            ExamDate = entity.ExamDate,
            SchoolName = schoolName,
            ClassName = className
        };

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

    private async Task<Dictionary<Guid, string>> BuildClassLookupAsync(IEnumerable<Guid> classIds)
    {
        var uniqueIds = classIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (uniqueIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var classes = await _classRepository.GetByIdsAsync(uniqueIds);
        return classes.ToDictionary(c => c.Id, c => c.Name);
    }
}
