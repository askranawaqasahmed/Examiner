using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Helpers;
using Ideageek.Examiner.Core.Options;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Ideageek.Examiner.Core.Services;

public class QuestionSheetService : IQuestionSheetService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IExamRepository _examRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerSheetService _answerSheetService;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly IQuestionSheetTemplateRepository _templateRepository;
    private readonly IQuestionOptionRepository _questionOptionRepository;
    private readonly QuestionSheetGenerationOptions _generationOptions;

    public QuestionSheetService(
        IStudentRepository studentRepository,
        IExamRepository examRepository,
        IQuestionRepository questionRepository,
        IAnswerSheetService answerSheetService,
        IQrCodeGenerator qrCodeGenerator,
        IQuestionSheetTemplateRepository templateRepository,
        IQuestionOptionRepository questionOptionRepository,
        IOptions<QuestionSheetGenerationOptions> generationOptions)
    {
        _studentRepository = studentRepository;
        _examRepository = examRepository;
        _questionRepository = questionRepository;
        _answerSheetService = answerSheetService;
        _qrCodeGenerator = qrCodeGenerator;
        _templateRepository = templateRepository;
        _questionOptionRepository = questionOptionRepository;
        _generationOptions = generationOptions?.Value ?? new QuestionSheetGenerationOptions();
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

        var questionIds = questions.Select(q => q.Id);
        var optionLookup = (await _questionOptionRepository.GetByQuestionsAsync(questionIds))
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.Select(MapDummyOption).ToArray());

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
                    Options = optionLookup.TryGetValue(q.Id, out var opts) ? opts : Array.Empty<DummyQuestionSheetQuestionOptionDto>()
                })
                .ToArray()
        };
    }

    public async Task<QuestionSheetTemplateResponseDto?> GetTemplateAsync(Guid examId)
    {
        var data = await LoadExamQuestionDataAsync(examId);
        if (data is null)
        {
            return null;
        }

        return BuildTemplateResponse(data);
    }

    public async Task<QuestionSheetAnswerResponseDto?> GetAnswerSheetAsync(Guid examId)
    {
        var data = await LoadExamQuestionDataAsync(examId);
        if (data is null)
        {
            return null;
        }

        return new QuestionSheetAnswerResponseDto
        {
            ExamId = data.Exam.Id,
            ExamName = data.Exam.Name,
            QuestionCount = data.Questions.Count,
            Questions = data.Questions
                .Select(q =>
                {
                    var options = data.Options.TryGetValue(q.Id, out var opts) ? opts : Array.Empty<QuestionOption>();
                    var optionCount = CalculateOptionsPerQuestion(options);
                    return new QuestionSheetAnswerQuestionDto
                    {
                        QuestionNumber = q.QuestionNumber,
                        OptionCount = optionCount,
                        OptionLabelSet = BuildOptionLabelSet(optionCount)
                    };
                })
                .ToArray()
        };
    }

    public async Task<QuestionSheetGenerationResponseDto?> GenerateQuestionSheetAsync(Guid examId)
    {
        var data = await LoadExamQuestionDataAsync(examId);
        if (data is null)
        {
            return null;
        }

        var templateDto = BuildTemplateResponse(data);
        var payload = BuildScriptPayload(templateDto);
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var scriptOutput = await RunQuestionSheetScriptAsync(payloadJson, "questionSheet");
        var imageBase64 = ExtractImageBase64(scriptOutput);
        var fileName = await SaveSheetImageAsync(data.Exam, imageBase64, "question");

        data.Exam.QuestionSheetFileName = fileName;
        await _examRepository.UpdateAsync(data.Exam);

        return new QuestionSheetGenerationResponseDto
        {
            FileName = fileName,
            Url = BuildDocumentUrl(fileName)
        };
    }

    public async Task<QuestionSheetGenerationResponseDto?> GenerateAnswerSheetAsync(Guid examId)
    {
        var data = await LoadExamQuestionDataAsync(examId);
        if (data is null)
        {
            return null;
        }

        var payload = BuildScriptPayload(BuildTemplateResponse(data));
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var scriptOutput = await RunQuestionSheetScriptAsync(payloadJson, "answerSheet");
        var imageBase64 = ExtractImageBase64(scriptOutput);
        var fileName = await SaveSheetImageAsync(data.Exam, imageBase64, "answer");

        data.Exam.AnswerSheetFileName = fileName;
        await _examRepository.UpdateAsync(data.Exam);

        return new QuestionSheetGenerationResponseDto
        {
            FileName = fileName,
            Url = BuildDocumentUrl(fileName)
        };
    }

    public async Task<CalculateScoreResponseDto?> CalculateScoreAsync(
        Guid examId,
        string studentId,
        Stream sheetStream,
        string originalFileName)
    {
        if (sheetStream is null)
        {
            throw new ArgumentNullException(nameof(sheetStream));
        }

        var data = await LoadExamQuestionDataAsync(examId);
        if (data is null)
        {
            return null;
        }

        var savedPath = await SaveUploadedSheetAsync(data.Exam, studentId, sheetStream, originalFileName);
        var payload = BuildScriptPayload(BuildTemplateResponse(data));
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var scriptOutput = await RunQuestionSheetScriptAsync(payloadJson, "scoreCheck", studentId, savedPath);
        return ParseCalculateScoreResult(scriptOutput);
    }

    private async Task<ExamQuestionData?> LoadExamQuestionDataAsync(Guid examId)
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
        var questionIds = questions.Select(q => q.Id);
        var options = (await _questionOptionRepository.GetByQuestionsAsync(questionIds))
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return new ExamQuestionData(exam, questions, options, template);
    }

    private static QuestionSheetTemplateResponseDto BuildTemplateResponse(ExamQuestionData data)
    {
        return new QuestionSheetTemplateResponseDto
        {
            ExamId = data.Exam.Id,
            ExamName = data.Exam.Name,
            QuestionCount = data.Questions.Count,
            Template = new QuestionSheetTemplateMetadataDto
            {
                Name = data.Template?.Name ?? "Default"
            },
            Questions = data.Questions.Select(q =>
            {
                var options = data.Options.TryGetValue(q.Id, out var opts) ? opts : Array.Empty<QuestionOption>();
                return new QuestionSheetTemplateQuestionDto
                {
                    Id = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    OptionsPerQuestion = CalculateOptionsPerQuestion(options),
                    Text = q.Text,
                    CorrectOption = q.CorrectOption.ToString(),
                    Options = BuildOptions(options).ToArray()
                };
            }).ToArray()
        };
    }

    private static object BuildScriptPayload(QuestionSheetTemplateResponseDto template)
    {
        return new
        {
            examId = template.ExamId,
            examName = template.ExamName,
            questionCount = template.QuestionCount,
            template = new
            {
                name = template.Template.Name
            },
            questions = template.Questions.Select(q => new
            {
                id = q.Id,
                questionNumber = q.QuestionNumber,
                text = q.Text,
                options = q.Options
                    .OrderBy(o => o.Order)
                    .ToDictionary(o => o.Key, o => o.Text),
                correct = q.CorrectOption
            })
        };
    }

    private async Task<string> RunQuestionSheetScriptAsync(
        string payloadJson,
        string mode,
        string? studentId = null,
        string? scannedSheetPath = null)
    {
        var scriptPath = _generationOptions.ScriptPath;
        if (string.IsNullOrWhiteSpace(scriptPath))
        {
            throw new InvalidOperationException("Path to the question sheet generation script is not configured.");
        }

        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException("Question sheet script not found.", scriptPath);
        }

        var tempPayloadPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(tempPayloadPath, payloadJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        try
        {
            var startInfo = new ProcessStartInfo("python")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? Environment.CurrentDirectory
            };

            startInfo.ArgumentList.Add(scriptPath);
            startInfo.ArgumentList.Add("--mode");
            startInfo.ArgumentList.Add(mode);
            if (!string.IsNullOrWhiteSpace(studentId))
            {
                startInfo.ArgumentList.Add("--student-id");
                startInfo.ArgumentList.Add(studentId);
            }
            if (!string.IsNullOrWhiteSpace(scannedSheetPath))
            {
                startInfo.ArgumentList.Add("--scanned-sheet");
                startInfo.ArgumentList.Add(scannedSheetPath);
            }
            startInfo.ArgumentList.Add("--json");
            startInfo.ArgumentList.Add(tempPayloadPath);

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Question sheet script failed (exit code: {process.ExitCode}).\n{stderr.Trim()}");
            }

            return stdout;
        }
        finally
        {
            if (File.Exists(tempPayloadPath))
            {
                File.Delete(tempPayloadPath);
            }
        }
    }

    private static string ExtractImageBase64(string scriptOutput)
    {
        if (string.IsNullOrWhiteSpace(scriptOutput))
        {
            throw new InvalidOperationException("Question sheet script returned empty output.");
        }

        using var document = JsonDocument.Parse(scriptOutput);
        var root = document.RootElement;

        if (root.TryGetProperty("image_base64", out var imageElement) ||
            root.TryGetProperty("imageBase64", out imageElement))
        {
            var value = imageElement.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        throw new InvalidOperationException("Question sheet script response did not include an image_base64 payload.");
    }

    private async Task<string> SaveSheetImageAsync(Exam exam, string base64Value, string sheetLabel)
    {
        var bytes = Convert.FromBase64String(base64Value);
        var safeExamName = SanitizeFileName(exam.Name);
        var datedSuffix = DateTime.UtcNow.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        var labelPart = string.IsNullOrWhiteSpace(sheetLabel) ? string.Empty : $"_{sheetLabel}";
        var fileName = $"{safeExamName}{labelPart}_{datedSuffix}.png";
        var documentsFolder = _generationOptions.DocumentsFolder;

        if (string.IsNullOrWhiteSpace(documentsFolder))
        {
            throw new InvalidOperationException("Documents folder for question sheets is not configured.");
        }

        Directory.CreateDirectory(documentsFolder);
        var filePath = Path.Combine(documentsFolder, fileName);
        await File.WriteAllBytesAsync(filePath, bytes);
        return fileName;
    }

    private async Task<string> SaveUploadedSheetAsync(Exam exam, string studentId, Stream sheetStream, string originalFileName)
    {
        var documentsFolder = _generationOptions.DocumentsFolder;
        if (string.IsNullOrWhiteSpace(documentsFolder))
        {
            throw new InvalidOperationException("Documents folder for question sheets is not configured.");
        }

        Directory.CreateDirectory(documentsFolder);

        var safeExamName = SanitizeFileName(exam.Name);
        var safeStudent = string.IsNullOrWhiteSpace(studentId) ? "student" : SanitizeFileName(studentId);
        var datedSuffix = DateTime.UtcNow.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".png";
        }

        var fileName = $"{safeExamName}_jackscore_{safeStudent}_{datedSuffix}{extension}";
        var filePath = Path.Combine(documentsFolder, fileName);

        if (sheetStream.CanSeek)
        {
            sheetStream.Position = 0;
        }

        await using var targetStream = File.Create(filePath);
        await sheetStream.CopyToAsync(targetStream);
        await targetStream.FlushAsync();

        return filePath;
    }

    private static CalculateScoreResponseDto ParseCalculateScoreResult(string scriptOutput)
    {
        using var document = JsonDocument.Parse(scriptOutput);
        var root = document.RootElement;

        var response = new CalculateScoreResponseDto();
        if (root.TryGetProperty("exam", out var examElement) && examElement.ValueKind == JsonValueKind.Object)
        {
            response.ExamName = examElement.TryGetProperty("name", out var examName) ? examName.GetString() ?? string.Empty : string.Empty;
            if (examElement.TryGetProperty("id", out var examId) && Guid.TryParse(examId.GetString(), out var parsed))
            {
                response.ExamId = parsed;
            }
        }

        if (root.TryGetProperty("student_id", out var studentIdElement))
        {
            response.StudentId = studentIdElement.GetString() ?? string.Empty;
        }

        if (root.TryGetProperty("question_count", out var questionCountElement) && questionCountElement.TryGetInt32(out var questionCount))
        {
            response.QuestionCount = questionCount;
        }

        if (root.TryGetProperty("evaluation_error", out var evaluationError) && evaluationError.ValueKind == JsonValueKind.String)
        {
            response.EvaluationError = evaluationError.GetString();
        }

        if (root.TryGetProperty("evaluation", out var evaluationElement) && evaluationElement.ValueKind == JsonValueKind.Object)
        {
            if (evaluationElement.TryGetProperty("correct_count", out var correctCountElement) && correctCountElement.TryGetInt32(out var correctCount))
            {
                response.CorrectCount = correctCount;
            }
            if (evaluationElement.TryGetProperty("wrong_count", out var wrongCountElement) && wrongCountElement.TryGetInt32(out var wrongCount))
            {
                response.WrongCount = wrongCount;
            }
            if (evaluationElement.TryGetProperty("details", out var detailsElement) && detailsElement.ValueKind == JsonValueKind.Array)
            {
                var details = new List<CalculateScoreEvaluationDetailDto>();
                var detailIndex = 0;
                foreach (var detail in detailsElement.EnumerateArray())
                {
                    var questionNumber = 0;
                    if (detail.TryGetProperty("question_number", out var questionNumberElement) && questionNumberElement.TryGetInt32(out var questionNumberValue))
                    {
                        questionNumber = questionNumberValue;
                    }
                    else if (detail.TryGetProperty("question_id", out var questionIdElement) && questionIdElement.TryGetInt32(out var questionIdValue))
                    {
                        questionNumber = questionIdValue;
                    }
                    if (questionNumber == 0)
                    {
                        questionNumber = detailIndex + 1;
                    }

                        var detailDto = new CalculateScoreEvaluationDetailDto
                    {
                        QuestionNumber = questionNumber,
                        CorrectOption = detail.TryGetProperty("correct", out var correctOptionElement) ? correctOptionElement.GetString() ?? string.Empty : string.Empty,
                        SelectedOption = detail.TryGetProperty("detected", out var detectedElement) ? detectedElement.GetString() ?? string.Empty : string.Empty,
                        IsCorrect = detail.TryGetProperty("is_correct", out var isCorrectElement) && isCorrectElement.GetBoolean()
                    };

                    details.Add(detailDto);
                    detailIndex++;
                }

                response.Details = details;
            }
        }

        return response;
    }

    private string BuildDocumentUrl(string fileName)
    {
        var prefix = _generationOptions.DocumentsUrlPrefix?.TrimEnd('/') ?? string.Empty;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return fileName;
        }

        if (Uri.TryCreate(prefix, UriKind.Absolute, out var absolutePrefix))
        {
            return $"{absolutePrefix}/{fileName}";
        }

        if (!prefix.StartsWith("/"))
        {
            prefix = "/" + prefix;
        }

        return $"{prefix}/{fileName}";
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "question-sheet";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            builder.Append(Array.IndexOf(invalidChars, ch) >= 0 ? '_' : ch);
        }

        return builder.ToString().Replace(' ', '_');
    }

    private static IEnumerable<QuestionSheetTemplateOptionDto> BuildOptions(IEnumerable<QuestionOption> options)
    {
        return options
            .Select(o => new QuestionSheetTemplateOptionDto
            {
                Key = o.Key,
                Text = o.Text,
                Order = o.Order
            });
    }

    private static int CalculateOptionsPerQuestion(IEnumerable<QuestionOption> options)
    {
        return options.Count();
    }

    private static string BuildOptionLabelSet(int optionCount)
    {
        if (optionCount <= 0)
        {
            return string.Empty;
        }

        if (optionCount == 2)
        {
            return "TF";
        }

        const string labelSequence = "ABCDEF";
        return new string(labelSequence.Take(Math.Min(optionCount, labelSequence.Length)).ToArray());
    }

    private static DummyQuestionSheetQuestionOptionDto MapDummyOption(QuestionOption option)
        => new()
        {
            Key = option.Key,
            Text = option.Text
        };

    private sealed record ExamQuestionData(Exam Exam, List<Question> Questions, Dictionary<Guid, QuestionOption[]> Options, QuestionSheetTemplate? Template);
}
