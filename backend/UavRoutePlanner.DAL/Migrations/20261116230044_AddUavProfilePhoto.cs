using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UavRoutePlanner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUavProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PhotoUpdatedAtUtc",
                table: "UavProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UavProfilePhotos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UavProfileId = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UavProfilePhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UavProfilePhotos_UavProfiles_UavProfileId",
                        column: x => x.UavProfileId,
                        principalTable: "UavProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UavProfilePhotos_UavProfileId",
                table: "UavProfilePhotos",
                column: "UavProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UavProfilePhotos");

            migrationBuilder.DropColumn(
                name: "PhotoUpdatedAtUtc",
                table: "UavProfiles");
        }
    }
}
