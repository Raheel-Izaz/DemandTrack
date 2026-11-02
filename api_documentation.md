# DemandTrack API Documentation & Frontend Implementation Guide

This document provides complete documentation for the DemandTrack REST API, including base URLs, request/response models, query parameters, HTTP status codes, JSON payload examples, and TypeScript interfaces tailored for frontend developers (Angular, React, Vue, etc.).

---

## 1. General Configuration & Conventions

### Base URL
- **Local Development**: `https://localhost:7196` or `http://localhost:5246` (see your `launchSettings.json`)
- **API Prefix**: `/api`

### CORS Configuration
Configured out of the box for the following frontend development servers:
- `http://localhost:4200` (Angular)
- `http://localhost:5173` (Vite / React / Vue)

### Headers
| Header | Description | Default / Value |
| :--- | :--- | :--- |
| `Content-Type` | MIME format for JSON payloads | `application/json` |
| `Accept` | Desired response format | `application/json` |

---

## 2. API Endpoints Overview

| Area | HTTP Method | Endpoint | Description |
| :--- | :--- | :--- | :--- |
| **Categories** | `GET` | `/api/categories` | List all categories (flat or tree filtering) |
| | `GET` | `/api/categories/{id}` | Get category by ID |
| | `POST` | `/api/categories` | Create a new category |
| | `PUT` | `/api/categories/{id}` | Update an existing category |
| | `DELETE` | `/api/categories/{id}` | Delete category (soft-deactivates if items exist) |
| **Suppliers** | `GET` | `/api/suppliers` | List all suppliers (name & optional contact) |
| | `GET` | `/api/suppliers/{id}` | Get supplier by ID |
| | `POST` | `/api/suppliers` | Create a new supplier |
| | `PUT` | `/api/suppliers/{id}` | Update an existing supplier |
| | `DELETE` | `/api/suppliers/{id}` | Delete supplier (soft-deactivates if records exist) |
| **Seasons** | `GET` | `/api/seasons` | List all academic/procurement seasons |
| | `GET` | `/api/seasons/default` | Get the currently active default season |
| | `POST` | `/api/seasons` | Create a new season |
| | `PUT` | `/api/seasons/{id}/set-default` | Set a season as the system default |
| **Products & Catalog** | `GET` | `/api/products` | Paged search, filter by category/grade/status |
| | `GET` | `/api/products/{id}` | Get product details with inventory snapshot |
| | `POST` | `/api/products` | Create product with optional initial inventory & JSON attributes |
| | `PUT` | `/api/products/{id}` | Update product information |
| | `DELETE` | `/api/products/{id}` | Delete product (soft-deactivates if demand history exists) |
| | `POST` | `/api/products/{id}/adjust-inventory` | Adjust inventory quantity with audit record |
| | `GET` | `/api/products/{id}/transactions` | Get inventory transaction history for a product |
| | `GET` | `/api/products/grades` | List all active school/curriculum grades |
| | `POST` | `/api/products/grades` | Create a new grade |
| | `GET` | `/api/products/export-excel` | Download product catalog as an `.xlsx` file |
| | `POST` | `/api/products/import-excel` | Multipart file upload for bulk importing products from Excel |
| **Demands (Requisitions)** | `GET` | `/api/demands` | Paginated demand list with filtering (season, supplier, status, search) |
| | `GET` | `/api/demands/{id}` | Get detailed demand with line items and totals |
| | `POST` | `/api/demands` | Create a new demand requisition (Draft or Open) |
| | `PUT` | `/api/demands/{id}` | Update demand header and line items (if editable) |
| | `PATCH` | `/api/demands/{id}/status` | Update demand status (e.g. Open, Cancelled) |
| | `GET` | `/api/demands/{id}/export/pdf` | Export quantity-only purchase demand document as PDF |
| | `GET` | `/api/demands/{id}/export/excel` | Export quantity-only purchase demand document as Excel (.xlsx) |
| **Invoices & Reconciliation** | `POST` | `/api/invoices/reconcile` *(DTO Spec)* | Reconcile invoice lines against demands & generate backorders |

---

## 3. Categories API (`/api/categories`)

### 3.1. Get All Categories
- **Method**: `GET`
- **Route**: `/api/categories`
- **Query Parameters**:
  - `activeOnly` (optional, `boolean`, default: `true`): If `true`, returns only active categories.

#### Example Request
```http
GET /api/categories?activeOnly=true HTTP/1.1
Host: localhost:7196
Accept: application/json
```

