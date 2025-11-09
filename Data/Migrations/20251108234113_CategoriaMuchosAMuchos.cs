using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoriaMuchosAMuchos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventario_Categoria_CategoriasId",
                table: "Inventario");

            migrationBuilder.DropIndex(
                name: "IX_Inventario_CategoriasId",
                table: "Inventario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categoria",
                table: "Categoria");

            migrationBuilder.DropColumn(
                name: "CategoriasId",
                table: "Inventario");

            migrationBuilder.RenameTable(
                name: "Categoria",
                newName: "Categorias");

            migrationBuilder.AddColumn<int>(
                name: "CategoriasId",
                table: "Categorias",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categorias",
                table: "Categorias",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CategoriasInventario",
                columns: table => new
                {
                    CategoriasId = table.Column<int>(type: "INTEGER", nullable: false),
                    InventarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasInventario", x => new { x.CategoriasId, x.InventarioId });
                    table.ForeignKey(
                        name: "FK_CategoriasInventario_Categorias_CategoriasId",
                        column: x => x.CategoriasId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoriasInventario_Inventario_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "CategoriasId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "CategoriasId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "CategoriasId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "CategoriasId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "CategoriasId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6,
                column: "CategoriasId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7,
                column: "CategoriasId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 8,
                column: "CategoriasId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_CategoriasId",
                table: "Categorias",
                column: "CategoriasId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasInventario_InventarioId",
                table: "CategoriasInventario",
                column: "InventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_CategoriasId",
                table: "Categorias",
                column: "CategoriasId",
                principalTable: "Categorias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Categorias_CategoriasId",
                table: "Categorias");

            migrationBuilder.DropTable(
                name: "CategoriasInventario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categorias",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_CategoriasId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "CategoriasId",
                table: "Categorias");

            migrationBuilder.RenameTable(
                name: "Categorias",
                newName: "Categoria");

            migrationBuilder.AddColumn<int>(
                name: "CategoriasId",
                table: "Inventario",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categoria",
                table: "Categoria",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_CategoriasId",
                table: "Inventario",
                column: "CategoriasId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventario_Categoria_CategoriasId",
                table: "Inventario",
                column: "CategoriasId",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
