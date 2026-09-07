using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Configurations;

internal sealed class LoginRateLimitBucketConfiguration : IEntityTypeConfiguration<LoginRateLimitBucket>
{
    public void Configure(EntityTypeBuilder<LoginRateLimitBucket> builder)
    {
        builder.ToTable("login_rate_limit_buckets");
        builder.HasKey(bucket => bucket.Key);
        builder.Property(bucket => bucket.Key).HasColumnName("key").HasMaxLength(320);
        builder.Property(bucket => bucket.WindowStarted).HasColumnName("window_started").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(bucket => bucket.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.HasIndex(bucket => bucket.WindowStarted)
            .HasDatabaseName("ix_login_rate_limit_buckets_window_started");
    }
}
