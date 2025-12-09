namespace Ideageek.Examiner.Core.Entities;

public class QuestionSheetTemplate : IEntity
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
