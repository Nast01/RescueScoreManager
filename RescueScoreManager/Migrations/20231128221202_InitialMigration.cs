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
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

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
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    StartHour = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndHour = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MeetingType = table.Column<int>(type: "INTEGER", nullable: false),
                    HeatType = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelatedMeetingId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meetings_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_Meetings_RelatedMeetingId",
                        column: x => x.RelatedMeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Speciality = table.Column<int>(type: "INTEGER", nullable: false),
                    Discipline = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberByTeam = table.Column<int>(type: "INTEGER", nullable: false),
                    Distance = table.Column<int>(type: "INTEGER", nullable: true),
                    Interval = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRelay = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompetitionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Races_Competitions_CompetitionId",
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
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    LicenseeType = table.Column<string>(type: "TEXT", nullable: false),
                    IsForfeit = table.Column<bool>(type: "INTEGER", nullable: true),
                    OrderNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    RefereeLevel = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licensees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licensees_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Licensees_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    StartHour = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndHour = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    isFinalA = table.Column<bool>(type: "INTEGER", nullable: false),
                    isFinalB = table.Column<bool>(type: "INTEGER", nullable: false),
                    MeetingId = table.Column<int>(type: "INTEGER", nullable: false),
                    RaceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingElements_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingElements_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RaceCategory",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "INTEGER", nullable: false),
                    RacesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceCategory", x => new { x.CategoriesId, x.RacesId });
                    table.ForeignKey(
                        name: "FK_RaceCategory_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceCategory_Races_RacesId",
                        column: x => x.RacesId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefereeDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Availability = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RefereeId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereeDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefereeDates_Licensees_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "Licensees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsForfeit = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsForfeitFinal = table.Column<bool>(type: "INTEGER", nullable: false),
                    EntryTime = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    RaceId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamType = table.Column<string>(type: "TEXT", nullable: false),
                    AthleteId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Licensees_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Licensees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Teams_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingElementCategory",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "INTEGER", nullable: false),
                    MeetingElementsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingElementCategory", x => new { x.CategoriesId, x.MeetingElementsId });
                    table.ForeignKey(
                        name: "FK_MeetingElementCategory_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingElementCategory_MeetingElements_MeetingElementsId",
                        column: x => x.MeetingElementsId,
                        principalTable: "MeetingElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    HeatType = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    MeetingElementId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rounds_MeetingElements_MeetingElementId",
                        column: x => x.MeetingElementId,
                        principalTable: "MeetingElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AthleteRelayTeam",
                columns: table => new
                {
                    AthletesId = table.Column<string>(type: "TEXT", nullable: false),
                    RelayTeamsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteRelayTeam", x => new { x.AthletesId, x.RelayTeamsId });
                    table.ForeignKey(
                        name: "FK_AthleteRelayTeam_Licensees_AthletesId",
                        column: x => x.AthletesId,
                        principalTable: "Licensees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AthleteRelayTeam_Teams_RelayTeamsId",
                        column: x => x.RelayTeamsId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Heats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    StartHour = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndHour = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsFinalA = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFinalB = table.Column<bool>(type: "INTEGER", nullable: false),
                    RoundId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Heats_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeatResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lane = table.Column<int>(type: "INTEGER", nullable: false),
                    Disqualification = table.Column<string>(type: "TEXT", nullable: false),
                    IsDisqualified = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsForfeit = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsOfficial = table.Column<bool>(type: "INTEGER", nullable: false),
                    HeatId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeatResultType = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: true),
                    Time = table.Column<int>(type: "INTEGER", nullable: true),
                    IsForfeitFinal = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeatResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeatResults_Heats_HeatId",
                        column: x => x.HeatId,
                        principalTable: "Heats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeatResults_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AthleteRelayTeam_RelayTeamsId",
                table: "AthleteRelayTeam",
                column: "RelayTeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_CompetitionId",
                table: "Clubs",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HeatResults_HeatId",
                table: "HeatResults",
                column: "HeatId");

            migrationBuilder.CreateIndex(
                name: "IX_HeatResults_TeamId",
                table: "HeatResults",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Heats_RoundId",
                table: "Heats",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Licensees_CategoryId",
                table: "Licensees",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Licensees_ClubId",
                table: "Licensees",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingElementCategory_MeetingElementsId",
                table: "MeetingElementCategory",
                column: "MeetingElementsId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingElements_MeetingId",
                table: "MeetingElements",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingElements_RaceId",
                table: "MeetingElements",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CompetitionId",
                table: "Meetings",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_RelatedMeetingId",
                table: "Meetings",
                column: "RelatedMeetingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaceCategory_RacesId",
                table: "RaceCategory",
                column: "RacesId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_CompetitionId",
                table: "Races",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefereeDates_RefereeId",
                table: "RefereeDates",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_CategoryId",
                table: "Rounds",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_MeetingElementId",
                table: "Rounds",
                column: "MeetingElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_AthleteId",
                table: "Teams",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_RaceId",
                table: "Teams",
                column: "RaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AthleteRelayTeam");

            migrationBuilder.DropTable(
                name: "HeatResults");

            migrationBuilder.DropTable(
                name: "MeetingElementCategory");

            migrationBuilder.DropTable(
                name: "RaceCategory");

            migrationBuilder.DropTable(
                name: "RefereeDates");

            migrationBuilder.DropTable(
                name: "Heats");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Licensees");

            migrationBuilder.DropTable(
                name: "MeetingElements");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Clubs");

            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.DropTable(
                name: "Races");

            migrationBuilder.DropTable(
                name: "Competitions");
        }
    }
}
