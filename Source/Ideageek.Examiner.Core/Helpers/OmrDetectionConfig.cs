namespace Ideageek.Examiner.Core.Helpers;

public static class OmrDetectionConfig
{
    public const double FillIntensityThreshold = 0.25;
    public const double IntensityDeltaThreshold = 0.05;
    public const double SampleRadiusFactor = 0.75;
    public const double InnerHoleFactor = 0.0;
    public static readonly string[] OptionLetters = { "A", "B", "C", "D" };
}
