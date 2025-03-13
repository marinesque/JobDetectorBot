using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Criteria_Пользователь_UserId",
                schema: "public",
                table: "Criteria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Пользователь",
                table: "Пользователь");

            migrationBuilder.RenameTable(
                name: "Пользователь",
                newName: "Users",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Статус",
                schema: "public",
                table: "Users",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "From.Id из Telegram",
                schema: "public",
                table: "Users",
                newName: "TelegramId");

            migrationBuilder.RenameIndex(
                name: "IX_Пользователь_From.Id из Telegram",
                schema: "public",
                table: "Users",
                newName: "IX_Users_TelegramId");

            migrationBuilder.AlterTable(
                name: "Users",
                schema: "public",
                comment: "Таблица пользователей");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentCriteriaStep",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                comment: "Текущий шаг ввода критериев",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "public",
                table: "Users",
                type: "bigint",
                nullable: false,
                comment: "Уникальный идентификатор пользователя",
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "State",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                comment: "Текущее состояние пользователя",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "TelegramId",
                schema: "public",
                table: "Users",
                type: "bigint",
                nullable: false,
                comment: "Идентификатор пользователя в Telegram",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Время последнего обновления");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "public",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Criteria_Users_UserId",
                schema: "public",
                table: "Criteria",
                column: "UserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Criteria_Users_UserId",
                schema: "public",
                table: "Criteria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                schema: "public",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "public",
                newName: "Пользователь");

            migrationBuilder.RenameColumn(
                name: "TelegramId",
                table: "Пользователь",
                newName: "From.Id из Telegram");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Пользователь",
                newName: "Статус");

            migrationBuilder.RenameIndex(
                name: "IX_Users_TelegramId",
                table: "Пользователь",
                newName: "IX_Пользователь_From.Id из Telegram");

            migrationBuilder.AlterTable(
                name: "Пользователь",
                oldComment: "Таблица пользователей");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentCriteriaStep",
                table: "Пользователь",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Текущий шаг ввода критериев");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Пользователь",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Уникальный идентификатор пользователя")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "From.Id из Telegram",
                table: "Пользователь",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Идентификатор пользователя в Telegram");

            migrationBuilder.AlterColumn<int>(
                name: "Статус",
                table: "Пользователь",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Текущее состояние пользователя");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Пользователь",
                table: "Пользователь",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Criteria_Пользователь_UserId",
                schema: "public",
                table: "Criteria",
                column: "UserId",
                principalTable: "Пользователь",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
