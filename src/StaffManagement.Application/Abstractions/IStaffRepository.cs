using StaffManagement.Application.Models;

namespace StaffManagement.Application.Abstractions;

public interface IStaffRepository
{
    Task<List<StaffDto>> GetAll();
    Task<bool> Save(StaffDto staff);
    Task<bool> Update(StaffDto staff, string id);
    Task<bool> Delete(string id);
    Task<List<StaffDto>> Search(string? staffId, int? gender, int? startYear, int? endYear);
}
