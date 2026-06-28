using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCore.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "ID",
                keyValue: 1,
                column: "Password",
                value: "1OubmybQyMYpetU/JF2JNg==:PA3m0/NloaZJlG72BhRBEuwQJ6MTWxSx632tuVtGZ1E=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Teachers",
                keyColumn: "ID",
                keyValue: 1,
                column: "Password",
                value: "password");
        }
    }
}
