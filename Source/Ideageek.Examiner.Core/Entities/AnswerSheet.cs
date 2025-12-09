namespace Ideageek.Examiner.Core.Entities;

public class AnswerSheet : IEntity
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string SheetCode { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime? ScannedAt { get; set; }
    public int? TotalMarks { get; set; }
    public int? CorrectCount { get; set; }
    public int? WrongCount { get; set; }
    public int? BlankCount { get; set; }
}
