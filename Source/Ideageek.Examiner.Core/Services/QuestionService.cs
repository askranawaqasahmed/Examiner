using System.Linq;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;

    public QuestionService(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public Task<IEnumerable<QuestionDto>> GetByExamAsync(Guid examId)
        => MapCollection(_questionRepository.GetByExamAsync(examId));

    public async Task<QuestionDto?> GetByIdAsync(Guid id)
    {
        var entity = await _questionRepository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public Task<Guid> CreateAsync(QuestionRequestDto request)
    {
        var entity = new Question
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            QuestionNumber = request.QuestionNumber,
            Text = request.Text,
            OptionA = request.OptionA,
            OptionB = request.OptionB,
            OptionC = request.OptionC,
            OptionD = request.OptionD,
            CorrectOption = char.ToUpperInvariant(request.CorrectOption)
        };

        return _questionRepository.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, QuestionRequestDto request)
    {
        var entity = await _questionRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.QuestionNumber = request.QuestionNumber;
        entity.Text = request.Text;
        entity.OptionA = request.OptionA;
        entity.OptionB = request.OptionB;
        entity.OptionC = request.OptionC;
        entity.OptionD = request.OptionD;
        entity.CorrectOption = char.ToUpperInvariant(request.CorrectOption);

        return await _questionRepository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id) => _questionRepository.DeleteAsync(id);

    private async Task<IEnumerable<QuestionDto>> MapCollection(Task<IEnumerable<Question>> task)
        => (await task).Select(Map);

    private static QuestionDto Map(Question entity)
        => new()
        {
            Id = entity.Id,
            ExamId = entity.ExamId,
            QuestionNumber = entity.QuestionNumber,
            Text = entity.Text,
            OptionA = entity.OptionA,
            OptionB = entity.OptionB,
            OptionC = entity.OptionC,
            OptionD = entity.OptionD,
            CorrectOption = entity.CorrectOption
        };
}