#### Example Response (`200 OK`)
```json
[
  {
    "id": 1,
    "name": "Textbooks",
    "code": "CAT-TXT",
    "description": "Curriculum books and syllabus materials",
    "isActive": true,
    "parentCategoryId": null,
    "parentCategoryName": null,
    "subCategoriesCount": 2,
    "itemsCount": 45
  },
  {
    "id": 2,
    "name": "Stationery & Consumables",
    "code": "CAT-STN",
    "description": "Notebooks, pens, pencils, geometry kits",
    "isActive": true,
    "parentCategoryId": null,
    "parentCategoryName": null,
    "subCategoriesCount": 0,
    "itemsCount": 120
  }
]
```

---

### 3.2. Get Category By ID
- **Method**: `GET`
- **Route**: `/api/categories/{id}`

#### Example Response (`200 OK`)
```json
{
  "id": 1,
  "name": "Textbooks",
  "code": "CAT-TXT",
  "description": "Curriculum books and syllabus materials",
  "isActive": true,
  "parentCategoryId": null,
  "parentCategoryName": null,
  "subCategoriesCount": 2,
  "itemsCount": 45
}
```

---

### 3.3. Create Category
- **Method**: `POST`
- **Route**: `/api/categories`

#### Request Payload
```json
{
  "name": "Science Kits",
  "code": "CAT-SCI",
  "description": "Lab apparatus and student biology kits",
  "parentCategoryId": null
}
```

#### Response (`201 Created`)
```json
{
  "id": 3,
  "name": "Science Kits",
  "code": "CAT-SCI",
  "description": "Lab apparatus and student biology kits",
  "isActive": true,
  "parentCategoryId": null,
  "parentCategoryName": null,
  "subCategoriesCount": 0,
  "itemsCount": 0
}
```

---

### 3.4. Update Category
- **Method**: `PUT`
- **Route**: `/api/categories/{id}`

#### Request Payload
```json
{
  "name": "Science & Lab Equipment",
  "code": "CAT-SCI-EQ",
  "description": "Updated description",
  "parentCategoryId": null,
  "isActive": true
}
```

#### Response
- `204 No Content` on success.
- `404 Not Found` if category does not exist.

---

### 3.5. Delete Category
- **Method**: `DELETE`
- **Route**: `/api/categories/{id}`

#### Behavior
- If products or subcategories exist under this category, the category is **soft-deactivated** (`isActive = false`) and returns `200 OK`:
  ```json
  {
    "message": "Category has related products or subcategories, so it was marked as inactive instead of permanently deleted."
  }
  ```
- If no child records exist, permanently deletes and returns `204 No Content`.

---

## 4. Suppliers API (`/api/suppliers`)

The supplier model is streamlined to keep only essentials: **Name** and optional **Contact**.

### 4.1. Get All Suppliers
- **Method**: `GET`
- **Route**: `/api/suppliers`
- **Query Parameters**:
  - `activeOnly` (optional, `boolean`, default: `true`): Return only active suppliers.

#### Example Response (`200 OK`)
```json
[
  {
    "id": 1,
    "name": "Oxford University Press",
    "contact": "+92 300 1234567 / orders@oup.example.com",
    "isActive": true,
    "demandsCount": 4,
    "invoicesCount": 2
  },
  {
    "id": 2,
    "name": "National Book Foundation",
    "contact": "procurement@nbf.gov.pk",
    "isActive": true,
    "demandsCount": 1,
    "invoicesCount": 0
  }
]
```

---

### 4.2. Get Supplier By ID
- **Method**: `GET`
- **Route**: `/api/suppliers/{id}`

#### Example Response (`200 OK`)
```json
{
  "id": 1,
  "name": "Oxford University Press",
  "contact": "+92 300 1234567 / orders@oup.example.com",
  "isActive": true,
  "demandsCount": 4,
  "invoicesCount": 2
}
```

---

### 4.3. Create Supplier
- **Method**: `POST`
- **Route**: `/api/suppliers`

#### Request Payload (`CreateSupplierDto`)
```json
{
  "name": "Paramount Books",
  "contact": "+92 21 34384620"
}
```

#### Response (`201 Created`)
```json
{
  "id": 3,
  "name": "Paramount Books",
  "contact": "+92 21 34384620",
  "isActive": true,
  "demandsCount": 0,
  "invoicesCount": 0
}
```

---

### 4.4. Update Supplier
- **Method**: `PUT`
- **Route**: `/api/suppliers/{id}`

#### Request Payload (`UpdateSupplierDto`)
```json
{
  "name": "Paramount Publishing House",
  "contact": "sales@paramount.example.com",
  "isActive": true
}
```

#### Response
- `204 No Content` on success.
- `404 Not Found` if supplier does not exist.

