﻿// <auto-generated />
using System;
using Budget.Telegram.Bot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Budget.Telegram.Bot.DataAccess.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241220143554_Initial Create")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Budget.Telegram.Bot.Entity.Entities.Budget", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<double?>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Budgets");
                });

            modelBuilder.Entity("Budget.Telegram.Bot.Entity.Entities.Deposit", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<double>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Deposits");
                });

            modelBuilder.Entity("Budget.Telegram.Bot.Entity.Entities.Expense", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<double>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Expenses");
                });

            modelBuilder.Entity("Budget.Telegram.Bot.Entity.Entities.TelegramUser", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LanguageCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("TelegramUsers");
                });

            modelBuilder.Entity("Budget.Telegram.Bot.Entity.Entities.UsersGroup", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("UsersGroups");
                });

            modelBuilder.Entity("BudgetDeposit", b =>
                {
                    b.Property<long>("BudgetId")
                        .HasColumnType("bigint");

                    b.Property<long>("DepositId")
                        .HasColumnType("bigint");

                    b.HasKey("BudgetId", "DepositId");

                    b.HasIndex("DepositId");

                    b.ToTable("BudgetDeposit");
                });

            modelBuilder.Entity("BudgetExpense", b =>
                {
                    b.Property<long>("BudgetId")
                        .HasColumnType("bigint");

                    b.Property<long>("ExpenseId")
                        .HasColumnType("bigint");

                    b.HasKey("BudgetId", "ExpenseId");

                    b.HasIndex("ExpenseId");

                    b.ToTable("BudgetExpense");
                });

            modelBuilder.Entity("TelegramUserUsersGroup", b =>
                {
                    b.Property<long>("TelegramUserId")
                        .HasColumnType("bigint");

                    b.Property<long>("UsersGroupId")
                        .HasColumnType("bigint");

                    b.HasKey("TelegramUserId", "UsersGroupId");

                    b.HasIndex("UsersGroupId");

                    b.ToTable("TelegramUserUsersGroup");
                });

            modelBuilder.Entity("UsersGroupBudget", b =>
                {
                    b.Property<long>("BudgetId")
                        .HasColumnType("bigint");

                    b.Property<long>("UsersGroupId")
                        .HasColumnType("bigint");

                    b.HasKey("BudgetId", "UsersGroupId");

                    b.HasIndex("UsersGroupId");

                    b.ToTable("UsersGroupBudget");
                });

            modelBuilder.Entity("BudgetDeposit", b =>
                {
                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.Budget", null)
                        .WithMany()
                        .HasForeignKey("BudgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.Deposit", null)
                        .WithMany()
                        .HasForeignKey("DepositId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BudgetExpense", b =>
                {
                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.Budget", null)
                        .WithMany()
                        .HasForeignKey("BudgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.Expense", null)
                        .WithMany()
                        .HasForeignKey("ExpenseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TelegramUserUsersGroup", b =>
                {
                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.TelegramUser", null)
                        .WithMany()
                        .HasForeignKey("TelegramUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.UsersGroup", null)
                        .WithMany()
                        .HasForeignKey("UsersGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UsersGroupBudget", b =>
                {
                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.Budget", null)
                        .WithMany()
                        .HasForeignKey("BudgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Budget.Telegram.Bot.Entity.Entities.UsersGroup", null)
                        .WithMany()
                        .HasForeignKey("UsersGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
