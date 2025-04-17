using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class Syncronization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CriteriaStepValues_CriteriaStepId_Prompt",
                schema: "public",
                table: "CriteriaStepValues");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaStepValues_CriteriaStepId_Value",
                schema: "public",
                table: "CriteriaStepValues",
                columns: new[] { "CriteriaStepId", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CriteriaStepValues_CriteriaStepId_Value",
                schema: "public",
                table: "CriteriaStepValues");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaStepValues_CriteriaStepId_Prompt",
                schema: "public",
                table: "CriteriaStepValues",
                columns: new[] { "CriteriaStepId", "Prompt" },
                unique: true);
        }
    }
}
