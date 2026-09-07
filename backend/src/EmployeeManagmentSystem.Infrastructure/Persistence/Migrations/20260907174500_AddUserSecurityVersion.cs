using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EmployeeManagementDbContext))]
[Migration("20260907174500_AddUserSecurityVersion")]
public partial class AddUserSecurityVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "security_version",
            table: "users",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "security_version",
            table: "users");
    }
}
