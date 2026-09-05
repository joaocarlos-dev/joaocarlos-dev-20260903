using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(employee => employee.Id);
        builder.Property(employee => employee.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(employee => employee.Code).HasColumnName("code").HasMaxLength(EntityFieldLengths.Code).IsRequired();
        builder.Property(employee => employee.Name).HasColumnName("name").HasMaxLength(EntityFieldLengths.Name).IsRequired();
        builder.Property(employee => employee.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(employee => employee.UnitId).HasColumnName("unit_id").IsRequired();
        builder.Property(employee => employee.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(employee => employee.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(employee => employee.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");
        builder.Ignore(employee => employee.IsDeleted);
        builder.HasIndex(employee => employee.Code).IsUnique().HasDatabaseName("ux_employees_code");
        builder.HasIndex(employee => employee.UserId).IsUnique().HasDatabaseName("ux_employees_user_id");
        builder.HasIndex(employee => employee.UnitId).HasDatabaseName("ix_employees_unit_id");
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Employee>(employee => employee.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(employee => employee.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(employee => employee.DeletedAt == null);
    }
}
