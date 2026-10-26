using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DemandTrack.Migrations
{
    /// <inheritdoc />
    public partial class AlignNamingToSuppliersAndRemovePriceLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierPriceLists");

            migrationBuilder.DropTable(
                name: "VendorInvoiceItems");

            migrationBuilder.DropTable(
                name: "VendorInvoices");

            migrationBuilder.CreateTable(
                name: "SupplierInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    DocumentFilePath = table.Column<string>(type: "text", nullable: true),
                    RawOcrText = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierInvoices_Demands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierInvoices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SupplierInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    DemandItemId = table.Column<int>(type: "integer", nullable: true),
                    StationeryItemId = table.Column<int>(type: "integer", nullable: true),
                    RawItemDescription = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    IsManuallyConfirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierInvoiceItems_DemandItems_DemandItemId",
                        column: x => x.DemandItemId,
                        principalTable: "DemandItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplierInvoiceItems_StationeryItems_StationeryItemId",
                        column: x => x.StationeryItemId,
                        principalTable: "StationeryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplierInvoiceItems_SupplierInvoices_SupplierInvoiceId",
                        column: x => x.SupplierInvoiceId,
                        principalTable: "SupplierInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoiceItems_DemandItemId",
                table: "SupplierInvoiceItems",
                column: "DemandItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoiceItems_StationeryItemId",
                table: "SupplierInvoiceItems",
                column: "StationeryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoiceItems_SupplierInvoiceId",
                table: "SupplierInvoiceItems",
                column: "SupplierInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoices_DemandId",
                table: "SupplierInvoices",
                column: "DemandId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoices_SupplierId",
                table: "SupplierInvoices",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierInvoiceItems");

            migrationBuilder.DropTable(
                name: "SupplierInvoices");

            migrationBuilder.CreateTable(
                name: "SupplierPriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StationeryItemId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumOrderQuantity = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPriceLists_StationeryItems_StationeryItemId",
                        column: x => x.StationeryItemId,
                        principalTable: "StationeryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierPriceLists_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    DocumentFilePath = table.Column<string>(type: "text", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    RawOcrText = table.Column<string>(type: "text", nullable: true),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorInvoices_Demands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorInvoices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VendorInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandItemId = table.Column<int>(type: "integer", nullable: true),
                    StationeryItemId = table.Column<int>(type: "integer", nullable: true),
                    VendorInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    IsManuallyConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    RawItemDescription = table.Column<string>(type: "text", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorInvoiceItems_DemandItems_DemandItemId",
                        column: x => x.DemandItemId,
                        principalTable: "DemandItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VendorInvoiceItems_StationeryItems_StationeryItemId",
                        column: x => x.StationeryItemId,
                        principalTable: "StationeryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VendorInvoiceItems_VendorInvoices_VendorInvoiceId",
                        column: x => x.VendorInvoiceId,
                        principalTable: "VendorInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPriceLists_StationeryItemId",
                table: "SupplierPriceLists",
                column: "StationeryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPriceLists_SupplierId",
                table: "SupplierPriceLists",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoiceItems_DemandItemId",
                table: "VendorInvoiceItems",
                column: "DemandItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoiceItems_StationeryItemId",
                table: "VendorInvoiceItems",
                column: "StationeryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoiceItems_VendorInvoiceId",
                table: "VendorInvoiceItems",
                column: "VendorInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoices_DemandId",
                table: "VendorInvoices",
                column: "DemandId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoices_SupplierId",
                table: "VendorInvoices",
                column: "SupplierId");
        }
    }
}
