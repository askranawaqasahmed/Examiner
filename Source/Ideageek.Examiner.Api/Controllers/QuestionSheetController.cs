using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<QuestionSheetTemplateResponseDto>> GetTemplate(Guid examId)
    {
        var result = await _questionSheetService.GetTemplateAsync(examId);
        return result is null ? NotFound() : Ok(result);
    }
}
