using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Migrations
{
    /// <inheritdoc />
    public partial class LinkOrderToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing orders before applying schema changes
            migrationBuilder.Sql("DELETE FROM \"Orders\";");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "GuestName",
                table: "Orders",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Orders",
                newName: "GuestName");

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
