using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemandTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddStationeryItemAttributesAndGrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorPublisher",
                table: "StationeryItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorOrVariant",
                table: "StationeryItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "StationeryItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Isbn",
                table: "StationeryItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPages",
                table: "StationeryItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrintingYear",
                table: "StationeryItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RulingType",
                table: "StationeryItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeOrSpecification",
                table: "StationeryItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "StationeryItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorPublisher",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "ColorOrVariant",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "Isbn",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "NumberOfPages",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "PrintingYear",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "RulingType",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "SizeOrSpecification",
                table: "StationeryItems");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "StationeryItems");
        }
    }
}
