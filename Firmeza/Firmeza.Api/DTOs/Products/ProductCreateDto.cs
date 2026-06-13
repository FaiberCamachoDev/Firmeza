using System.ComponentModel.DataAnnotations;

namespace Firmeza.Api.DTOs.Products;

public class ProductCreateDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
