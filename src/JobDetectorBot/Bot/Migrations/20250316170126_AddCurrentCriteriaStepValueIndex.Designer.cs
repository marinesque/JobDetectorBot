﻿// <auto-generated />
using System;
using Bot.Domain.DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bot.Migrations
{
    [DbContext(typeof(BotDbContext))]
    [Migration("20250316170126_AddCurrentCriteriaStepValueIndex")]
    partial class AddCurrentCriteriaStepValueIndex
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
                        .HasColumnType("text")
                        .HasColumnName("Post")
                        .HasComment("Должность");

                    b.Property<string>("Region")
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

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.CriteriaStep", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("Id")
                        .HasComment("Уникальный идентификатор шага критерия");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("IsCustom")
                        .HasColumnType("boolean")
                        .HasColumnName("IsCustom")
                        .HasComment("Возможно ли кастомное значение");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Name")
                        .HasComment("Название шага");

                    b.Property<int>("OrderBy")
                        .HasColumnType("integer")
                        .HasColumnName("OrderBy")
                        .HasComment("Порядок сортировки");

                    b.Property<string>("Prompt")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Prompt")
                        .HasComment("Отображаемое значение для шага");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("CriteriaStep", "public", t =>
                        {
                            t.HasComment("Таблица шагов критериев");
                        });
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.CriteriaStepValue", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("Id")
                        .HasComment("Уникальный идентификатор значения шага критерия");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("CriteriaStepId")
                        .HasColumnType("bigint")
                        .HasColumnName("CriteriaStepId")
                        .HasComment("Идентификатор шага критерия");

                    b.Property<string>("Prompt")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Prompt")
                        .HasComment("Отображаемое значение шага критерия");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Value")
                        .HasComment("Значение шага критерия");

                    b.HasKey("Id");

                    b.HasIndex("CriteriaStepId", "Prompt")
                        .IsUnique();

                    b.ToTable("CriteriaStepValues", "public", t =>
                        {
                            t.HasComment("Таблица значений шагов критериев");
                        });
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("Id")
                        .HasComment("Уникальный идентификатор пользователя");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("CurrentCriteriaStep")
                        .HasColumnType("integer")
                        .HasColumnName("CurrentCriteriaStep")
                        .HasComment("Текущий шаг ввода критериев");

                    b.Property<int>("CurrentCriteriaStepValueIndex")
                        .HasColumnType("integer")
                        .HasColumnName("CurrentCriteriaStepValueIndex")
                        .HasComment("Текущий индекс значения в CriteriaStepValues");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("LastUpdated")
                        .HasComment("Время последнего обновления");

                    b.Property<int>("State")
                        .HasColumnType("integer")
                        .HasColumnName("State")
                        .HasComment("Текущее состояние пользователя");

                    b.Property<long>("TelegramId")
                        .HasColumnType("bigint")
                        .HasColumnName("TelegramId")
                        .HasComment("Идентификатор пользователя в Telegram");

                    b.HasKey("Id");

                    b.HasIndex("TelegramId")
                        .IsUnique();

                    b.ToTable("Users", "public", t =>
                        {
                            t.HasComment("Таблица пользователей");
                        });
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.Criteria", b =>
                {
                    b.HasOne("Bot.Domain.DataAccess.Model.User", null)
                        .WithOne("Criteria")
                        .HasForeignKey("Bot.Domain.DataAccess.Model.Criteria", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.CriteriaStepValue", b =>
                {
                    b.HasOne("Bot.Domain.DataAccess.Model.CriteriaStep", "CriteriaStep")
                        .WithMany("CriteriaStepValues")
                        .HasForeignKey("CriteriaStepId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CriteriaStep");
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.CriteriaStep", b =>
                {
                    b.Navigation("CriteriaStepValues");
                });

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.User", b =>
                {
                    b.Navigation("Criteria");
                });
#pragma warning restore 612, 618
        }
    }
}
