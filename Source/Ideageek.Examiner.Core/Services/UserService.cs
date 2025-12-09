using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Helpers;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ideageek.Examiner.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var entity = await _userRepository.GetByUsernameAsync(username);
        return entity is null ? null : Map(entity);
    }

    public async Task<UserDto?> ValidateCredentialsAsync(string username, string password)
    {
        var entity = await _userRepository.GetByUsernameAsync(username);
        if (entity is null)
        {
            return null;
        }

        var valid = PasswordHasher.Verify(password, entity.PasswordHash);
        return valid ? Map(entity) : null;
    }

    public async Task<Guid> CreateAsync(UserCreateRequestDto request)
    {
        var existing = await _userRepository.GetByUsernameAsync(request.Username);
        if (existing is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var entity = new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = request.Role,
            StudentId = request.StudentId,
            TeacherId = request.TeacherId,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.InsertAsync(entity);
        return entity.Id;
    }

    public async Task<Guid> CreateStudentUserAsync(Student student, string? username, string? password)
    {
        var existing = await _userRepository.GetByStudentIdAsync(student.Id);
        if (existing is not null)
        {
            return existing.Id;
        }

        var userNameToUse = string.IsNullOrWhiteSpace(username) ? student.StudentNumber : username.Trim();
        var passwordToUse = string.IsNullOrWhiteSpace(password) ? student.StudentNumber : password;

        _logger.LogInformation("Creating user account for student {StudentNumber} with username {Username}", student.StudentNumber, userNameToUse);

        return await CreateAsync(new UserCreateRequestDto
        {
            Username = userNameToUse,
            Password = passwordToUse,
            Role = "Student",
            StudentId = student.Id
        });
    }

    public async Task EnsureSuperAdminAsync(string username, string password)
    {
        var existing = await _userRepository.GetByUsernameAsync(username);
        if (existing is not null)
        {
            return;
        }

        _logger.LogInformation("Seeding superadmin account with username {Username}", username);

        await CreateAsync(new UserCreateRequestDto
        {
            Username = username,
            Password = password,
            Role = "SuperAdmin"
        });
    }

    public async Task DeleteStudentUserAsync(Guid studentId)
    {
        var deleted = await _userRepository.DeleteByStudentIdAsync(studentId);
        if (deleted)
        {
            _logger.LogInformation("Deleted user account linked to student {StudentId}", studentId);
        }
    }

    private static UserDto Map(UserAccount entity)
        => new()
        {
            Id = entity.Id,
            Username = entity.Username,
            Role = entity.Role,
            StudentId = entity.StudentId,
            TeacherId = entity.TeacherId
        };
}
