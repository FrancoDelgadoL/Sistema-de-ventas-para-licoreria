using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class LibreDeErrores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrito_Inventario_InventarioId",
                table: "Carrito");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialInventarios_Inventario_InventarioId",
                table: "HistorialInventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventario_Categorias_CategoriasId",
                table: "Inventario");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidoDetalles_Inventario_InventarioId",
                table: "PedidoDetalles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventario",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "CategoriasId",
                table: "Inventario");

            migrationBuilder.RenameTable(
                name: "Inventario",
                newName: "Inventarios");

            migrationBuilder.RenameColumn(
                name: "CategoriasId",
                table: "Categorias",
                newName: "InventarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Categorias_CategoriasId",
                table: "Categorias",
                newName: "IX_Categorias_InventarioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CategoriaInventarios",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    InventarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaInventarios", x => new { x.CategoriaId, x.InventarioId });
                    table.ForeignKey(
                        name: "FK_CategoriaInventarios_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoriaInventarios_Inventarios_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaInventarios_InventarioId",
                table: "CategoriaInventarios",
                column: "InventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carrito_Inventarios_InventarioId",
                table: "Carrito",
                column: "InventarioId",
                principalTable: "Inventarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Inventarios_InventarioId",
                table: "Categorias",
                column: "InventarioId",
                principalTable: "Inventarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialInventarios_Inventarios_InventarioId",
                table: "HistorialInventarios",
                column: "InventarioId",
                principalTable: "Inventarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoDetalles_Inventarios_InventarioId",
                table: "PedidoDetalles",
                column: "InventarioId",
                principalTable: "Inventarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrito_Inventarios_InventarioId",
                table: "Carrito");

            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Inventarios_InventarioId",
                table: "Categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialInventarios_Inventarios_InventarioId",
                table: "HistorialInventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidoDetalles_Inventarios_InventarioId",
                table: "PedidoDetalles");

            migrationBuilder.DropTable(
                name: "CategoriaInventarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios");

            migrationBuilder.RenameTable(
                name: "Inventarios",
                newName: "Inventario");

            migrationBuilder.RenameColumn(
                name: "InventarioId",
                table: "Categorias",
                newName: "CategoriasId");

            migrationBuilder.RenameIndex(
                name: "IX_Categorias_InventarioId",
                table: "Categorias",
                newName: "IX_Categorias_CategoriasId");

            migrationBuilder.AddColumn<int>(
                name: "CategoriasId",
                table: "Inventario",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventario",
                table: "Inventario",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carrito_Inventario_InventarioId",
                table: "Carrito",
                column: "InventarioId",
                principalTable: "Inventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialInventarios_Inventario_InventarioId",
                table: "HistorialInventarios",
                column: "InventarioId",
                principalTable: "Inventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventario_Categorias_CategoriasId",
                table: "Inventario",
                column: "CategoriasId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoDetalles_Inventario_InventarioId",
                table: "PedidoDetalles",
                column: "InventarioId",
                principalTable: "Inventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
