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
| **Suppliers** | `GET` | `/api/suppliers` | List all suppliers with stats counts |
| | `GET` | `/api/suppliers/{id}` | Get supplier by ID |
| | `POST` | `/api/suppliers` | Create a new supplier |
| | `PUT` | `/api/suppliers/{id}` | Update an existing supplier |
| | `DELETE` | `/api/suppliers/{id}` | Delete supplier (soft-deactivates if records exist) |
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
| **Demands (Requisitions)** | `GET` | `/api/demands` *(DTO Spec)* | List all demands / purchase requisitions |
| | `GET` | `/api/demands/{id}` *(DTO Spec)* | Get detailed demand with items & backorder chain |
| | `POST` | `/api/demands` *(DTO Spec)* | Create a new demand order |
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

#### Error Response (`404 Not Found`)
```json
{
  "message": "Category with ID 999 not found."
}
```

---

### 3.3. Create Category
- **Method**: `POST`
- **Route**: `/api/categories`
- **Headers**: `Content-Type: application/json`

#### Request Payload
```json
{
  "name": "Science Kits",
  "code": "CAT-SCI",
  "description": "Laboratory consumables and experimental models",
  "parentCategoryId": null
}
```

#### Response (`201 Created`)
```json
{
  "id": 3,
  "name": "Science Kits",
  "code": "CAT-SCI",
  "description": "Laboratory consumables and experimental models",
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
- **Headers**: `Content-Type: application/json`

#### Request Payload
```json
{
  "name": "Science & Stem Kits",
  "code": "CAT-SCI-STEM",
  "description": "Updated lab items, robotics and science kits",
  "parentCategoryId": null,
  "isActive": true
}
```

#### Response
- `204 No Content` on success.
- `400 Bad Request` if `parentCategoryId == id` (category cannot be its own parent).
- `404 Not Found` if the category does not exist.

---

### 3.5. Delete Category
- **Method**: `DELETE`
- **Route**: `/api/categories/{id}`

#### Behavior
- If the category has linked products or subcategories, the API performs a **soft-delete** (sets `isActive = false`) and returns `200 OK`:
  ```json
  {
    "message": "Category has linked items or subcategories, so it was marked as inactive instead of permanently deleted."
  }
  ```
- If the category has no linked items or subcategories, it is permanently deleted and returns `204 No Content`.

---

## 4. Suppliers API (`/api/suppliers`)

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
    "code": "SUP-OUP",
    "contactPerson": "Jane Doe",
    "phone": "+92 300 1234567",
    "email": "orders@oup.example.com",
    "address": "Plot 12, Industrial Area, Karachi",
    "isActive": true,
    "demandsCount": 4,
    "invoicesCount": 2
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
  "code": "SUP-OUP",
  "contactPerson": "Jane Doe",
  "phone": "+92 300 1234567",
  "email": "orders@oup.example.com",
  "address": "Plot 12, Industrial Area, Karachi",
  "isActive": true,
  "demandsCount": 4,
  "invoicesCount": 2
}
```

---

### 4.3. Create Supplier
- **Method**: `POST`
- **Route**: `/api/suppliers`

#### Request Payload
```json
{
  "name": "National Book Foundation",
  "code": "SUP-NBF",
  "contactPerson": "Ahmad Ali",
  "phone": "+92 321 9876543",
  "email": "procurement@nbf.gov.pk",
  "address": "Sector H-8/4, Islamabad"
}
```

#### Response (`201 Created`)
```json
{
  "id": 2,
  "name": "National Book Foundation",
  "code": "SUP-NBF",
  "contactPerson": "Ahmad Ali",
  "phone": "+92 321 9876543",
  "email": "procurement@nbf.gov.pk",
  "address": "Sector H-8/4, Islamabad",
  "isActive": true,
  "demandsCount": 0,
  "invoicesCount": 0
}
```

---

### 4.4. Update Supplier
- **Method**: `PUT`
- **Route**: `/api/suppliers/{id}`

