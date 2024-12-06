using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budget.Telegram.Bot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBudgetEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetDeposit",
                columns: table => new
                {
                    BudgetId = table.Column<long>(type: "bigint", nullable: false),
                    DepositId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetDeposit", x => new { x.BudgetId, x.DepositId });
                    table.ForeignKey(
                        name: "FK_BudgetDeposit_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetDeposit_Deposits_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetExpense",
                columns: table => new
                {
                    BudgetId = table.Column<long>(type: "bigint", nullable: false),
                    ExpenseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetExpense", x => new { x.BudgetId, x.ExpenseId });
                    table.ForeignKey(
                        name: "FK_BudgetExpense_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetExpense_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUserUsersGroup",
                columns: table => new
                {
                    TelegramUserId = table.Column<long>(type: "bigint", nullable: false),
                    UsersGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUserUsersGroup", x => new { x.TelegramUserId, x.UsersGroupId });
                    table.ForeignKey(
                        name: "FK_TelegramUserUsersGroup_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramUserUsersGroup_UsersGroups_UsersGroupId",
                        column: x => x.UsersGroupId,
                        principalTable: "UsersGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersGroupBudget",
                columns: table => new
                {
                    BudgetId = table.Column<long>(type: "bigint", nullable: false),
                    UsersGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersGroupBudget", x => new { x.BudgetId, x.UsersGroupId });
                    table.ForeignKey(
                        name: "FK_UsersGroupBudget_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersGroupBudget_UsersGroups_UsersGroupId",
                        column: x => x.UsersGroupId,
                        principalTable: "UsersGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetDeposit_DepositId",
                table: "BudgetDeposit",
                column: "DepositId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetExpense_ExpenseId",
                table: "BudgetExpense",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUserUsersGroup_UsersGroupId",
                table: "TelegramUserUsersGroup",
                column: "UsersGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersGroupBudget_UsersGroupId",
                table: "UsersGroupBudget",
                column: "UsersGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetDeposit");

            migrationBuilder.DropTable(
                name: "BudgetExpense");

            migrationBuilder.DropTable(
                name: "TelegramUserUsersGroup");

            migrationBuilder.DropTable(
                name: "UsersGroupBudget");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "UsersGroups");
        }
    }
}
