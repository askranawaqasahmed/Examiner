using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ideageek.Examiner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/classes")]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClassDto>>> GetAll()
        => Ok(await _classService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassDto>> GetById(Guid id)
    {
        var result = await _classService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-school/{schoolId:guid}")]
    public async Task<ActionResult<IEnumerable<ClassDto>>> GetBySchool(Guid schoolId)
        => Ok(await _classService.GetBySchoolAsync(schoolId));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ClassRequestDto request)
    {
        var id = await _classService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] ClassRequestDto request)
    {
        var updated = await _classService.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _classService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