---

### 4.5. Delete Supplier
- **Method**: `DELETE`
- **Route**: `/api/suppliers/{id}`

#### Behavior
- If demands or invoices are linked, marks supplier `isActive = false` and returns `200 OK`:
  ```json
  {
    "message": "Supplier has linked demands or invoices, so it was marked as inactive instead of permanently deleted."
  }
  ```
- If no records linked, permanently deletes and returns `204 No Content`.

---

## 5. Seasons API (`/api/seasons`)

Manages academic or procurement seasons (e.g., `"2026-2027"`). A single season can be marked as default.

### 5.1. List All Seasons
- **Method**: `GET`
- **Route**: `/api/seasons`

#### Example Response (`200 OK`)
```json
[
  {
    "id": 1,
    "name": "2026-2027",
    "isActive": true,
    "isDefault": true
  },
  {
    "id": 2,
    "name": "2025-2026",
    "isActive": true,
    "isDefault": false
  }
]
```

---

### 5.2. Get Default Season
- **Method**: `GET`
- **Route**: `/api/seasons/default`

#### Example Response (`200 OK`)
```json
{
  "id": 1,
  "name": "2026-2027",
  "isActive": true,
  "isDefault": true
}
```

#### Response Codes
- `200 OK`: Returned when an active default season exists.
- `404 Not Found`: If no season is currently flagged as default.

---

### 5.3. Create Season
- **Method**: `POST`
- **Route**: `/api/seasons`

#### Request Payload (`CreateSeasonRequest`)
```json
{
  "name": "2027-2028",
  "isDefault": false
}
```

#### Response (`201 Created`)
```json
{
  "id": 3,
  "name": "2027-2028",
  "isActive": true,
  "isDefault": false
}
```

> **Note**: Setting `"isDefault": true` during creation will automatically unset `isDefault` on all other seasons in an atomic transaction.

---

### 5.4. Set Default Season
- **Method**: `PUT`
- **Route**: `/api/seasons/{id}/set-default`

Atomically marks the specified season as default and unsets all other seasons.

#### Example Response (`200 OK`)
```json
{
  "id": 3,
  "name": "2027-2028",
  "isActive": true,
  "isDefault": true
}
```

---

## 6. Products & Catalog API (`/api/products`)

### 6.1. List Products (Paginated & Filtered)
- **Method**: `GET`
- **Route**: `/api/products`
- **Query Parameters**:
  - `search` (optional, `string`): Fuzzy search across `Name`, `Sku`, and `Barcode`.
  - `categoryId` (optional, `int`): Filters by category.
  - `gradeId` (optional, `int`): Filters by grade level.
  - `isActive` (optional, `bool`): Default `true`. Set `false` to include/filter archived products.
  - `page` (optional, `int`, default: `1`): 1-based page number.
  - `pageSize` (optional, `int`, default: `10`): Number of items per page (maximum: `100`).

#### Example Request
```http
GET /api/products?search=notebook&page=1&pageSize=10 HTTP/1.1
Host: localhost:7196
Accept: application/json
```

#### Example Response (`200 OK`)
```json
{
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1,
  "items": [
    {
      "id": 10,
      "name": "Four Lines Notebook 120 Pages",
      "sku": "NB-4L-120",
      "barcode": "8964000123456",
      "description": "Standard school softcover notebook with margin lines",
      "unitOfMeasure": "Piece",
      "defaultUnitPrice": 150.00,
      "costPrice": 110.00,
      "isActive": true,
      "categoryId": 2,
      "categoryName": "Stationery & Consumables",
      "gradeId": 1,
      "gradeName": "Grade 1",
      "createdAt": "2026-10-25T14:30:00Z",
      "updatedAt": null,
      "attributes": {
        "binding": "Stapled",
        "pageCount": 120,
        "paperGsm": 70,
        "language": "English"
      },
      "quantityOnHand": 350,
      "quantityReserved": 50,
      "quantityAvailable": 300
    }
  ]
}
```

---

### 6.2. Get Product By ID
- **Method**: `GET`
- **Route**: `/api/products/{id}`

Returns the full product entity, current inventory snapshot, and category/grade metadata.

---

### 6.3. Create Product
- **Method**: `POST`
- **Route**: `/api/products`

#### Request Payload (`CreateProductDto`)
```json
{
  "name": "Oxford Progressive English Book 4",
  "sku": "BK-OPE-04",
  "barcode": "9780199062345",
  "description": "Oxford primary English textbook for class 4",
  "unitOfMeasure": "Piece",
  "defaultUnitPrice": 850.00,
  "costPrice": 620.00,
  "categoryId": 1,
  "gradeId": 4,
  "initialQuantityOnHand": 100,
  "attributes": {
    "author": "Rachel Redford",
    "edition": "3rd Edition",
    "curriculum": "National Curriculum / Cambridge Primary"
  }
}
```

