# Project Context: DemandTrack System

## 1. Project Overview
**DemandTrack** is an enterprise supply chain and procurement tracking web API built on **.NET 10 (ASP.NET Core Web API)** and **PostgreSQL** (using Entity Framework Core and Npgsql).

The application originally focused on book demands and supply shipments, and has undergone an active domain evolution to a unified, multi-category **Stationery & Procurement Demand Management System** featuring:
- Master catalog management (Categories, Stationery & Textbook Items, Suppliers).
- Requisition / Demand management with hierarchical backorder tracking.
- Invoice ingestion, document attachment, and automated / semi-automated reconciliation.
- Support for tabular import/export (Excel via `ClosedXML`, CSV parsing) and document generation/parsing (PDF via `iText` and `QuestPDF`).

---

## 2. Technology Stack & Dependencies

- **Runtime & Framework**: .NET 10.0 (`net10.0`), C# 13, ASP.NET Core Web API
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Tools`)
- **API Documentation**: Swagger / OpenAPI (`Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi`)
- **Document & Spreadsheet Processing**:
  - `ClosedXML` (v0.105.0) - Excel spreadsheet reading/writing
  - `iText` (v9.4.0) - PDF extraction & parsing (used for transcript ingestion)
  - `QuestPDF` (v2025.12.0) - High-performance PDF generation
  - `System.Security.Cryptography.Pkcs`
- **JSON Serialization**: `System.Text.Json` with `ReferenceHandler.IgnoreCycles` to handle relational graph circularity.
- **CORS Support**: Configured for Angular (`http://localhost:4200`) and Vite/React/Vue (`http://localhost:5173`).

---

## 3. Architecture & Domain Model

### 3.1. Master Data Entities
1. **`Category`** (`DemandTrack.Models.Category`)
   - Self-referencing hierarchical structure (`ParentCategoryId`, `ParentCategory`, `SubCategories`).
   - Fields: `Id`, `Name`, `Code`, `Description`, `IsActive`, `CreatedAt`.
2. **`StationeryItem`** (`DemandTrack.Models.StationeryItem`)
   - Represents all catalog items (books, notebooks, pencils, uniform items, consumables).
   - Fields: `Id`, `ItemName`, `ItemDescription`, `Sku`, `Barcode`, `CategoryId`, `UnitOfMeasure`, `CurrentPrice`, `CostPrice`, `MinimumStockLevel`, `ReorderQuantity`, `Grade` (`GradeLevel` enum), `Subject`, `Medium`, `IsActive`, `CreatedAt`, `UpdatedAt`.
3. **`GradeLevel`** (`DemandTrack.Models.GradeLevel`)
   - Standardized academic grade enum (`None`, `Playgroup`, `Nursery`, `Prep`, `Class1` - `Class10`, `OLevels`, `ALevels`, etc.).
4. **`Supplier`** (`DemandTrack.Models.Supplier`)
   - Vendor master data.
   - Fields: `Id`, `Name`, `Code`, `ContactPerson`, `Phone`, `Email`, `Address`, `IsActive`, `CreatedAt`.

### 3.2. Demand & Supply Reconciliation Entities
1. **`Demand`** (`DemandTrack.Models.Demand`)
   - Master requisition or order.
   - Self-referencing link for split shipments: `ParentDemandId` and `Backorders` collection.
   - Fields: `Id`, `DemandNumber` (e.g. `DEM-20261025-XXXX`), `Title`, `SupplierId`, `Status` (`Draft`, `Open`, `PartiallyFulfilled`, `Fulfilled`, `Cancelled`), `CreatedAt`, `TargetDeliveryDate`, `Notes`.
2. **`DemandItem`** (`DemandTrack.Models.DemandItem`)
   - Individual line items within a demand.
   - Fields: `DemandId`, `StationeryItemId`, `RequestedQty`, `SuppliedQty`, `RemainingQty`, `UnitPrice`, `Status` (`Pending`, `Partial`, `Fulfilled`).
3. **`SupplierInvoice`** (`DemandTrack.Models.SupplierInvoice`)
   - Delivery challan or supplier invoice record parsed from uploaded documents (PDF/Excel) or entered manually.
   - Fields: `DemandId`, `SupplierId`, `InvoiceNumber`, `InvoiceDate`, `SourceType` (`Manual`, `PdfUpload`, `ExcelUpload`), `DocumentFilePath`, `TotalAmount`, `Status` (`Uploaded`, `Draft`, `PartiallyReconciled`, `Reconciled`).
4. **`SupplierInvoiceItem`** (`DemandTrack.Models.SupplierInvoiceItem`)
   - Extracted line item from invoice, mapped against `DemandItem` / `StationeryItem`.
   - Fields: `SupplierInvoiceId`, `DemandItemId`, `StationeryItemId`, `RawItemDescription`, `Quantity`, `UnitPrice`, `LineTotal`, `ConfidenceScore`, `IsManuallyConfirmed`.

