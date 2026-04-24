using Microsoft.EntityFrameworkCore;
using StaffManagement.Application.Models;
using StaffManagement.Domain.Entities;
using StaffManagement.Infrastructure.Data;
using StaffManagement.Infrastructure.Repositories;
using Xunit;

namespace StaffManagement.UnitTests;

public class StaffRepositoryTests
{
    [Fact]
    public async Task GetAll_ReturnsMappedStaffDtos()
    {
        await using var context = CreateContext();
        context.Staff.Add(new Staff
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 2,
            BirthDay = new DateOnly(1998, 5, 12),
        });
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);

        var result = await repository.GetAll();

        var staff = Assert.Single(result);
        Assert.Equal("ST001", staff.StaffId);
        Assert.Equal("Alice", staff.FullName);
        Assert.Equal(2, staff.Gender);
    }

    [Fact]
    public async Task Save_ReturnsFalse_WhenGenderIsOutsideSupportedRange()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Save(new StaffDto
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 9,
        });

        Assert.False(result);
        Assert.Empty(context.Staff);
    }

    [Fact]
    public async Task Save_DefaultsGenderToOne_WhenGenderIsNull()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Save(new StaffDto
        {
            StaffId = "ST002",
            FullName = "Bob",
            BirthDay = new DateOnly(2000, 1, 1),
            Gender = null,
        });

        Assert.True(result);

        var saved = await context.Staff.SingleAsync();
        Assert.Equal(1, saved.Gender);
    }

    [Fact]
    public async Task Save_CreatesStaff_WhenGenderIsOne()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Save(new StaffDto
        {
            StaffId = "ST003",
            FullName = "Carol",
            Gender = 1,
        });

        Assert.True(result);

        var saved = await context.Staff.SingleAsync();
        Assert.Equal(1, saved.Gender);
    }

    [Fact]
    public async Task Save_CreatesStaff_WhenGenderIsTwo()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Save(new StaffDto
        {
            StaffId = "ST004",
            FullName = "Dana",
            Gender = 2,
        });

        Assert.True(result);

        var saved = await context.Staff.SingleAsync();
        Assert.Equal(2, saved.Gender);
    }

    [Fact]
    public async Task ExistsById_ReturnsTrue_WhenStaffExists()
    {
        await using var context = CreateContext();
        context.Staff.Add(new Staff
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 2,
        });
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);

        var result = await repository.ExistsById("ST001");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsById_ReturnsFalse_WhenStaffDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.ExistsById("ST404");

        Assert.False(result);
    }

    [Fact]
    public async Task Save_ReturnsFalse_WhenStaffIdAlreadyExists()
    {
        await using var context = CreateContext();
        context.Staff.Add(new Staff
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 2,
        });
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);

        var result = await repository.Save(new StaffDto
        {
            StaffId = "ST001",
            FullName = "Duplicate Alice",
            Gender = 1,
        });

        Assert.False(result);
        Assert.Equal(1, await context.Staff.CountAsync());
    }

    [Fact]
    public async Task Delete_RemovesExistingStaff_AndReturnsTrue()
    {
        await using var context = CreateContext();
        context.Staff.Add(new Staff
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 2,
        });
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);

        var result = await repository.Delete("ST001");

        Assert.True(result);
        Assert.Empty(context.Staff);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_WhenStaffDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Delete("ST404");

        Assert.False(result);
    }

    [Fact]
    public async Task Search_AppliesAllProvidedFilters()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);

        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto
        {
            StaffId = "ST001",
            Gender = 2,
            StartYear = 1990,
            EndYear = 2000,
        });

        var match = Assert.Single(result);
        Assert.Equal("ST001", match.StaffId);
    }

    [Fact]
    public async Task Search_ReturnsAllStaff_WhenNoFiltersAreProvided()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);
        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto());

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task Search_FiltersByExactStaffId_WhenOnlyStaffIdIsProvided()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);
        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto
        {
            StaffId = "AB002",
        });

        var match = Assert.Single(result);
        Assert.Equal("AB002", match.StaffId);
    }

    [Fact]
    public async Task Search_IgnoresWhitespaceStaffId_Filter()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);
        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto
        {
            StaffId = "   ",
            Gender = 2,
        });

        Assert.Equal(2, result.Count);
        Assert.All(result, staff => Assert.Equal(2, staff.Gender));
    }

    [Fact]
    public async Task Search_ExcludesStaffWithoutBirthDay_WhenYearFiltersAreUsed()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);
        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto
        {
            StartYear = 1990,
            EndYear = 2005,
        });

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, staff => staff.StaffId == "ST004");
    }

    [Fact]
    public async Task Search_DoesNotApplyYearFilter_WhenOnlyStartYearIsProvided()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);
        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto
        {
            StartYear = 1998,
        });

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task Search_DoesNotApplyYearFilter_WhenOnlyEndYearIsProvided()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);
        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto
        {
            EndYear = 1998,
        });

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task Search_UsesInclusiveYearBoundaries()
    {
        await using var context = CreateContext();
        await SeedSearchData(context);
        var repository = new StaffRepository(context);

        var result = await repository.Search(new StaffSearchFilterDto
        {
            StartYear = 1998,
            EndYear = 2001,
        });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, staff => staff.StaffId == "ST001");
        Assert.Contains(result, staff => staff.StaffId == "AB002");
    }

    [Fact]
    public async Task Update_ChangesExistingStaff_AndReturnsTrue()
    {
        await using var context = CreateContext();
        context.Staff.Add(new Staff
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 2,
            BirthDay = new DateOnly(1998, 5, 12),
        });
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);

        var result = await repository.Update(
            new StaffDto
            {
                FullName = "Alice Updated",
                Gender = 1,
                BirthDay = new DateOnly(1999, 6, 10),
            },
            "ST001");

        Assert.True(result);

        var updated = await context.Staff.SingleAsync();
        Assert.Equal("Alice Updated", updated.FullName);
        Assert.Equal(1, updated.Gender);
        Assert.Equal(new DateOnly(1999, 6, 10), updated.BirthDay);
    }

    [Fact]
    public async Task Update_ReturnsFalse_WhenStaffDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Update(
            new StaffDto
            {
                FullName = "Missing",
            },
            "ST404");

        Assert.False(result);
    }

    [Fact]
    public async Task Update_DefaultsGenderToOne_WhenGenderIsNull()
    {
        await using var context = CreateContext();
        context.Staff.Add(new Staff
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 2,
            BirthDay = new DateOnly(1998, 5, 12),
        });
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);

        var result = await repository.Update(
            new StaffDto
            {
                FullName = "Alice Updated",
                Gender = null,
                BirthDay = null,
            },
            "ST001");

        Assert.True(result);

        var updated = await context.Staff.SingleAsync();
        Assert.Equal(1, updated.Gender);
        Assert.Null(updated.BirthDay);
        Assert.Equal("ST001", updated.StaffId);
    }

    [Fact]
    public async Task Update_ReturnsFalse_WhenGenderIsOutsideSupportedRange()
    {
        await using var context = CreateContext();
        context.Staff.Add(new Staff
        {
            StaffId = "ST001",
            FullName = "Alice",
            Gender = 2,
        });
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);

        var result = await repository.Update(
            new StaffDto
            {
                FullName = "Alice Updated",
                Gender = 9,
            },
            "ST001");

        Assert.False(result);

        var unchanged = await context.Staff.SingleAsync();
        Assert.Equal(2, unchanged.Gender);
        Assert.Equal("Alice", unchanged.FullName);
    }

    private static StaffManagementContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StaffManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StaffManagementContext(options);
    }

    private static async Task SeedSearchData(StaffManagementContext context)
    {
        context.Staff.AddRange(
            new Staff
            {
                StaffId = "ST001",
                FullName = "Alice",
                Gender = 2,
                BirthDay = new DateOnly(1998, 5, 12),
            },
            new Staff
            {
                StaffId = "AB002",
                FullName = "Bob",
                Gender = 1,
                BirthDay = new DateOnly(2001, 3, 4),
            },
            new Staff
            {
                StaffId = "ST003",
                FullName = "Carol",
                Gender = 2,
                BirthDay = new DateOnly(1988, 7, 20),
            },
            new Staff
            {
                StaffId = "ST004",
                FullName = "Dana",
                Gender = 1,
                BirthDay = null,
            });

        await context.SaveChangesAsync();
    }
}
