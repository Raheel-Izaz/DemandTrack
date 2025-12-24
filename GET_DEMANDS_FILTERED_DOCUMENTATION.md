# GetDemandsFiltered Endpoint Documentation

## Overview
New GET endpoint that returns demands with calculated shipping status, supporting optional filtering by item status.

## Endpoint Details

**URL**: `GET /api/demands/filtered`

**Query Parameters**:
- `status` (optional): Filter by shipping status
  - `Pending` - Only items with 0 shipped quantity
  - `PartiallyShipped` - Only items with partial shipments (0 < shipped < requested)
  - `Completed` - Only items fully or over-shipped (shipped >= requested)
  - `All` or omitted - Returns all items (no filtering)

## Request Examples

### Get All Demands (No Filter)
```http
GET /api/demands/filtered
GET /api/demands/filtered?status=All
```

### Get Demands with Pending Items Only
```http
GET /api/demands/filtered?status=Pending
```

### Get Demands with Partially Shipped Items Only
```http
GET /api/demands/filtered?status=PartiallyShipped
```

### Get Demands with Completed Items Only
```http
GET /api/demands/filtered?status=Completed
```

## Response Format

### Success Response (200 OK)

```json
[
  {
    "id": 1,
    "seasonName": "Fall 2024",
    "createdAt": "2024-01-15T10:30:00Z",
    "items": [
      {
        "demandItemId": 101,
        "bookTitle": "Introduction to Algorithms",
        "isbn": "9780262033848",
        "requestedQty": 100,
        "shippedQty": 0,
        "status": "Pending"
      },
      {
        "demandItemId": 102,
        "bookTitle": "Clean Code",
        "isbn": "9780132350884",
        "requestedQty": 50,
        "shippedQty": 25,
        "status": "PartiallyShipped"
      }
    ]
  },
  {
    "id": 2,
    "seasonName": "Spring 2024",
    "createdAt": "2024-03-20T14:15:00Z",
    "items": [
      {
        "demandItemId": 201,
        "bookTitle": "Design Patterns",
        "isbn": "9780201633610",
        "requestedQty": 75,
        "shippedQty": 80,
        "status": "Completed"
      }
    ]
  }
]
```

### Empty Response
If no demands match the filter, returns empty array:
```json
[]
```

## Status Logic

The endpoint calculates status for each DemandItem based on shipped quantity:

| Condition | Status |
|-----------|--------|
| `ShippedQty = 0` | `Pending` |
| `0 < ShippedQty < RequestedQty` | `PartiallyShipped` |
| `ShippedQty >= RequestedQty` | `Completed` |

## Performance Optimizations

### 1. Efficient Database Queries
- Single query to load all demands with related Books
- Single aggregated query to sum SupplyItems by DemandItemId
- Uses `GroupBy` and `Sum` at the database level

### 2. In-Memory Processing
- Shipped quantities are calculated once and cached in a Dictionary
- O(1) lookup for each DemandItem's shipped quantity
- Filtering happens in-memory after data is loaded

### 3. Query Execution
```csharp
// Step 1: Load demands with books (1 query)
var demands = await _context.Demands
    .Include(d => d.Items)
    .ThenInclude(i => i.Book)
    .ToListAsync();

// Step 2: Aggregate supply items (1 query)
var supplyItemsGrouped = await _context.SupplyItems
    .Where(si => allDemandItemIds.Contains(si.DemandItemId))
    .GroupBy(si => si.DemandItemId)
    .Select(g => new { DemandItemId = g.Key, TotalShippedQty = g.Sum(si => si.ShippedQty) })
    .ToDictionaryAsync(x => x.DemandItemId, x => x.TotalShippedQty);

// Step 3: Build response in-memory with filtering
```

**Total Database Queries**: 2 (regardless of number of demands)

## Filtering Behavior

### Demand Exclusion
Demands with **zero items** after filtering are excluded from the response.

**Example**: If a demand has 3 items (2 Completed, 1 Pending) and you filter by `status=Completed`, the demand will appear with only 2 items. If you filter by `status=PartiallyShipped`, that demand will not appear at all.

### Case Insensitivity
The status parameter is case-insensitive:
- `?status=pending` ?
- `?status=Pending` ?
- `?status=PENDING` ?
- `?status=partiallyshipped` ?
- `?status=PartiallyShipped` ?

