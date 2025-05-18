using ITTP.UsersManagement.API.DataAccess.Configurations;
using ITTP.UsersManagement.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITTP.UsersManagement.API.DataAccess;

public class UsersManagementDbContext(DbContextOptions<UsersManagementDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}