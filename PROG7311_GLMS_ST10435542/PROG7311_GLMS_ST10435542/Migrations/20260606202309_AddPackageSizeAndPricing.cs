using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG7311_GLMS_ST10435542.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageSizeAndPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedDriverId",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageSizeCategory",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientPhone",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SlaType",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedDriverId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "PackageSizeCategory",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "PickupAddress",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RecipientPhone",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "SlaType",
                table: "ServiceRequests");
        }
    }
}
