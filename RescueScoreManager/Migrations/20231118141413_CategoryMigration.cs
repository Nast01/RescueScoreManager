using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueScoreManager.Migrations
{
    /// <inheritdoc />
    public partial class CategoryMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefereeDate_Licensees_RefereeId",
                table: "RefereeDate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeDate",
                table: "RefereeDate");

            migrationBuilder.RenameTable(
                name: "RefereeDate",
                newName: "RefereeDates");

            migrationBuilder.RenameIndex(
                name: "IX_RefereeDate_RefereeId",
                table: "RefereeDates",
                newName: "IX_RefereeDates_RefereeId");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Licensees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeDates",
                table: "RefereeDates",
                column: "Id");

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

            migrationBuilder.CreateIndex(
                name: "IX_Licensees_CategoryId",
                table: "Licensees",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Licensees_Categories_CategoryId",
                table: "Licensees",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefereeDates_Licensees_RefereeId",
                table: "RefereeDates",
                column: "RefereeId",
                principalTable: "Licensees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licensees_Categories_CategoryId",
                table: "Licensees");

            migrationBuilder.DropForeignKey(
                name: "FK_RefereeDates_Licensees_RefereeId",
                table: "RefereeDates");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Licensees_CategoryId",
                table: "Licensees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeDates",
                table: "RefereeDates");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Licensees");

            migrationBuilder.RenameTable(
                name: "RefereeDates",
                newName: "RefereeDate");

            migrationBuilder.RenameIndex(
                name: "IX_RefereeDates_RefereeId",
                table: "RefereeDate",
                newName: "IX_RefereeDate_RefereeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeDate",
                table: "RefereeDate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefereeDate_Licensees_RefereeId",
                table: "RefereeDate",
                column: "RefereeId",
                principalTable: "Licensees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
