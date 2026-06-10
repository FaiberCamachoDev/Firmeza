using Firmeza.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Web.Services;

public class PdfReceiptService
{
    private readonly string _outputDirectory;

    public PdfReceiptService(IWebHostEnvironment env)
    {
        _outputDirectory = Path.Combine(env.WebRootPath, "recibos");
        Directory.CreateDirectory(_outputDirectory);
    }

    // Generates the PDF, saves it to wwwroot/recibos and returns the relative path.
    public string Generate(Sale sale)
    {
        var fileName = $"recibo-{sale.Id:D6}-{sale.CreatedAt:yyyyMMdd}.pdf";
        var filePath = Path.Combine(_outputDirectory, fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, sale));
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Firmeza — Sistema Administrativo | ").FontSize(9).FontColor(Colors.Grey.Medium);
                    t.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });

        document.GeneratePdf(filePath);
        return $"/recibos/{fileName}";
    }

    public bool Exists(int saleId, DateTime createdAt)
    {
        var fileName = $"recibo-{saleId:D6}-{createdAt:yyyyMMdd}.pdf";
        return File.Exists(Path.Combine(_outputDirectory, fileName));
    }

    public string GetRelativePath(int saleId, DateTime createdAt)
    {
        var fileName = $"recibo-{saleId:D6}-{createdAt:yyyyMMdd}.pdf";
        return $"/recibos/{fileName}";
    }

    private static void ComposeHeader(IContainer container)
    {
        container.BorderBottom(1).BorderColor(Colors.Indigo.Medium).PaddingBottom(10).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("FIRMEZA").Bold().FontSize(22).FontColor(Colors.Indigo.Darken2);
                col.Item().Text("Comprobante de Venta").FontSize(12).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void ComposeContent(IContainer container, Sale sale)
    {
        decimal subtotal = sale.Details.Sum(d => d.Quantity * d.UnitPrice);
        decimal iva      = Math.Round(subtotal * 0.16m, 2);
        decimal total    = subtotal + iva;

        container.PaddingTop(20).Column(col =>
        {
            // Sale info + customer
            col.Item().Row(row =>
            {
                // Left: sale data
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                {
                    c.Item().Text("Datos de la Venta").Bold().FontSize(12);
                    c.Item().PaddingTop(4).Text($"N° de Venta: #{sale.Id:D6}");
                    c.Item().Text($"Fecha: {sale.CreatedAt:dd/MM/yyyy HH:mm}");
                    c.Item().Text($"Estado: {sale.Status}");
                    if (!string.IsNullOrEmpty(sale.Notes))
                        c.Item().Text($"Notas: {sale.Notes}");
                });

                row.ConstantItem(16);

                // Right: customer
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                {
                    c.Item().Text("Datos del Cliente").Bold().FontSize(12);
                    c.Item().PaddingTop(4).Text($"{sale.Customer.FirstName} {sale.Customer.LastName}");
                    c.Item().Text($"Documento: {sale.Customer.DocumentNumber}");
                    if (!string.IsNullOrEmpty(sale.Customer.Phone))
                        c.Item().Text($"Teléfono: {sale.Customer.Phone}");
                    if (!string.IsNullOrEmpty(sale.Customer.Email))
                        c.Item().Text($"Email: {sale.Customer.Email}");
                    if (!string.IsNullOrEmpty(sale.Customer.Address))
                        c.Item().Text($"Dirección: {sale.Customer.Address}");
                });
            });

            // Products table
            col.Item().PaddingTop(20).Text("Detalle de Productos").Bold().FontSize(13);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(4);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                });

                // Header
                table.Header(h =>
                {
                    foreach (var label in new[] { "Producto", "Cant.", "Precio Unit.", "Subtotal" })
                    {
                        h.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                            .Text(label).Bold().FontColor(Colors.White);
                    }
                });

                // Rows
                bool alt = false;
                foreach (var d in sale.Details)
                {
                    string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                    alt = !alt;

                    table.Cell().Background(bg).Padding(6).Text(d.Product.Name);
                    table.Cell().Background(bg).Padding(6).AlignCenter().Text(d.Quantity.ToString());
                    table.Cell().Background(bg).Padding(6).AlignRight().Text($"${d.UnitPrice:N2}");
                    table.Cell().Background(bg).Padding(6).AlignRight().Text($"${d.Quantity * d.UnitPrice:N2}");
                }
            });

            // Totals
            col.Item().PaddingTop(10).AlignRight().Width(220).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("Subtotal:").Bold();
                    r.ConstantItem(80).AlignRight().Text($"${subtotal:N2}");
                });
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("IVA (16%):").Bold();
                    r.ConstantItem(80).AlignRight().Text($"${iva:N2}");
                });
                c.Item().BorderTop(1).BorderColor(Colors.Indigo.Medium).PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("TOTAL:").Bold().FontSize(13).FontColor(Colors.Indigo.Darken2);
                    r.ConstantItem(80).AlignRight().Text($"${total:N2}").Bold().FontSize(13).FontColor(Colors.Indigo.Darken2);
                });
            });
        });
    }
}
