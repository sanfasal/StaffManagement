namespace StaffManagement.Application.Models;

public class StaffFilterDto
{
    public string? StaffId { get; set; }
    public int? Gender { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
