using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Partners.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Latin1_General_100_CI_AS"),
                    TaxNumber = table.Column<string>(type: "varchar(9)", unicode: false, maxLength: 9, nullable: false),
                    Type = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.CheckConstraint("CK_Partners_CreditLimit", "[CreditLimit] >= 0 AND [CreditLimit] <= 1000000");
                    table.CheckConstraint("CK_Partners_TaxNumber", "LEN([TaxNumber]) = 9 AND [TaxNumber] NOT LIKE '%[^0-9]%'");
                    table.CheckConstraint("CK_Partners_Type", "[Type] IN ('CUSTOMER', 'SUPPLIER', 'BOTH')");
                });

            migrationBuilder.InsertData(
                table: "Partners",
                columns: new[] { "Id", "CreatedAt", "CreditLimit", "Email", "IsActive", "Name", "TaxNumber", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1001"), new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 50000m, "compras@kianda.example.com", true, "Mercado Kianda, Lda", "500100200", "CUSTOMER", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1002"), new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 0m, "geral@luandasul.example.com", true, "Distribuidora Luanda Sul", "500200300", "SUPPLIER", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1003"), new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 150000m, null, true, "Transportes Benguela", "500300400", "BOTH", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1004"), new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 2500.50m, "ana.costa@example.com", true, "Ana Costa", "123456780", "CUSTOMER", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1005"), new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 10000m, null, true, "Ana Costa", "987654321", "BOTH", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1006"), new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 0m, "vendas@papelariacentral.example.com", false, "Papelaria Central", "500400500", "SUPPLIER", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Name_Id",
                table: "Partners",
                columns: new[] { "Name", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_TaxNumber",
                table: "Partners",
                column: "TaxNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partners");
        }
    }
}
