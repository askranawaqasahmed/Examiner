using Ideageek.Examiner.Core.Dtos;

namespace Ideageek.Examiner.Core.Services.Interfaces;

public interface IEvaluationService
{
    Task<DummySheetEvaluationResponseDto?> EvaluateDummySheetAsync(DummySheetEvaluationRequestDto request);
}