## Use Cases

### 1. Dashboard Overview
```http
GET /api/demands/filtered
```
Shows all demands with complete shipping status information.

### 2. Urgent Orders
```http
GET /api/demands/filtered?status=Pending
```
Identifies items that haven't been shipped yet.

### 3. Incomplete Shipments
```http
GET /api/demands/filtered?status=PartiallyShipped
```
Tracks orders that need additional shipments.

### 4. Fulfilled Orders
```http
GET /api/demands/filtered?status=Completed
```
Shows successfully completed shipments.

## Comparison with GetDemands

| Feature | GetDemands | GetDemandsFiltered |
|---------|------------|-------------------|
| URL | `/api/demands` | `/api/demands/filtered` |
| Filtering | ? No | ? Yes (by status) |
| ShippedQty | ? Not included | ? Calculated |
| Status | ? Not included | ? Calculated |
| Use DTOs | ? Yes | ? Anonymous types |
| Performance | Fast (no supply items) | Optimized (2 queries) |

## Error Handling

The endpoint handles edge cases gracefully:

- **No demands exist**: Returns `[]`
- **Invalid status parameter**: Treats as "All" (no filtering)
- **Demand has no items**: Excluded from results
- **Item has no book**: Returns empty string for BookTitle/Isbn
- **No supply items exist**: ShippedQty = 0, Status = "Pending"

## Testing Scenarios

### Test 1: No Filter
**Request**: `GET /api/demands/filtered`
**Expected**: All demands with all items

### Test 2: Pending Filter
**Request**: `GET /api/demands/filtered?status=Pending`
**Expected**: Only demands with at least one pending item

### Test 3: PartiallyShipped Filter
**Request**: `GET /api/demands/filtered?status=PartiallyShipped`
**Expected**: Only demands with at least one partially shipped item

### Test 4: Completed Filter
**Request**: `GET /api/demands/filtered?status=Completed`
**Expected**: Only demands with at least one completed item

### Test 5: Invalid Status
**Request**: `GET /api/demands/filtered?status=InvalidStatus`
**Expected**: Same as no filter (all items)

### Test 6: Empty Database
**Request**: `GET /api/demands/filtered`
**Expected**: `[]`

## Performance Benchmarks (Estimated)

| Scenario | Database Queries | Memory Usage | Response Time |
|----------|-----------------|--------------|---------------|
| 10 demands, 50 items, 200 supply items | 2 | Low | < 50ms |
| 100 demands, 500 items, 2000 supply items | 2 | Medium | < 200ms |
| 1000 demands, 5000 items, 20000 supply items | 2 | High | < 1000ms |

## API Contract

### Response Schema
```typescript
interface FilteredDemandResponse {
  id: number;
  seasonName: string;
  createdAt: string; // ISO 8601 format
  items: FilteredDemandItem[];
}

interface FilteredDemandItem {
  demandItemId: number;
  bookTitle: string;
  isbn: string;
  requestedQty: number;
  shippedQty: number;
  status: "Pending" | "PartiallyShipped" | "Completed";
}
```

## Integration Notes

### Frontend Implementation Example
```javascript
// Fetch all demands
const response = await fetch('/api/demands/filtered');
const demands = await response.json();

// Fetch only pending items
const pendingResponse = await fetch('/api/demands/filtered?status=Pending');
const pendingDemands = await pendingResponse.json();

// Display status badge
const getStatusColor = (status) => {
  switch(status) {
    case 'Pending': return 'red';
    case 'PartiallyShipped': return 'yellow';
    case 'Completed': return 'green';
  }
};
```

## Future Enhancements

Potential improvements for future versions:
1. ? Pagination support (`?page=1&pageSize=20`)
2. ? Date range filtering (`?from=2024-01-01&to=2024-12-31`)
3. ? Multiple status filters (`?status=Pending,PartiallyShipped`)
4. ? Sorting options (`?sortBy=createdAt&order=desc`)
5. ? Search by season name or ISBN
6. ? Include total counts in response

## Swagger Documentation

The endpoint will appear in Swagger UI as:

**GET** `/api/demands/filtered`

**Parameters**:
- `status` (query, string, optional): Filter by shipping status

**Responses**:
- `200 OK`: Success - Returns array of filtered demands
- `500 Internal Server Error`: Unexpected error
