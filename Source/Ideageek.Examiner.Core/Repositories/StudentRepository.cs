using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class StudentRepository : SqlKataRepository<Student>, IStudentRepository
{
    public StudentRepository(QueryFactory queryFactory) : base(queryFactory, "Student")
    {
    }

    public Task<IEnumerable<Student>> GetByClassAsync(Guid classId)
        => QueryFactory.Query(TableName).Where("ClassId", classId).GetAsync<Student>();

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber)
    {
        var entity = await QueryFactory.Query(TableName).Where("StudentNumber", studentNumber).FirstOrDefaultAsync<Student>();
        return entity;
    }
}
