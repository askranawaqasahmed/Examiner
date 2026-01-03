using Ideageek.Examiner.Api.Helpers;
using Ideageek.Examiner.Api.Models;
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
    public async Task<ActionResult<ApiResponse<IEnumerable<QuestionDto>>>> GetByExam(Guid examId)
    {
        var questions = (await _questionService.GetByExamAsync(examId)).ToList();
        return this.ApiOk<IEnumerable<QuestionDto>>(questions, count: questions.Count);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object?>>> Create([FromBody] QuestionRequestDto request)
    {
        await _questionService.CreateAsync(request);
        return this.ApiOk<object?>(null);
    }

    [HttpPost("update")]
    public async Task<ActionResult<ApiResponse<object?>>> Update([FromBody] QuestionUpdateRequestDto request)
    {
        var updated = await _questionService.UpdateAsync(request);
        if (!updated)
        {
            return this.ApiNotFound<object?>();
        }

        return this.ApiOk<object?>(null);
    }

    [HttpPost("delete/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var deleted = await _questionService.DeleteAsync(id);
        return deleted ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }
}
