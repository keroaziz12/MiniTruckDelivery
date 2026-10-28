using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTruckDelivery.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminTripDriverControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Trips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Drivers");
        }
    }
}