#### Request Payload
```json
{
  "name": "National Book Foundation",
  "code": "SUP-NBF-HQ",
  "contactPerson": "Ahmad Ali (Director)",
  "phone": "+92 321 9876543",
  "email": "info@nbf.gov.pk",
  "address": "Main Campus, Islamabad",
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

## 5. Products & Catalog API (`/api/products`)

### 5.1. List Products (Paginated & Filtered)
- **Method**: `GET`
- **Route**: `/api/products`
- **Query Parameters**:
  - `search` (optional, `string`): Searches inside `Name`, `Sku`, `Barcode`, and `Description`.
  - `categoryId` (optional, `number`): Filter by category ID.
  - `gradeId` (optional, `number`): Filter by grade ID.
  - `isActive` (optional, `boolean`, default: `true`): Active status filter.
  - `page` (optional, `number`, default: `1`): Current page (1-based index).
  - `pageSize` (optional, `number`, default: `50`, max: `200`): Items per page.

#### Example Request
```http
GET /api/products?search=notebook&page=1&pageSize=20 HTTP/1.1
```

#### Example Response (`200 OK`)
```json
{
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1,
  "items": [
    {
      "id": 10,
      "name": "Four Lines Notebook 120 Pages",
      "sku": "NB-4L-120",
      "barcode": "8964000123456",
      "description": "Standard softcover 4-lines notebook for primary classes",
      "unitOfMeasure": "Piece",
      "defaultUnitPrice": 150.00,
      "costPrice": 95.00,
      "isActive": true,
      "categoryId": 2,
      "categoryName": "Stationery & Consumables",
      "gradeId": 3,
      "gradeName": "Class 1",
      "createdAt": "2026-10-25T17:15:00Z",
      "updatedAt": null,
      "attributes": {
        "pages": 120,
        "binding": "Stapled",
        "paperGsm": 70
      },
      "quantityOnHand": 500,
      "quantityReserved": 50,
      "quantityAvailable": 450
    }
  ]
}
```

---

### 5.2. Get Product By ID
- **Method**: `GET`
- **Route**: `/api/products/{id}`

#### Example Response (`200 OK`)
```json
{
  "id": 10,
  "name": "Four Lines Notebook 120 Pages",
  "sku": "NB-4L-120",
  "barcode": "8964000123456",
  "description": "Standard softcover 4-lines notebook for primary classes",
  "unitOfMeasure": "Piece",
  "defaultUnitPrice": 150.00,
  "costPrice": 95.00,
  "isActive": true,
  "categoryId": 2,
  "categoryName": "Stationery & Consumables",
  "gradeId": 3,
  "gradeName": "Class 1",
  "createdAt": "2026-10-25T17:15:00Z",
  "updatedAt": null,
  "attributes": {
    "pages": 120,
    "binding": "Stapled",
    "paperGsm": 70
  },
  "quantityOnHand": 500,
  "quantityReserved": 50,
  "quantityAvailable": 450
}
```

---

### 5.3. Create Product
- **Method**: `POST`
- **Route**: `/api/products`

#### Request Payload
```json
{
  "name": "Oxford Progressive English Book 1",
  "sku": "OPE-BK-1",
  "barcode": "9780190700011",
  "description": "English Coursebook for Grade 1",
  "unitOfMeasure": "Copy",
  "defaultUnitPrice": 850.00,
  "costPrice": 620.00,
  "categoryId": 1,
  "gradeId": 3,
  "initialQuantityOnHand": 100,
  "attributes": {
    "author": "Rachel Redford",
    "edition": "3rd Edition",
    "subject": "English",
    "medium": "English"
  }
}
```

#### Response (`201 Created`)
Returns the complete `ProductDto` object with assigned `id` and HTTP header `Location: /api/products/{id}`.

---

### 5.4. Update Product
- **Method**: `PUT`
- **Route**: `/api/products/{id}`

#### Request Payload
```json
{
  "name": "Oxford Progressive English Book 1 (Revised)",
  "sku": "OPE-BK-1",
  "barcode": "9780190700011",
  "description": "Updated English Coursebook for Grade 1 with Audio QR",
  "unitOfMeasure": "Copy",
  "defaultUnitPrice": 890.00,
  "costPrice": 650.00,
  "categoryId": 1,
  "gradeId": 3,
  "isActive": true,
  "attributes": {
    "author": "Rachel Redford",
    "edition": "4th Revised Edition",
    "subject": "English"
  }
}
```

#### Response
- `204 No Content` on success.
- `400 Bad Request` if duplicate SKU/Barcode or invalid Category/Grade ID.
- `404 Not Found` if product does not exist.

---

### 5.5. Delete Product
- **Method**: `DELETE`
- **Route**: `/api/products/{id}`

#### Behavior
- If product has existing requisition / demand line history (`DemandItems`), the server protects history by soft-deleting (`isActive = false`):
  ```json
  {
    "message": "Product has associated demand items, so it was marked as inactive instead of permanently deleted."
  }
  ```
- If no demand items exist, removes inventory record, transactions, and product permanently (`204 No Content`).

---

### 5.6. Adjust Product Inventory
- **Method**: `POST`
- **Route**: `/api/products/{id}/adjust-inventory`

#### Request Payload
```json
{
  "quantityDelta": 25,
  "reason": "Stock count recount discrepancy adjustment"
}
```
*(Use negative values like `-5` for damages, shrinkage, or return write-offs).*

#### Response (`200 OK`)
Returns the updated `ProductDto` including the recalculated `quantityOnHand` and `quantityAvailable`.

---

### 5.7. Get Inventory Transactions
- **Method**: `GET`
- **Route**: `/api/products/{id}/transactions`

#### Example Response (`200 OK`)
```json
[
  {
    "id": 101,
    "productId": 10,
    "quantityDelta": 25,
    "transactionType": "Adjustment",
    "referenceId": "Stock count recount discrepancy adjustment",
    "createdAt": "2026-10-26T17:30:00Z"
  },
  {
    "id": 85,
    "productId": 10,
    "quantityDelta": 100,
    "transactionType": "PurchaseReceipt",
    "referenceId": "Initial Stock",
    "createdAt": "2026-10-25T17:15:00Z"
  }
]
```

---

### 5.8. Grades Endpoints (`/api/products/grades`)

#### List Grades
- **Method**: `GET`
- **Route**: `/api/products/grades`

```json
[
  { "id": 1, "name": "Playgroup", "code": "PG", "displayOrder": 1, "isActive": true },
  { "id": 2, "name": "Nursery", "code": "NUR", "displayOrder": 2, "isActive": true },
  { "id": 3, "name": "Class 1", "code": "C1", "displayOrder": 3, "isActive": true },
  { "id": 4, "name": "O-Levels", "code": "OLVL", "displayOrder": 10, "isActive": true }
]
```

#### Create Grade
- **Method**: `POST`
- **Route**: `/api/products/grades`

```json
{
  "name": "Class 2",
  "code": "C2",
  "displayOrder": 4
}
```

---

### 5.9. Excel Import & Export

#### Export Catalog
- **Method**: `GET`
- **Route**: `/api/products/export-excel`
- **Query Parameters**: Same filters as product list (`categoryId`, `gradeId`, `isActive`).
- **Response**: Binary `.xlsx` file (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`).

