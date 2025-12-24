using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemandTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddShippedQty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShippedQty",
                table: "DemandItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippedQty",
                table: "DemandItems");
        }
    }
}
