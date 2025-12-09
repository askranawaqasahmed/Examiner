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
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetAll()
        => Ok(await _examService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExamDto>> GetById(Guid id)
    {
        var exam = await _examService.GetByIdAsync(id);
        return exam is null ? NotFound() : Ok(exam);
    }

    [HttpGet("by-class/{classId:guid}")]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetByClass(Guid classId)
        => Ok(await _examService.GetByClassAsync(classId));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ExamRequestDto request)
    {
        var id = await _examService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] ExamRequestDto request)
    {
        var updated = await _examService.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _examService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
