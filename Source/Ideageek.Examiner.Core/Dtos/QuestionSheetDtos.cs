namespace Ideageek.Examiner.Core.Dtos;

public class DummyQuestionSheetResponseDto
{
    public string SheetCode { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public string Base64QrPng { get; set; } = string.Empty;
    public string QrPayload { get; set; } = string.Empty;
    public IReadOnlyCollection<DummyQuestionSheetQuestionDto> Questions { get; set; } = Array.Empty<DummyQuestionSheetQuestionDto>();
}

public class DummyQuestionSheetQuestionDto
{
    public int QuestionNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public IReadOnlyCollection<DummyQuestionSheetQuestionOptionDto> Options { get; set; } = Array.Empty<DummyQuestionSheetQuestionOptionDto>();
}

public class DummyQuestionSheetQuestionOptionDto
{
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class DummySheetEvaluationRequestDto
{
    public string SheetCode { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public Dictionary<int, string> Answers { get; set; } = new();
}

public class DummySheetEvaluationResponseDto
{
    public string SheetCode { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public int TotalMarks { get; set; }
    public int ObtainedMarks { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int BlankCount { get; set; }
    public IReadOnlyCollection<DummySheetEvaluationDetailDto> Details { get; set; } = Array.Empty<DummySheetEvaluationDetailDto>();
}

public class DummySheetEvaluationDetailDto
{
    public int QuestionNumber { get; set; }
    public string? SelectedOption { get; set; }
    public string CorrectOption { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Marks { get; set; }
}

public class QuestionSheetTemplateResponseDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public QuestionSheetTemplateMetadataDto Template { get; set; } = new();
    public IReadOnlyCollection<QuestionSheetTemplateQuestionDto> Questions { get; set; } = Array.Empty<QuestionSheetTemplateQuestionDto>();
}

public class QuestionSheetTemplateMetadataDto
{
    public string Name { get; set; } = "Default";
}

public class QuestionSheetTemplateQuestionDto
{
    public Guid Id { get; set; }
    public int QuestionNumber { get; set; }
    public int OptionsPerQuestion { get; set; }
    public string Text { get; set; } = string.Empty;
    public IReadOnlyCollection<QuestionSheetTemplateOptionDto> Options { get; set; } = Array.Empty<QuestionSheetTemplateOptionDto>();
    public string CorrectOption { get; set; } = string.Empty;
}

public class QuestionSheetTemplateOptionDto
{
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class QuestionSheetGenerationResponseDto
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class QuestionSheetAnswerResponseDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public IReadOnlyCollection<QuestionSheetAnswerQuestionDto> Questions { get; set; } = Array.Empty<QuestionSheetAnswerQuestionDto>();
}

public class QuestionSheetAnswerQuestionDto
{
    public int QuestionNumber { get; set; }
    public int OptionCount { get; set; }
    public string OptionLabelSet { get; set; } = string.Empty;
}

public class CalculateScoreResponseDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public string? EvaluationError { get; set; }
    public IReadOnlyCollection<CalculateScoreEvaluationDetailDto> Details { get; set; } = Array.Empty<CalculateScoreEvaluationDetailDto>();
}

public class CalculateScoreEvaluationDetailDto
{
    public int QuestionNumber { get; set; }
    public string CorrectOption { get; set; } = string.Empty;
    public string SelectedOption { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
