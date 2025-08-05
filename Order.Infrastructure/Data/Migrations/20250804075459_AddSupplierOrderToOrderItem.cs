using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierOrderToOrderItem : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierOrders_Suppliers_SupplierId",
                table: "SupplierOrders");

            migrationBuilder.CreateIndex(
                name: "IX_MyOrders_BuyerOrderId",
                table: "MyOrders",
                column: "BuyerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MyOrders_SupplierProductId",
                table: "MyOrders",
                column: "SupplierProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_MyOrders_BuyerOrders_BuyerOrderId",
                table: "MyOrders",
                column: "BuyerOrderId",
                principalTable: "BuyerOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MyOrders_SupplierProducts_SupplierProductId",
                table: "MyOrders",
                column: "SupplierProductId",
                principalTable: "SupplierProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierOrders_Suppliers_SupplierId",
                table: "SupplierOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyOrders_BuyerOrders_BuyerOrderId",
                table: "MyOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_MyOrders_SupplierProducts_SupplierProductId",
                table: "MyOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_SupplierOrders_SupplierOrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierOrders_BuyerOrders_BuyerOrderId",
                table: "SupplierOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierOrders_Suppliers_SupplierId",
                table: "SupplierOrders");

            migrationBuilder.DropIndex(
                name: "IX_MyOrders_BuyerOrderId",
                table: "MyOrders");

            migrationBuilder.DropIndex(
                name: "IX_MyOrders_SupplierProductId",
                table: "MyOrders");

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
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierOrders_Suppliers_SupplierId",
                table: "SupplierOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
