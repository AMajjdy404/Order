using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMyOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyOrders_SupplierProducts_SupplierProductId",
                table: "MyOrders");

            migrationBuilder.DropIndex(
                name: "IX_MyOrders_SupplierProductId",
                table: "MyOrders");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "MyOrders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "MyOrders");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "MyOrders");

            migrationBuilder.DropColumn(
                name: "SupplierProductId",
                table: "MyOrders");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "MyOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "MyOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "MyOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "MyOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SupplierProductId",
                table: "MyOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "MyOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_MyOrders_SupplierProductId",
                table: "MyOrders",
                column: "SupplierProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_MyOrders_SupplierProducts_SupplierProductId",
                table: "MyOrders",
                column: "SupplierProductId",
                principalTable: "SupplierProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
