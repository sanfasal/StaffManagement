namespace StaffManagement.Application.Models;

public class StaffDto
{
    public string StaffId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DateOnly? BirthDay { get; set; }

    public int? Gender { get; set; } 
}