### 3.3. Legacy & Transition Entities
- **`Book`**: Legacy single-entity book record (`Id`, `Title`, `Author`, `ISBN`, `Publisher`, `Price`, `StockQuantity`).
- **`Shipment` / `ShipmentItem`**: Legacy initial shipment tracking.
- **`SupplyUpload` / `SupplyItem`**: PDF transcript upload records linking back to demand lines for ISBN-based deliveries.

---

## 4. Current Implementation Status

### 4.1. Implemented Controllers & Endpoints
| Controller | Base Route | Key Operations |
| :--- | :--- | :--- |
| **`CategoriesController`** | `api/categories` | • `GET api/categories` (list all, active/tree filter)<br>• `GET api/categories/{id}`<br>• `POST api/categories` (create with parent category validation)<br>• `PUT api/categories/{id}`<br>• `DELETE api/categories/{id}` (soft-delete with child guard) |
| **`SuppliersController`** | `api/suppliers` | • `GET api/suppliers` (list all, stats counts)<br>• `GET api/suppliers/{id}`<br>• `POST api/suppliers` (code uniqueness verification)<br>• `PUT api/suppliers/{id}`<br>• `DELETE api/suppliers/{id}` (soft-delete) |
| **`ItemsController`** | `api/items` | • `GET api/items` (paged search & filter by Category, Grade, Subject, Medium, Query)<br>• `GET api/items/{id}`<br>• `POST api/items` (create with SKU/barcode uniqueness check)<br>• `PUT api/items/{id}`<br>• `DELETE api/items/{id}` (soft-delete)<br>• `POST api/items/import-excel` (bulk import via ClosedXML)<br>• `GET api/items/export-excel` (export template & catalog) |

### 4.2. In-Progress / Transition Areas
- **Demand Management**:
  - `DemandsLegacyController.cs.bak` preserved legacy book/transcript endpoints (PDF transcript parser using iText, book demand creation).
  - Modern `DemandDtos.cs` and `InvoiceDtos.cs` have been defined, covering `DemandListDto`, `DemandDetailDto`, `CreateDemandRequestDto`, `SupplierInvoiceDto`, and `ConfirmReconciliationRequestDto`.
  - Next milestone is the updated `DemandsController` and `InvoicesController` implementing end-to-end reconciliation and automatic backorder generation.

---

## 5. Database Migrations History

1. `20251215103925_InitialMigration`: Initial Book, Demand, DemandItem setup.
2. `20251215201434_AddShippedQty`: Added shipped quantity tracking.
3. `20251216184422_AddSupplyUploadAndSupplyItem`: Separate supply tracking for PDF transcript uploads.
4. `20261025115046_InitialStationeryDemandSetup`: Introduced StationeryItem, Supplier, SupplierInvoice models.
5. `20261025121822_AddStationeryItemAttributesAndGrades`: Added school/education metadata (Grades, Subjects, Mediums).
6. `20261025123051_AddingItemsCategoriestables`: Added category hierarchies and related constraints.
7. `20261025123556_AlignNamingToSuppliersAndRemovePriceLists`: Standardized supplier naming, aligned FK relationships.

---

## 6. Directory Layout

```
DemandTrack/
├── .github/workflows/                 # CI/CD pipelines
├── DemandTrack/                       # Main Web API Project
│   ├── Controllers/                   # API Endpoints
│   │   ├── CategoriesController.cs    # Master categories CRUD
│   │   ├── ItemsController.cs         # Master items CRUD & Excel I/O
│   │   ├── SuppliersController.cs     # Master suppliers CRUD
│   │   └── DemandsLegacyController.cs.bak # Backup of legacy implementation
│   ├── Data/
│   │   └── AppDbContext.cs            # EF Core DB context & relational config
│   ├── Dtos/                          # Request & Response DTOs
│   │   ├── CategoryDtos.cs
│   │   ├── DemandDtos.cs
│   │   ├── DemandResponseDto.cs
│   │   ├── InvoiceDtos.cs
│   │   ├── StationeryItemDtos.cs
│   │   └── SupplierDtos.cs
│   ├── Migrations/                    # EF Core Migrations
│   ├── Models/                        # Domain entities
│   ├── Program.cs                     # Web host, DI, middleware & CORS configuration
│   └── appsettings.json               # Database connections and app settings
├── BOOKS_IMPORT_DOCUMENTATION.md      # Legacy book import documentation
├── REFACTORING_SUMMARY.md             # Upload transcript refactoring summary
└── project context.md                 # System overview and project context (this file)
```

---

## 7. Configuration & Running the Application

### 7.1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- PostgreSQL Server running locally or in a container.

### 7.2. Connection String Configuration
Configured in `DemandTrack/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=DemandTrackDb;Username=postgres;Password=your_password"
}
```

### 7.3. Commands
- **Run project**: `dotnet run --project DemandTrack/DemandTrack.csproj`
- **Apply migrations**: `dotnet ef database update --project DemandTrack/DemandTrack.csproj`
- **Add migration**: `dotnet ef migrations add <MigrationName> --project DemandTrack/DemandTrack.csproj`
- **Access Swagger UI**: `http://localhost:<port>/swagger`
