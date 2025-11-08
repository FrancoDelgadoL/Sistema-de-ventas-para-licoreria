using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaHistorialInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioVentaMayorista",
                table: "Inventario",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "HistorialInventarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InventarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CantidadAnterior = table.Column<int>(type: "INTEGER", nullable: false),
                    CantidadNueva = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "TEXT", nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialInventarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialInventarios_Inventario_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialInventarios_InventarioId",
                table: "HistorialInventarios",
                column: "InventarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialInventarios");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioVentaMayorista",
                table: "Inventario",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");
        }
    }
}
