using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuyerOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuyerOrders_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerOrderId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierOrders_BuyerOrders_BuyerOrderId",
                        column: x => x.BuyerOrderId,
                        principalTable: "BuyerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuyerOrderId = table.Column<int>(type: "int", nullable: false),
                    SupplierOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_BuyerOrders_BuyerOrderId",
                        column: x => x.BuyerOrderId,
                        principalTable: "BuyerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_SupplierOrders_SupplierOrderId",
                        column: x => x.SupplierOrderId,
                        principalTable: "SupplierOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_SupplierProducts_SupplierProductId",
                        column: x => x.SupplierProductId,
                        principalTable: "SupplierProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuyerOrders_BuyerId",
                table: "BuyerOrders",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_BuyerOrderId",
                table: "OrderItems",
                column: "BuyerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SupplierOrderId",
                table: "OrderItems",
                column: "SupplierOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SupplierProductId",
                table: "OrderItems",
                column: "SupplierProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrders_BuyerOrderId",
                table: "SupplierOrders",
                column: "BuyerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrders_SupplierId",
                table: "SupplierOrders",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "SupplierOrders");

            migrationBuilder.DropTable(
                name: "BuyerOrders");
        }
    }
}
