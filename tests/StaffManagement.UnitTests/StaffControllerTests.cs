using Microsoft.AspNetCore.Mvc;
using StaffManagement.API.Controllers;
using StaffManagement.Application.Abstractions;
using StaffManagement.Application.Models;
using Xunit;

namespace StaffManagement.UnitTests;

public class StaffControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkWithRepositoryData()
    {
        var expected = new List<StaffDto>
        {
            new()
            {
                StaffId = "ST001",
                FullName = "Alice",
                Gender = 2,
            },
        };
        var repository = new FakeStaffRepository
        {
            GetAllResult = expected,
        };
        var controller = new StaffController(repository);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<List<StaffDto>>(okResult.Value);
        var staff = Assert.Single(payload);
        Assert.Equal("ST001", staff.StaffId);
    }

    [Fact]
    public async Task Create_ReturnsOkWithRepositoryResult()
    {
        var repository = new FakeStaffRepository
        {
            SaveResult = true,
        };
        var controller = new StaffController(repository);
        var request = new StaffDto
        {
            StaffId = "ST001",
            FullName = "Test User",
            Gender = 2,
        };

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<bool>(okResult.Value));
        Assert.Equal(request.StaffId, repository.SavedStaff?.StaffId);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenGenderIsInvalid()
    {
        var repository = new FakeStaffRepository();
        var controller = new StaffController(repository);

        var result = await controller.Create(new StaffDto
        {
            StaffId = "ST002",
            FullName = "Rejected User",
            Gender = 9,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Gender must be 1 for male or 2 for female.", badRequest.Value);
        Assert.Equal(0, repository.SaveCallCount);
    }

    [Fact]
    public async Task Create_DefaultsGenderToOne_WhenGenderIsNotProvided()
    {
        var repository = new FakeStaffRepository
        {
            SaveResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Create(new StaffDto
        {
            StaffId = "ST010",
            FullName = "Default Gender User",
            BirthDay = new DateOnly(2001, 2, 3),
            Gender = null,
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<bool>(okResult.Value));
        Assert.Equal(1, repository.SavedStaff?.Gender);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenStaffIdAlreadyExists()
    {
        var repository = new FakeStaffRepository
        {
            ExistsByIdResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Create(new StaffDto
        {
            StaffId = "ST001",
            FullName = "Duplicate User",
            Gender = 1,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("StaffId already exists.", badRequest.Value);
        Assert.Equal(0, repository.SaveCallCount);
        Assert.Equal("ST001", repository.ExistingIdLookup);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRepositoryRejectsCreate()
    {
        var repository = new FakeStaffRepository
        {
            SaveResult = false,
        };
        var controller = new StaffController(repository);

        var result = await controller.Create(new StaffDto
        {
            StaffId = "ST002",
            FullName = "Rejected User",
            BirthDay = new DateOnly(2000, 1, 1),
            Gender = 1,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Unable to create staff record.", badRequest.Value);
        Assert.Equal("ST002", repository.SavedStaff?.StaffId);
        Assert.Equal(new DateOnly(2000, 1, 1), repository.SavedStaff?.BirthDay);
    }


    [Fact]
    public async Task Update_ReturnsNotFound_WhenRepositoryDoesNotFindRecord()
    {
        var repository = new FakeStaffRepository
        {
            UpdateResult = false,
        };
        var controller = new StaffController(repository);

        var result = await controller.Update("ST404", new StaffDto());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRepositoryUpdatesRecord()
    {
        var repository = new FakeStaffRepository
        {
            UpdateResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Update("ST001", new StaffDto { FullName = "Updated User" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Updated successfully", okResult.Value);
        Assert.Equal("ST001", repository.UpdatedId);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenGenderIsInvalid()
    {
        var repository = new FakeStaffRepository();
        var controller = new StaffController(repository);

        var result = await controller.Update("ST001", new StaffDto
        {
            FullName = "Updated User",
            Gender = 9,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Gender must be 1 for male or 2 for female.", badRequest.Value);
        Assert.Null(repository.UpdatedId);
    }

    [Fact]
    public async Task Update_DefaultsGenderToOne_WhenGenderIsNotProvided()
    {
        var repository = new FakeStaffRepository
        {
            UpdateResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Update("ST001", new StaffDto
        {
            FullName = "Updated User",
            Gender = null,
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Updated successfully", okResult.Value);
        Assert.Equal("ST001", repository.UpdatedId);
        Assert.Equal(1, repository.UpdatedStaff?.Gender);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenRepositoryDoesNotFindRecord()
    {
        var repository = new FakeStaffRepository
        {
            DeleteResult = false,
        };
        var controller = new StaffController(repository);

        var result = await controller.Delete("ST404");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenRepositoryDeletesRecord()
    {
        var repository = new FakeStaffRepository
        {
            DeleteResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Delete("ST001");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Deleted successfully", okResult.Value);
        Assert.Equal("ST001", repository.DeletedId);
    }

    [Fact]
    public async Task Search_ReturnsOkWithRepositoryData()
    {
        var expected = new List<StaffDto>
        {
            new()
            {
                StaffId = "ST001",
                FullName = "Alice",
                Gender = 2,
            },
        };
        var repository = new FakeStaffRepository
        {
            SearchResult = expected,
        };
        var controller = new StaffController(repository);
        var filter = new StaffSearchFilterDto
        {
            StaffId = "ST001",
            Gender = 2,
            StartYear = 1990,
            EndYear = 2000,
        };

        var result = await controller.Search(filter);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<List<StaffDto>>(okResult.Value);
        var staff = Assert.Single(payload);
        Assert.Equal("ST001", staff.StaffId);
        Assert.NotNull(repository.SearchFilter);
        Assert.Equal("ST001", repository.SearchFilter.StaffId);
        Assert.Equal(2, repository.SearchFilter.Gender);
        Assert.Equal(1990, repository.SearchFilter.StartYear);
        Assert.Equal(2000, repository.SearchFilter.EndYear);
    }

    [Fact]
    public async Task Search_ForwardsNullFilters_ToRepository()
    {
        var repository = new FakeStaffRepository
        {
            SearchResult = [],
        };
        var controller = new StaffController(repository);

        var result = await controller.Search(new StaffSearchFilterDto());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<List<StaffDto>>(okResult.Value);
        Assert.NotNull(repository.SearchFilter);
        Assert.Null(repository.SearchFilter.StaffId);
        Assert.Null(repository.SearchFilter.Gender);
        Assert.Null(repository.SearchFilter.StartYear);
        Assert.Null(repository.SearchFilter.EndYear);
    }

    [Fact]
    public async Task Search_ReturnsBadRequest_WhenOnlyOneYearBoundaryIsProvided()
    {
        var repository = new FakeStaffRepository();
        var controller = new StaffController(repository);

        var result = await controller.Search(new StaffSearchFilterDto
        {
            StartYear = 1990,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("StartYear and EndYear must both be provided.", badRequest.Value);
        Assert.Null(repository.SearchFilter);
    }

    private sealed class FakeStaffRepository : IStaffRepository
    {
        public List<StaffDto> GetAllResult { get; set; } = [];

        public bool SaveResult { get; set; }

        public bool ExistsByIdResult { get; set; }

        public bool UpdateResult { get; set; } = true;

        public bool DeleteResult { get; set; } = true;

        public List<StaffDto> SearchResult { get; set; } = [];

        public StaffDto? SavedStaff { get; private set; }

        public int SaveCallCount { get; private set; }

        public string? UpdatedId { get; private set; }

        public StaffDto? UpdatedStaff { get; private set; }

        public string? DeletedId { get; private set; }

        public StaffSearchFilterDto? SearchFilter { get; private set; }

        public string? ExistingIdLookup { get; private set; }

        public Task<bool> Delete(string id)
        {
            DeletedId = id;
            return Task.FromResult(DeleteResult);
        }

        public Task<List<StaffDto>> GetAll()
        {
            return Task.FromResult(GetAllResult);
        }

        public Task<bool> ExistsById(string id)
        {
            ExistingIdLookup = id;
            return Task.FromResult(ExistsByIdResult);
        }

        public Task<bool> Save(StaffDto staff)
        {
            SaveCallCount++;
            SavedStaff = staff;
            return Task.FromResult(SaveResult);
        }

        public Task<List<StaffDto>> Search(StaffSearchFilterDto filter)
        {
            SearchFilter = filter;
            return Task.FromResult(SearchResult);
        }

        public Task<bool> Update(StaffDto staff, string id)
        {
            UpdatedId = id;
            UpdatedStaff = staff;
            return Task.FromResult(UpdateResult);
        }
    }
}