#### Import Catalog via Excel
- **Method**: `POST`
- **Route**: `/api/products/import-excel`
- **Headers**: `Content-Type: multipart/form-data`
- **Form Data**:
  - `file`: The `.xlsx` file to upload.

##### Excel Column Layout Expected
| Col 1 | Col 2 | Col 3 | Col 4 | Col 5 | Col 6 | Col 7 | Col 8 | Col 9 | Col 10 | Col 11 | Col 12 | Col 13 |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| ID | Name | SKU | Barcode | Category | Grade | UnitOfMeasure | DefaultUnitPrice | CostPrice | QuantityOnHand | QuantityReserved | IsActive | AttributesJSON |

##### Response (`200 OK`)
```json
{
  "message": "Import completed successfully.",
  "created": 15,
  "updated": 3
}
```

---

## 6. Demands & Requisitions API (`/api/demands`)

The system manages purchase demands / requisitions with self-referencing hierarchical backorders.

### 6.1. Create Demand
- **Method**: `POST`
- **Route**: `/api/demands`

#### Request Payload (`CreateDemandRequestDto`)
```json
{
  "title": "Annual Academic Stationery Demand 2026-2027",
  "supplierId": 1,
  "targetDeliveryDate": "2026-11-15T00:00:00Z",
  "notes": "Deliver all Grade 1 to 5 books and student spiral diaries",
  "items": [
    {
      "productId": 10,
      "requestedQty": 500,
      "unitPrice": 140.00
    },
    {
      "productId": 12,
      "requestedQty": 200,
      "unitPrice": 850.00
    }
  ]
}
```

