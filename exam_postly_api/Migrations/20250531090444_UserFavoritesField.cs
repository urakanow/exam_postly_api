using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace exam_postly_api.Migrations
{
    /// <inheritdoc />
    public partial class UserFavoritesField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Offers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offers_UserId1",
                table: "Offers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Users_UserId1",
                table: "Offers",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Users_UserId1",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Offers_UserId1",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Offers");
        }
    }
}
