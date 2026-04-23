namespace StaffManagement.Application.Models;

public class StaffSearchFilterDto
{
    public string? StaffId { get; set; }

    public int? Gender { get; set; }

    public int? StartYear { get; set; }

    public int? EndYear { get; set; }
}
