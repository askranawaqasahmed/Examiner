using System.Linq;
using Ideageek.Examiner.Api.Helpers;
using Ideageek.Examiner.Api.Models;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ideageek.Examiner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/exams")]
public class ExamController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExamDto>>>> GetAll()
    {
        var exams = (await _examService.GetAllAsync()).ToList();
        return this.ApiOk<IEnumerable<ExamDto>>(exams, count: exams.Count);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ExamDto>>> GetById(Guid id)
    {
        var exam = await _examService.GetByIdAsync(id);
        return exam is null ? this.ApiNotFound<ExamDto>() : this.ApiOk(exam);
    }

    [HttpGet("by-class/{classId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExamDto>>>> GetByClass(Guid classId)
    {
        var exams = (await _examService.GetByClassAsync(classId)).ToList();
        return this.ApiOk<IEnumerable<ExamDto>>(exams, count: exams.Count);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object?>>> Create([FromBody] ExamRequestDto request)
    {
        await _examService.CreateAsync(request);
        return this.ApiOk<object?>(null);
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult<ApiResponse<object?>>> Update(Guid id, [FromBody] ExamRequestDto request)
    {
        var updated = await _examService.UpdateAsync(id, request);
        return updated ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var deleted = await _examService.DeleteAsync(id);
        return deleted ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }
}
