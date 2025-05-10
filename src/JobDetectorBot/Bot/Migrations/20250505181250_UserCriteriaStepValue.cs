using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class UserCriteriaStepValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Criteria",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "UserCriteriaStepValues",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор пользователя"),
                    CriteriaStepId = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор шага критерия"),
                    CriteriaStepValueId = table.Column<long>(type: "bigint", nullable: true, comment: "Идентификатор значения шага критерия (если выбрано из списка)"),
                    CustomValue = table.Column<string>(type: "text", nullable: true, comment: "Пользовательское значение (если введено вручную)"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Дата создания записи"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Дата последнего обновления")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCriteriaStepValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCriteriaStepValues_CriteriaStepValues_CriteriaStepValue~",
                        column: x => x.CriteriaStepValueId,
                        principalSchema: "public",
                        principalTable: "CriteriaStepValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCriteriaStepValues_CriteriaStep_CriteriaStepId",
                        column: x => x.CriteriaStepId,
                        principalSchema: "public",
                        principalTable: "CriteriaStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCriteriaStepValues_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Таблица значений критериев пользователя");

            migrationBuilder.CreateIndex(
                name: "IX_UserCriteriaStepValues_CriteriaStepId",
                schema: "public",
                table: "UserCriteriaStepValues",
                column: "CriteriaStepId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCriteriaStepValues_CriteriaStepValueId",
                schema: "public",
                table: "UserCriteriaStepValues",
                column: "CriteriaStepValueId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCriteriaStepValues_UserId_CriteriaStepId",
                schema: "public",
                table: "UserCriteriaStepValues",
                columns: new[] { "UserId", "CriteriaStepId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCriteriaStepValues",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "Criteria",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "Уникальный идентификатор критерия")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Disability = table.Column<bool>(type: "boolean", nullable: false, comment: "Доступно для людей с инвалидностью"),
                    Experience = table.Column<int>(type: "integer", nullable: true, comment: "Опыт работы"),
                    ForChildren = table.Column<bool>(type: "boolean", nullable: false, comment: "Доступно для детей с 14 лет"),
                    Post = table.Column<string>(type: "text", nullable: true, comment: "Должность"),
                    Region = table.Column<string>(type: "text", nullable: true, comment: "Регион"),
                    Salary = table.Column<decimal>(type: "numeric", nullable: true, comment: "Зарплата"),
                    Schedule = table.Column<string>(type: "text", nullable: true, comment: "График работы"),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор пользователя"),
                    WorkType = table.Column<string>(type: "text", nullable: true, comment: "Тип занятости")
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
        }
    }
}
