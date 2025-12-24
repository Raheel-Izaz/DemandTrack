using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DemandTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyUploadAndSupplyItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippedQty",
                table: "DemandItems");

            migrationBuilder.CreateTable(
                name: "SupplyUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplyUploads_Demands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplyUploadId = table.Column<int>(type: "integer", nullable: false),
                    DemandItemId = table.Column<int>(type: "integer", nullable: false),
                    Isbn = table.Column<string>(type: "text", nullable: false),
                    ShippedQty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplyItems_DemandItems_DemandItemId",
                        column: x => x.DemandItemId,
                        principalTable: "DemandItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyItems_SupplyUploads_SupplyUploadId",
                        column: x => x.SupplyUploadId,
                        principalTable: "SupplyUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyItems_DemandItemId",
                table: "SupplyItems",
                column: "DemandItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyItems_SupplyUploadId",
                table: "SupplyItems",
                column: "SupplyUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyUploads_DemandId",
                table: "SupplyUploads",
                column: "DemandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplyItems");

            migrationBuilder.DropTable(
                name: "SupplyUploads");

            migrationBuilder.AddColumn<int>(
                name: "ShippedQty",
                table: "DemandItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
