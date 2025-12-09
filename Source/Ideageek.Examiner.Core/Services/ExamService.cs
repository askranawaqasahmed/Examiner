using System.Linq;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;

    public ExamService(IExamRepository examRepository)
    {
        _examRepository = examRepository;
    }

    public async Task<IEnumerable<ExamDto>> GetAllAsync()
        => (await _examRepository.GetAllAsync()).Select(Map);

    public async Task<ExamDto?> GetByIdAsync(Guid id)
    {
        var entity = await _examRepository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<IEnumerable<ExamDto>> GetByClassAsync(Guid classId)
        => (await _examRepository.GetByClassAsync(classId)).Select(Map);

    public Task<Guid> CreateAsync(ExamRequestDto request)
    {
        var entity = new Exam
        {
            Id = Guid.NewGuid(),
            SchoolId = request.SchoolId,
            ClassId = request.ClassId,
            Name = request.Name,
            Subject = request.Subject,
            TotalMarks = request.TotalMarks,
            QuestionCount = request.QuestionCount,
            ExamDate = request.ExamDate,
            CreatedAt = DateTime.UtcNow
        };

        return _examRepository.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, ExamRequestDto request)
    {
        var entity = await _examRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.SchoolId = request.SchoolId;
        entity.ClassId = request.ClassId;
        entity.Name = request.Name;
        entity.Subject = request.Subject;
        entity.TotalMarks = request.TotalMarks;
        entity.QuestionCount = request.QuestionCount;
        entity.ExamDate = request.ExamDate;

        return await _examRepository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id) => _examRepository.DeleteAsync(id);

    private static ExamDto Map(Exam entity)
        => new()
        {
            Id = entity.Id,
            SchoolId = entity.SchoolId,
            ClassId = entity.ClassId,
            Name = entity.Name,
            Subject = entity.Subject,
            TotalMarks = entity.TotalMarks,
            QuestionCount = entity.QuestionCount,
            ExamDate = entity.ExamDate
        };
}
