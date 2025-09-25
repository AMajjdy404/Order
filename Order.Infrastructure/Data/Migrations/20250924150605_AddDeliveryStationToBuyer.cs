using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryStationToBuyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryStationId",
                table: "Buyers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_DeliveryStationId",
                table: "Buyers",
                column: "DeliveryStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buyers_DeliveryStations_DeliveryStationId",
                table: "Buyers",
                column: "DeliveryStationId",
                principalTable: "DeliveryStations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buyers_DeliveryStations_DeliveryStationId",
                table: "Buyers");

            migrationBuilder.DropIndex(
                name: "IX_Buyers_DeliveryStationId",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "DeliveryStationId",
                table: "Buyers");
        }
    }
}
