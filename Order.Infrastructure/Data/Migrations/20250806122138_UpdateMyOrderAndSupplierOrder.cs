using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMyOrderAndSupplierOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MyOrderId",
                table: "SupplierOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WalletPaymentAmount",
                table: "SupplierOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MyOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "MyOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "MyOrderItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MyOrderId = table.Column<int>(type: "int", nullable: false),
                    SupplierProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyOrderItem_MyOrders_MyOrderId",
                        column: x => x.MyOrderId,
                        principalTable: "MyOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MyOrderItem_MyOrderId",
                table: "MyOrderItem",
                column: "MyOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyOrderItem");

            migrationBuilder.DropColumn(
                name: "MyOrderId",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "WalletPaymentAmount",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MyOrders");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "MyOrders");
        }
    }
}
