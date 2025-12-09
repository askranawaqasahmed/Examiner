namespace Ideageek.Examiner.Core.Dtos;

public class ClassDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
}

public class ClassRequestDto
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
}
