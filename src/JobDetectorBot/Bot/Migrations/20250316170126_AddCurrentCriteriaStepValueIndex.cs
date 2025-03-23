using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentCriteriaStepValueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentCriteriaStepValueIndex",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Текущий индекс значения в CriteriaStepValues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCriteriaStepValueIndex",
                schema: "public",
                table: "Users");
        }
    }
}
