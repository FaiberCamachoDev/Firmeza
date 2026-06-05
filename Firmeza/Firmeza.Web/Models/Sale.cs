using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firmeza.Web.Models;

public class Sale
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Pendiente";

    [Column(TypeName = "numeric(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    public ICollection<SaleDetail> Details { get; set; } = [];
}
