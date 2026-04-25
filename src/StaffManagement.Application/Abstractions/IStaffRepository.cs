using StaffManagement.Application.Models;

namespace StaffManagement.Application.Abstractions;

public interface IStaffRepository
{
    Task<StaffPagedResponseDto> GetAll(StaffFilterDto filter);
    Task<bool> Save(CreateStaffDto staff);
    Task<bool> Update(StaffDto staff, int id);
    Task<bool> Delete(int id);
}
