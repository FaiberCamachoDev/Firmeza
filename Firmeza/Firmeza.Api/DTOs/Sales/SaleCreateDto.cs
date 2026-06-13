using System.ComponentModel.DataAnnotations;

namespace Firmeza.Api.DTOs.Sales;

public class SaleCreateDto
{
    [Required]
    public int CustomerId { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    [Required, MinLength(1, ErrorMessage = "Debe incluir al menos un producto.")]
    public List<SaleDetailCreateDto> Items { get; set; } = [];
}

public class SaleDetailCreateDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Quantity { get; set; }
}
