using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTariffAdvisor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LiveInUK = table.Column<bool>(type: "INTEGER", nullable: true),
                    ProjectRelevant = table.Column<bool>(type: "INTEGER", nullable: true),
                    AIProfileAccurate = table.Column<bool>(type: "INTEGER", nullable: true),
                    Suggestions = table.Column<string>(type: "TEXT", nullable: false),
                    CalculatorBetter = table.Column<bool>(type: "INTEGER", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackResponses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackResponses");
        }
    }
}
