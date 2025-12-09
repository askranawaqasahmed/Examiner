using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface IUserRepository : IRepository<UserAccount>
{
    Task<UserAccount?> GetByUsernameAsync(string username);
    Task<UserAccount?> GetByStudentIdAsync(Guid studentId);
    Task<bool> DeleteByStudentIdAsync(Guid studentId);
}
