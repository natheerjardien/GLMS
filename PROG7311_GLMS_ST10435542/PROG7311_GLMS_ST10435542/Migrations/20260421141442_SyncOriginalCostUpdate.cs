using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG7311_GLMS_ST10435542.Migrations
{
    /// <inheritdoc />
    public partial class SyncOriginalCostUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OriginalCost",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalCost",
                table: "ServiceRequests");
        }
    }
}
