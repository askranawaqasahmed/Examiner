using Ideageek.Examiner.Core.Entities;

namespace Ideageek.Examiner.Core.Repositories.Interfaces;

public interface IQuestionSheetTemplateRepository : IRepository<QuestionSheetTemplate>
{
    Task<QuestionSheetTemplate?> GetDefaultByExamAsync(Guid examId);
}
