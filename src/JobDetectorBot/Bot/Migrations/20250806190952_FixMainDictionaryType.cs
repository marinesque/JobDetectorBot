using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class FixMainDictionaryType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MainDictionary",
                schema: "public",
                table: "CriteriaStep",
                type: "text",
                nullable: true,
                comment: "Основной словарь значений",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "Основной словарь значений");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MainDictionary",
                schema: "public",
                table: "CriteriaStep",
                type: "jsonb",
                nullable: true,
                comment: "Основной словарь значений",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Основной словарь значений");
        }
    }
}
