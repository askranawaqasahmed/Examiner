using Ideageek.Examiner.Api.Helpers;
using Ideageek.Examiner.Api.Models;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Ideageek.Examiner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/question-sheets")]
public class QuestionSheetController : ControllerBase
{
    private readonly IQuestionSheetService _questionSheetService;

    public QuestionSheetController(IQuestionSheetService questionSheetService)
    {
        _questionSheetService = questionSheetService;
    }

    /// <summary>
    /// Returns template-style data for an exam containing questions, options, correct answers, and metadata.
    /// Intended to be consumed by external tooling (e.g., Python image generation).
    /// </summary>
    [HttpGet("template/{examId:guid}")]
    public Task<ActionResult<ApiResponse<QuestionSheetTemplateResponseDto>>> GetTemplate(Guid examId)
        => HandleQuestionSheetTemplateRequestAsync(examId);

    [HttpGet("generate-question-sheet/{examId:guid}")]
    public async Task<ActionResult<ApiResponse<QuestionSheetGenerationResponseDto>>> GenerateQuestionSheet(Guid examId)
    {
        try
        {
            var result = await _questionSheetService.GenerateQuestionSheetAsync(examId);
            return BuildGenerationResponse(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse<QuestionSheetGenerationResponseDto>(ex);
        }
    }

    [HttpGet("generate-answer-sheet/{examId:guid}")]
    public async Task<ActionResult<ApiResponse<QuestionSheetGenerationResponseDto>>> GenerateAnswerSheet(Guid examId)
    {
        try
        {
            var result = await _questionSheetService.GenerateAnswerSheetAsync(examId);
            return BuildGenerationResponse(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse<QuestionSheetGenerationResponseDto>(ex);
        }
    }

    [HttpPost("{examId:guid}/calculate-score")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CalculateScoreResponseDto>>> CalculateScore(
        Guid examId,
        [FromForm] string studentId,
        [FromForm] IFormFile? answerSheet)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return this.ApiBadRequest<CalculateScoreResponseDto>("Student ID is required.");
        }

        if (answerSheet is null || answerSheet.Length == 0)
        {
            return this.ApiBadRequest<CalculateScoreResponseDto>("Answer sheet file is required.");
        }

        await using var sheetStream = answerSheet.OpenReadStream();
        try
        {
            var result = await _questionSheetService.CalculateScoreAsync(
                examId,
                studentId.Trim(),
                sheetStream,
                answerSheet.FileName);
            return result is null ? this.ApiNotFound<CalculateScoreResponseDto>() : this.ApiOk(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse<CalculateScoreResponseDto>(ex);
        }
    }

    private ActionResult<ApiResponse<QuestionSheetGenerationResponseDto>> BuildGenerationResponse(QuestionSheetGenerationResponseDto? result)
    {
        if (result is null)
        {
            return this.ApiNotFound<QuestionSheetGenerationResponseDto>();
        }

        result.Url = BuildAbsoluteDocumentUrl(result.Url, result.FileName);
        return this.ApiOk(result);
    }

    private ActionResult<ApiResponse<T>> BuildExceptionResponse<T>(Exception ex)
    {
        return this.ApiServerError<T>(ex.Message);
    }

    private string BuildAbsoluteDocumentUrl(string? url, string fileName)
    {
        var candidate = string.IsNullOrWhiteSpace(url) ? fileName : url;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(candidate, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        var request = HttpContext.Request;
        var baseUri = new Uri($"{request.Scheme}://{request.Host.ToUriComponent()}{request.PathBase}");
        var normalizedPath = candidate.StartsWith("/") ? candidate : "/" + candidate;

        return new Uri(baseUri, normalizedPath).ToString();
    }

    private async Task<ActionResult<ApiResponse<QuestionSheetTemplateResponseDto>>> HandleQuestionSheetTemplateRequestAsync(Guid examId)
    {
        try
        {
            var result = await _questionSheetService.GetTemplateAsync(examId);
            return result is null ? this.ApiNotFound<QuestionSheetTemplateResponseDto>() : this.ApiOk(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse<QuestionSheetTemplateResponseDto>(ex);
        }
    }
}
