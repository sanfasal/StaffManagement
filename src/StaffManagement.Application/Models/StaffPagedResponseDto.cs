using System.Collections.Generic;

namespace StaffManagement.Application.Models;

public class StaffPagedResponseDto
{
    public List<StaffDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