---

### 6.2. Demand Detail Model
When fetching `/api/demands/{id}`, the response structure is:

#### Example Response (`DemandDetailDto`)
```json
{
  "id": 105,
  "demandNumber": "DEM-2026-0042",
  "title": "Annual Academic Stationery Demand 2026-2027",
  "supplierId": 1,
  "supplierName": "Oxford University Press",
  "parentDemandId": null,
  "parentDemandNumber": null,
  "status": "PartiallyFulfilled",
  "createdAt": "2026-10-25T10:00:00Z",
  "targetDeliveryDate": "2026-11-15T00:00:00Z",
  "notes": "Deliver all Grade 1 to 5 books and student spiral diaries",
  "items": [
    {
      "id": 501,
      "productId": 10,
      "productName": "Four Lines Notebook 120 Pages",
      "sku": "NB-4L-120",
      "barcode": "8964000123456",
      "unitOfMeasure": "Piece",
      "categoryName": "Stationery & Consumables",
      "requestedQty": 500,
      "suppliedQty": 400,
      "remainingQty": 100,
      "unitPrice": 140.00,
      "status": "PartiallyReceived"
    }
  ],
  "backorders": [
    {
      "id": 106,
      "demandNumber": "DEM-2026-0042-BO1",
      "status": "Draft",
      "totalItems": 1,
      "createdAt": "2026-10-26T09:00:00Z"
    }
  ]
}
```

---

## 7. Invoice & Reconciliation API (`/api/invoices`)

### 7.1. Confirm Reconciliation & Generate Backorders
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
      "matchedDemandItemId": 501,
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
  "demandId": 105,
  "isDemandFullyFulfilled": false,
  "generatedBackorderDemandId": 106,
  "generatedBackorderDemandNumber": "DEM-2026-0042-BO1",
  "remainingItemsCount": 1,
  "message": "Reconciliation finalized. 1 backorder demand created for unfulfilled balance."
}
```

---

## 8. TypeScript Frontend Definitions

Frontend applications can copy and paste the following TypeScript models directly into `models/api-contracts.ts`:

```typescript
// ==========================================
// Generic API Models
// ==========================================
export interface PagedResult<T> {
  totalCount: number;
  page: number;
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
  code?: string | null;
  contactPerson?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  isActive: boolean;
  demandsCount: number;
  invoicesCount: number;
}

export interface CreateSupplierDto {
  name: string;
  code?: string | null;
  contactPerson?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
}

