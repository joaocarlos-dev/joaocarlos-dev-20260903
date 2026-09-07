
using System;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(EmployeeManagementDbContext))]
    partial class EmployeeManagementDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "citext");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EmployeeManagmentSystem.Domain.Entities.Employee", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("code");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)")
                        .HasColumnName("name");

                    b.Property<Guid>("UnitId")
                        .HasColumnType("uuid")
                        .HasColumnName("unit_id");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasDatabaseName("ux_employees_code");

                    b.HasIndex("UnitId")
                        .HasDatabaseName("ix_employees_unit_id");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasDatabaseName("ux_employees_user_id");

                    b.ToTable("employees", (string)null);
                });

            modelBuilder.Entity("EmployeeManagmentSystem.Domain.Entities.Unit", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("code");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)")
                        .HasColumnName("name");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasDatabaseName("ux_units_code");

                    b.ToTable("units", (string)null);
                });

            modelBuilder.Entity("EmployeeManagmentSystem.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("code");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("citext")
                        .HasColumnName("login");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("password_hash");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<int>("Role")
                        .HasColumnType("integer")
                        .HasColumnName("role");

                    b.Property<int>("SecurityVersion")
                        .HasColumnType("integer")
                        .HasColumnName("security_version");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasDatabaseName("ux_users_code");

                    b.HasIndex("Login")
                        .IsUnique()
                        .HasDatabaseName("ux_users_login");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("EmployeeManagmentSystem.Infrastructure.Persistence.LoginRateLimitBucket", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(320)
                        .HasColumnType("character varying(320)")
                        .HasColumnName("key");

                    b.Property<int>("AttemptCount")
                        .HasColumnType("integer")
                        .HasColumnName("attempt_count");

                    b.Property<DateTimeOffset>("WindowStarted")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("window_started");

                    b.HasKey("Key");

                    b.HasIndex("WindowStarted")
                        .HasDatabaseName("ix_login_rate_limit_buckets_window_started");

                    b.ToTable("login_rate_limit_buckets", (string)null);
                });

            modelBuilder.Entity("EmployeeManagmentSystem.Application.Common.Events.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("Attempts")
                        .HasColumnType("integer")
                        .HasColumnName("attempts");

                    b.Property<DateTimeOffset>("OccurredAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_at");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("payload");

                    b.Property<DateTimeOffset?>("ProcessedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_at");

                    b.Property<DateTimeOffset?>("NextAttemptAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("next_attempt_at");

                    b.Property<DateTimeOffset?>("ClaimedUntil")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("claimed_until");

                    b.Property<string>("ClaimedBy")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("claimed_by");

                    b.Property<DateTimeOffset?>("DeadLetteredAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("dead_lettered_at");

                    b.Property<string>("LastError")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("last_error");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("ProcessedAt", "DeadLetteredAt", "NextAttemptAt", "ClaimedUntil", "OccurredAt")
                        .HasDatabaseName("IX_outbox_messages_ProcessedAt_DeadLetteredAt_NextAttemptAt_ClaimedUntil_OccurredAt");

                    b.ToTable("outbox_messages", (string)null);
                });

            modelBuilder.Entity("EmployeeManagmentSystem.Domain.Entities.Employee", b =>
                {
                    b.HasOne("EmployeeManagmentSystem.Domain.Entities.Unit", null)
                        .WithMany()
                        .HasForeignKey("UnitId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("EmployeeManagmentSystem.Domain.Entities.User", null)
                        .WithOne()
                        .HasForeignKey("EmployeeManagmentSystem.Domain.Entities.Employee", "UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
