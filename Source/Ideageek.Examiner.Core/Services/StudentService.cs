using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserService _userService;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IClassRepository _classRepository;

    public StudentService(
        IStudentRepository studentRepository,
        IUserService userService,
        ISchoolRepository schoolRepository,
        IClassRepository classRepository)
    {
        _studentRepository = studentRepository;
        _userService = userService;
        _schoolRepository = schoolRepository;
        _classRepository = classRepository;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync()
    {
        var entities = (await _studentRepository.GetAllAsync()).ToList();
        var schoolLookup = await BuildSchoolLookupAsync(entities.Select(e => e.SchoolId));
        var classLookup = await BuildClassLookupAsync(entities.Select(e => e.ClassId));
        return entities.Select(entity => Map(entity, schoolLookup, classLookup));
    }

    public async Task<StudentDto?> GetByIdAsync(Guid id)
    {
        var entity = await _studentRepository.GetByIdAsync(id);
        return entity is null ? null : await MapWithNamesAsync(entity);
    }

    public async Task<StudentDto?> GetByStudentNumberAsync(string studentNumber)
    {
        var entity = await _studentRepository.GetByStudentNumberAsync(studentNumber);
        return entity is null ? null : await MapWithNamesAsync(entity);
    }

    public async Task<IEnumerable<StudentDto>> GetByClassAsync(Guid classId)
    {
        var entities = (await _studentRepository.GetByClassAsync(classId)).ToList();
        var schoolLookup = await BuildSchoolLookupAsync(entities.Select(e => e.SchoolId));
        var classLookup = await BuildClassLookupAsync(entities.Select(e => e.ClassId));
        return entities.Select(entity => Map(entity, schoolLookup, classLookup));
    }

    public async Task<Guid> CreateAsync(StudentRequestDto request)
    {
        var usernameToUse = string.IsNullOrWhiteSpace(request.Username)
            ? request.StudentNumber
            : request.Username.Trim();
        var passwordToUse = string.IsNullOrWhiteSpace(request.Password)
            ? request.StudentNumber
            : request.Password;

        var existingUser = await _userService.GetByUsernameAsync(usernameToUse);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var entity = new Student
        {
            Id = Guid.NewGuid(),
            SchoolId = request.SchoolId,
            ClassId = request.ClassId,
            StudentNumber = request.StudentNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        await _studentRepository.InsertAsync(entity);
        await _userService.CreateStudentUserAsync(entity, usernameToUse, passwordToUse);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, StudentRequestDto request)
    {
        var entity = await _studentRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.SchoolId = request.SchoolId;
        entity.ClassId = request.ClassId;
        entity.StudentNumber = request.StudentNumber;
        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;

        return await _studentRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _userService.DeleteStudentUserAsync(id);
        return await _studentRepository.DeleteAsync(id);
    }

    private async Task<StudentDto> MapWithNamesAsync(Student entity)
    {
        var schoolName = (await _schoolRepository.GetByIdAsync(entity.SchoolId))?.Name ?? string.Empty;
        var className = (await _classRepository.GetByIdAsync(entity.ClassId))?.Name ?? string.Empty;
        return Map(entity, schoolName, className);
    }

    private static StudentDto Map(Student entity, IReadOnlyDictionary<Guid, string> schoolLookup, IReadOnlyDictionary<Guid, string> classLookup)
    {
        var schoolName = schoolLookup.TryGetValue(entity.SchoolId, out var school) ? school : string.Empty;
        var className = classLookup.TryGetValue(entity.ClassId, out var cls) ? cls : string.Empty;
        return Map(entity, schoolName, className);
    }

    private static StudentDto Map(Student entity, string schoolName, string className)
        => new()
        {
            Id = entity.Id,
            SchoolId = entity.SchoolId,
            ClassId = entity.ClassId,
            StudentNumber = entity.StudentNumber,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
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
