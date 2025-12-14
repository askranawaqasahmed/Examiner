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
    public Task<ActionResult<QuestionSheetTemplateResponseDto>> GetTemplate(Guid examId)
        => HandleQuestionSheetTemplateRequestAsync(examId);

    [HttpGet("generate-question-sheet/{examId:guid}")]
    public async Task<ActionResult<QuestionSheetGenerationResponseDto>> GenerateQuestionSheet(Guid examId)
    {
        try
        {
            var result = await _questionSheetService.GenerateQuestionSheetAsync(examId);
            return BuildGenerationResponse(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse(ex);
        }
    }

    [HttpGet("generate-answer-sheet/{examId:guid}")]
    public async Task<ActionResult<QuestionSheetGenerationResponseDto>> GenerateAnswerSheet(Guid examId)
    {
        try
        {
            var result = await _questionSheetService.GenerateAnswerSheetAsync(examId);
            return BuildGenerationResponse(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse(ex);
        }
    }

    [HttpPost("{examId:guid}/calculate-score")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CalculateScoreResponseDto>> CalculateScore(
        Guid examId,
        [FromForm] string studentId,
        [FromForm] IFormFile? answerSheet)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest("Student ID is required.");
        }

        if (answerSheet is null || answerSheet.Length == 0)
        {
            return BadRequest("Answer sheet file is required.");
        }

        await using var sheetStream = answerSheet.OpenReadStream();
        try
        {
            var result = await _questionSheetService.CalculateScoreAsync(
                examId,
                studentId.Trim(),
                sheetStream,
                answerSheet.FileName);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse(ex);
        }
    }

    private ActionResult<QuestionSheetGenerationResponseDto> BuildGenerationResponse(QuestionSheetGenerationResponseDto? result)
    {
        if (result is null)
        {
            return NotFound();
        }

        result.Url = BuildAbsoluteDocumentUrl(result.Url, result.FileName);
        return Ok(result);
    }

    private ObjectResult BuildExceptionResponse(Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            error = ex.Message,
            exception = ex.ToString()
        });
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

    private async Task<ActionResult<QuestionSheetTemplateResponseDto>> HandleQuestionSheetTemplateRequestAsync(Guid examId)
    {
        try
        {
            var result = await _questionSheetService.GetTemplateAsync(examId);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex)
        {
            return BuildExceptionResponse(ex);
        }
    }
}
