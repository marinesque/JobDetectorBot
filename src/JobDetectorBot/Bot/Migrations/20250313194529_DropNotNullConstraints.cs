using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DropNotNullConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Region",
                schema: "public",
                table: "Criteria",
                type: "text",
                nullable: true,
                comment: "Регион",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Регион");

            migrationBuilder.AlterColumn<string>(
                name: "Post",
                schema: "public",
                table: "Criteria",
                type: "text",
                nullable: true,
                comment: "Должность",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Должность");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Region",
                schema: "public",
                table: "Criteria",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Регион",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Регион");

            migrationBuilder.AlterColumn<string>(
                name: "Post",
                schema: "public",
                table: "Criteria",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Должность",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Должность");
        }
    }
}
