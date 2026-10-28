using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DemandTrack.Migrations
{
    /// <inheritdoc />
    public partial class Add_Season_And_Demand_Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Demands_ParentDemandId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Suppliers_SupplierId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_ParentDemandId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_DemandItems_DemandId",
                table: "DemandItems");

            migrationBuilder.DropColumn(
                name: "ParentDemandId",
                table: "Demands");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Demands",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TargetDeliveryDate",
                table: "Demands",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Demands",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""Seasons"" (""Name"", ""StartDate"", ""EndDate"", ""IsActive"", ""IsDefault"", ""CreatedAt"")
                SELECT '2026-2027', '2026-01-01', '2027-12-31', true, true, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Seasons"");
            ");

            // Drop column default before changing type, map existing string values, then alter type
            migrationBuilder.Sql(@"
                ALTER TABLE ""Demands"" ALTER COLUMN ""Status"" DROP DEFAULT;
                UPDATE ""Demands"" SET ""Status"" = '0' WHERE ""Status"" = 'Draft' OR ""Status"" IS NULL;
                UPDATE ""Demands"" SET ""Status"" = '1' WHERE ""Status"" IN ('Open', 'SentToSupplier');
                UPDATE ""Demands"" SET ""Status"" = '2' WHERE ""Status"" = 'PartiallyFulfilled';
                UPDATE ""Demands"" SET ""Status"" = '3' WHERE ""Status"" = 'Fulfilled';
                UPDATE ""Demands"" SET ""Status"" = '4' WHERE ""Status"" = 'Cancelled';
                ALTER TABLE ""Demands"" ALTER COLUMN ""Status"" TYPE integer USING (""Status""::integer);
                ALTER TABLE ""Demands"" ALTER COLUMN ""Status"" SET DEFAULT 0;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "DemandNumber",
                table: "Demands",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            // Add nullable column first, backfill with default season ID, then set NOT NULL
            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Demands",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Demands""
                SET ""SeasonId"" = (SELECT ""Id"" FROM ""Seasons"" ORDER BY ""Id"" ASC LIMIT 1)
                WHERE ""SeasonId"" IS NULL;
                ALTER TABLE ""Demands"" ALTER COLUMN ""SeasonId"" SET NOT NULL;
            ");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Demands",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(@"
                ALTER TABLE ""DemandItems"" ALTER COLUMN ""Status"" DROP DEFAULT;
                UPDATE ""DemandItems"" SET ""Status"" = '0' WHERE ""Status"" = 'Pending' OR ""Status"" IS NULL;
                UPDATE ""DemandItems"" SET ""Status"" = '1' WHERE ""Status"" IN ('Partial', 'PartiallyReceived');
                UPDATE ""DemandItems"" SET ""Status"" = '2' WHERE ""Status"" IN ('Fulfilled', 'Completed', 'Backordered');
                ALTER TABLE ""DemandItems"" ALTER COLUMN ""Status"" TYPE integer USING (""Status""::integer);
                ALTER TABLE ""DemandItems"" ALTER COLUMN ""Status"" SET DEFAULT 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_DemandNumber",
                table: "Demands",
                column: "DemandNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Demands_SeasonId",
                table: "Demands",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandItems_DemandId_ProductId",
                table: "DemandItems",
                columns: new[] { "DemandId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_IsDefault",
                table: "Seasons",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Name",
                table: "Seasons",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Seasons_SeasonId",
                table: "Demands",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Suppliers_SupplierId",
                table: "Demands",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Seasons_SeasonId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Suppliers_SupplierId",
                table: "Demands");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Demands_DemandNumber",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_SeasonId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_DemandItems_DemandId_ProductId",
                table: "DemandItems");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Demands");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Demands",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TargetDeliveryDate",
                table: "Demands",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Demands",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Demands",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DemandNumber",
                table: "Demands",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "ParentDemandId",
                table: "Demands",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DemandItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_ParentDemandId",
                table: "Demands",
                column: "ParentDemandId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandItems_DemandId",
                table: "DemandItems",
                column: "DemandId");

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
    }
}
