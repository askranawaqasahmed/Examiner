using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface IExamRepository : IRepository<Exam>
{
    Task<IEnumerable<Exam>> GetByClassAsync(Guid classId);
}
