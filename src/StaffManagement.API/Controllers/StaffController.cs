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
    public async Task<IActionResult> GetAll([FromQuery] StaffFilterDto filter)
    {
        var staff = await _staffRepository.GetAll(filter);
        return Ok(staff);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CreateStaffDto model)
    {
        model.Gender ??= 1;

        if (model.Gender is not 1 and not 2)
        {
            return BadRequest("Gender must be 1 for male or 2 for female.");
        }

        try
        {
            var result = await _staffRepository.Save(model);
            return Ok("Staff created successfully.");
        }
        catch (Exception ex)
        {
            // Returns the real error so we can diagnose the root cause
            return BadRequest(new { error = "Unable to create staff record.", detail = ex.InnerException?.Message ?? ex.Message });
        }
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] StaffDto model)
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
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _staffRepository.Delete(id);

        if (!result)
        {
            return NotFound();
        }

        return Ok("Deleted successfully");
    }
}
