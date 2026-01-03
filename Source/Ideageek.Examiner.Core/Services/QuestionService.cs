using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Enums;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuestionOptionRepository _questionOptionRepository;
    private readonly IExamRepository _examRepository;

    public QuestionService(
        IQuestionRepository questionRepository,
        IQuestionOptionRepository questionOptionRepository,
        IExamRepository examRepository)
    {
        _questionRepository = questionRepository;
        _questionOptionRepository = questionOptionRepository;
        _examRepository = examRepository;
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
        var exam = await _examRepository.GetByIdAsync(request.ExamId)
                   ?? throw new InvalidOperationException("Exam not found.");

        var questionType = exam.Type == ExamType.Detailed ? QuestionType.Detailed : QuestionType.Mcq;
        var entity = new Question
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            QuestionNumber = request.QuestionNumber,
            Text = request.Text,
            CorrectOption = questionType == QuestionType.Mcq ? char.ToUpperInvariant(request.CorrectOption) : 'A',
            Type = questionType,
            Lines = questionType == QuestionType.Detailed ? request.Lines ?? 3 : null,
            Marks = questionType == QuestionType.Detailed ? request.Marks ?? 0 : null
        };

        await _questionRepository.InsertAsync(entity);
        if (questionType == QuestionType.Mcq)
        {
            await _questionOptionRepository.InsertManyAsync(CreateOptions(entity.Id, request.Options));
        }

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(QuestionUpdateRequestDto request)
    {
        var entity = await _questionRepository.GetByIdAsync(request.Id);
        if (entity is null)
        {
            return false;
        }

        var exam = await _examRepository.GetByIdAsync(entity.ExamId);
        if (exam is null)
        {
            return false;
        }

        var questionType = exam.Type == ExamType.Detailed ? QuestionType.Detailed : QuestionType.Mcq;

        entity.QuestionNumber = request.QuestionNumber;
        entity.Text = request.Text;
        entity.CorrectOption = questionType == QuestionType.Mcq ? char.ToUpperInvariant(request.CorrectOption) : 'A';
        entity.Type = questionType;
        entity.Lines = questionType == QuestionType.Detailed ? request.Lines ?? 3 : null;
        entity.Marks = questionType == QuestionType.Detailed ? request.Marks ?? 0 : null;

        var updated = await _questionRepository.UpdateAsync(entity);
        if (!updated)
        {
            return false;
        }

        await _questionOptionRepository.DeleteByQuestionIdAsync(entity.Id);
        if (questionType == QuestionType.Mcq)
        {
            await _questionOptionRepository.InsertManyAsync(CreateOptions(entity.Id, request.Options));
        }

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
            Type = entity.Type,
            Lines = entity.Lines,
            Marks = entity.Marks,
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
