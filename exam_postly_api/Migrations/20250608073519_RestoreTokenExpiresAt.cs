using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace exam_postly_api.Migrations
{
    /// <inheritdoc />
    public partial class RestoreTokenExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RestoreTokens",
                newName: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "RestoreTokens",
                newName: "CreatedAt");
        }
    }
}
