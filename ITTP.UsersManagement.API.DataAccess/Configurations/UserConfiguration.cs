using System.ComponentModel.DataAnnotations;
using ITTP.UsersManagement.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITTP.UsersManagement.API.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(u => u.Login)
            .IsRequired()
            .HasAnnotation(
                "RegularExpression",
                new RegularExpressionAttribute(@"^[a-zA-Z0-9]+$")
                    { ErrorMessage = "can only contain Latin letters and numbers" });
        
        builder.HasIndex(u => u.Login)
            .IsUnique();

        builder.Property(u => u.Password)
            .IsRequired()
            .HasAnnotation(
                "RegularExpression",
                new RegularExpressionAttribute(@"^[a-zA-Z0-9]+$")
                    { ErrorMessage = "can only contain Latin letters and numbers" });

        builder.Property(u => u.Name)
            .IsRequired()
            .HasAnnotation(
                "RegularExpression",
                new RegularExpressionAttribute(@"^[a-zA-Z0-9]+$")
                    { ErrorMessage = "can only contain Latin letters and numbers" });

        builder.Property(u => u.Gender)
            .IsRequired()
            .HasAnnotation("Range", new RangeAttribute(0, 2));

        builder.Property(u => u.Admin)
            .IsRequired();
    }
}