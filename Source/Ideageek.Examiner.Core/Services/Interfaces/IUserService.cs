using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<UserDto?> ValidateCredentialsAsync(string username, string password);
    Task<Guid> CreateAsync(UserCreateRequestDto request);
    Task<Guid> CreateStudentUserAsync(Student student, string? username, string? password);
    Task EnsureSuperAdminAsync(string username, string password);
    Task DeleteStudentUserAsync(Guid studentId);
}
