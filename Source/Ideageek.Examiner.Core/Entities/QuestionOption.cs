namespace Ideageek.Examiner.Core.Entities;

public class QuestionOption : IEntity
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}
