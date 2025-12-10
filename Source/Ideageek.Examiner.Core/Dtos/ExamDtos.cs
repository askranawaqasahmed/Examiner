namespace Ideageek.Examiner.Core.Dtos;

public class ExamDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int TotalMarks { get; set; }
    public int QuestionCount { get; set; }
    public DateTime? ExamDate { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
}

public class ExamRequestDto
{
    public Guid SchoolId { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int TotalMarks { get; set; }
    public int QuestionCount { get; set; }
    public DateTime? ExamDate { get; set; }
}