export interface UpdateSupplierDto {
  name: string;
  code?: string | null;
  contactPerson?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  isActive: boolean;
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
// Demand & Requisition Models
// ==========================================
export interface DemandListDto {
  id: number;
  demandNumber: string;
  title: string;
  supplierId?: number | null;
  supplierName?: string | null;
  parentDemandId?: number | null;
  parentDemandNumber?: string | null;
  status: string;
  totalItemsCount: number;
  requestedTotalQty: number;
  suppliedTotalQty: number;
  remainingTotalQty: number;
  createdAt: string;
  targetDeliveryDate?: string | null;
}

export interface DemandDetailDto {
  id: number;
  demandNumber: string;
  title: string;
  supplierId?: number | null;
  supplierName?: string | null;
  parentDemandId?: number | null;
  parentDemandNumber?: string | null;
  status: string;
  createdAt: string;
  targetDeliveryDate?: string | null;
  notes?: string | null;
  items: DemandItemDetailDto[];
  backorders: DemandBackorderSummaryDto[];
}

export interface DemandItemDetailDto {
  id: number;
  productId: number;
  productName: string;
  sku?: string | null;
  barcode?: string | null;
  unitOfMeasure: string;
  categoryName?: string | null;
  requestedQty: number;
  suppliedQty: number;
  remainingQty: number;
  unitPrice: number;
  status: string;
}

export interface DemandBackorderSummaryDto {
  id: number;
  demandNumber: string;
  status: string;
  totalItems: number;
  createdAt: string;
}

export interface CreateDemandItemRequestDto {
  productId: number;
  requestedQty: number;
  unitPrice: number;
}

export interface CreateDemandRequestDto {
  title: string;
  supplierId?: number | null;
  targetDeliveryDate?: string | null;
  notes?: string | null;
  items: CreateDemandItemRequestDto[];
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

## 9. Frontend Integration Examples

### Angular HTTP Service Example
```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  PagedResult, 
  ProductDto, 
  ProductFilterQuery, 
  CreateProductDto, 
  InventoryAdjustmentDto 
} from '../models/api-contracts';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly baseUrl = 'https://localhost:7196/api/products';

  constructor(private http: HttpClient) {}

  getProducts(filter: ProductFilterQuery): Observable<PagedResult<ProductDto>> {
    let params = new HttpParams();
    if (filter.search) params = params.set('search', filter.search);
    if (filter.categoryId != null) params = params.set('categoryId', filter.categoryId.toString());
    if (filter.gradeId != null) params = params.set('gradeId', filter.gradeId.toString());
    if (filter.isActive != null) params = params.set('isActive', filter.isActive.toString());
    if (filter.page != null) params = params.set('page', filter.page.toString());
    if (filter.pageSize != null) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<ProductDto>>(this.baseUrl, { params });
  }

  createProduct(dto: CreateProductDto): Observable<ProductDto> {
    return this.http.post<ProductDto>(this.baseUrl, dto);
  }

  adjustInventory(id: number, adjustment: InventoryAdjustmentDto): Observable<ProductDto> {
    return this.http.post<ProductDto>(`${this.baseUrl}/${id}/adjust-inventory`, adjustment);
  }

  uploadExcel(file: File): Observable<{ message: string; created: number; updated: number }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ message: string; created: number; updated: number }>(
      `${this.baseUrl}/import-excel`,
      formData
    );
  }
}
```

### React / Axios Service Example
```typescript
import axios from 'axios';
import { ProductDto, CreateProductDto, PagedResult, ProductFilterQuery } from '../models/api-contracts';

const api = axios.create({
  baseURL: 'https://localhost:7196/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

export const getProducts = async (filter?: ProductFilterQuery): Promise<PagedResult<ProductDto>> => {
  const response = await api.get<PagedResult<ProductDto>>('/products', { params: filter });
  return response.data;
};

export const createProduct = async (payload: CreateProductDto): Promise<ProductDto> => {
  const response = await api.post<ProductDto>('/products', payload);
  return response.data;
};

export const exportProductsExcel = async (): Promise<Blob> => {
  const response = await api.get('/products/export-excel', {
    responseType: 'blob'
  });
  return response.data;
};
```
