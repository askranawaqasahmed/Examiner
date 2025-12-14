namespace Ideageek.Examiner.Core.Options;

public class QuestionSheetGenerationOptions
{
    public string PythonPath { get; set; } = "python";
    public string ScriptPath { get; set; } = string.Empty;
    public string DocumentsFolder { get; set; } = string.Empty;
    public string DocumentsUrlPrefix { get; set; } = "/Documents/Exam";
}
