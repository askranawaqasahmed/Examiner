using System.Linq;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class SchoolService : ISchoolService
{
    private readonly ISchoolRepository _schoolRepository;

    public SchoolService(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<IEnumerable<SchoolDto>> GetAllAsync()
        => (await _schoolRepository.GetAllAsync()).Select(Map);

    public async Task<SchoolDto?> GetByIdAsync(Guid id)
    {
        var entity = await _schoolRepository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public Task<Guid> CreateAsync(SchoolRequestDto request)
    {
        var entity = new School
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow
        };

        return _schoolRepository.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, SchoolRequestDto request)
    {
        var entity = await _schoolRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.Address = request.Address;

        return await _schoolRepository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id) => _schoolRepository.DeleteAsync(id);

    private static SchoolDto Map(School entity)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Address = entity.Address
        };
}
