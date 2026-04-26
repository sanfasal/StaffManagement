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
        var repository = new FakeStaffRepository
        {
            GetAllResult = new StaffPagedResponseDto
            {
                Items =
                [
                    new StaffDto
                    {
                        Id = 1,
                        StaffId = "ST001",
                        FullName = "Alice",
                        Gender = 2,
                    },
                ],
                TotalCount = 1,
            },
        };
        var controller = new StaffController(repository);
        var filter = new StaffFilterDto
        {
            StaffId = "ST001",
            Page = 2,
            PageSize = 5,
        };

        var result = await controller.GetAll(filter);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<StaffPagedResponseDto>(okResult.Value);
        Assert.Single(payload.Items);
        Assert.Equal(1, payload.TotalCount);
        Assert.NotNull(repository.GetAllFilter);
        Assert.Equal("ST001", repository.GetAllFilter.StaffId);
        Assert.Equal(2, repository.GetAllFilter.Page);
        Assert.Equal(5, repository.GetAllFilter.PageSize);
    }

    [Fact]
    public async Task Create_ReturnsOk_WhenRepositorySaveSucceeds()
    {
        var repository = new FakeStaffRepository
        {
            SaveResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Create(new CreateStaffDto
        {
            StaffId = "ST001",
            FullName = "Test User",
            BirthDay = new DateOnly(2000, 1, 1),
            Gender = 2,
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Staff created successfully.", okResult.Value);
        Assert.Equal(1, repository.SaveCallCount);
        Assert.Equal(2, repository.SavedStaff?.Gender);
    }

    [Fact]
    public async Task Create_DefaultsGenderToOne_WhenGenderIsNotProvided()
    {
        var repository = new FakeStaffRepository
        {
            SaveResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Create(new CreateStaffDto
        {
            StaffId = "ST002",
            FullName = "Default Gender User",
            BirthDay = new DateOnly(2001, 2, 3),
            Gender = null,
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Staff created successfully.", okResult.Value);
        Assert.Equal(1, repository.SavedStaff?.Gender);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenGenderIsInvalid()
    {
        var repository = new FakeStaffRepository();
        var controller = new StaffController(repository);

        var result = await controller.Create(new CreateStaffDto
        {
            StaffId = "ST003",
            FullName = "Rejected User",
            Gender = 9,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Gender must be 1 for male or 2 for female.", badRequest.Value);
        Assert.Equal(0, repository.SaveCallCount);
    }

    [Fact]
    public async Task Create_ReturnsOk_EvenWhenRepositoryReturnsFalse()
    {
        var repository = new FakeStaffRepository
        {
            SaveResult = false,
        };
        var controller = new StaffController(repository);

        var result = await controller.Create(new CreateStaffDto
        {
            StaffId = "ST004",
            FullName = "Still Ok User",
            Gender = 1,
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Staff created successfully.", okResult.Value);
        Assert.Equal(1, repository.SaveCallCount);
    }

    [Fact]
    public async Task Create_ReturnsBadRequestWithErrorPayload_WhenRepositoryThrows()
    {
        var repository = new FakeStaffRepository
        {
            SaveException = new InvalidOperationException("outer", new Exception("inner-db")),
        };
        var controller = new StaffController(repository);

        var result = await controller.Create(new CreateStaffDto
        {
            StaffId = "ST005",
            FullName = "Error User",
            Gender = 1,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);

        var valueType = badRequest.Value!.GetType();
        var error = valueType.GetProperty("error")?.GetValue(badRequest.Value) as string;
        var detail = valueType.GetProperty("detail")?.GetValue(badRequest.Value) as string;

        Assert.Equal("Unable to create staff record.", error);
        Assert.Equal("inner-db", detail);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRepositoryUpdatesRecord()
    {
        var repository = new FakeStaffRepository
        {
            UpdateResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Update(7, new StaffDto
        {
            StaffId = "ST007",
            FullName = "Updated User",
            Gender = 2,
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Updated successfully", okResult.Value);
        Assert.Equal(7, repository.UpdatedId);
        Assert.Equal(2, repository.UpdatedStaff?.Gender);
    }

    [Fact]
    public async Task Update_DefaultsGenderToOne_WhenGenderIsNotProvided()
    {
        var repository = new FakeStaffRepository
        {
            UpdateResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Update(8, new StaffDto
        {
            StaffId = "ST008",
            FullName = "Updated User",
            Gender = null,
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Updated successfully", okResult.Value);
        Assert.Equal(1, repository.UpdatedStaff?.Gender);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenGenderIsInvalid()
    {
        var repository = new FakeStaffRepository();
        var controller = new StaffController(repository);

        var result = await controller.Update(9, new StaffDto
        {
            FullName = "Updated User",
            Gender = 9,
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Gender must be 1 for male or 2 for female.", badRequest.Value);
        Assert.Null(repository.UpdatedStaff);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenRepositoryDoesNotFindRecord()
    {
        var repository = new FakeStaffRepository
        {
            UpdateResult = false,
        };
        var controller = new StaffController(repository);

        var result = await controller.Update(404, new StaffDto());

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, repository.UpdatedId);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenRepositoryDeletesRecord()
    {
        var repository = new FakeStaffRepository
        {
            DeleteResult = true,
        };
        var controller = new StaffController(repository);

        var result = await controller.Delete(5);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Deleted successfully", okResult.Value);
        Assert.Equal(5, repository.DeletedId);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenRepositoryDoesNotFindRecord()
    {
        var repository = new FakeStaffRepository
        {
            DeleteResult = false,
        };
        var controller = new StaffController(repository);

        var result = await controller.Delete(404);

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, repository.DeletedId);
    }

    private sealed class FakeStaffRepository : IStaffRepository
    {
        public StaffPagedResponseDto GetAllResult { get; set; } = new();

        public bool SaveResult { get; set; } = true;

        public bool UpdateResult { get; set; } = true;

        public bool DeleteResult { get; set; } = true;

        public Exception? SaveException { get; set; }

        public StaffFilterDto? GetAllFilter { get; private set; }

        public CreateStaffDto? SavedStaff { get; private set; }

        public int SaveCallCount { get; private set; }

        public int? UpdatedId { get; private set; }

        public StaffDto? UpdatedStaff { get; private set; }

        public int? DeletedId { get; private set; }

        public Task<bool> Delete(int id)
        {
            DeletedId = id;
            return Task.FromResult(DeleteResult);
        }

        public Task<StaffPagedResponseDto> GetAll(StaffFilterDto filter)
        {
            GetAllFilter = filter;
            return Task.FromResult(GetAllResult);
        }

        public Task<bool> Save(CreateStaffDto staff)
        {
            SaveCallCount++;
            SavedStaff = staff;

            if (SaveException is not null)
            {
                throw SaveException;
            }

            return Task.FromResult(SaveResult);
        }

        public Task<bool> Update(StaffDto staff, int id)
        {
            UpdatedId = id;
            UpdatedStaff = staff;
            return Task.FromResult(UpdateResult);
        }
    }
}
