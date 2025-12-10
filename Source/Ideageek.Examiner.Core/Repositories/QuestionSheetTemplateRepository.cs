using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public class QuestionSheetTemplateRepository : SqlKataRepository<QuestionSheetTemplate>, IQuestionSheetTemplateRepository
{
    public QuestionSheetTemplateRepository(QueryFactory queryFactory) : base(queryFactory, "QuestionSheetTemplate")
    {
    }

    public async Task<QuestionSheetTemplate?> GetDefaultByExamAsync(Guid examId)
    {
        var entity = await QueryFactory.Query(TableName)
            .Where("ExamId", examId)
            .Where("IsDefault", true)
            .FirstOrDefaultAsync<QuestionSheetTemplate>();

        return entity;
    }
}
