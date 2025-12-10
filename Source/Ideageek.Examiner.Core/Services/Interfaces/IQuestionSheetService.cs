using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IQuestionSheetService
{
    Task<DummyQuestionSheetResponseDto?> GenerateDummySheetAsync(Guid examId, string studentNumber);
    Task<QuestionSheetTemplateResponseDto?> GetTemplateAsync(Guid examId);
}
