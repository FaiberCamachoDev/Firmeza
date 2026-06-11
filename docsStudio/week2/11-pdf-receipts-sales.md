# PDF Receipts & Sales Module — Week 2

## Overview

When a sale is registered, the system automatically generates a PDF receipt and stores it in `wwwroot/recibos/`. The receipt can be downloaded from the sale details page at any time. A "Regenerate" button allows recreating the PDF if the file is missing.

---

## PdfReceiptService

File: `Services/PdfReceiptService.cs`
Library: **QuestPDF** (Community license, already configured in `Program.cs`)

### File naming

```
recibo-{saleId:D6}-{createdAt:yyyyMMdd}.pdf
```

Examples: `recibo-000001-20260610.pdf`

### Receipt structure

| Section | Content |
|---|---|
| Header | "FIRMEZA" title + "Comprobante de Venta" subtitle, indigo bottom border |
| Sale data box | Sale number, date, status, optional notes |
| Customer box | Full name, document, phone, email, address |
| Products table | Product name, quantity, unit price, subtotal; alternating row colors |
| Totals | Subtotal, IVA 16%, **Total** |
| Footer | "Firmeza — Sistema Administrativo" + generation timestamp |

### IVA

IVA is calculated as **16%** of the subtotal at generation time (display only — not persisted separately in the database).

### Public methods

```csharp
string Generate(Sale sale)           // Generates and saves, returns relative URL
bool   Exists(int saleId, DateTime)  // Checks if the file exists
string GetRelativePath(int, DateTime) // Returns /recibos/recibo-XXXXXX-YYYYMMDD.pdf
```

---

## Sales Module Pages

### `/Admin/Sales` (Index)
- Lists all sales ordered by date descending.
- Each row shows: sale number, date, customer, status, total.
- PDF badge appears when the receipt file exists.
- "Exportar Excel" button → downloads `ventas-YYYYMMDD.xlsx`.

### `/Admin/Sales/Create`
- Dropdown for customer selection (active customers only).
- Dynamic product rows: JS adds/removes rows, each with a product dropdown and quantity input.
- Client-side total preview recalculates on every change.
- On submit: creates `Sale` + `SaleDetail` records, then calls `PdfReceiptService.Generate`.
- Redirects to Details page on success.

### `/Admin/Sales/Details/{id}`
- Shows full sale + customer info + product table + totals with IVA.
- "Descargar recibo PDF" link when the file exists.
- "Generar PDF" / "Regenerar PDF" button via POST `?handler=Regenerate`.

---

## Service Registration

In `Program.cs`:

```csharp
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<PdfReceiptService>();
```

`PdfReceiptService` receives `IWebHostEnvironment` to resolve `wwwroot/recibos`.

---

## File Storage

Receipts are stored in `wwwroot/recibos/` and served as static files through the existing `UseStaticFiles()` middleware. No additional configuration is needed.

The folder ships with a `.gitkeep` so it is tracked by git but its contents (PDFs) are not.
