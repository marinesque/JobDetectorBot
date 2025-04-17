using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "CriteriaStep",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор шага критерия")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false, comment: "Название шага"),
                    Prompt = table.Column<string>(type: "text", nullable: false, comment: "Отображаемое значение для шага"),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false, comment: "Возможно ли кастомное значение"),
                    OrderBy = table.Column<int>(type: "integer", nullable: false, comment: "Порядок сортировки")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaStep", x => x.Id);
                },
                comment: "Таблица шагов критериев");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор пользователя")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор пользователя в Telegram"),
                    State = table.Column<int>(type: "integer", nullable: false, comment: "Текущее состояние пользователя"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Время последнего обновления"),
                    CurrentCriteriaStep = table.Column<int>(type: "integer", nullable: false, comment: "Текущий шаг ввода критериев")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                },
                comment: "Таблица пользователей");

            migrationBuilder.CreateTable(
                name: "CriteriaStepValues",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор значения шага критерия")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriteriaStepId = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор шага критерия"),
                    Prompt = table.Column<string>(type: "text", nullable: false, comment: "Отображаемое значение шага критерия"),
                    Value = table.Column<string>(type: "text", nullable: false, comment: "Значение шага критерия")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaStepValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaStepValues_CriteriaStep_CriteriaStepId",
                        column: x => x.CriteriaStepId,
                        principalSchema: "public",
                        principalTable: "CriteriaStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Таблица значений шагов критериев");

            migrationBuilder.CreateTable(
                name: "Criteria",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор критерия")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор пользователя"),
                    Region = table.Column<string>(type: "text", nullable: true, comment: "Регион"),
                    Post = table.Column<string>(type: "text", nullable: true, comment: "Должность"),
                    Salary = table.Column<decimal>(type: "numeric", nullable: true, comment: "Зарплата"),
                    Experience = table.Column<int>(type: "integer", nullable: true, comment: "Опыт работы"),
                    WorkType = table.Column<string>(type: "text", nullable: true, comment: "Тип занятости"),
                    Schedule = table.Column<string>(type: "text", nullable: true, comment: "График работы"),
                    Disability = table.Column<bool>(type: "boolean", nullable: false, comment: "Доступно для людей с инвалидностью"),
                    ForChildren = table.Column<bool>(type: "boolean", nullable: false, comment: "Доступно для детей с 14 лет")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Criteria_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Таблица критериев");

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_UserId",
                schema: "public",
                table: "Criteria",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaStep_Name",
                schema: "public",
                table: "CriteriaStep",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaStepValues_CriteriaStepId_Prompt",
                schema: "public",
                table: "CriteriaStepValues",
                columns: new[] { "CriteriaStepId", "Prompt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                schema: "public",
                table: "Users",
                column: "TelegramId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Criteria",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CriteriaStepValues",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CriteriaStep",
                schema: "public");
        }
    }
}