#### Response (`201 Created`)
Returns the created `ProductDto`.

---

### 6.4. Update Product
- **Method**: `PUT`
- **Route**: `/api/products/{id}`

#### Request Payload (`UpdateProductDto`)
```json
{
  "name": "Oxford Progressive English Book 4 (Revised Edition)",
  "sku": "BK-OPE-04",
  "barcode": "9780199062345",
  "description": "Revised with new exercises",
  "unitOfMeasure": "Piece",
  "defaultUnitPrice": 890.00,
  "costPrice": 650.00,
  "categoryId": 1,
  "gradeId": 4,
  "isActive": true,
  "attributes": {
    "edition": "Revised 3rd Edition"
  }
}
```

---

### 6.5. Delete Product
- **Method**: `DELETE`
- **Route**: `/api/products/{id}`

#### Behavior
- If product has associated demand line items, invoice line items, or ledger transactions, it is **soft-deactivated** (`isActive = false`) and returns `200 OK`.
- If completely unused, hard deleted and returns `204 No Content`.

---

### 6.6. Adjust Inventory
- **Method**: `POST`
- **Route**: `/api/products/{id}/adjust-inventory`

Adjusts stock with an append-only audit entry in `InventoryTransactions`.

#### Request Payload
```json
{
  "quantityDelta": -5,
  "reason": "Damaged water-stained stock written off"
}
```

#### Response (`200 OK`)
Returns updated `ProductDto` with recalculated quantities.

---

### 6.7. Get Inventory Transactions Ledger
- **Method**: `GET`
- **Route**: `/api/products/{id}/transactions`

#### Example Response (`200 OK`)
```json
[
  {
    "id": 101,
    "productId": 10,
    "quantityDelta": 100,
    "transactionType": "InitialStock",
    "referenceId": "INIT-UPLOAD",
    "createdAt": "2026-10-25T14:30:00Z"
  },
  {
    "id": 102,
    "productId": 10,
    "quantityDelta": -5,
    "transactionType": "ManualAdjustment",
    "referenceId": "Damaged water-stained stock written off",
    "createdAt": "2026-10-26T09:15:00Z"
  }
]
```

---

### 6.8. List Grades
- **Method**: `GET`
- **Route**: `/api/products/grades`

#### Example Response (`200 OK`)
```json
[
  { "id": 1, "name": "Grade 1", "code": "G1", "displayOrder": 1, "isActive": true },
  { "id": 2, "name": "Grade 2", "code": "G2", "displayOrder": 2, "isActive": true }
]
```

---

### 6.9. Create Grade
- **Method**: `POST`
- **Route**: `/api/products/grades`

#### Request Payload
```json
{
  "name": "Playgroup",
  "code": "PG",
  "displayOrder": 0
}
```

---

