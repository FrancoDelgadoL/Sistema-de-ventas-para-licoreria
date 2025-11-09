using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class UltimosArreglos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Inventarios_InventarioId",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_InventarioId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "SoloPrimeraCompra",
                table: "Cupones");

            migrationBuilder.DropColumn(
                name: "InventarioId",
                table: "Categorias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SoloPrimeraCompra",
                table: "Cupones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "InventarioId",
                table: "Categorias",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "InventarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "InventarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "InventarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "InventarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "InventarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6,
                column: "InventarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7,
                column: "InventarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 8,
                column: "InventarioId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_InventarioId",
                table: "Categorias",
                column: "InventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Inventarios_InventarioId",
                table: "Categorias",
                column: "InventarioId",
                principalTable: "Inventarios",
                principalColumn: "Id");
        }
    }
}
