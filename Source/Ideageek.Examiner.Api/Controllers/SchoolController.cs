using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ideageek.Examiner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/schools")]
public class SchoolController : ControllerBase
{
    private readonly ISchoolService _schoolService;

    public SchoolController(ISchoolService schoolService)
    {
        _schoolService = schoolService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SchoolDto>>> GetAll()
        => Ok(await _schoolService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SchoolDto>> GetById(Guid id)
    {
        var school = await _schoolService.GetByIdAsync(id);
        return school is null ? NotFound() : Ok(school);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] SchoolRequestDto request)
    {
        var id = await _schoolService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] SchoolRequestDto request)
    {
        var updated = await _schoolService.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _schoolService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
