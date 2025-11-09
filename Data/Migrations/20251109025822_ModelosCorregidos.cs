using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelosCorregidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoDetalles_Inventario_InventarioId",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "PedidoDetalles");

            migrationBuilder.AlterColumn<int>(
                name: "InventarioId",
                table: "PedidoDetalles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoDetalles_Inventario_InventarioId",
                table: "PedidoDetalles",
                column: "InventarioId",
                principalTable: "Inventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoDetalles_Inventario_InventarioId",
                table: "PedidoDetalles");

            migrationBuilder.AlterColumn<int>(
                name: "InventarioId",
                table: "PedidoDetalles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "PedidoDetalles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoDetalles_Inventario_InventarioId",
                table: "PedidoDetalles",
                column: "InventarioId",
                principalTable: "Inventario",
                principalColumn: "Id");
        }
    }
}
