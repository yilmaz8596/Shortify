using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortify.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urls_AspNetUsers_UserId1",
                table: "Urls");

            migrationBuilder.DropIndex(
                name: "IX_Urls_UserId1",
                table: "Urls");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Urls");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Urls",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Urls_UserId",
                table: "Urls",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_AspNetUsers_UserId",
                table: "Urls",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urls_AspNetUsers_UserId",
                table: "Urls");

            migrationBuilder.DropIndex(
                name: "IX_Urls_UserId",
                table: "Urls");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Urls",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Urls",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Urls_UserId1",
                table: "Urls",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_AspNetUsers_UserId1",
                table: "Urls",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
