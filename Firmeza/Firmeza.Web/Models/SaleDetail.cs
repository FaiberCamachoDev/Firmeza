using System.ComponentModel.DataAnnotations.Schema;

namespace Firmeza.Web.Models;

public class SaleDetail
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    // Precio capturado al momento de la venta — no cambia si el producto se actualiza
    [Column(TypeName = "numeric(18,2)")]
    public decimal UnitPrice { get; set; }
    
    public decimal Subtotal => Quantity * UnitPrice;
}
