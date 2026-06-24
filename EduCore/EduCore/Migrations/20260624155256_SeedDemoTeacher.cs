using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCore.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "ID", "Biography", "Email", "FName", "LName", "Password", "PhoneNumber" },
                values: new object[] { 1, "Seeded demo teacher account for development.", "teacher@educore.local", "Demo", "Teacher", "password", "0000000000" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "ID",
                keyValue: 1);
        }
    }
}
