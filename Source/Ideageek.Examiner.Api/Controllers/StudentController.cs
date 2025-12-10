using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ideageek.Examiner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/students")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetAll()
        => Ok(await _studentService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id)
    {
        var student = await _studentService.GetByIdAsync(id);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpGet("by-class/{classId:guid}")]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetByClass(Guid classId)
        => Ok(await _studentService.GetByClassAsync(classId));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] StudentRequestDto request)
    {
        try
        {
            var id = await _studentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] StudentRequestDto request)
    {
        var updated = await _studentService.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
