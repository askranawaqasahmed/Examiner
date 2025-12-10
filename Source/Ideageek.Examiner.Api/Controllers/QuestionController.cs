using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ideageek.Examiner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/questions")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet("by-exam/{examId:guid}")]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetByExam(Guid examId)
        => Ok(await _questionService.GetByExamAsync(examId));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] QuestionRequestDto request)
    {
        var id = await _questionService.CreateAsync(request);
        return Created(string.Empty, new { id });
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] QuestionRequestDto request)
    {
        var updated = await _questionService.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _questionService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
