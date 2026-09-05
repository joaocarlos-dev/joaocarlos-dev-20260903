using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Configurations;

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("units");
        builder.HasKey(unit => unit.Id);
        builder.Property(unit => unit.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(unit => unit.Code).HasColumnName("code").HasMaxLength(EntityFieldLengths.Code).IsRequired();
        builder.Property(unit => unit.Name).HasColumnName("name").HasMaxLength(EntityFieldLengths.Name).IsRequired();
        builder.Property(unit => unit.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(unit => unit.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(unit => unit.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(unit => unit.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");
        builder.Ignore(unit => unit.IsActive);
        builder.Ignore(unit => unit.IsDeleted);
        builder.HasIndex(unit => unit.Code).IsUnique().HasDatabaseName("ux_units_code");
    }
}
