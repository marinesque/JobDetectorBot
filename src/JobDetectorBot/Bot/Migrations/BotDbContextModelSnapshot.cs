﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bot.Migrations
{
    [DbContext(typeof(BotDbContext))]
    partial class BotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("Bot.Domain.DataAccess.Model.User", b =>
                {
                    b.Navigation("Criteria");
                });
#pragma warning restore 612, 618
        }
    }
}
