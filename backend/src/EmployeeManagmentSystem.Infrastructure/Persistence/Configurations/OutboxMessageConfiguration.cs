using EmployeeManagmentSystem.Application.Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(message => message.Type).HasColumnName("type").HasMaxLength(200).IsRequired();
        builder.Property(message => message.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.OccurredAt).HasColumnName("occurred_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(message => message.ProcessedAt).HasColumnName("processed_at").HasColumnType("timestamp with time zone");
        builder.Property(message => message.NextAttemptAt).HasColumnName("next_attempt_at").HasColumnType("timestamp with time zone");
        builder.Property(message => message.ClaimedUntil).HasColumnName("claimed_until").HasColumnType("timestamp with time zone");
        builder.Property(message => message.ClaimedBy).HasColumnName("claimed_by").HasMaxLength(100);
        builder.Property(message => message.DeadLetteredAt).HasColumnName("dead_lettered_at").HasColumnType("timestamp with time zone");
        builder.Property(message => message.Attempts).HasColumnName("attempts").IsRequired();
        builder.Property(message => message.LastError).HasColumnName("last_error").HasMaxLength(2000);
        builder.HasIndex(message => new { message.ProcessedAt, message.DeadLetteredAt, message.NextAttemptAt, message.ClaimedUntil, message.OccurredAt });
    }
}
