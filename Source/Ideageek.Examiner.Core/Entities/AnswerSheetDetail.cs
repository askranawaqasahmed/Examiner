namespace Ideageek.Examiner.Core.Entities;

public class AnswerSheetDetail : IEntity
{
    public Guid Id { get; set; }
    public Guid AnswerSheetId { get; set; }
    public Guid QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public char? SelectedOption { get; set; }
    public bool? IsCorrect { get; set; }
    public int? Marks { get; set; }
}
