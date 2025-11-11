using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventarioToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bebida",
                table: "Reviews");

            migrationBuilder.AddColumn<int>(
                name: "InventarioId",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_InventarioId",
                table: "Reviews",
                column: "InventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Inventarios_InventarioId",
                table: "Reviews",
                column: "InventarioId",
                principalTable: "Inventarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Inventarios_InventarioId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_InventarioId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "InventarioId",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "Bebida",
                table: "Reviews",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
