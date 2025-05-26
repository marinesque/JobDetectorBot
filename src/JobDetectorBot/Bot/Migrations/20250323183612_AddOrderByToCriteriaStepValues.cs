using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderByToCriteriaStepValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderBy",
                schema: "public",
                table: "CriteriaStepValues",
                type: "integer",
                nullable: true,
                comment: "Порядок сортировки значения");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderBy",
                schema: "public",
                table: "CriteriaStepValues");
        }
    }
}
