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
                    { ErrorMessage = "запрещены все символы кроме латинских букв и цифр" });
        
        builder.HasIndex(u => u.Login)
            .IsUnique();

        builder.Property(u => u.Password)
            .IsRequired()
            .HasAnnotation(
                "RegularExpression",
                new RegularExpressionAttribute(@"^[a-zA-Z0-9]+$")
                    { ErrorMessage = "запрещены все символы кроме латинских букв и цифр" });

        builder.Property(u => u.Name)
            .IsRequired()
            .HasAnnotation(
                "RegularExpression",
                new RegularExpressionAttribute(@"^[a-zA-Zа-яА-Я]+$")
                    { ErrorMessage = "запрещены все символы кроме латинских букв и цифр" });

        builder.Property(u => u.Gender)
            .IsRequired()
            .HasAnnotation("Range", new RangeAttribute(0, 2));

        builder.Property(u => u.Birthday)
            .IsRequired(false);

        builder.Property(u => u.Admin)
            .IsRequired();

        builder.Property(u => u.CreatedOn)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .IsRequired();

        builder.Property(u => u.ModifiedOn)
            .IsRequired();

        builder.Property(u => u.ModifiedBy)
            .IsRequired();
    }
}