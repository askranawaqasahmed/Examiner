namespace Ideageek.Examiner.Core.Entities;

public class ClassEntity : IEntity
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public DateTime CreatedAt { get; set; }
}
