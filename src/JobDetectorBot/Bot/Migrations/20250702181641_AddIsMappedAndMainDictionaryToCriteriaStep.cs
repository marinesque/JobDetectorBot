using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class AddIsMappedAndMainDictionaryToCriteriaStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMapped",
                schema: "public",
                table: "CriteriaStep",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Маппинг с сервисом вакансий");

            migrationBuilder.AddColumn<string>(
                name: "MainDictionary",
                schema: "public",
                table: "CriteriaStep",
                type: "jsonb",
                nullable: true,
                comment: "Основной словарь значений");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMapped",
                schema: "public",
                table: "CriteriaStep");

            migrationBuilder.DropColumn(
                name: "MainDictionary",
                schema: "public",
                table: "CriteriaStep");
        }
    }
}
