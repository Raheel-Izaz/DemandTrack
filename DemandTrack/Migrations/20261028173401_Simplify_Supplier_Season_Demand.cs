using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemandTrack.Migrations
{
    /// <inheritdoc />
    public partial class Simplify_Supplier_Season_Demand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "TargetDeliveryDate",
                table: "Demands");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Suppliers",
                newName: "Contact");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Contact",
                table: "Suppliers",
                newName: "Phone");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Suppliers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Suppliers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Suppliers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Suppliers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Seasons",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Seasons",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "TargetDeliveryDate",
                table: "Demands",
                type: "date",
                nullable: true);
        }
    }
}
