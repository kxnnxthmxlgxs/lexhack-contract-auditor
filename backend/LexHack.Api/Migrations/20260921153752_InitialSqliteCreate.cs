using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LexHack.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqliteCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RiskScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Playbooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playbooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuditRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClauseType = table.Column<string>(type: "TEXT", nullable: false),
                    ExtractedValue = table.Column<string>(type: "TEXT", nullable: false),
                    Passed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditResults_AuditRuns_AuditRunId",
                        column: x => x.AuditRunId,
                        principalTable: "AuditRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybookRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlaybookId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClauseType = table.Column<string>(type: "TEXT", nullable: false),
                    Operator = table.Column<string>(type: "TEXT", nullable: false),
                    TargetValue = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybookRules_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Playbooks",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Standard Vendor NDA" });

            migrationBuilder.InsertData(
                table: "PlaybookRules",
                columns: new[] { "Id", "ClauseType", "Operator", "PlaybookId", "Severity", "TargetValue" },
                values: new object[,]
                {
                    { 1, "governing_law", "Equals", 1, "Critical", "South Africa" },
                    { 2, "liability_cap_amount", "LessThanOrEqual", 1, "Warning", "500000" },
                    { 3, "is_indemnification_mutual", "Equals", 1, "Critical", "true" },
                    { 4, "non_solicitation_months", "LessThanOrEqual", 1, "Warning", "12" },
                    { 5, "unilateral_termination_days", "GreaterThanOrEqual", 1, "Critical", "30" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditResults_AuditRunId",
                table: "AuditResults",
                column: "AuditRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookRules_PlaybookId",
                table: "PlaybookRules",
                column: "PlaybookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditResults");

            migrationBuilder.DropTable(
                name: "PlaybookRules");

            migrationBuilder.DropTable(
                name: "AuditRuns");

            migrationBuilder.DropTable(
                name: "Playbooks");
        }
    }
}
