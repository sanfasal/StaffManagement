using Microsoft.EntityFrameworkCore;
using StaffManagement.Application.Abstractions;
using StaffManagement.Application.Models;
using StaffManagement.Domain.Entities;
using StaffManagement.Infrastructure.Data;

namespace StaffManagement.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly StaffManagementContext _context;

    public StaffRepository(StaffManagementContext context)
    {
        _context = context;
    }

    public async Task<bool> Delete(int id)
    {
        var staff = await _context.Staff
            .FirstOrDefaultAsync(s => s.Id == id);

        if (staff is null)
        {
            return false;
        }

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<StaffDto>> GetAll(StaffFilterDto filter)
    {
        var query = _context.Staff.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.StaffId))
        {
            query = query.Where(s => s.StaffId == filter.StaffId);
        }

        if (filter.Gender.HasValue)
        {
            query = query.Where(s => s.Gender == filter.Gender);
        }

        if (filter.StartYear.HasValue || filter.EndYear.HasValue)
        {
            query = query.Where(s => s.BirthDay.HasValue);

            if (filter.StartYear.HasValue)
            {
                query = query.Where(s => s.BirthDay!.Value.Year >= filter.StartYear.Value);
            }

            if (filter.EndYear.HasValue)
            {
                query = query.Where(s => s.BirthDay!.Value.Year <= filter.EndYear.Value);
            }
        }

        // Apply pagination
        var skip = (filter.Page - 1) * filter.PageSize;

        return await query
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(s => new StaffDto
            {
                Id = s.Id,
                StaffId = s.StaffId,
                FullName = s.FullName,
                BirthDay = s.BirthDay,
                Gender = s.Gender,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            })
            .ToListAsync();
    }

    public async Task<bool> Save(CreateStaffDto staff)
    {
        var entity = new Staff
        {
            StaffId = staff.StaffId,
            FullName = staff.FullName,
            BirthDay = staff.BirthDay,
            Gender = staff.Gender ?? 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.Staff.Add(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Update(StaffDto staff, int id)
    {
        var existing = await _context.Staff
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existing is null)
        {
            return false;
        }

        existing.StaffId = staff.StaffId;
        existing.FullName = staff.FullName;
        existing.BirthDay = staff.BirthDay;
        existing.Gender = staff.Gender;
        existing.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
