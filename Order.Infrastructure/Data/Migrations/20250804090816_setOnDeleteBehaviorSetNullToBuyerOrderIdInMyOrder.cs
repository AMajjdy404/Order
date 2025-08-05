using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class setOnDeleteBehaviorSetNullToBuyerOrderIdInMyOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyOrders_BuyerOrders_BuyerOrderId",
                table: "MyOrders");

            migrationBuilder.AlterColumn<int>(
                name: "BuyerOrderId",
                table: "MyOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_MyOrders_BuyerOrders_BuyerOrderId",
                table: "MyOrders",
                column: "BuyerOrderId",
                principalTable: "BuyerOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyOrders_BuyerOrders_BuyerOrderId",
                table: "MyOrders");

            migrationBuilder.AlterColumn<int>(
                name: "BuyerOrderId",
                table: "MyOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MyOrders_BuyerOrders_BuyerOrderId",
                table: "MyOrders",
                column: "BuyerOrderId",
                principalTable: "BuyerOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
