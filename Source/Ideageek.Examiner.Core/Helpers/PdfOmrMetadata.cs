namespace Ideageek.Examiner.Core.Helpers;

public class PdfOmrMetadata
{
    public string? SheetCode { get; set; }
    public string? StudentNumber { get; set; }
    public int? QuestionCount { get; set; }
    public Guid? ExamId { get; set; }
}
