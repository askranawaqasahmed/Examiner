using Ideageek.Examiner.Core.Enums;

namespace Ideageek.Examiner.Core.Entities;

public class Exam : IEntity
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int TotalMarks { get; set; }
    public int QuestionCount { get; set; }
    public DateTime? ExamDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ExamType Type { get; set; } = ExamType.Mcq;
    public string? QuestionSheetFileName { get; set; }
    public string? AnswerSheetFileName { get; set; }
}
