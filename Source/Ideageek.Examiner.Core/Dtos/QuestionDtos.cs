using Ideageek.Examiner.Core.Enums;

namespace Ideageek.Examiner.Core.Dtos;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int QuestionNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public char CorrectOption { get; set; }
    public QuestionType Type { get; set; }
    public int? Lines { get; set; }
    public int? Marks { get; set; }
    public DiagramBoxSize? BoxSize { get; set; }
    public IReadOnlyCollection<QuestionOptionDto> Options { get; set; } = Array.Empty<QuestionOptionDto>();
}

public class QuestionRequestDto
{
    public Guid ExamId { get; set; }
    public int QuestionNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public char CorrectOption { get; set; }
    public QuestionType Type { get; set; } = QuestionType.Mcq;
    public int? Lines { get; set; }
    public int? Marks { get; set; }
    public DiagramBoxSize? BoxSize { get; set; }
    public IEnumerable<QuestionOptionDto> Options { get; set; } = Array.Empty<QuestionOptionDto>();
}

public class QuestionUpdateRequestDto : QuestionRequestDto
{
    public Guid Id { get; set; }
}

public class QuestionOptionDto
{
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}
