using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Firmeza.Web.Services;

public class ImportResult
{
    public int ProductsInserted { get; set; }
    public int ProductsUpdated { get; set; }
    public int CustomersInserted { get; set; }
    public int CustomersUpdated { get; set; }
    public int SalesInserted { get; set; }
    public List<string> Errors { get; set; } = [];
    public bool HasErrors => Errors.Count > 0;
}

public class ExcelImportService
{
    private readonly ApplicationDbContext _db;

    public ExcelImportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ImportResult> ImportAsync(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var result = new ImportResult();

        using var package = new ExcelPackage(fileStream);

        foreach (var worksheet in package.Workbook.Worksheets)
        {
            if (worksheet.Dimension is null) continue;

            var headers = ReadHeaders(worksheet);

            if (IsProductSheet(headers))
                await ProcessProducts(worksheet, headers, result);
            else if (IsCustomerSheet(headers))
                await ProcessCustomers(worksheet, headers, result);
            else if (IsSaleSheet(headers))
                await ProcessSales(worksheet, headers, result);
            else
                result.Errors.Add($"Hoja '{worksheet.Name}': no se reconocieron las columnas. Se esperan columnas de productos, clientes o ventas.");
        }

        return result;
    }

    private static Dictionary<string, int> ReadHeaders(ExcelWorksheet ws)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (ws.Dimension is null) return map;

