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
[Route("api/students")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentDto>>>> GetAll()
    {
        var students = (await _studentService.GetAllAsync()).ToList();
        return this.ApiOk<IEnumerable<StudentDto>>(students, count: students.Count);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(Guid id)
    {
        var student = await _studentService.GetByIdAsync(id);
        return student is null ? this.ApiNotFound<StudentDto>() : this.ApiOk(student);
    }

    [HttpGet("by-class/{classId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentDto>>>> GetByClass(Guid classId)
    {
        var students = (await _studentService.GetByClassAsync(classId)).ToList();
        return this.ApiOk<IEnumerable<StudentDto>>(students, count: students.Count);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object?>>> Create([FromBody] StudentRequestDto request)
    {
        try
        {
            await _studentService.CreateAsync(request);
            return this.ApiOk<object?>(null);
        }
        catch (InvalidOperationException ex)
        {
            return this.ApiBadRequest<object?>(ex.Message);
        }
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult<ApiResponse<object?>>> Update(Guid id, [FromBody] StudentRequestDto request)
    {
        var updated = await _studentService.UpdateAsync(id, request);
        return updated ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        return deleted ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }
}
