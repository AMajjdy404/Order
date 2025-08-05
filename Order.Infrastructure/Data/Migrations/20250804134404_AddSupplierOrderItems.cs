using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_SupplierOrders_SupplierOrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierOrders_BuyerOrders_BuyerOrderId",
                table: "SupplierOrders");

            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "SupplierOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SupplierOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierOrderId = table.Column<int>(type: "int", nullable: false),
                    SupplierProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierOrderItems_SupplierOrders_SupplierOrderId",
                        column: x => x.SupplierOrderId,
                        principalTable: "SupplierOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierOrderItems_SupplierProducts_SupplierProductId",
                        column: x => x.SupplierProductId,
                        principalTable: "SupplierProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrderItems_SupplierOrderId",
                table: "SupplierOrderItems",
                column: "SupplierOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrderItems_SupplierProductId",
                table: "SupplierOrderItems",
                column: "SupplierProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_SupplierOrders_SupplierOrderId",
                table: "OrderItems",
                column: "SupplierOrderId",
                principalTable: "SupplierOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierOrders_BuyerOrders_BuyerOrderId",
                table: "SupplierOrders",
                column: "BuyerOrderId",
                principalTable: "BuyerOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_SupplierOrders_SupplierOrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierOrders_BuyerOrders_BuyerOrderId",
                table: "SupplierOrders");

            migrationBuilder.DropTable(
                name: "SupplierOrderItems");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "SupplierOrders");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_SupplierOrders_SupplierOrderId",
                table: "OrderItems",
                column: "SupplierOrderId",
                principalTable: "SupplierOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierOrders_BuyerOrders_BuyerOrderId",
                table: "SupplierOrders",
                column: "BuyerOrderId",
                principalTable: "BuyerOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
