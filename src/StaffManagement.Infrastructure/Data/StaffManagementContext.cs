using Microsoft.EntityFrameworkCore;
using StaffManagement.Domain.Entities;

namespace StaffManagement.Infrastructure.Data;

public class StaffManagementContext : DbContext
{
    public StaffManagementContext(DbContextOptions<StaffManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Staff> Staff { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId);
            entity.Property(e => e.StaffId).HasMaxLength(8);
            entity.Property(e => e.FullName).HasMaxLength(100);
        });
    }
}
