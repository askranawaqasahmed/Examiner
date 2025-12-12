using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuestionOptionRepository _questionOptionRepository;

    public QuestionService(
        IQuestionRepository questionRepository,
        IQuestionOptionRepository questionOptionRepository)
    {
        _questionRepository = questionRepository;
        _questionOptionRepository = questionOptionRepository;
    }

    public async Task<IEnumerable<QuestionDto>> GetByExamAsync(Guid examId)
    {
        var questions = (await _questionRepository.GetByExamAsync(examId)).ToList();
        var options = (await _questionOptionRepository.GetByExamAsync(examId))
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.Select(MapOption).ToArray());

        return questions
            .Select(q => Map(q, options.TryGetValue(q.Id, out var opts) ? opts : Array.Empty<QuestionOptionDto>()))
            .ToArray();
    }

    public async Task<QuestionDto?> GetByIdAsync(Guid id)
    {
        var entity = await _questionRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return null;
        }

        var options = (await _questionOptionRepository.GetByQuestionsAsync(new[] { entity.Id }))
            .Select(MapOption)
            .ToArray();

        return Map(entity, options);
    }

    public async Task<Guid> CreateAsync(QuestionRequestDto request)
    {
        var entity = new Question
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            QuestionNumber = request.QuestionNumber,
            Text = request.Text,
            CorrectOption = char.ToUpperInvariant(request.CorrectOption)
        };

        await _questionRepository.InsertAsync(entity);
        await _questionOptionRepository.InsertManyAsync(CreateOptions(entity.Id, request.Options));
        return entity.Id;
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
        entity.CorrectOption = char.ToUpperInvariant(request.CorrectOption);

        var updated = await _questionRepository.UpdateAsync(entity);
        if (!updated)
        {
            return false;
        }

        await _questionOptionRepository.DeleteByQuestionIdAsync(entity.Id);
        await _questionOptionRepository.InsertManyAsync(CreateOptions(entity.Id, request.Options));
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _questionOptionRepository.DeleteByQuestionIdAsync(id);
        return await _questionRepository.DeleteAsync(id);
    }

    private static QuestionDto Map(Question entity, IReadOnlyCollection<QuestionOptionDto> options)
        => new()
        {
            Id = entity.Id,
            ExamId = entity.ExamId,
            QuestionNumber = entity.QuestionNumber,
            Text = entity.Text,
            CorrectOption = entity.CorrectOption,
            Options = options
        };

    private static QuestionOptionDto MapOption(QuestionOption option)
        => new()
        {
            Key = option.Key,
            Text = option.Text,
            Order = option.Order
        };

    private static IEnumerable<QuestionOption> CreateOptions(Guid questionId, IEnumerable<QuestionOptionDto> dtos)
        => dtos.Select(dto => new QuestionOption
        {
            QuestionId = questionId,
            Key = dto.Key,
            Text = dto.Text,
            Order = dto.Order
        });
}
