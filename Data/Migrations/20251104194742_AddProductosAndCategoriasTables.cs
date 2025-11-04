using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductosAndCategoriasTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    Precio = table.Column<decimal>(type: "TEXT", nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriasId = table.Column<int>(type: "INTEGER", nullable: false),
                    Marca = table.Column<string>(type: "TEXT", nullable: true),
                    GradoAlcohol = table.Column<decimal>(type: "TEXT", nullable: true),
                    Imagen = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categoria_CategoriasId",
                        column: x => x.CategoriasId,
                        principalTable: "Categoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categoria",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Bebidas a base de cebada fermentada", "Cervezas" },
                    { 2, "Bebidas a base de uva fermentada", "Vinos" },
                    { 3, "Destilados de uva", "Piscos" },
                    { 4, "Destilados de caña de azúcar", "Rones" },
                    { 5, "Destilados de grano envejecidos en madera", "Whisky" },
                    { 6, "Destilados de agave azul", "Tequila" },
                    { 7, "Destilados blancos y ginebras", "Vodka y Gin" },
                    { 8, "Mezcladores, gaseosas y otros", "Complementos" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriasId",
                table: "Productos",
                column: "CategoriasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Categoria");
        }
    }
}
