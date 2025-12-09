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
    public DummyQuestionSheetQuestionOptionsDto Options { get; set; } = new();
}

public class DummyQuestionSheetQuestionOptionsDto
{
    public string A { get; set; } = string.Empty;
    public string B { get; set; } = string.Empty;
    public string C { get; set; } = string.Empty;
    public string D { get; set; } = string.Empty;
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
