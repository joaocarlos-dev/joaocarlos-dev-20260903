using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagmentSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EmployeeManagementDbContext))]
[Migration("20260907173000_AddUserRole")]
public partial class AddUserRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "role",
            table: "users",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "role",
            table: "users");
    }
}
