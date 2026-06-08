using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG7311_GLMS_ST10435542.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ClientId",
                table: "ServiceRequests",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ClientId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ServiceRequests");
        }
    }
}