### 6.10. Export Products to Excel
- **Method**: `GET`
- **Route**: `/api/products/export-excel`
- **Response**: Binary `.xlsx` stream (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`).

---

### 6.11. Bulk Import Products from Excel
- **Method**: `POST`
- **Route**: `/api/products/import-excel`
- **Content-Type**: `multipart/form-data`
- **Form Field**: `file` (the uploaded `.xlsx` file)

#### Response (`200 OK`)
```json
{
  "message": "Import completed successfully. Processed 125 rows.",
  "created": 115,
  "updated": 10
}
```

---

## 7. Demands & Requisitions API (`/api/demands`)

The demands module handles procurement orders linked to a **Season** and **Supplier**. Quantities and totals are automatically calculated (`RemainingQty = RequestedQty - SuppliedQty`).

### 7.1. Demand Status & Item Status Enums

Demands and items serialize enum values as strings via `JsonStringEnumConverter`:

#### `DemandStatus`
| Name | Integer Value | Description |
| :--- | :--- | :--- |
| `Draft` | `0` | Work-in-progress requisition |
| `Open` | `1` | Submitted / Active order sent to supplier |
| `PartiallyFulfilled` | `2` | Some items received |
| `Fulfilled` | `3` | All items fully received |
| `Cancelled` | `4` | Demand cancelled |

#### `DemandItemStatus`
| Name | Integer Value | Description |
| :--- | :--- | :--- |
| `Pending` | `0` | No quantity received yet |
| `Partial` | `1` | Partially supplied |
| `Fulfilled` | `2` | Fully supplied |

---

### 7.2. List Demands (Paginated & Filtered)
- **Method**: `GET`
- **Route**: `/api/demands`
- **Query Parameters**:
  - `seasonId` (optional, `int`): Filter by season ID.
  - `status` (optional, `DemandStatus` or `int`): Filter by status (`Draft`, `Open`, `PartiallyFulfilled`, `Fulfilled`, `Cancelled`).
  - `supplierId` (optional, `int`): Filter by supplier ID.
  - `search` (optional, `string`): Search across `DemandNumber`, `Title`, and `Supplier.Name`.
  - `pageNumber` (optional, `int`, default: `1`): 1-based page index.
  - `pageSize` (optional, `int`, default: `10`): Page size (max: `100`).

#### Example Response (`200 OK`)
```json
{
  "items": [
    {
      "id": 1,
      "demandNumber": "DEM-2026-0001",
      "title": "Annual Stationery Requisition 2026",
      "seasonId": 1,
      "seasonName": "2026-2027",
      "supplierId": 1,
      "supplierName": "Oxford University Press",
      "status": "Open",
      "totalRequestedQty": 700,
      "totalSuppliedQty": 0,
      "totalAmount": 240000.00,
      "createdAt": "2026-10-28T16:15:00Z"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

---

### 7.3. Get Demand By ID
- **Method**: `GET`
- **Route**: `/api/demands/{id}`

#### Example Response (`200 OK`)
```json
{
  "id": 1,
  "demandNumber": "DEM-2026-0001",
  "title": "Annual Stationery Requisition 2026",
  "seasonId": 1,
  "seasonName": "2026-2027",
  "supplierId": 1,
  "supplierName": "Oxford University Press",
  "status": "Open",
  "notes": "Deliver directly to campus central store",
  "createdAt": "2026-10-28T16:15:00Z",
  "updatedAt": null,
  "totalRequestedQty": 700,
  "totalSuppliedQty": 0,
  "totalAmount": 240000.00,
  "items": [
    {
      "id": 1,
      "productId": 10,
      "productName": "Four Lines Notebook 120 Pages",
      "sku": "NB-4L-120",
      "requestedQty": 500,
      "suppliedQty": 0,
      "remainingQty": 500,
      "unitPrice": 140.00,
      "lineTotal": 70000.00,
      "status": "Pending"
    },
    {
      "id": 2,
      "productId": 12,
      "productName": "Oxford Progressive English Book 4",
      "sku": "BK-OPE-04",
      "requestedQty": 200,
      "suppliedQty": 0,
      "remainingQty": 200,
      "unitPrice": 850.00,
      "lineTotal": 170000.00,
      "status": "Pending"
    }
  ]
}
```

---

### 7.4. Create Demand
- **Method**: `POST`
- **Route**: `/api/demands`

Demand numbers are auto-generated sequentially as `DEM-YYYY-XXXX` based on the season year or current year.

#### Request Payload (`CreateDemandRequest`)
```json
{
  "title": "Annual Academic Stationery Demand 2026-2027",
  "seasonId": 1,
  "supplierId": 1,
  "status": "Draft",
  "notes": "Deliver all Grade 1 to 5 books and student spiral diaries",
  "items": [
    {
      "productId": 10,
      "requestedQty": 500
    },
    {
      "productId": 12,
      "requestedQty": 200
    }
  ]
}
```

> **Note**: `UnitPrice` is not submitted in `CreateDemandRequest`. The system automatically retrieves and locks in the product's `DefaultUnitPrice` from the catalog.

#### Response (`201 Created`)
Returns `DemandDetailDto` with auto-assigned `demandNumber`, populated unit prices, and line item calculations.

---

### 7.5. Update Demand
- **Method**: `PUT`
- **Route**: `/api/demands/{id}`

Allows updating the title, supplier, notes, and full line item collection.

> **Validation Rule**: Demands that are in `PartiallyFulfilled` or `Fulfilled` status cannot have their items modified and will return `409 Conflict`.

#### Request Payload (`UpdateDemandRequest`)
```json
{
  "title": "Annual Academic Stationery Demand 2026-2027 (Revised)",
  "supplierId": 1,
  "notes": "Updated delivery instructions",
  "items": [
    {
      "productId": 10,
      "requestedQty": 600
    }
  ]
}
```

#### Response
- `200 OK`: Returns the updated `DemandDetailDto`.
- `400 Bad Request`: Invalid payload or missing products.
- `404 Not Found`: Demand not found.
- `409 Conflict`: Editing items is disallowed for `PartiallyFulfilled` or `Fulfilled` demands.

---

### 7.6. Update Demand Status
- **Method**: `PATCH`
- **Route**: `/api/demands/{id}/status`

#### Request Payload (`UpdateDemandStatusRequest`)
```json
{
  "status": "Open"
}
```

#### Response (`200 OK`)
Returns the updated `DemandDetailDto`.

---

### 7.7. Export Demand PDF (Quantity-Only)
- **Method**: `GET`
- **Route**: `/api/demands/{id}/export/pdf`
- **Response**: Binary PDF file (`application/pdf`) with filename `{DemandNumber}.pdf`.
- **Format**:
  - A4 Portrait layout with 25pt margins.
  - Header: "PURCHASE DEMAND", `DemandNumber`.
  - Metadata block: Supplier Name, Phone/Contact, Date, Status.
  - 4-column Table: `S.No`, `SKU / Code`, `Product Name`, `Demanded Quantity`.
  - Footer: Total quantity sum and page numbering ("Page X of Y").
  - Strictly quantity-only: omits prices, unit of measure, and notes.

---

### 7.8. Export Demand Excel (Quantity-Only)
- **Method**: `GET`
- **Route**: `/api/demands/{id}/export/excel`
- **Response**: Binary Excel file (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`) with filename `{DemandNumber}.xlsx`.
- **Format**:
  - Worksheet named after the demand number.
  - Rows 1-3: Demand Number, Status, Supplier Name, Phone/Contact, Creation Date.
  - Row 5: Table Header (`S.No`, `SKU / Code`, `Product Name`, `Demanded Quantity`) with dark slate fill (`#1E293B`) and white bold text.
  - Rows 6+: Item rows with formatted integer quantities (`#,##0`).
  - Total Row: `=SUM(D6:D{lastRow})` formula for Total Quantity.
  - Auto-fitted column widths.

---

## 8. Invoice & Reconciliation API (`/api/invoices`)

### 8.1. Confirm Reconciliation & Generate Backorders *(DTO Specification)*
- **Method**: `POST`
- **Route**: `/api/invoices/reconcile`

#### Request Payload (`ConfirmReconciliationRequestDto`)
```json
{
  "invoiceId": 44,
  "autoCreateBackorderForRemaining": true,
  "items": [
    {
      "invoiceItemId": 801,
      "matchedProductId": 10,
      "matchedDemandItemId": 1,
      "confirmedQuantity": 400,
      "confirmedUnitPrice": 140.00
    }
  ]
}
```

#### Example Response (`ReconciliationResultDto`)
```json
{
  "invoiceId": 44,
  "demandId": 1,
  "isDemandFullyFulfilled": false,
  "generatedBackorderDemandId": 2,
  "generatedBackorderDemandNumber": "DEM-2026-0002",
  "remainingItemsCount": 1,
  "message": "Reconciliation finalized. Backorder requisition created for unfulfilled balance."
}
```

---

## 9. TypeScript Frontend Definitions

Frontend applications can copy and paste the following TypeScript models directly into `models/api-contracts.ts`:

```typescript
// ==========================================
// Generic API Models
// ==========================================
export interface PagedResult<T> {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  items: T[];
}

export interface ApiMessageResponse {
  message: string;
}

// ==========================================
// Category Models
// ==========================================
export interface CategoryDto {
  id: number;
  name: string;
  code?: string | null;
  description?: string | null;
  isActive: boolean;
  parentCategoryId?: number | null;
  parentCategoryName?: string | null;
  subCategoriesCount: number;
  itemsCount: number;
}

export interface CreateCategoryDto {
  name: string;
  code?: string | null;
  description?: string | null;
  parentCategoryId?: number | null;
}

export interface UpdateCategoryDto {
  name: string;
  code?: string | null;
  description?: string | null;
  parentCategoryId?: number | null;
  isActive: boolean;
}

// ==========================================
// Supplier Models
// ==========================================
export interface SupplierDto {
  id: number;
  name: string;
  contact?: string | null;
  isActive: boolean;
  demandsCount: number;
  invoicesCount: number;
}

export interface CreateSupplierDto {
  name: string;
  contact?: string | null;
}

export interface UpdateSupplierDto {
  name: string;
  contact?: string | null;
  isActive: boolean;
}

// ==========================================
// Season Models
// ==========================================
export interface SeasonDto {
  id: number;
  name: string;
  isActive: boolean;
  isDefault: boolean;
}

export interface CreateSeasonRequest {
  name: string;
  isDefault?: boolean;
}

// ==========================================
// Grade Models
// ==========================================
export interface GradeDto {
  id: number;
  name: string;
  code?: string | null;
  displayOrder: number;
  isActive: boolean;
}

export interface CreateGradeDto {
  name: string;
  code?: string | null;
  displayOrder: number;
}

// ==========================================
// Product Models
// ==========================================
export interface ProductDto {
  id: number;
  name: string;
  sku?: string | null;
  barcode?: string | null;
  description?: string | null;
  unitOfMeasure: string;
  defaultUnitPrice: number;
  costPrice: number;
  isActive: boolean;
  categoryId: number;
  categoryName?: string | null;
  gradeId?: number | null;
  gradeName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  attributes?: Record<string, any> | null;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
}

export interface CreateProductDto {
  name: string;
  sku?: string | null;
  barcode?: string | null;
  description?: string | null;
  unitOfMeasure?: string;
  defaultUnitPrice: number;
  costPrice: number;
  categoryId: number;
  gradeId?: number | null;
  attributes?: Record<string, any> | null;
  initialQuantityOnHand?: number;
}

export interface UpdateProductDto {
  name: string;
  sku?: string | null;
  barcode?: string | null;
  description?: string | null;
  unitOfMeasure: string;
  defaultUnitPrice: number;
  costPrice: number;
  categoryId: number;
  gradeId?: number | null;
  isActive: boolean;
  attributes?: Record<string, any> | null;
}

export interface ProductFilterQuery {
  search?: string;
  categoryId?: number;
  gradeId?: number;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface InventoryAdjustmentDto {
  quantityDelta: number;
  reason?: string | null;
}

export interface InventoryTransactionDto {
  id: number;
  productId: number;
  quantityDelta: number;
  transactionType: string;
  referenceId?: string | null;
  createdAt: string;
}

// ==========================================
// Demand & Requisition Enums and Models
// ==========================================
export type DemandStatus = 'Draft' | 'Open' | 'PartiallyFulfilled' | 'Fulfilled' | 'Cancelled';
export type DemandItemStatus = 'Pending' | 'Partial' | 'Fulfilled';

export interface DemandListDto {
  id: number;
  demandNumber: string;
  title: string;
  seasonId: number;
  seasonName: string;
  supplierId: number;
  supplierName: string;
  status: DemandStatus;
  totalRequestedQty: number;
  totalSuppliedQty: number;
  totalAmount: number;
  createdAt: string;
}

export interface DemandDetailDto {
  id: number;
  demandNumber: string;
  title: string;
  seasonId: number;
  seasonName: string;
  supplierId: number;
  supplierName: string;
  status: DemandStatus;
  notes?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  totalRequestedQty: number;
  totalSuppliedQty: number;
  totalAmount: number;
  items: DemandItemDto[];
}

export interface DemandItemDto {
  id: number;
  productId: number;
  productName: string;
  sku?: string | null;
  requestedQty: number;
  suppliedQty: number;
  remainingQty: number;
  unitPrice: number;
  lineTotal: number;
  status: DemandItemStatus;
}

export interface CreateDemandItemRequest {
  productId: number;
  requestedQty: number;
}

export interface CreateDemandRequest {
  title: string;
  seasonId: number;
  supplierId: number;
  status?: DemandStatus;
  notes?: string | null;
  items: CreateDemandItemRequest[];
}

export interface UpdateDemandRequest {
  title: string;
  supplierId: number;
  notes?: string | null;
  items: CreateDemandItemRequest[];
}

export interface UpdateDemandStatusRequest {
  status: DemandStatus;
}

export interface DemandFilterQuery {
  seasonId?: number;
  status?: DemandStatus;
  supplierId?: number;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

// ==========================================
// Invoice & Reconciliation Models
// ==========================================
export interface ConfirmReconciliationItemRequestDto {
  invoiceItemId: number;
  matchedProductId?: number | null;
  matchedDemandItemId?: number | null;
  confirmedQuantity: number;
  confirmedUnitPrice: number;
}

export interface ConfirmReconciliationRequestDto {
  invoiceId: number;
  autoCreateBackorderForRemaining: boolean;
  items: ConfirmReconciliationItemRequestDto[];
}

export interface ReconciliationResultDto {
  invoiceId: number;
  demandId: number;
  isDemandFullyFulfilled: boolean;
  generatedBackorderDemandId?: number | null;
  generatedBackorderDemandNumber?: string | null;
  remainingItemsCount: number;
  message: string;
}
```

---

## 10. Frontend Integration Examples

### Angular Demand & Season Service Example
```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PagedResult,
  DemandListDto,
  DemandDetailDto,
  CreateDemandRequest,
  UpdateDemandRequest,
  UpdateDemandStatusRequest,
  DemandFilterQuery,
  SeasonDto,
  CreateSeasonRequest
} from '../models/api-contracts';

@Injectable({
  providedIn: 'root'
})
export class ProcurementService {
  private readonly baseUrl = 'https://localhost:7196/api';

  constructor(private http: HttpClient) {}

  // Seasons
  getSeasons(): Observable<SeasonDto[]> {
    return this.http.get<SeasonDto[]>(`${this.baseUrl}/seasons`);
  }

  getDefaultSeason(): Observable<SeasonDto> {
    return this.http.get<SeasonDto>(`${this.baseUrl}/seasons/default`);
  }

  createSeason(req: CreateSeasonRequest): Observable<SeasonDto> {
    return this.http.post<SeasonDto>(`${this.baseUrl}/seasons`, req);
  }

  setDefaultSeason(id: number): Observable<SeasonDto> {
    return this.http.put<SeasonDto>(`${this.baseUrl}/seasons/${id}/set-default`, {});
  }

  // Demands
  getDemands(query: DemandFilterQuery): Observable<PagedResult<DemandListDto>> {
    let params = new HttpParams();
    if (query.seasonId != null) params = params.set('seasonId', query.seasonId.toString());
    if (query.status != null) params = params.set('status', query.status);
    if (query.supplierId != null) params = params.set('supplierId', query.supplierId.toString());
    if (query.search) params = params.set('search', query.search);
    if (query.pageNumber != null) params = params.set('pageNumber', query.pageNumber.toString());
    if (query.pageSize != null) params = params.set('pageSize', query.pageSize.toString());

    return this.http.get<PagedResult<DemandListDto>>(`${this.baseUrl}/demands`, { params });
  }

  getDemandById(id: number): Observable<DemandDetailDto> {
    return this.http.get<DemandDetailDto>(`${this.baseUrl}/demands/${id}`);
  }

  createDemand(req: CreateDemandRequest): Observable<DemandDetailDto> {
    return this.http.post<DemandDetailDto>(`${this.baseUrl}/demands`, req);
  }

  updateDemand(id: number, req: UpdateDemandRequest): Observable<DemandDetailDto> {
    return this.http.put<DemandDetailDto>(`${this.baseUrl}/demands/${id}`, req);
  }

  updateDemandStatus(id: number, statusReq: UpdateDemandStatusRequest): Observable<DemandDetailDto> {
    return this.http.patch<DemandDetailDto>(`${this.baseUrl}/demands/${id}/status`, statusReq);
  }
}
```

### React / Axios Service Example
```typescript
import axios from 'axios';
import {
  DemandListDto,
  DemandDetailDto,
  CreateDemandRequest,
  UpdateDemandRequest,
  UpdateDemandStatusRequest,
  DemandFilterQuery,
  PagedResult,
  SeasonDto,
  CreateSeasonRequest
} from '../models/api-contracts';

const api = axios.create({
  baseURL: 'https://localhost:7196/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Seasons
export const getSeasons = async (): Promise<SeasonDto[]> => {
  const response = await api.get<SeasonDto[]>('/seasons');
  return response.data;
};

export const getDefaultSeason = async (): Promise<SeasonDto> => {
  const response = await api.get<SeasonDto>('/seasons/default');
  return response.data;
};

export const createSeason = async (req: CreateSeasonRequest): Promise<SeasonDto> => {
  const response = await api.post<SeasonDto>('/seasons', req);
  return response.data;
};

export const setDefaultSeason = async (id: number): Promise<SeasonDto> => {
  const response = await api.put<SeasonDto>(`/seasons/${id}/set-default`);
  return response.data;
};

// Demands
export const getDemands = async (filter?: DemandFilterQuery): Promise<PagedResult<DemandListDto>> => {
  const response = await api.get<PagedResult<DemandListDto>>('/demands', { params: filter });
  return response.data;
};

export const getDemandById = async (id: number): Promise<DemandDetailDto> => {
  const response = await api.get<DemandDetailDto>(`/demands/${id}`);
  return response.data;
};

export const createDemand = async (payload: CreateDemandRequest): Promise<DemandDetailDto> => {
  const response = await api.post<DemandDetailDto>('/demands', payload);
  return response.data;
};

export const updateDemand = async (id: number, payload: UpdateDemandRequest): Promise<DemandDetailDto> => {
  const response = await api.put<DemandDetailDto>(`/demands/${id}`, payload);
  return response.data;
};

export const updateDemandStatus = async (id: number, status: UpdateDemandStatusRequest): Promise<DemandDetailDto> => {
  const response = await api.patch<DemandDetailDto>(`/demands/${id}/status`, status);
  return response.data;
};
```
