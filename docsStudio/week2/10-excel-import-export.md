# Excel Import & Export — Week 2

## Overview

The system supports two Excel-related workflows:

1. **Bulk import** — upload a `.xlsx` file with denormalized/mixed data; the system normalizes and upserts into the correct tables.
2. **Export** — download the current dataset (products, customers, or sales) as a formatted `.xlsx` file.

Both are powered by **EPPlus 7** (NonCommercial license).

---

## Bulk Import (`ExcelImportService`)

File: `Services/ExcelImportService.cs`

### Detection strategy

Each worksheet is inspected independently. Detection is based on which headers are present (case-insensitive, accents stripped):

| Condition | Detected as |
|---|---|
| Has `Nombre`/`Producto` **and** `Precio` | Products sheet |
| Has `Cliente`/`Nombre` **and** `Documento`/`Cedula` | Customers sheet |
| Has `Documento_Cliente` **and** `Producto_Venta` **and** `Cantidad` | Sales sheet |

A single file may contain multiple sheets of different types — all are processed.

### Required columns per type

**Products**
| Column | Required | Aliases |
|---|---|---|
| Nombre | Yes | Producto, Name |
| Precio | Yes | Price |
| Descripcion | No | Description |
| Categoria | No | Category |
| Unidad | No | Unit |
| Stock | No | Inventario, Cantidad |

**Customers**
| Column | Required | Aliases |
|---|---|---|
| Cliente / Nombre | Yes | First_Name, Nombres |
| Documento | Yes | Cedula, RFC, Document_Number |
| Apellido | No | Last_Name, Apellidos |
| Telefono | No | Phone |
| Email | No | Correo |
| Direccion | No | Address |

**Sales**
| Column | Required | Notes |
|---|---|---|
| Documento_Cliente | Yes | Must match an existing Customer |
| Producto_Venta | Yes | Must match an existing Product |
| Cantidad | Yes | Integer > 0 |
| Precio_Unitario | No | Defaults to current product price |
| Notas | No | |

### Upsert logic

- **Products**: matched by `Name`. If found → update Price, Stock, Category, Unit, Description. If not → insert.
- **Customers**: matched by `DocumentNumber`. If found → update fields. If not → insert.
- **Sales**: always inserted (one row = one sale with one detail line). Customer and Product must pre-exist.

### Error log

`ImportResult` accumulates all row-level errors (invalid price, missing customer, etc.). The UI shows the count and each message after import.

---

## Export (`ExcelExportService`)

File: `Services/ExcelExportService.cs`

Three methods, each returns `byte[]`:

| Method | Sheet name | Endpoint |
|---|---|---|
| `ExportProductsAsync()` | Productos | `/Admin/Products/Index?handler=ExportExcel` |
| `ExportCustomersAsync()` | Clientes | `/Admin/Customers/Index?handler=ExportExcel` |
| `ExportSalesAsync()` | Ventas | `/Admin/Sales/Index?handler=ExportExcel` |

Headers use indigo background (`#4F46E5`) with white bold text. Numeric columns are formatted `#,##0.00`. Columns auto-fit.

Sales export denormalizes: one row per `SaleDetail` (each product line), repeating the sale header columns.

---

## Pages

### `/Admin/Import`
- `GET` — shows the upload form and column reference table.
- `POST` — receives the file, runs `ExcelImportService.ImportAsync`, renders the result summary.

The reference table on the page lists the accepted column names so users can prepare their files correctly.
