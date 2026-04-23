using Microsoft.AspNetCore.Mvc;
using StaffManagement.Application.Abstractions;
using StaffManagement.Application.Models;

namespace StaffManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StaffController : ControllerBase
{
    private readonly IStaffRepository _staffRepository;

    public StaffController(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _staffRepository.GetAll();
        return Ok(staff);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] StaffDto model)
    {
        model.Gender ??= 1;

        if (model.Gender is not 1 and not 2)
        {
            return BadRequest("Gender must be 1 for male or 2 for female.");
        }

        if (await _staffRepository.ExistsById(model.StaffId))
        {
            return BadRequest("StaffId already exists.");
        }

        var result = await _staffRepository.Save(model);

        if (!result)
        {
            return BadRequest("Unable to create staff record.");
        }

        return Ok(result);
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] StaffDto model)
    {
        model.Gender ??= 1;

        if (model.Gender is not 1 and not 2)
        {
            return BadRequest("Gender must be 1 for male or 2 for female.");
        }

        var result = await _staffRepository.Update(model, id);

        if (!result)
        {
            return NotFound();
        }

        return Ok("Updated successfully");
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _staffRepository.Delete(id);

        if (!result)
        {
            return NotFound();
        }

        return Ok("Deleted successfully");
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search([FromQuery] StaffSearchFilterDto filter)
    {
        if (filter.StartYear.HasValue != filter.EndYear.HasValue)
        {
            return BadRequest("StartYear and EndYear must both be provided.");
        }

        var result = await _staffRepository.Search(filter);
        return Ok(result);
    }
}
