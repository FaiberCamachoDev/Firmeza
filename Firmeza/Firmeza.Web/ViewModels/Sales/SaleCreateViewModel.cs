using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels.Sales;

public class SaleCreateViewModel
{
    [Required(ErrorMessage = "Selecciona un cliente.")]
    public int CustomerId { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    public List<SaleDetailItem> Items { get; set; } = [];

    public class SaleDetailItem
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Quantity { get; set; }
    }
}
