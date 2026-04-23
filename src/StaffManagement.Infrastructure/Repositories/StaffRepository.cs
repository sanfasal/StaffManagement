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

    public async Task<bool> Delete(string id)
    {
        var staff = await _context.Staff
            .FirstOrDefaultAsync(s => s.StaffId == id);

        if (staff is null)
        {
            return false;
        }

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<StaffDto>> GetAll()
    {
        return await _context.Staff
            .Select(s => new StaffDto
            {
                StaffId = s.StaffId,
                FullName = s.FullName,
                BirthDay = s.BirthDay,
                Gender = s.Gender,
            })
            .ToListAsync();
    }

    public async Task<bool> ExistsById(string id)
    {
        return await _context.Staff.AnyAsync(s => s.StaffId == id);
    }

    public async Task<bool> Save(StaffDto staff)
    {
        staff.Gender ??= 1;

        if (staff.Gender is not 1 and not 2)
        {
            return false;
        }

        if (await ExistsById(staff.StaffId))
        {
            return false;
        }

        var entity = new Staff
        {
            StaffId = staff.StaffId,
            FullName = staff.FullName,
            BirthDay = staff.BirthDay,
            Gender = staff.Gender,
        };

        await _context.Staff.AddAsync(entity);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<StaffDto>> Search(StaffSearchFilterDto filter)
    {
        var query = _context.Staff.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.StaffId))
        {
            query = query.Where(s => s.StaffId == filter.StaffId);
        }

        if (filter.Gender.HasValue)
        {
            query = query.Where(s => s.Gender == filter.Gender.Value);
        }

        if (filter.StartYear.HasValue && filter.EndYear.HasValue)
        {
            query = query.Where(s =>
                s.BirthDay.HasValue &&
                s.BirthDay.Value.Year >= filter.StartYear.Value &&
                s.BirthDay.Value.Year <= filter.EndYear.Value);
        }

        return await query
            .Select(s => new StaffDto
            {
                StaffId = s.StaffId,
                FullName = s.FullName,
                BirthDay = s.BirthDay,
                Gender = s.Gender,
            })
            .ToListAsync();
    }

    public async Task<bool> Update(StaffDto staff, string id)
    {
        staff.Gender ??= 1;

        if (staff.Gender is not 1 and not 2)
        {
            return false;
        }

        var existing = await _context.Staff
            .FirstOrDefaultAsync(s => s.StaffId == id);

        if (existing is null)
        {
            return false;
        }

        existing.FullName = staff.FullName;
        existing.BirthDay = staff.BirthDay;
        existing.Gender = staff.Gender;

        await _context.SaveChangesAsync();

        return true;
    }
}
