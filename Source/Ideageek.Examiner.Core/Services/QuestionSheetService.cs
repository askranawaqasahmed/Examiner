using System.Linq;
using System.Text.Json;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Helpers;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class QuestionSheetService : IQuestionSheetService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IExamRepository _examRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerSheetService _answerSheetService;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly IQuestionSheetTemplateRepository _templateRepository;

    public QuestionSheetService(
        IStudentRepository studentRepository,
        IExamRepository examRepository,
        IQuestionRepository questionRepository,
        IAnswerSheetService answerSheetService,
        IQrCodeGenerator qrCodeGenerator,
        IQuestionSheetTemplateRepository templateRepository)
    {
        _studentRepository = studentRepository;
        _examRepository = examRepository;
        _questionRepository = questionRepository;
        _answerSheetService = answerSheetService;
        _qrCodeGenerator = qrCodeGenerator;
        _templateRepository = templateRepository;
    }

    public async Task<DummyQuestionSheetResponseDto?> GenerateDummySheetAsync(Guid examId, string studentNumber)
    {
        var student = await _studentRepository.GetByStudentNumberAsync(studentNumber);
        if (student is null)
        {
            return null;
        }

        var exam = await _examRepository.GetByIdAsync(examId);
        if (exam is null)
        {
            return null;
        }

        var questions = (await _questionRepository.GetByExamAsync(examId)).ToList();
        if (questions.Count == 0)
        {
            return null;
        }

        var sheetCode = SheetCodeGenerator.BuildSheetCode(examId, student.StudentNumber);
        await _answerSheetService.CreateAsync(exam.Id, student.Id, student.StudentNumber, sheetCode);

        var qrPayload = JsonSerializer.Serialize(new
        {
            SheetCode = sheetCode,
            StudentNumber = student.StudentNumber,
            ExamId = exam.Id,
            QuestionCount = exam.QuestionCount
        });

        var base64Qr = _qrCodeGenerator.GenerateBase64(qrPayload);

        return new DummyQuestionSheetResponseDto
        {
            SheetCode = sheetCode,
            StudentNumber = student.StudentNumber,
            StudentName = string.Join(" ", new[] { student.FirstName, student.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))),
            ExamName = exam.Name,
            Base64QrPng = base64Qr,
            QrPayload = qrPayload,
            Questions = questions
                .Select(q => new DummyQuestionSheetQuestionDto
                {
                    QuestionNumber = q.QuestionNumber,
                    Text = q.Text,
                    Options = new DummyQuestionSheetQuestionOptionsDto
                    {
                        A = q.OptionA,
                        B = q.OptionB,
                        C = q.OptionC,
                        D = q.OptionD
                    }
                })
                .ToArray()
        };
    }

    public async Task<QuestionSheetTemplateResponseDto?> GetTemplateAsync(Guid examId)
    {
        var exam = await _examRepository.GetByIdAsync(examId);
        if (exam is null)
        {
            return null;
        }

        var questions = (await _questionRepository.GetByExamAsync(examId))
            .OrderBy(q => q.QuestionNumber)
            .ToList();

        if (questions.Count == 0)
        {
            return null;
        }

        var template = await _templateRepository.GetDefaultByExamAsync(examId);
        var optionsPerQuestion = CalculateOptionsPerQuestion(questions.First());

        return new QuestionSheetTemplateResponseDto
        {
            ExamId = exam.Id,
            ExamName = exam.Name,
            QuestionCount = questions.Count,
            Template = new QuestionSheetTemplateMetadataDto
            {
                Name = template?.Name ?? "Default",
                OptionsPerQuestion = optionsPerQuestion
            },
            Questions = questions.Select(q => new QuestionSheetTemplateQuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                Text = q.Text,
                CorrectOption = q.CorrectOption.ToString(),
                Options = BuildOptions(q).ToArray()
            }).ToArray()
        };
    }

    private static IEnumerable<QuestionSheetTemplateOptionDto> BuildOptions(Entities.Question q)
    {
        var options = new[]
        {
            ("A", q.OptionA, 1),
            ("B", q.OptionB, 2),
            ("C", q.OptionC, 3),
            ("D", q.OptionD, 4)
        };

        return options
            .Where(o => !string.IsNullOrWhiteSpace(o.Item2))
            .Select(o => new QuestionSheetTemplateOptionDto
            {
                Key = o.Item1,
                Text = o.Item2,
                Order = o.Item3
            });
    }

    private static int CalculateOptionsPerQuestion(Entities.Question q)
    {
        var options = new[] { q.OptionA, q.OptionB, q.OptionC, q.OptionD };
        return options.Count(o => !string.IsNullOrWhiteSpace(o));
    }
}
