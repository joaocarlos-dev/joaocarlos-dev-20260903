using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EmployeeManagementDbContext))]
[Migration("20260907183000_AddOutboxMessages")]
public partial class AddOutboxMessages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "outbox_messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                payload = table.Column<string>(type: "jsonb", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                claimed_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                claimed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                dead_lettered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                attempts = table.Column<int>(type: "integer", nullable: false),
                last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_outbox_messages", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_ProcessedAt_DeadLetteredAt_NextAttemptAt_ClaimedUntil_OccurredAt",
            table: "outbox_messages",
            columns: new[] { "processed_at", "dead_lettered_at", "next_attempt_at", "claimed_until", "occurred_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "outbox_messages");
}
