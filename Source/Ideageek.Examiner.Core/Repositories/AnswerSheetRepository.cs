using System.Collections.Generic;
using System.Linq;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class AnswerSheetRepository : SqlKataRepository<AnswerSheet>, IAnswerSheetRepository
{
    private const string DetailTable = "AnswerSheetDetail";

    public AnswerSheetRepository(QueryFactory queryFactory) : base(queryFactory, "AnswerSheet")
    {
    }

    public async Task<AnswerSheet?> GetBySheetCodeAsync(string sheetCode, string studentNumber)
    {
        var entity = await QueryFactory.Query(TableName)
            .Where("SheetCode", sheetCode)
            .Where("StudentNumber", studentNumber)
            .FirstOrDefaultAsync<AnswerSheet>();

        return entity;
    }

    public async Task<bool> InsertDetailsAsync(IEnumerable<AnswerSheetDetail> details)
    {
        var payload = details.ToList();
        if (payload.Count == 0)
        {
            return true;
        }

        var rows = payload.Select(detail =>
        {
            if (detail.Id == Guid.Empty)
            {
                detail.Id = Guid.NewGuid();
            }

            return new Dictionary<string, object?>
            {
                ["Id"] = detail.Id,
                ["AnswerSheetId"] = detail.AnswerSheetId,
                ["QuestionId"] = detail.QuestionId,
                ["QuestionNumber"] = detail.QuestionNumber,
                ["SelectedOption"] = detail.SelectedOption,
                ["IsCorrect"] = detail.IsCorrect,
                ["Marks"] = detail.Marks
            };
        }).ToList();

        var affected = 0;
        foreach (var row in rows)
        {
            affected += await QueryFactory.Query(DetailTable).InsertAsync(row);
        }

        return affected > 0;
    }

    public async Task<bool> DeleteDetailsAsync(Guid answerSheetId)
    {
        var affected = await QueryFactory.Query(DetailTable).Where("AnswerSheetId", answerSheetId).DeleteAsync();
        return affected > 0;
    }

    public async Task<bool> UpdateSummaryAsync(Guid answerSheetId, int totalMarks, int correctCount, int wrongCount, int blankCount)
    {
        var affected = await QueryFactory.Query(TableName)
            .Where("Id", answerSheetId)
            .UpdateAsync(new
            {
                TotalMarks = totalMarks,
                CorrectCount = correctCount,
                WrongCount = wrongCount,
                BlankCount = blankCount,
                ScannedAt = DateTime.UtcNow
            });

        return affected > 0;
    }
}
