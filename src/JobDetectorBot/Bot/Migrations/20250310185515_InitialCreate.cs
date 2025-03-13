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
                name: "Пользователь",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromIdизTelegram = table.Column<long>(name: "From.Id из Telegram", type: "bigint", nullable: false),
                    Статус = table.Column<int>(type: "integer", nullable: false),
                    CurrentCriteriaStep = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Пользователь", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Criteria",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор критерия")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор пользователя"),
                    Region = table.Column<string>(type: "text", nullable: false, comment: "Регион"),
                    Post = table.Column<string>(type: "text", nullable: false, comment: "Должность"),
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
                        name: "FK_Criteria_Пользователь_UserId",
                        column: x => x.UserId,
                        principalTable: "Пользователь",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Таблица критериев");

            migrationBuilder.CreateIndex(
                name: "IX_Пользователь_From.Id из Telegram",
                table: "Пользователь",
                column: "From.Id из Telegram",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_UserId",
                schema: "public",
                table: "Criteria",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Criteria",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Пользователь");
        }
    }
}
