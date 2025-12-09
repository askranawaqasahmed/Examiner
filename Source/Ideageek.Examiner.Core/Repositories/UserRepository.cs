using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class UserRepository : SqlKataRepository<UserAccount>, IUserRepository
{
    public UserRepository(QueryFactory queryFactory) : base(queryFactory, "UserAccount")
    {
    }

    public async Task<UserAccount?> GetByUsernameAsync(string username)
    {
        var entity = await QueryFactory.Query(TableName)
            .Where("Username", username)
            .FirstOrDefaultAsync<UserAccount>();

        return entity;
    }

    public async Task<UserAccount?> GetByStudentIdAsync(Guid studentId)
    {
        var entity = await QueryFactory.Query(TableName)
            .Where("StudentId", studentId)
            .FirstOrDefaultAsync<UserAccount>();

        return entity;
    }

    public async Task<bool> DeleteByStudentIdAsync(Guid studentId)
    {
        var affected = await QueryFactory.Query(TableName)
            .Where("StudentId", studentId)
            .DeleteAsync();

        return affected > 0;
    }
}
