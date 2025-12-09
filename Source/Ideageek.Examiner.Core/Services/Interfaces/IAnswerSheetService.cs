using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IAnswerSheetService
{
    Task<AnswerSheet> CreateAsync(Guid examId, Guid studentId, string studentNumber, string sheetCode);
    Task<AnswerSheet?> GetBySheetCodeAsync(string sheetCode, string studentNumber);
    Task<bool> SaveEvaluationAsync(Guid answerSheetId, IEnumerable<AnswerSheetDetail> details, int totalMarks, int correctCount, int wrongCount, int blankCount);
}
