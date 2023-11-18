using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueScoreManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    BeginDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EntryLimitDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BeachType = table.Column<int>(type: "INTEGER", nullable: true),
                    SwimType = table.Column<int>(type: "INTEGER", nullable: true),
                    Speciality = table.Column<int>(type: "INTEGER", nullable: false),
                    ChronoType = table.Column<int>(type: "INTEGER", nullable: true),
                    IsEligibleToNationalRecord = table.Column<bool>(type: "INTEGER", nullable: false),
                    PriceByAthlete = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceByEntry = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceByClub = table.Column<int>(type: "INTEGER", nullable: false),
                    Organizer = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CompetitionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clubs_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Licensees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    BirthYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    IsLicencee = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsGuest = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClubId = table.Column<int>(type: "INTEGER", nullable: false),
                    LicenseeType = table.Column<string>(type: "TEXT", nullable: false),
                    IsForfeit = table.Column<bool>(type: "INTEGER", nullable: true),
                    OrderNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    RefereeLevel = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licensees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licensees_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefereeDate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Availability = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RefereeId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereeDate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefereeDate_Licensees_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "Licensees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_CompetitionId",
                table: "Clubs",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Licensees_ClubId",
                table: "Licensees",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_RefereeDate_RefereeId",
                table: "RefereeDate",
                column: "RefereeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefereeDate");

            migrationBuilder.DropTable(
                name: "Licensees");

            migrationBuilder.DropTable(
                name: "Clubs");

            migrationBuilder.DropTable(
                name: "Competitions");
        }
    }
}
