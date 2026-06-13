using Firmeza.Web.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Firmeza.Web.Services;

public class ExcelExportService
{
    private readonly ApplicationDbContext _db;

    public ExcelExportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> ExportProductsAsync()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var products = await _db.Products.OrderBy(p => p.Name).ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Productos");

        string[] headers = ["Nombre", "Descripción", "Categoría", "Unidad", "Precio", "Stock", "Activo", "Creado"];
        WriteHeaders(ws, headers);

        for (int i = 0; i < products.Count; i++)
        {
            int row = i + 2;
            var p = products[i];
            ws.Cells[row, 1].Value = p.Name;
            ws.Cells[row, 2].Value = p.Description;
            ws.Cells[row, 3].Value = p.Category;
            ws.Cells[row, 4].Value = p.Unit;
            ws.Cells[row, 5].Value = p.Price;
            ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 6].Value = p.Stock;
            ws.Cells[row, 7].Value = p.IsActive ? "Sí" : "No";
            ws.Cells[row, 8].Value = p.CreatedAt.ToString("dd/MM/yyyy");
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportCustomersAsync()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var customers = await _db.Customers.OrderBy(c => c.LastName).ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Clientes");

        string[] headers = ["Nombre", "Apellido", "Documento", "Teléfono", "Email", "Dirección", "Activo", "Registrado"];
        WriteHeaders(ws, headers);

        for (int i = 0; i < customers.Count; i++)
        {
            int row = i + 2;
            var c = customers[i];
            ws.Cells[row, 1].Value = c.FirstName;
            ws.Cells[row, 2].Value = c.LastName;
            ws.Cells[row, 3].Value = c.DocumentNumber;
            ws.Cells[row, 4].Value = c.Phone;
            ws.Cells[row, 5].Value = c.Email;
            ws.Cells[row, 6].Value = c.Address;
            ws.Cells[row, 7].Value = c.IsActive ? "Sí" : "No";
            ws.Cells[row, 8].Value = c.CreatedAt.ToString("dd/MM/yyyy");
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportSalesAsync()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var sales = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Ventas");

        string[] headers = ["# Venta", "Fecha", "Cliente", "Documento", "Producto", "Cantidad", "Precio Unit.", "Subtotal", "Total Venta", "Estado"];
        WriteHeaders(ws, headers);

        int row = 2;
        foreach (var sale in sales)
        {
            foreach (var detail in sale.Details)
            {
                ws.Cells[row, 1].Value = sale.Id;
                ws.Cells[row, 2].Value = sale.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                ws.Cells[row, 3].Value = $"{sale.Customer.FirstName} {sale.Customer.LastName}";
                ws.Cells[row, 4].Value = sale.Customer.DocumentNumber;
                ws.Cells[row, 5].Value = detail.Product.Name;
                ws.Cells[row, 6].Value = detail.Quantity;
                ws.Cells[row, 7].Value = detail.UnitPrice;
                ws.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 8].Value = detail.Quantity * detail.UnitPrice;
                ws.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 9].Value = sale.Total;
                ws.Cells[row, 9].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 10].Value = sale.Status;
                row++;
            }
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    private static void WriteHeaders(ExcelWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 70, 229));
            cell.Style.Font.Color.SetColor(Color.White);
        }
    }
}
