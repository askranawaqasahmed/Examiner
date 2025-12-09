using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface IAnswerSheetRepository : IRepository<AnswerSheet>
{
    Task<AnswerSheet?> GetBySheetCodeAsync(string sheetCode, string studentNumber);
    Task<bool> InsertDetailsAsync(IEnumerable<AnswerSheetDetail> details);
    Task<bool> DeleteDetailsAsync(Guid answerSheetId);
    Task<bool> UpdateSummaryAsync(Guid answerSheetId, int totalMarks, int correctCount, int wrongCount, int blankCount);
}
