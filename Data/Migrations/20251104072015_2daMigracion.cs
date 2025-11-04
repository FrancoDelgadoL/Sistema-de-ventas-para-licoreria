using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class _2daMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2c040acd-d2fb-43ef-5fc5-c2e3f886ff01", "f7a8b9c0-d1e2-4f5a-8b7c-9d0e1f2a3b4c", "Cliente", "CLIENTE" },
                    { "3a8e1fdb-7c2d-4a5e-8f1c-9d3b2a1edf5c", "e6d5c4b3-a2b1-4c8d-9e0f-1a2b3c4d5e6f", "Administrador", "ADMINISTRADOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c040acd-d2fb-43ef-5fc5-c2e3f886ff01");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3a8e1fdb-7c2d-4a5e-8f1c-9d3b2a1edf5c");
        }
    }
}
