using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ezel_Market.Data.Migrations
{
    /// <inheritdoc />
    public partial class Mi3eramigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "c29b1a1f-8b3c-4d5e-9f6a-1b2c3d4e5f6a", "Gerente", "GERENTE" },
                    { "f47ac10b-58cc-4372-a567-0e02b2c3d479", "7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a", "Inventario", "INVENTARIO" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f47ac10b-58cc-4372-a567-0e02b2c3d479");
        }
    }
}
