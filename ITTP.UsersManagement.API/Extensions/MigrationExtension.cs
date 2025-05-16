using ITTP.UsersManagement.API.DataAccess;
using ITTP.UsersManagement.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITTP.UsersManagement.API.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        try
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();

            using UsersManagementDbContext context = serviceScope.ServiceProvider.GetService<UsersManagementDbContext>();
            
            IConfiguration? configuration = serviceScope.ServiceProvider.GetService<IConfiguration>();
            string? connectionString = configuration.GetConnectionString("UsersManagementDbContext");
            Console.WriteLine($"Connection string: {connectionString}");

            context.Database.EnsureCreated();
            context.Database.Migrate();
            
            // Seed Admin user if not exists
            if (!context.Users.Any(u => u.Login == "Admin"))
            {
                context.Users.Add(new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Login = "admin",
                    Password = "admin",
                    Name = "Administrator",
                    Gender = 2,
                    Admin = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "System",
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedBy = "System",
                    RevokedBy = ""
                });
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while applying migrations: {ex.Message}");
            throw;
        }
    }
}