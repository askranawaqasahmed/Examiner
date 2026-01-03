using Ideageek.Examiner.Core.Enums;

namespace Ideageek.Examiner.Core.Entities;

public class Question : IEntity
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int QuestionNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public char CorrectOption { get; set; }
    public QuestionType Type { get; set; } = QuestionType.Mcq;
    public int? Lines { get; set; }
    public int? Marks { get; set; }
}
