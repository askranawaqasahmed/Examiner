using System.IO;
using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IQuestionSheetService
{
    Task<DummyQuestionSheetResponseDto?> GenerateDummySheetAsync(Guid examId, string studentNumber);
    Task<QuestionSheetTemplateResponseDto?> GetTemplateAsync(Guid examId);
    Task<QuestionSheetAnswerResponseDto?> GetAnswerSheetAsync(Guid examId);
    Task<QuestionSheetGenerationResponseDto?> GenerateQuestionSheetAsync(Guid examId);
    Task<QuestionSheetGenerationResponseDto?> GenerateAnswerSheetAsync(Guid examId);
    Task<CalculateScoreResponseDto?> CalculateScoreAsync(Guid examId, string studentId, Stream sheetStream, string originalFileName);
}
