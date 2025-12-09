using System.Linq;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services.Interfaces;

namespace Ideageek.Examiner.Core.Services;

public class AnswerSheetService : IAnswerSheetService
{
    private readonly IAnswerSheetRepository _answerSheetRepository;

    public AnswerSheetService(IAnswerSheetRepository answerSheetRepository)
    {
        _answerSheetRepository = answerSheetRepository;
    }

    public async Task<AnswerSheet> CreateAsync(Guid examId, Guid studentId, string studentNumber, string sheetCode)
    {
        var sheet = new AnswerSheet
        {
            Id = Guid.NewGuid(),
            ExamId = examId,
            StudentId = studentId,
            StudentNumber = studentNumber,
            SheetCode = sheetCode,
            GeneratedAt = DateTime.UtcNow
        };

        await _answerSheetRepository.InsertAsync(sheet);
        return sheet;
    }

    public Task<AnswerSheet?> GetBySheetCodeAsync(string sheetCode, string studentNumber)
        => _answerSheetRepository.GetBySheetCodeAsync(sheetCode, studentNumber);

    public async Task<bool> SaveEvaluationAsync(Guid answerSheetId, IEnumerable<AnswerSheetDetail> details, int totalMarks, int correctCount, int wrongCount, int blankCount)
    {
        var detailList = details.Select(d =>
        {
            d.AnswerSheetId = answerSheetId;
            if (d.Id == Guid.Empty)
            {
                d.Id = Guid.NewGuid();
            }

            return d;
        }).ToList();

        await _answerSheetRepository.DeleteDetailsAsync(answerSheetId);
        await _answerSheetRepository.InsertDetailsAsync(detailList);
        return await _answerSheetRepository.UpdateSummaryAsync(answerSheetId, totalMarks, correctCount, wrongCount, blankCount);
    }
}
