using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class EvaluationService : IEvaluationService
{
    private readonly IAnswerSheetService _answerSheetService;
    private readonly IExamRepository _examRepository;
    private readonly IQuestionRepository _questionRepository;

    public EvaluationService(
        IAnswerSheetService answerSheetService,
        IExamRepository examRepository,
        IQuestionRepository questionRepository)
    {
        _answerSheetService = answerSheetService;
        _examRepository = examRepository;
        _questionRepository = questionRepository;
    }

    public async Task<DummySheetEvaluationResponseDto?> EvaluateDummySheetAsync(DummySheetEvaluationRequestDto request)
    {
        var sheet = await _answerSheetService.GetBySheetCodeAsync(request.SheetCode, request.StudentNumber);
        if (sheet is null)
        {
            return null;
        }

        var exam = await _examRepository.GetByIdAsync(sheet.ExamId);
        if (exam is null)
        {
            return null;
        }

        var questions = (await _questionRepository.GetByExamAsync(sheet.ExamId)).ToDictionary(q => q.QuestionNumber);
        if (questions.Count == 0)
        {
            return null;
        }

        var responseDetails = new List<DummySheetEvaluationDetailDto>();
        var detailEntities = new List<AnswerSheetDetail>();

        var normalizedAnswers = request.Answers?.ToDictionary(
            keySelector: pair => pair.Key,
            elementSelector: pair => NormalizeOption(pair.Value))
            ?? new Dictionary<int, char?>();

        var correctCount = 0;
        var wrongCount = 0;
        var blankCount = 0;

        for (var number = 1; number <= exam.QuestionCount; number++)
        {
            if (!questions.TryGetValue(number, out var question))
            {
                continue;
            }

            normalizedAnswers.TryGetValue(number, out var selectedOption);
            var isCorrect = selectedOption.HasValue && char.ToUpperInvariant(selectedOption.Value) == char.ToUpperInvariant(question.CorrectOption);
            var marks = isCorrect ? 1 : 0;

            if (!selectedOption.HasValue)
            {
                blankCount++;
            }
            else if (isCorrect)
            {
                correctCount++;
            }
            else
            {
                wrongCount++;
            }

            detailEntities.Add(new AnswerSheetDetail
            {
                Id = Guid.NewGuid(),
                AnswerSheetId = sheet.Id,
                QuestionId = question.Id,
                QuestionNumber = question.QuestionNumber,
                SelectedOption = selectedOption.HasValue ? char.ToUpperInvariant(selectedOption.Value) : null,
                IsCorrect = isCorrect,
                Marks = marks
            });

            responseDetails.Add(new DummySheetEvaluationDetailDto
            {
                QuestionNumber = question.QuestionNumber,
                SelectedOption = selectedOption.HasValue ? char.ToUpperInvariant(selectedOption.Value).ToString() : null,
                CorrectOption = question.CorrectOption.ToString(),
                IsCorrect = isCorrect,
                Marks = marks
            });
        }

        var obtainedMarks = correctCount; // one mark each
        await _answerSheetService.SaveEvaluationAsync(sheet.Id, detailEntities, exam.TotalMarks, correctCount, wrongCount, blankCount);

        return new DummySheetEvaluationResponseDto
        {
            SheetCode = sheet.SheetCode,
            StudentNumber = sheet.StudentNumber,
            ExamName = exam.Name,
            TotalMarks = exam.TotalMarks,
            ObtainedMarks = obtainedMarks,
            CorrectCount = correctCount,
            WrongCount = wrongCount,
            BlankCount = blankCount,
            Details = responseDetails
        };
    }

    private static char? NormalizeOption(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return char.ToUpperInvariant(value.Trim()[0]);
    }
}
