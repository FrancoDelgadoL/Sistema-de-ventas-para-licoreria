using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCarritoRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrito_Inventario_InventarioId",
                table: "Carrito");

            migrationBuilder.DropIndex(
                name: "IX_Carrito_UsuarioId_ProductoId",
                table: "Carrito");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Carrito");

            migrationBuilder.AlterColumn<int>(
                name: "InventarioId",
                table: "Carrito",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carrito_UsuarioId_InventarioId",
                table: "Carrito",
                columns: new[] { "UsuarioId", "InventarioId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Carrito_Inventario_InventarioId",
                table: "Carrito",
                column: "InventarioId",
                principalTable: "Inventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrito_Inventario_InventarioId",
                table: "Carrito");

            migrationBuilder.DropIndex(
                name: "IX_Carrito_UsuarioId_InventarioId",
                table: "Carrito");

            migrationBuilder.AlterColumn<int>(
                name: "InventarioId",
                table: "Carrito",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Carrito",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Carrito_UsuarioId_ProductoId",
                table: "Carrito",
                columns: new[] { "UsuarioId", "ProductoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Carrito_Inventario_InventarioId",
                table: "Carrito",
                column: "InventarioId",
                principalTable: "Inventario",
                principalColumn: "Id");
        }
    }
}
