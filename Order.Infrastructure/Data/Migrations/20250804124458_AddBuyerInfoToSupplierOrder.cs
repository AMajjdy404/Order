using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyerInfoToSupplierOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "SupplierOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                table: "SupplierOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropertyAddress",
                table: "SupplierOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropertyLocation",
                table: "SupplierOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                table: "SupplierOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "PropertyAddress",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "PropertyLocation",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "PropertyName",
                table: "SupplierOrders");
        }
    }
}
