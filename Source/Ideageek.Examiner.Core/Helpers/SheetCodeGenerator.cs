namespace Ideageek.Examiner.Core.Helpers;

public static class SheetCodeGenerator
{
    public static string BuildSheetCode(Guid examId, string studentNumber)
    {
        var examFragment = examId.ToString("N").Substring(0, 6).ToUpperInvariant();
        return $"EX-{examFragment}-{studentNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}