        for (int col = 1; col <= ws.Dimension.Columns; col++)
        {
            var val = ws.Cells[1, col].Text.Trim();
            if (!string.IsNullOrEmpty(val))
                map[Normalize(val)] = col;
        }
        return map;
    }

    private static string Normalize(string s) => s.ToLowerInvariant()
        .Replace("á", "a").Replace("é", "e").Replace("í", "i")
        .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");

    private static bool IsProductSheet(Dictionary<string, int> h) =>
        (h.ContainsKey("nombre") || h.ContainsKey("producto") || h.ContainsKey("name")) &&
        (h.ContainsKey("precio") || h.ContainsKey("price"));

    private static bool IsCustomerSheet(Dictionary<string, int> h) =>
        (h.ContainsKey("cliente") || h.ContainsKey("nombre") || h.ContainsKey("first_name") || h.ContainsKey("nombres")) &&
        (h.ContainsKey("documento") || h.ContainsKey("cedula") || h.ContainsKey("document_number") || h.ContainsKey("rfc"));

    private static bool IsSaleSheet(Dictionary<string, int> h) =>
        (h.ContainsKey("documento_cliente") || h.ContainsKey("cliente_doc") || h.ContainsKey("customer_doc")) &&
        (h.ContainsKey("producto_venta") || h.ContainsKey("product") || h.ContainsKey("producto")) &&
        (h.ContainsKey("cantidad") || h.ContainsKey("quantity"));

    private async Task ProcessProducts(ExcelWorksheet ws, Dictionary<string, int> h, ImportResult result)
    {
        int nameCol   = GetCol(h, "nombre", "producto", "name");
        int descCol   = GetCol(h, "descripcion", "description", "desc");
        int priceCol  = GetCol(h, "precio", "price");
        int stockCol  = GetCol(h, "stock", "inventario", "cantidad");
        int catCol    = GetCol(h, "categoria", "category");
        int unitCol   = GetCol(h, "unidad", "unit");

        if (nameCol == 0 || priceCol == 0)
        {
            result.Errors.Add($"Hoja '{ws.Name}': faltan columnas requeridas (Nombre, Precio).");
            return;
        }

        for (int row = 2; row <= ws.Dimension.Rows; row++)
        {
            string name = ws.Cells[row, nameCol].Text.Trim();
            if (string.IsNullOrEmpty(name)) continue;

            if (!decimal.TryParse(ws.Cells[row, priceCol].Text.Replace(",", "."), out decimal price) || price < 0)
            {
                result.Errors.Add($"Hoja '{ws.Name}' fila {row}: Precio inválido para '{name}'.");
                continue;
            }

            var existing = await _db.Products
                .FirstOrDefaultAsync(p => p.Name == name);

            if (existing is null)
            {
                _db.Products.Add(new Product
                {
                    Name        = name,
                    Description = descCol > 0 ? ws.Cells[row, descCol].Text.Trim() : string.Empty,
                    Price       = price,
                    Stock       = stockCol > 0 && int.TryParse(ws.Cells[row, stockCol].Text, out int s) ? s : 0,
                    Category    = catCol  > 0 ? ws.Cells[row, catCol].Text.Trim()  : string.Empty,
                    Unit        = unitCol > 0 ? ws.Cells[row, unitCol].Text.Trim()  : string.Empty,
                });
                result.ProductsInserted++;
            }
            else
            {
                existing.Price       = price;
                existing.Description = descCol > 0 ? ws.Cells[row, descCol].Text.Trim() : existing.Description;
                existing.Stock       = stockCol > 0 && int.TryParse(ws.Cells[row, stockCol].Text, out int s2) ? s2 : existing.Stock;
                existing.Category    = catCol  > 0 ? ws.Cells[row, catCol].Text.Trim()  : existing.Category;
                existing.Unit        = unitCol > 0 ? ws.Cells[row, unitCol].Text.Trim()  : existing.Unit;
                existing.UpdatedAt   = DateTime.UtcNow;
                result.ProductsUpdated++;
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task ProcessCustomers(ExcelWorksheet ws, Dictionary<string, int> h, ImportResult result)
    {
        int firstCol = GetCol(h, "cliente", "nombre", "first_name", "nombres");
        int lastCol  = GetCol(h, "apellido", "apellidos", "last_name");
        int docCol   = GetCol(h, "documento", "cedula", "document_number", "rfc");
        int phoneCol = GetCol(h, "telefono", "phone");
        int emailCol = GetCol(h, "email", "correo");
        int addrCol  = GetCol(h, "direccion", "address");

        if (firstCol == 0 || docCol == 0)
        {
            result.Errors.Add($"Hoja '{ws.Name}': faltan columnas requeridas (Nombre, Documento).");
            return;
        }

        for (int row = 2; row <= ws.Dimension.Rows; row++)
        {
            string firstName = ws.Cells[row, firstCol].Text.Trim();
            string doc       = docCol > 0 ? ws.Cells[row, docCol].Text.Trim() : string.Empty;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(doc)) continue;

            var existing = await _db.Customers
                .FirstOrDefaultAsync(c => c.DocumentNumber == doc);

            if (existing is null)
            {
                _db.Customers.Add(new Customer
                {
                    FirstName      = firstName,
                    LastName       = lastCol  > 0 ? ws.Cells[row, lastCol].Text.Trim()  : string.Empty,
                    DocumentNumber = doc,
                    Phone          = phoneCol > 0 ? ws.Cells[row, phoneCol].Text.Trim() : string.Empty,
                    Email          = emailCol > 0 ? ws.Cells[row, emailCol].Text.Trim() : string.Empty,
                    Address        = addrCol  > 0 ? ws.Cells[row, addrCol].Text.Trim()  : string.Empty,
                });
                result.CustomersInserted++;
            }
            else
            {
                existing.FirstName = firstName;
                existing.LastName  = lastCol  > 0 ? ws.Cells[row, lastCol].Text.Trim()  : existing.LastName;
                existing.Phone     = phoneCol > 0 ? ws.Cells[row, phoneCol].Text.Trim() : existing.Phone;
                existing.Email     = emailCol > 0 ? ws.Cells[row, emailCol].Text.Trim() : existing.Email;
                existing.Address   = addrCol  > 0 ? ws.Cells[row, addrCol].Text.Trim()  : existing.Address;
                existing.UpdatedAt = DateTime.UtcNow;
                result.CustomersUpdated++;
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task ProcessSales(ExcelWorksheet ws, Dictionary<string, int> h, ImportResult result)
    {
        int docClienteCol  = GetCol(h, "documento_cliente", "cliente_doc", "customer_doc");
        int productoCol    = GetCol(h, "producto_venta", "product", "producto");
        int cantidadCol    = GetCol(h, "cantidad", "quantity");
        int precioUnitCol  = GetCol(h, "precio_unitario", "unit_price", "precio");
        int notasCol       = GetCol(h, "notas", "notes", "observaciones");

        if (docClienteCol == 0 || productoCol == 0 || cantidadCol == 0)
        {
            result.Errors.Add($"Hoja '{ws.Name}': faltan columnas requeridas (Documento_Cliente, Producto_Venta, Cantidad).");
            return;
        }

        for (int row = 2; row <= ws.Dimension.Rows; row++)
        {
            string clienteDoc  = ws.Cells[row, docClienteCol].Text.Trim();
            string productoNom = ws.Cells[row, productoCol].Text.Trim();

            if (string.IsNullOrEmpty(clienteDoc) || string.IsNullOrEmpty(productoNom)) continue;

            if (!int.TryParse(ws.Cells[row, cantidadCol].Text, out int cantidad) || cantidad <= 0)
            {
                result.Errors.Add($"Hoja '{ws.Name}' fila {row}: Cantidad inválida.");
                continue;
            }

            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.DocumentNumber == clienteDoc);
            if (customer is null)
            {
                result.Errors.Add($"Hoja '{ws.Name}' fila {row}: No existe cliente con documento '{clienteDoc}'.");
                continue;
            }

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Name == productoNom);
            if (product is null)
            {
                result.Errors.Add($"Hoja '{ws.Name}' fila {row}: No existe producto '{productoNom}'.");
                continue;
            }

            decimal unitPrice = precioUnitCol > 0 &&
                decimal.TryParse(ws.Cells[row, precioUnitCol].Text.Replace(",", "."), out decimal pu)
                ? pu : product.Price;

            var detail = new SaleDetail
            {
                ProductId = product.Id,
                Quantity  = cantidad,
                UnitPrice = unitPrice,
            };

            var sale = new Sale
            {
                CustomerId = customer.Id,
                Notes      = notasCol > 0 ? ws.Cells[row, notasCol].Text.Trim() : string.Empty,
                Total      = detail.Quantity * detail.UnitPrice,
                Details    = [detail],
            };

            _db.Sales.Add(sale);
            result.SalesInserted++;
        }

        await _db.SaveChangesAsync();
    }

    // Returns first matching column index (1-based) or 0 if none found.
    private static int GetCol(Dictionary<string, int> h, params string[] candidates)
    {
        foreach (var key in candidates)
            if (h.TryGetValue(key, out int col)) return col;
        return 0;
    }
}
