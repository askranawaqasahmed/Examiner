using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface IClassRepository : IRepository<ClassEntity>
{
    Task<IEnumerable<ClassEntity>> GetBySchoolAsync(Guid schoolId);
}
