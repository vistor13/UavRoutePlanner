using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UavRoutePlanner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherForecasts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    ForecastTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeightM = table.Column<int>(type: "integer", nullable: false),
                    WindSpeedMs = table.Column<double>(type: "double precision", nullable: false),
                    WindDirectionFromDeg = table.Column<double>(type: "double precision", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherForecasts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UavProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AirspeedMs = table.Column<double>(type: "double precision", nullable: false),
                    MaxFlightTimeMin = table.Column<double>(type: "double precision", nullable: false),
                    ReservePercent = table.Column<double>(type: "double precision", nullable: false),
                    FlightAltitudeM = table.Column<double>(type: "double precision", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UavProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UavProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Start_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Start_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Finish_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Finish_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UavProfileId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_UavProfiles_UavProfileId",
                        column: x => x.UavProfileId,
                        principalTable: "UavProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Missions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlgorithmSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alpha = table.Column<double>(type: "double precision", nullable: false),
                    Beta = table.Column<double>(type: "double precision", nullable: false),
                    EvaporationRate = table.Column<double>(type: "double precision", nullable: false),
                    PheromoneDeposit = table.Column<double>(type: "double precision", nullable: false),
                    AntsCount = table.Column<int>(type: "integer", nullable: false),
                    IterationsCount = table.Column<int>(type: "integer", nullable: false),
                    MissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlgorithmSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlgorithmSettings_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DepartureTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindSpeedMs = table.Column<double>(type: "double precision", nullable: false),
                    WindDirectionFromDeg = table.Column<double>(type: "double precision", nullable: false),
                    VisitedObjectsCount = table.Column<int>(type: "integer", nullable: false),
                    TotalTimeS = table.Column<double>(type: "double precision", nullable: false),
                    MissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteResults_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyObjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    MissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyObjects_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UavParameters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AirspeedMs = table.Column<double>(type: "double precision", nullable: false),
                    MaxFlightTimeMin = table.Column<double>(type: "double precision", nullable: false),
                    ReservePercent = table.Column<double>(type: "double precision", nullable: false),
                    FlightAltitudeM = table.Column<double>(type: "double precision", nullable: false),
                    MissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UavParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UavParameters_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteLegs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PointName = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    DistanceM = table.Column<double>(type: "double precision", nullable: false),
                    TrackDeg = table.Column<double>(type: "double precision", nullable: false),
                    HeadingDeg = table.Column<double>(type: "double precision", nullable: false),
                    GroundSpeedMs = table.Column<double>(type: "double precision", nullable: false),
                    TimeS = table.Column<double>(type: "double precision", nullable: false),
                    RouteResultId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteLegs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteLegs_RouteResults_RouteResultId",
                        column: x => x.RouteResultId,
                        principalTable: "RouteResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmSettings_MissionId",
                table: "AlgorithmSettings",
                column: "MissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Missions_UavProfileId",
                table: "Missions",
                column: "UavProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_UserId_CreatedAt",
                table: "Missions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RouteLegs_RouteResultId",
                table: "RouteLegs",
                column: "RouteResultId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteResults_MissionId",
                table: "RouteResults",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyObjects_MissionId",
                table: "SurveyObjects",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UavParameters_MissionId",
                table: "UavParameters",
                column: "MissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UavProfiles_UserId",
                table: "UavProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherForecasts_Latitude_Longitude_HeightM_ForecastTimeUtc",
                table: "WeatherForecasts",
                columns: new[] { "Latitude", "Longitude", "HeightM", "ForecastTimeUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlgorithmSettings");

            migrationBuilder.DropTable(
                name: "RouteLegs");

            migrationBuilder.DropTable(
                name: "SurveyObjects");

            migrationBuilder.DropTable(
                name: "UavParameters");

            migrationBuilder.DropTable(
                name: "WeatherForecasts");

            migrationBuilder.DropTable(
                name: "RouteResults");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropTable(
                name: "UavProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
