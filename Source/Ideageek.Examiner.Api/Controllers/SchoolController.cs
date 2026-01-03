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
[Route("api/schools")]
public class SchoolController : ControllerBase
{
    private readonly ISchoolService _schoolService;

    public SchoolController(ISchoolService schoolService)
    {
        _schoolService = schoolService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SchoolDto>>>> GetAll()
    {
        var schools = (await _schoolService.GetAllAsync()).ToList();
        return this.ApiOk<IEnumerable<SchoolDto>>(schools, count: schools.Count);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SchoolDto>>> GetById(Guid id)
    {
        var school = await _schoolService.GetByIdAsync(id);
        return school is null ? this.ApiNotFound<SchoolDto>() : this.ApiOk(school);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object?>>> Create([FromBody] SchoolRequestDto request)
    {
        await _schoolService.CreateAsync(request);
        return this.ApiOk<object?>(null);
    }

    [HttpPost("{id:guid}/update")]
    public async Task<ActionResult<ApiResponse<object?>>> Update(Guid id, [FromBody] SchoolRequestDto request)
    {
        var updated = await _schoolService.UpdateAsync(id, request);
        return updated ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var deleted = await _schoolService.DeleteAsync(id);
        return deleted ? this.ApiOk<object?>(null) : this.ApiNotFound<object?>();
    }
}
