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
    public async Task GetAll_ReturnsPagedResult_WithTotalCount()
    {
        await using var context = CreateContext();
        await SeedData(context);
        var repository = new StaffRepository(context);

        var result = await repository.GetAll(new StaffFilterDto
        {
            Page = 1,
            PageSize = 10,
        });

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(4, result.Items.Count);
    }

    [Fact]
    public async Task GetAll_FiltersByStaffId()
    {
        await using var context = CreateContext();
        await SeedData(context);
        var repository = new StaffRepository(context);

        var result = await repository.GetAll(new StaffFilterDto
        {
            StaffId = "AB002",
            Page = 1,
            PageSize = 10,
        });

        var match = Assert.Single(result.Items);
        Assert.Equal("AB002", match.StaffId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetAll_IgnoresWhitespaceStaffIdFilter()
    {
        await using var context = CreateContext();
        await SeedData(context);
        var repository = new StaffRepository(context);

        var result = await repository.GetAll(new StaffFilterDto
        {
            StaffId = "   ",
            Page = 1,
            PageSize = 10,
        });

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(4, result.Items.Count);
    }

    [Fact]
    public async Task GetAll_FiltersByGender()
    {
        await using var context = CreateContext();
        await SeedData(context);
        var repository = new StaffRepository(context);

        var result = await repository.GetAll(new StaffFilterDto
        {
            Gender = 2,
            Page = 1,
            PageSize = 10,
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, staff => Assert.Equal(2, staff.Gender));
    }

    [Fact]
    public async Task GetAll_FiltersByYearRange_IncludingBoundaries()
    {
        await using var context = CreateContext();
        await SeedData(context);
        var repository = new StaffRepository(context);

        var result = await repository.GetAll(new StaffFilterDto
        {
            StartYear = 1998,
            EndYear = 2001,
            Page = 1,
            PageSize = 10,
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, staff => staff.StaffId == "ST001");
        Assert.Contains(result.Items, staff => staff.StaffId == "AB002");
    }

    [Fact]
    public async Task GetAll_ExcludesRecordsWithoutBirthDay_WhenYearFilterProvided()
    {
        await using var context = CreateContext();
        await SeedData(context);
        var repository = new StaffRepository(context);

        var result = await repository.GetAll(new StaffFilterDto
        {
            StartYear = 1990,
            Page = 1,
            PageSize = 10,
        });

        Assert.Equal(2, result.TotalCount);
        Assert.DoesNotContain(result.Items, staff => staff.StaffId == "ST004");
    }

    [Fact]
    public async Task GetAll_AppliesPagination_AfterFiltering()
    {
        await using var context = CreateContext();
        await SeedData(context);
        var repository = new StaffRepository(context);

        var result = await repository.GetAll(new StaffFilterDto
        {
            Page = 2,
            PageSize = 2,
        });

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Save_CreatesStaff_AndDefaultsGenderToOne_WhenGenderIsNull()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = await repository.Save(new CreateStaffDto
        {
            StaffId = "ST010",
            FullName = "New User",
            BirthDay = new DateOnly(2000, 1, 1),
            Gender = null,
        });
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(result);
        var saved = await context.Staff.SingleAsync();
        Assert.Equal("ST010", saved.StaffId);
        Assert.Equal("New User", saved.FullName);
        Assert.Equal(new DateOnly(2000, 1, 1), saved.BirthDay);
        Assert.Equal(1, saved.Gender);
        Assert.True(saved.CreatedDate >= before && saved.CreatedDate <= after);
    }

    [Fact]
    public async Task Save_UsesProvidedGender()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Save(new CreateStaffDto
        {
            StaffId = "ST011",
            FullName = "Gender Two",
            Gender = 2,
        });

        Assert.True(result);
        var saved = await context.Staff.SingleAsync();
        Assert.Equal(2, saved.Gender);
    }

    [Fact]
    public async Task Update_ReturnsFalse_WhenRecordDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Update(new StaffDto
        {
            StaffId = "MISSING",
            FullName = "Missing User",
            Gender = 1,
        }, 999);

        Assert.False(result);
    }

    [Fact]
    public async Task Update_ChangesFields_AndSetsUpdatedDate()
    {
        await using var context = CreateContext();
        var existing = new Staff
        {
            StaffId = "ST020",
            FullName = "Original",
            BirthDay = new DateOnly(1998, 5, 12),
            Gender = 2,
            CreatedDate = DateTime.UtcNow.AddDays(-1),
        };
        context.Staff.Add(existing);
        await context.SaveChangesAsync();

        var repository = new StaffRepository(context);
        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = await repository.Update(new StaffDto
        {
            StaffId = "ST020-UPD",
            FullName = "Updated Name",
            BirthDay = null,
            Gender = null,
        }, existing.Id);
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(result);
        var updated = await context.Staff.SingleAsync();
        Assert.Equal("ST020-UPD", updated.StaffId);
        Assert.Equal("Updated Name", updated.FullName);
        Assert.Null(updated.BirthDay);
        Assert.Null(updated.Gender);
        Assert.True(updated.UpdatedDate >= before && updated.UpdatedDate <= after);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_WhenRecordDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new StaffRepository(context);

        var result = await repository.Delete(12345);

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_RemovesRecord_WhenIdExists()
    {
        await using var context = CreateContext();
        var existing = new Staff
        {
            StaffId = "ST030",
            FullName = "Delete Me",
            Gender = 1,
        };
        context.Staff.Add(existing);
        await context.SaveChangesAsync();
        var repository = new StaffRepository(context);

        var result = await repository.Delete(existing.Id);

        Assert.True(result);
        Assert.Empty(context.Staff);
    }

    private static StaffManagementContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StaffManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StaffManagementContext(options);
    }

    private static async Task SeedData(StaffManagementContext context)
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
