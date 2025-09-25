using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierDeliveryStationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryStations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDeliveryStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    DeliveryStationId = table.Column<int>(type: "int", nullable: false),
                    MinimumOrderPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDeliveryStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDeliveryStations_DeliveryStations_DeliveryStationId",
                        column: x => x.DeliveryStationId,
                        principalTable: "DeliveryStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierDeliveryStations_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDeliveryStations_DeliveryStationId",
                table: "SupplierDeliveryStations",
                column: "DeliveryStationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDeliveryStations_SupplierId",
                table: "SupplierDeliveryStations",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierDeliveryStations");

            migrationBuilder.DropTable(
                name: "DeliveryStations");
        }
    }
}
