using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHub.API.Migrations
{
    /// <inheritdoc />
    public partial class GeneralizeDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the new generic table FIRST (before dropping DigestDrafts)
            // so the data-copy step below has both tables available.
            migrationBuilder.CreateTable(
                name: "Drafts",
                columns: table => new
                {
                    DraftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DraftDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drafts", x => x.DraftId);
                    table.ForeignKey(
                        name: "FK_Drafts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_UserId_ModuleType",
                table: "Drafts",
                columns: new[] { "UserId", "ModuleType" },
                unique: true);

            // Data migration: copy every existing DigestDrafts row into the new
            // generic Drafts table (moduleType = "digest"), packing the old
            // per-column fields into a single JSON payload matching what
            // DraftsController now stores for every module. FOR JSON PATH
            // handles string escaping automatically, and null-valued columns
            // are simply omitted from the JSON (the frontend already treats a
            // missing field the same as an empty one). No existing draft data
            // is lost by this migration.
            migrationBuilder.Sql(@"
                INSERT INTO Drafts (UserId, ModuleType, DraftDataJson, LastUpdated)
                SELECT
                    d.UserID,
                    'digest',
                    (
                        SELECT
                            d.Title AS title,
                            d.Description AS description,
                            d.ImageUrl AS imageUrl,
                            d.SourceName AS sourceName,
                            d.SourceUrl AS sourceUrl,
                            CONVERT(varchar(10), d.PeriodFrom, 23) AS periodFrom,
                            CONVERT(varchar(10), d.PeriodTo, 23) AS periodTo,
                            d.IsFeatured AS isFeatured,
                            d.IsActive AS isActive
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    ),
                    d.LastUpdated
                FROM DigestDrafts d;
            ");

            migrationBuilder.DropTable(
                name: "DigestDrafts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NOTE: rolling back recreates the old DigestDrafts table structure
            // but does not attempt to reverse-migrate JSON data back into it
            // (and any Vacancy/News drafts created under the new table have no
            // equivalent old table at all). Downgrading past this migration
            // loses current draft data - acceptable for transient scratch state.
            migrationBuilder.DropTable(
                name: "Drafts");

            migrationBuilder.CreateTable(
                name: "DigestDrafts",
                columns: table => new
                {
                    DigestDraftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigestDrafts", x => x.DigestDraftID);
                    table.ForeignKey(
                        name: "FK_DigestDrafts_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigestDrafts_UserID",
                table: "DigestDrafts",
                column: "UserID",
                unique: true);
        }
    }
}
