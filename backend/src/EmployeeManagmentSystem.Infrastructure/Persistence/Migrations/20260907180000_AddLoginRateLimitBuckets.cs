using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EmployeeManagementDbContext))]
[Migration("20260907180000_AddLoginRateLimitBuckets")]
public partial class AddLoginRateLimitBuckets : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "login_rate_limit_buckets",
            columns: table => new
            {
                key = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                window_started = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                attempt_count = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_login_rate_limit_buckets", x => x.key);
            });

        migrationBuilder.CreateIndex(
            name: "ix_login_rate_limit_buckets_window_started",
            table: "login_rate_limit_buckets",
            column: "window_started");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "login_rate_limit_buckets");
}
