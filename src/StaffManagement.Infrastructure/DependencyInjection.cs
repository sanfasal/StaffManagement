using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StaffManagement.Application.Abstractions;
using StaffManagement.Infrastructure.Data;
using StaffManagement.Infrastructure.Repositories;

namespace StaffManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StaffManagementContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CoffeeDbConnection")));
        services.AddDistributedMemoryCache();
        services.AddScoped<IStaffRepository, StaffRepository>();

        return services;
    }
}
