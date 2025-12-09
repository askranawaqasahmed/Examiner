using System.Linq;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserService _userService;

    public StudentService(IStudentRepository studentRepository, IUserService userService)
    {
        _studentRepository = studentRepository;
        _userService = userService;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync()
        => (await _studentRepository.GetAllAsync()).Select(Map);

    public async Task<StudentDto?> GetByIdAsync(Guid id)
    {
        var entity = await _studentRepository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<StudentDto?> GetByStudentNumberAsync(string studentNumber)
    {
        var entity = await _studentRepository.GetByStudentNumberAsync(studentNumber);
        return entity is null ? null : Map(entity);
    }

    public async Task<IEnumerable<StudentDto>> GetByClassAsync(Guid classId)
        => (await _studentRepository.GetByClassAsync(classId)).Select(Map);

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

    private static StudentDto Map(Student entity)
        => new()
        {
            Id = entity.Id,
            SchoolId = entity.SchoolId,
            ClassId = entity.ClassId,
            StudentNumber = entity.StudentNumber,
            FirstName = entity.FirstName,
            LastName = entity.LastName
        };
}
