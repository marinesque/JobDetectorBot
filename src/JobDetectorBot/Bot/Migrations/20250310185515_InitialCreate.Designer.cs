﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bot.Migrations
{
    [DbContext(typeof(BotDbContext))]
    [Migration("20250310185515_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.Criteria", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("Id")
                        .HasComment("Уникальный идентификатор критерия");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("Disability")
                        .HasColumnType("boolean")
                        .HasColumnName("Disability")
                        .HasComment("Доступно для людей с инвалидностью");

                    b.Property<int?>("Experience")
                        .HasColumnType("integer")
                        .HasColumnName("Experience")
                        .HasComment("Опыт работы");

                    b.Property<bool>("ForChildren")
                        .HasColumnType("boolean")
                        .HasColumnName("ForChildren")
                        .HasComment("Доступно для детей с 14 лет");

                    b.Property<string>("Post")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Post")
                        .HasComment("Должность");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Region")
                        .HasComment("Регион");

                    b.Property<decimal?>("Salary")
                        .HasColumnType("numeric")
                        .HasColumnName("Salary")
                        .HasComment("Зарплата");

                    b.Property<string>("Schedule")
                        .HasColumnType("text")
                        .HasColumnName("Schedule")
                        .HasComment("График работы");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("UserId")
                        .HasComment("Идентификатор пользователя");

                    b.Property<string>("WorkType")
                        .HasColumnType("text")
                        .HasColumnName("WorkType")
                        .HasComment("Тип занятости");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Criteria", "public", t =>
                        {
                            t.HasComment("Таблица критериев");
                        });
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("CurrentCriteriaStep")
                        .HasColumnType("integer");

                    b.Property<int>("State")
                        .HasColumnType("integer")
                        .HasColumnName("Статус");

                    b.Property<long>("TelegramId")
                        .HasColumnType("bigint")
                        .HasColumnName("From.Id из Telegram");

                    b.HasKey("Id");

                    b.HasIndex("TelegramId")
                        .IsUnique();

                    b.ToTable("Пользователь");
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.Criteria", b =>
                {
                    b.HasOne("Bot.Domain.DataAccess.Model.User", null)
                        .WithOne("Criteria")
                        .HasForeignKey("Bot.Domain.DataAccess.Model.Criteria", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.User", b =>
                {
                    b.Navigation("Criteria");
                });
#pragma warning restore 612, 618
        }
    }
}
