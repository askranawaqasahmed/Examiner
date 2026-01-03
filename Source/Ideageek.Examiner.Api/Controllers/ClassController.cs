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
[Route("api/classes")]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClassDto>>>> GetAll()
    {
        var classes = (await _classService.GetAllAsync()).ToList();
        return this.ApiOk<IEnumerable<ClassDto>>(classes, count: classes.Count);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClassDto>>> GetById(Guid id)
    {
        var result = await _classService.GetByIdAsync(id);
        return result is null ? this.ApiNotFound<ClassDto>() : this.ApiOk(result);
    }

    [HttpGet("by-school/{schoolId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClassDto>>>> GetBySchool(Guid schoolId)
    {
        var classes = (await _classService.GetBySchoolAsync(schoolId)).ToList();
        return this.ApiOk<IEnumerable<ClassDto>>(classes, count: classes.Count);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object?>>> Create([FromBody] ClassRequestDto request)
    {
        await _classService.CreateAsync(request);
        return this.ApiOk<object?>(null);
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult<ApiResponse<object?>>> Update(Guid id, [FromBody] ClassRequestDto request)
    {
        var updated = await _classService.UpdateAsync(id, request);
        return updated ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var deleted = await _classService.DeleteAsync(id);
        return deleted ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }
}
