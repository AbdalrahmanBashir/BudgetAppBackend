using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetAppBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class firstone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.CreateTable(
                name: "PlaidSyncCursors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    Cursor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LastSynced = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaidSyncCursors", x => x.Id);
                });

            


            migrationBuilder.CreateTable(
                name: "PlaidTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaidTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MerchantName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Categories = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaidTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaidTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Payee = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Categories = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaidSyncCursors_UserId_AccessToken_ItemId",
                table: "PlaidSyncCursors",
                columns: new[] { "UserId", "AccessToken", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaidTransactions_AccountId",
                table: "PlaidTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaidTransactions_PlaidTransactionId",
                table: "PlaidTransactions",
                column: "PlaidTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaidTransactions_UserId_Date",
                table: "PlaidTransactions",
                columns: new[] { "UserId", "Date" });


            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedDate",
                table: "Transactions",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                table: "Transactions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "PlaidSyncCursors");

            migrationBuilder.DropTable(
                name: "PlaidTransactions");


            migrationBuilder.DropTable(
                name: "Transactions");

        }
    }
}
