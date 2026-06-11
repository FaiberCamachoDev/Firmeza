using System.ComponentModel.DataAnnotations;

namespace Firmeza.Api.DTOs.Products;

public class ProductUpdateDto
{
    [MaxLength(150)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal? Price { get; set; }

    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public bool? IsActive { get; set; }
}
