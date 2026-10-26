using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DemandTrack.Migrations
{
    /// <inheritdoc />
    public partial class InitialStationeryDemandSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandItems_Books_BookId",
                table: "DemandItems");

            migrationBuilder.DropIndex(
                name: "IX_DemandItems_BookId",
                table: "DemandItems");

            migrationBuilder.RenameColumn(
                name: "SeasonName",
                table: "Demands",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "DemandItems",
                newName: "SuppliedQty");

            migrationBuilder.AddColumn<string>(
                name: "DemandNumber",
                table: "Demands",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Demands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentDemandId",
                table: "Demands",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Demands",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Demands",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetDeliveryDate",
                table: "Demands",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemainingQty",
                table: "DemandItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StationeryItemId",
                table: "DemandItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DemandItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "DemandItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    ContactPerson = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StationeryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    DefaultUnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationeryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationeryItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorInvoices",
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
                name: "SupplierPriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    StationeryItemId = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MinimumOrderQuantity = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "VendorInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorInvoiceId = table.Column<int>(type: "integer", nullable: false),
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
                name: "IX_Demands_ParentDemandId",
                table: "Demands",
                column: "ParentDemandId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_SupplierId",
                table: "Demands",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandItems_StationeryItemId",
                table: "DemandItems",
                column: "StationeryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StationeryItems_CategoryId",
                table: "StationeryItems",
                column: "CategoryId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_DemandItems_StationeryItems_StationeryItemId",
                table: "DemandItems",
                column: "StationeryItemId",
                principalTable: "StationeryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Demands_ParentDemandId",
                table: "Demands",
                column: "ParentDemandId",
                principalTable: "Demands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Suppliers_SupplierId",
                table: "Demands",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandItems_StationeryItems_StationeryItemId",
                table: "DemandItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Demands_ParentDemandId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Suppliers_SupplierId",
                table: "Demands");

            migrationBuilder.DropTable(
                name: "SupplierPriceLists");

            migrationBuilder.DropTable(
                name: "VendorInvoiceItems");

            migrationBuilder.DropTable(
                name: "StationeryItems");

            migrationBuilder.DropTable(
                name: "VendorInvoices");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Demands_ParentDemandId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_SupplierId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_DemandItems_StationeryItemId",
                table: "DemandItems");

            migrationBuilder.DropColumn(
                name: "DemandNumber",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "ParentDemandId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "TargetDeliveryDate",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "RemainingQty",
                table: "DemandItems");

            migrationBuilder.DropColumn(
                name: "StationeryItemId",
                table: "DemandItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DemandItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "DemandItems");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Demands",
                newName: "SeasonName");

            migrationBuilder.RenameColumn(
                name: "SuppliedQty",
                table: "DemandItems",
                newName: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandItems_BookId",
                table: "DemandItems",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_DemandItems_Books_BookId",
                table: "DemandItems",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
