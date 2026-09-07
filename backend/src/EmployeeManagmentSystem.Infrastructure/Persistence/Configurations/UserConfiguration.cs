using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(user => user.Code).HasColumnName("code").HasMaxLength(EntityFieldLengths.Code).IsRequired();
        builder.Property(user => user.Login).HasColumnName("login").HasColumnType("citext").HasMaxLength(EntityFieldLengths.Login).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(EntityFieldLengths.PasswordHash).IsRequired();
        builder.Property(user => user.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(user => user.Role).HasColumnName("role").HasConversion<int>().IsRequired();
        builder.Property(user => user.SecurityVersion).HasColumnName("security_version").IsRequired();
        builder.Property(user => user.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(user => user.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(user => user.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");
        builder.Ignore(user => user.IsActive);
        builder.Ignore(user => user.IsDeleted);
        builder.HasIndex(user => user.Code).IsUnique().HasDatabaseName("ux_users_code");
        builder.HasIndex(user => user.Login).IsUnique().HasDatabaseName("ux_users_login");
    }
}
