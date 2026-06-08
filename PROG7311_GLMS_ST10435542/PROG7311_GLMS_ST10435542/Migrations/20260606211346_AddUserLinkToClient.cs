using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG7311_GLMS_ST10435542.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLinkToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Clients");
        }
    }
}
