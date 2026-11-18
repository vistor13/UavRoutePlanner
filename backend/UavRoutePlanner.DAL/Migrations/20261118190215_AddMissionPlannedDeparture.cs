using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UavRoutePlanner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionPlannedDeparture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedDepartureUtc",
                table: "Missions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedDepartureUtc",
                table: "Missions");
        }
    }
}
