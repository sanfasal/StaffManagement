namespace StaffManagement.Domain.Entities;

public class Staff
{
    public int Id { get; set; }
    public string? StaffId { get; set; }

    public string? FullName { get; set; }

    public DateOnly? BirthDay { get; set; }

    public int? Gender { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
